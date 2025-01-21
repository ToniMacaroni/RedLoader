using UnityGLTF.Plugins;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;
using Il2CppInterop.Runtime.Injection;
using RedLoader;
using UnityEngine;
using Color = System.Drawing.Color;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityGLTF
{
    public class GLTFSettings// : ScriptableObject
    {
	    [Flags]
	    public enum BlendShapeExportPropertyFlags
	    {
		    None = 0,
		    PositionOnly = 1,
		    Normal = 2,
		    Tangent = 4,
		    All = ~0
	    }

	    // Plugins
	    // [SerializeField, HideInInspector]
	    public List<GLTFImportPlugin> ImportPlugins = new();
	    
	    // [SerializeField, HideInInspector]
	    public List<GLTFExportPlugin> ExportPlugins = new();
	    
	    // [Header("Export Settings")]
		// 
		public bool exportNames = true;
		// 
		// // [Tooltip("If on, the entire texture path will be preserved. If off (default), textures are exported at root level.")]
		public bool exportFullPath = false;
		// 
		public bool requireExtensions = false;

		// [Header("Export Visibility")]
		// 
		// // [Tooltip("Uses Camera.main layer settings to filter which objects are exported")]
		public bool useMainCameraVisibility = true;
		// 
		// // [Tooltip("glTF does not support visibility state. If this setting is true, disabled GameObjects will still be exported and be visible in the glTF file.")]
		public bool exportDisabledGameObjects = false;

		// [Header("Export Textures")]
		// 
		// // [Tooltip("(Experimental) Exports PNG/JPEG directly from disk instead of re-encoding from Unity's import result. No channel repacking will happen for these textures. Textures in other formats (PSD, TGA etc) not supported by glTF and in-memory textures (e.g. RenderTextures) are always re-encoded.")]
		public bool tryExportTexturesFromDisk = false;
		//  // [Tooltip("Determines texture export type (PNG or JPEG) based on alpha channel. When false, always exports lossless PNG files.")]
		public bool useTextureFileTypeHeuristic = true;
		//  // [Tooltip("Quality setting for exported JPEG files.")]
		public int defaultJpegQuality = 90;

		// [Header("Export Animation")]
		// 
		public bool exportAnimations = true;
		// [SerializeField, Tooltip("When enabled the Animator State speed parameter is baked into the exported glTF animation")]
		public bool bakeAnimationSpeed = true;
		// // [Tooltip("(Experimental) Export animations using KHR_animation_pointer. Requires the viewer to also support this extension.")]
		// 
		// private bool useAnimationPointer = false;
		// 
		// // [Tooltip("Some viewers can't distinguish between animation clips that have the same name. This option ensures all exported animation names are unique.")]
		public bool uniqueAnimationNames = false;
		// 
		public bool bakeSkinnedMeshes = false;

		// [Header("Export Mesh Data")]
		// 
		public BlendShapeExportPropertyFlags blendShapeExportProperties = BlendShapeExportPropertyFlags.All;
		// 
		// // [Tooltip("(Experimental) Use Sparse Accessors for blend shape export. Not supported on some viewers.")]
		public bool blendShapeExportSparseAccessors = true;
		// 
	    // // [Tooltip("If off, vertex colors are not exported. Vertex Colors aren't supported in some viewers (e.g. Google's SceneViewer).")]
		public bool exportVertexColors = true;

		// [Header("Export Cache")]
		// // [Tooltip("When enabled textures will be cached to disc for faster export times.\n(The cache size is reduced to stay below 1024 MB when the Editor quits)")]
		public bool UseCaching = true;
		
		/// <summary>
		/// Override for the shader to use on created materials
		/// </summary>
		public string CustomShaderName { get; set; }

		public Type DefaultShaderMap = typeof(SonsUberMap);

		public bool ExportNames { get => exportNames; set  => exportNames = value; }
		public bool ExportFullPath { get => exportFullPath; set => exportFullPath = value; }
		public bool UseMainCameraVisibility { get => useMainCameraVisibility; set => useMainCameraVisibility = value; }
		public bool RequireExtensions { get => requireExtensions; set => requireExtensions = value; }
		public bool TryExportTexturesFromDisk { get => tryExportTexturesFromDisk; set => tryExportTexturesFromDisk = value; }
		public bool UseTextureFileTypeHeuristic { get => useTextureFileTypeHeuristic; set => useTextureFileTypeHeuristic = value; }
		public bool ExportVertexColors { get => exportVertexColors; set => exportVertexColors = value; }
		public int DefaultJpegQuality { get => defaultJpegQuality; set => defaultJpegQuality = value; }
		public bool ExportDisabledGameObjects { get => exportDisabledGameObjects; set => exportDisabledGameObjects = value; }
		public bool ExportAnimations { get => exportAnimations; set => exportAnimations = value; }
		public bool BakeAnimationSpeed { get => bakeAnimationSpeed; set => bakeAnimationSpeed = value; }

		[Obsolete("Add/remove \"AnimationPointerPlugin\" from ExportPlugins instead.")]
		public bool UseAnimationPointer
		{
			get
			{
				return ExportPlugins?.Any(x => x is AnimationPointerExport && x.Enabled) ?? false;
			}
			set
			{
				var plugin = ExportPlugins?.FirstOrDefault(x => x is AnimationPointerExport);
				if (plugin != null)
					plugin.Enabled = value;
				if (!value || plugin != null) return;
				
				if (ExportPlugins == null) ExportPlugins = new List<GLTFExportPlugin>();
				ExportPlugins.Add(GLTFPlugin.Create<AnimationPointerExport>());
			}
		}
		public bool UniqueAnimationNames { get => uniqueAnimationNames; set => uniqueAnimationNames = value; }
		public bool BlendShapeExportSparseAccessors { get => blendShapeExportSparseAccessors; set => blendShapeExportSparseAccessors = value; }
		public BlendShapeExportPropertyFlags BlendShapeExportProperties { get => blendShapeExportProperties; set => blendShapeExportProperties = value; }
		public bool BakeSkinnedMeshes { get => bakeSkinnedMeshes; set => bakeSkinnedMeshes = value; }

		public static GLTFSettings GetOrCreateSettings()
	    {
		    if(cachedSettings != null) 
			    return cachedSettings;

		    cachedSettings = GetNewSettings();
		    return cachedSettings;
	    }

	    public static GLTFSettings GetNewSettings()
	    {
			var freshSettings = new GLTFSettings();
		    RegisterPlugins(freshSettings);
		    return freshSettings;
	    }

	    private static GLTFSettings cachedSettings;
	    private static List<GLTFSettings> settingsWherePluginsAreRegistered = new List<GLTFSettings>();

	    private static List<Type> PluginTypes = new()
	    {
		    typeof(AnimationPointerExport),
		    typeof(DracoImport),
		    typeof(GPUInstancingImport),
		    typeof(Ktx2Import),
		    typeof(LightsPunctualExport),
		    typeof(LightsPunctualImport),
		    typeof(LodsExport),
		    typeof(LodsImport),
		    typeof(MaterialExtensionsExport),
		    typeof(MaterialExtensionsImport),
		    typeof(MeshoptImport),
		    typeof(TextureTransformExport),
		    typeof(TextureTransformImport),
		    typeof(UnlitMaterialsExport),
		    typeof(UnlitMaterialsImport),
		    typeof(BakeParticleSystem),
		    typeof(MaterialVariantsPlugin),
		    typeof(TextMeshGameObjectExport)
	    };

	    public static void RegisterPlugin<T>() where T : GLTFPlugin
	    {
		    PluginTypes.Add(typeof(T));
	    }
	    
	    public static void UnregisterPlugin<T>() where T : GLTFPlugin
	    {
		    PluginTypes.Remove(typeof(T));
	    }

	    private static void RegisterPlugins(GLTFSettings settings)
	    {
		    if (settingsWherePluginsAreRegistered.Contains(settings)) return;

		    static List<Type> GetTypesDerivedFrom<T>()
		    {
			    var types = new List<Type>();
			    try
			    {
				    types.AddRange(PluginTypes.Where(t => !(t is T) && typeof(T).IsAssignableFrom(t)));
			    }
			    catch (ReflectionTypeLoadException e)
			    {
				    types.AddRange(e.Types);
			    }
			    catch (Exception)
			    {
				    // ignored
			    }
			    return types;
		    }
		    
		    // Initialize
		    if (settings.ImportPlugins == null) settings.ImportPlugins = new List<GLTFImportPlugin>();
		    if (settings.ExportPlugins == null) settings.ExportPlugins = new List<GLTFExportPlugin>();

		    // Cleanup
		    settings.ImportPlugins.RemoveAll(x => x == null);
		    settings.ExportPlugins.RemoveAll(x => x == null);
		    
		    void FindAndRegisterPlugins<T>(List<T> plugins) where T : GLTFPlugin
		    {
			    foreach (var pluginType in GetTypesDerivedFrom<T>())
			    {
				    if (pluginType.IsAbstract) continue;
				    if (plugins.Any(p => p != null && p.GetType() == pluginType))
					    continue;

				    var newInstance = Activator.CreateInstance(pluginType) as T;
				    if (newInstance == null) continue;

				    newInstance.Enabled = newInstance.EnabledByDefault;

				    plugins.Add(newInstance);
			    }
		    }
		    
		    // Register with TypeCache
		    FindAndRegisterPlugins(settings.ImportPlugins);
		    FindAndRegisterPlugins(settings.ExportPlugins);
		    
		    settingsWherePluginsAreRegistered.Add(settings);
	    }
    }
}
