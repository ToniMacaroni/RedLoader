using UnityEditor;
using GLTF.Schema;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.AssetImporters;
#endif

namespace UnityGLTF.Plugins
{
	public class GLTFImportContext
	{
		public readonly List<GLTFImportPluginContext> Plugins;
		public readonly GLTFSettings Settings;

		public GLTFSceneImporter SceneImporter;
		public GLTFRoot Root => SceneImporter?.Root;

		private List<GLTFImportPluginContext> InitializePlugins(GLTFSettings settings)
		{
			var plugins = new List<GLTFImportPluginContext>();
			foreach (var plugin in settings.ImportPlugins)
			{
				if (plugin != null && plugin.Enabled)
				{
					var instance = plugin.CreateInstance(this);
					if (instance != null) plugins.Add(instance);
				}
			}

			return plugins;
		}
		
		internal GLTFImportContext(GLTFSettings settings)
		{
			Plugins = InitializePlugins(settings);
			Settings = settings;
		}

		public bool TryGetPlugin<T>(out GLTFImportPluginContext o) where T: GLTFImportPluginContext
		{
			foreach (var plugin in Plugins)
			{
				if (plugin is T t)
				{
					o = t;
					return true;
				}
			}

			o = null;
			return false;
		}
	}
}
