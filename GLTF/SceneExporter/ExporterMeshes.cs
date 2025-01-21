using UnityGLTF.Extensions;
using UnityEngine.Rendering;
using GLTF.Schema;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

using UnityEngine;
#if UNITY_EDITOR // required for in-editor access to non-readable meshes
using UnityEditor;
#endif

namespace UnityGLTF
{
	public partial class GLTFSceneExporter
	{
		private struct MeshAccessors
		{
			public AccessorId aPosition, aNormal, aTangent, aTexcoord0, aTexcoord1, aColor0, aJoints0, aWeights0;
			public Dictionary<int, MeshPrimitive> subMeshPrimitives;
		}

		private struct BlendShapeAccessors
		{
			public List<Dictionary<string, AccessorId>> targets;
			public List<Double> weights;
			public List<string> targetNames;
			internal SkinnedMeshRenderer firstSkinnedMeshRenderer; 
		}

		private readonly Dictionary<Mesh, MeshAccessors> _meshToPrims = new Dictionary<Mesh, MeshAccessors>();
		private readonly Dictionary<Mesh, BlendShapeAccessors> _meshToBlendShapeAccessors = new Dictionary<Mesh, BlendShapeAccessors>();
		private readonly Dictionary<SkinnedMeshRenderer, List<double>> _NodeBlendShapeWeights = new Dictionary<SkinnedMeshRenderer, List<double>>();

		public void RegisterPrimitivesWithNode(Node node, List<GLTFSceneExporter.UniquePrimitive> uniquePrimitives)
		{
			// associate unity meshes with gltf mesh id
			foreach (var primKey in uniquePrimitives)
			{
				_primOwner[primKey] = node.Mesh;
			}
		}

		private static List<GLTFSceneExporter.UniquePrimitive> GetUniquePrimitivesFromGameObjects(IEnumerable<GameObject> primitives)
		{
			var primKeys = new List<GLTFSceneExporter.UniquePrimitive>();

			foreach (var prim in primitives)
			{
				Mesh meshObj = null;
				SkinnedMeshRenderer smr = null;
				var filter = prim.GetComponent<MeshFilter>();
				if (filter)
				{
					meshObj = filter.sharedMesh;
				}
				else
				{
					smr = prim.GetComponent<SkinnedMeshRenderer>();
					if (smr)
					{
						meshObj = smr.sharedMesh;
					}
				}

				if (!meshObj)
				{
					UnityEngine.Debug.LogWarning($"MeshFilter.sharedMesh on GameObject:{prim.name} is missing, skipping", prim);
					GLTFSceneExporter.exportPrimitiveMarker.End();
					return null;
				}


#if UNITY_EDITOR
				if (!MeshIsReadable(meshObj) && EditorUtility.IsPersistent(meshObj))
				{
#if UNITY_2019_3_OR_NEWER
					var assetPath = AssetDatabase.GetAssetPath(meshObj);
					if (assetPath?.Length > 30) assetPath = "..." + assetPath.Substring(assetPath.Length - 30);
					var otherOption = Application.isPlaying ? "No, skip mesh" : "Cancel export";
					if(EditorUtility.DisplayDialog("Exporting mesh but mesh is not readable",
							$"The mesh {meshObj.name} is not readable. Do you want to change its import settings and make it readable now?\n\n" + assetPath,
							"Make it readable", otherOption,
							DialogOptOutDecisionType.ForThisSession, MakeMeshReadableDialogueDecisionKey))
#endif
					{
						var path = AssetDatabase.GetAssetPath(meshObj);
						var importer = AssetImporter.GetAtPath(path) as ModelImporter;
						if (importer)
						{
							importer.isReadable = true;
							importer.SaveAndReimport();
						}
					}
#if UNITY_2019_3_OR_NEWER
					else
					{
						if (Application.isPlaying)
						{
							UnityEngine.Debug.LogWarning(null, $"The mesh {meshObj.name} is not readable. Skipping", meshObj);
							exportPrimitiveMarker.End();
						}
						else
						{
							UnityEngine.Debug.LogError(null, $"The mesh {meshObj.name} is not readable and you decided to cancel the export. Canceling", meshObj);
							exportPrimitiveMarker.End();
							throw new OperationCanceledException($"Canceled export because a mesh ({meshObj}) is not readable.");
						}
						return null;
					}
#endif
				}
#endif

				if (Application.isPlaying && !MeshIsReadable(meshObj))
				{
					UnityEngine.Debug.LogWarning($"The mesh {meshObj.name} is not readable. Skipping", null);
					GLTFSceneExporter.exportPrimitiveMarker.End();
					return null;
				}

				var renderer = prim.GetComponent<MeshRenderer>();
				if (!renderer) smr = prim.GetComponent<SkinnedMeshRenderer>();

				if(!renderer && !smr)
				{
					UnityEngine.Debug.LogWarning("GameObject does have neither renderer nor SkinnedMeshRenderer! " + prim.name, prim);
					GLTFSceneExporter.exportPrimitiveMarker.End();
					return null;
				}

				var materialsObj = renderer ? renderer.sharedMaterials : smr.sharedMaterials;

				var primKey = new GLTFSceneExporter.UniquePrimitive();
				primKey.Mesh = meshObj;
				primKey.Materials = materialsObj;
				primKey.SkinnedMeshRenderer = smr;

				primKeys.Add(primKey);
			}

			return primKeys;
		}

		public NodeId ExportNode(GameObject gameObject) => ExportNode(gameObject.transform);

		/// <summary>
		/// Convenience wrapper around ExportMesh(string, List<UniquePrimitive>)
		/// </summary>
		public MeshId ExportMesh(Mesh mesh)
		{
			var uniquePrimitives = new List<GLTFSceneExporter.UniquePrimitive>
			{
				new GLTFSceneExporter.UniquePrimitive()
				{
					Mesh = mesh,
					SkinnedMeshRenderer = null,
					Materials = new [] { DefaultMaterial },
				}
			};
			return ExportMesh(mesh.name, uniquePrimitives);
		}

		public MeshId ExportMesh(string name, List<GLTFSceneExporter.UniquePrimitive> uniquePrimitives)
		{
			GLTFSceneExporter.exportMeshMarker.Begin();

			// check if this set of primitives is already a mesh
			MeshId existingMeshId = null;

			foreach (var prim in uniquePrimitives)
			{
				MeshId tempMeshId;
				if (_primOwner.TryGetValue(prim, out tempMeshId) && (existingMeshId == null || tempMeshId == existingMeshId))
				{
					existingMeshId = tempMeshId;
				}
				else
				{
					existingMeshId = null;
					break;
				}
			}

			// if so, return that mesh id
			if (existingMeshId != null)
			{
				GLTFSceneExporter.exportMeshMarker.End();
				return existingMeshId;
			}

			// if not, create new mesh and return its id
			var mesh = new GLTFMesh();

			if (settings.ExportNames)
			{
				mesh.Name = name;
			}

			mesh.Primitives = new List<MeshPrimitive>(uniquePrimitives.Count);
			foreach (var primKey in uniquePrimitives)
			{
				MeshPrimitive[] meshPrimitives = ExportPrimitive(primKey, mesh);
				if (meshPrimitives != null)
				{
					mesh.Primitives.AddRange(meshPrimitives);
				}
			}

			var id = new MeshId
			{
				Id = _root.Meshes.Count,
				Root = _root
			};

			GLTFSceneExporter.exportMeshMarker.End();

			if (mesh.Primitives.Count > 0)
			{
				_root.Meshes.Add(mesh);

				var uniquePrimitive = uniquePrimitives.FirstOrDefault();
				if (uniquePrimitive.Mesh)
				{
					foreach (var plugin in _plugins)
						plugin?.AfterMeshExport(this, uniquePrimitive.Mesh, mesh, id.Id);
				}
				
				return id;
			}

			return null;
		}

		// a mesh *might* decode to multiple prims if there are submeshes
		private MeshPrimitive[] ExportPrimitive(GLTFSceneExporter.UniquePrimitive primKey, GLTFMesh mesh)
		{
			GLTFSceneExporter.exportPrimitiveMarker.Begin();

			Mesh meshObj = primKey.Mesh;
			Material[] materialsObj = primKey.Materials;

			var maxOfSubMeshesAndMaterials = Math.Max(meshObj.subMeshCount, materialsObj.Length);
			var prims = new MeshPrimitive[maxOfSubMeshesAndMaterials];
			
			List<MeshPrimitive> nonEmptyPrims = null;
			var vertices = meshObj.vertices;
			if (vertices.Length < 1)
			{
				Console.WriteLine("MeshFilter does not contain any vertices or they can't be accessed, won't export: " + meshObj.name, meshObj);
				GLTFSceneExporter.exportPrimitiveMarker.End();
				return null;
			}

			if (!_meshToPrims.ContainsKey(meshObj))
			{
				AccessorId aPosition = null, aNormal = null, aTangent = null, aTexcoord0 = null, aTexcoord1 = null, aColor0 = null;

				aPosition = ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(meshObj.vertices, SchemaExtensions.CoordinateSpaceConversionScale));

				if (meshObj.normals.Length != 0)
					aNormal = ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(meshObj.normals, SchemaExtensions.CoordinateSpaceConversionScale));

				if (meshObj.tangents.Length != 0)
					aTangent = ExportAccessor(SchemaExtensions.ConvertTangentCoordinateSpaceAndCopy(meshObj.tangents, SchemaExtensions.TangentSpaceConversionScale));

				if (meshObj.uv.Length != 0)
					aTexcoord0 = ExportAccessor(SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv));

				if (meshObj.uv2.Length != 0)
					aTexcoord1 = ExportAccessor(SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv2));

				if (settings.ExportVertexColors && meshObj.colors.Length != 0)
					aColor0 = ExportAccessor(QualitySettings.activeColorSpace == ColorSpace.Linear ? meshObj.colors : meshObj.colors.ToLinear(), true);

				aPosition.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
				if (aNormal != null) aNormal.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
				if (aTangent != null) aTangent.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
				if (aTexcoord0 != null) aTexcoord0.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
				if (aTexcoord1 != null) aTexcoord1.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
				if (aColor0 != null) aColor0.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;

				_meshToPrims.Add(meshObj, new MeshAccessors()
				{
					aPosition = aPosition,
					aNormal = aNormal,
					aTangent = aTangent,
					aTexcoord0 = aTexcoord0,
					aTexcoord1 = aTexcoord1,
					aColor0 = aColor0,
					subMeshPrimitives = new Dictionary<int, MeshPrimitive>()
				});
			}

			var accessors = _meshToPrims[meshObj];

			// walk submeshes and export the ones with non-null meshes
			for (int id = 0; id < maxOfSubMeshesAndMaterials; id++)
			{
				var mat = materialsObj[id % materialsObj.Length];
				var submesh = id % meshObj.subMeshCount;
				
				if (!mat) continue;
				if (meshObj.GetIndexCount(submesh) <= 0) continue;

				if (!accessors.subMeshPrimitives.ContainsKey(submesh))
				{
					var primitive = new MeshPrimitive();

					var topology = meshObj.GetTopology(submesh);
					var indices = meshObj.GetIndices(submesh);
					if (topology == MeshTopology.Triangles) SchemaExtensions.FlipTriangleFaces(indices);

					primitive.Mode = GetDrawMode(topology);
					primitive.Indices = ExportAccessor(indices, true);
					primitive.Indices.Value.BufferView.Value.Target = BufferViewTarget.ElementArrayBuffer;

					primitive.Attributes = new Dictionary<string, AccessorId>();
					primitive.Attributes.Add(SemanticProperties.POSITION, accessors.aPosition);

					if (accessors.aNormal != null)
						primitive.Attributes.Add(SemanticProperties.NORMAL, accessors.aNormal);
					if (accessors.aTangent != null)
						primitive.Attributes.Add(SemanticProperties.TANGENT, accessors.aTangent);
					if (accessors.aTexcoord0 != null)
						primitive.Attributes.Add(SemanticProperties.TEXCOORD_0, accessors.aTexcoord0);
					if (accessors.aTexcoord1 != null)
						primitive.Attributes.Add(SemanticProperties.TEXCOORD_1, accessors.aTexcoord1);
					if (accessors.aColor0 != null)
						primitive.Attributes.Add(SemanticProperties.COLOR_0, accessors.aColor0);

					primitive.Material = null;

					ExportBlendShapes(primKey.SkinnedMeshRenderer, meshObj, submesh, primitive, mesh);

					accessors.subMeshPrimitives.Add(submesh, primitive);
				}

				var submeshPrimitive = accessors.subMeshPrimitives[submesh];
				prims[id] = new MeshPrimitive(submeshPrimitive, _root)
				{
					Material = ExportMaterial(mat),
				};
				// this will contain only the last one
				accessors.subMeshPrimitives[submesh] = prims[submesh];
			}

            nonEmptyPrims = new List<MeshPrimitive>(prims.Length);
            for (var i = 0; i < prims.Length; i++)
            {
	            var prim = prims[i];
	            // remove any prims that have empty triangles
	            if (EmptyPrimitive(prim)) continue;
	            // invoke pre export event
	            foreach (var plugin in _plugins)
		            plugin?.AfterPrimitiveExport(this, meshObj, prim, i);
	            nonEmptyPrims.Add(prim);
            }
            prims = nonEmptyPrims.ToArray();

            GLTFSceneExporter.exportPrimitiveMarker.End();

            return prims;
		}

		private List<double> GetBlendShapeWeights(SkinnedMeshRenderer smr, Mesh meshObj)
		{
			if (_NodeBlendShapeWeights.TryGetValue(smr, out var w))
				return w;

			List<Double> weights = new List<double>(meshObj.blendShapeCount);
			
			for (int blendShapeIndex = 0; blendShapeIndex < meshObj.blendShapeCount; blendShapeIndex++)
			{
				// We need to get the weight from the SkinnedMeshRenderer because this represents the currently
				// defined weight by the user to apply to this blend shape.  If we instead got the value from
				// the unityMesh, it would be a _per frame_ weight, and for a single-frame blend shape, that would
				// always be 100.  A blend shape might have more than one frame if a user wanted to more tightly
				// control how a blend shape will be animated during weight changes (e.g. maybe they want changes
				// between 0-50% to be really minor, but between 50-100 to be extreme, hence they'd have two frames
				// where the first frame would have a weight of 50 (meaning any weight between 0-50 should be relative
				// to the values in this frame) and then any weight between 50-100 would be relevant to the weights in
				// the second frame.  See Post 20 for more info:
				// https://forum.unity3d.com/threads/is-there-some-method-to-add-blendshape-in-editor.298002/#post-2015679
				var frameWeight = meshObj.GetBlendShapeFrameWeight(blendShapeIndex, 0);
				weights.Add(smr.GetBlendShapeWeight(blendShapeIndex) / frameWeight);
			}

			return weights;
		}
		
		// Blend Shapes / Morph Targets
		// Adopted from Gary Hsu (bghgary)
		// https://github.com/bghgary/glTF-Tools-for-Unity/blob/master/UnityProject/Assets/Gltf/Editor/Exporter.cs
		private void ExportBlendShapes(SkinnedMeshRenderer smr, Mesh meshObj, int submeshIndex, MeshPrimitive primitive, GLTFMesh mesh)
		{
			if (settings.BlendShapeExportProperties == GLTFSettings.BlendShapeExportPropertyFlags.None)
				return;

			if (_meshToBlendShapeAccessors.TryGetValue(meshObj, out var data))
			{
				primitive.Targets = data.targets;
				mesh.Weights = data.weights;
				mesh.TargetNames = data.targetNames;
				return;
			}

			if (smr != null && meshObj.blendShapeCount > 0)
			{
				List<Dictionary<string, AccessorId>> targets = new List<Dictionary<string, AccessorId>>(meshObj.blendShapeCount);
				List<Double> weights;
				List<string> targetNames = new List<string>(meshObj.blendShapeCount);

#if UNITY_2019_3_OR_NEWER
				var meshHasNormals = meshObj.HasVertexAttribute(VertexAttribute.Normal);
				var meshHasTangents = meshObj.HasVertexAttribute(VertexAttribute.Tangent);
#else
				var meshHasNormals = meshObj.normals.Length > 0;
				var meshHasTangents = meshObj.tangents.Length > 0;
#endif

				for (int blendShapeIndex = 0; blendShapeIndex < meshObj.blendShapeCount; blendShapeIndex++)
				{
					GLTFSceneExporter.exportBlendShapeMarker.Begin();

					targetNames.Add(meshObj.GetBlendShapeName(blendShapeIndex));
					// As described above, a blend shape can have multiple frames.  Given that glTF only supports a single frame
					// per blend shape, we'll always use the final frame (the one that would be for when 100% weight is applied).
					int frameIndex = meshObj.GetBlendShapeFrameCount(blendShapeIndex) - 1;

					var deltaVertices = new Vector3[meshObj.vertexCount];
					var deltaNormals = new Vector3[meshObj.vertexCount];
					var deltaTangents = new Vector3[meshObj.vertexCount];
					meshObj.GetBlendShapeFrameVertices(blendShapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

					var exportTargets = new Dictionary<string, AccessorId>();

					if (!settings.BlendShapeExportSparseAccessors)
					{
						var positionAccessor = ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(deltaVertices, SchemaExtensions.CoordinateSpaceConversionScale));
						positionAccessor.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
						exportTargets.Add(SemanticProperties.POSITION, positionAccessor);
					}
					else
					{
						// UnityEngine.Debug.Log("Delta Vertices:\n"+string.Join("\n ", deltaVertices));
						// UnityEngine.Debug.Log("Vertices:\n"+string.Join("\n ", meshObj.vertices));
						// Experimental: sparse accessor.
						// - get the accessor we want to base this upon
						// - this is how position is originally exported:
						//   ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(meshObj.vertices, SchemaExtensions.CoordinateSpaceConversionScale));
						var exportedAccessor = ExportSparseAccessor(null, null, SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(deltaVertices, SchemaExtensions.CoordinateSpaceConversionScale));
						if (exportedAccessor != null)
						{
							exportTargets.Add(SemanticProperties.POSITION, exportedAccessor);
						}
					}

					if (meshHasNormals && settings.BlendShapeExportProperties.HasFlag(GLTFSettings.BlendShapeExportPropertyFlags.Normal))
					{
						if (!settings.BlendShapeExportSparseAccessors)
						{
							var accessor = ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(deltaNormals, SchemaExtensions.CoordinateSpaceConversionScale));
							accessor.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
							exportTargets.Add(SemanticProperties.NORMAL, accessor);
						}
						else
						{
							exportTargets.Add(SemanticProperties.NORMAL, ExportSparseAccessor(null, null, SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(deltaNormals, SchemaExtensions.CoordinateSpaceConversionScale)));
						}
					}
					if (meshHasTangents && settings.BlendShapeExportProperties.HasFlag(GLTFSettings.BlendShapeExportPropertyFlags.Tangent))
					{
						if (!settings.BlendShapeExportSparseAccessors)
						{
							var accessor = ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(deltaTangents, SchemaExtensions.CoordinateSpaceConversionScale));
							accessor.Value.BufferView.Value.Target = BufferViewTarget.ArrayBuffer;
							exportTargets.Add(SemanticProperties.TANGENT, accessor);
						}
						else
						{
							exportTargets.Add(SemanticProperties.TANGENT, ExportSparseAccessor(null, null, SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(deltaTangents, SchemaExtensions.CoordinateSpaceConversionScale)));
						}
					}

					targets.Add(exportTargets);
					
					GLTFSceneExporter.exportBlendShapeMarker.End();
				}

				weights = GetBlendShapeWeights(smr, meshObj);
				if(weights.Any() && targets.Any())
				{
					mesh.Weights = weights;
					mesh.TargetNames = targetNames;
					primitive.Targets = targets;
					_NodeBlendShapeWeights.Add(smr, weights);
				}
				else
				{
					mesh.Weights = null;
					mesh.TargetNames = null;
					primitive.Targets = null;
				}

				// cache the exported data; we can re-use it between all submeshes of a mesh.
				_meshToBlendShapeAccessors.Add(meshObj, new BlendShapeAccessors()
				{
					targets = targets,
					weights = weights,
					targetNames = targetNames,
					firstSkinnedMeshRenderer = smr
				});
			}
		}

		private static bool EmptyPrimitive(MeshPrimitive prim)
		{
			if (prim == null || prim.Attributes == null)
			{
				return true;
			}
			return false;
		}

		private static DrawMode GetDrawMode(MeshTopology topology)
		{
			switch (topology)
			{
				case MeshTopology.Points: return DrawMode.Points;
				case MeshTopology.Lines: return DrawMode.Lines;
				case MeshTopology.LineStrip: return DrawMode.LineStrip;
				case MeshTopology.Triangles: return DrawMode.Triangles;
			}

			throw new Exception("glTF does not support Unity mesh topology: " + topology);
		}

#if UNITY_EDITOR
		private const string MakeMeshReadableDialogueDecisionKey = nameof(MakeMeshReadableDialogueDecisionKey);
		private static PropertyInfo canAccessProperty =
			typeof(Mesh).GetProperty("canAccess", BindingFlags.Instance | BindingFlags.Default | BindingFlags.NonPublic);
#endif

		private static bool MeshIsReadable(Mesh mesh)
		{
#if UNITY_EDITOR
			return mesh.isReadable || (bool) (canAccessProperty?.GetMethod?.Invoke(mesh, null) ?? true);
#else
			return mesh.isReadable;
#endif
		}
	}
}
