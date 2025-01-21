

using Alt.Json;
using GLTF.Schema;
using Il2CppSystem.Reflection;
using RedLoader;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Plugins;
using Color = System.Drawing.Color;

namespace SonsSdk.AssetImporting.GLTF
{
    public class IngredientImport: GLTFImportPlugin
    {
        public override string DisplayName => "MSFT_Ingredient";
        public override string Description => "Import ingredients for structure crafting.";
        public override GLTFImportPluginContext CreateInstance(GLTFImportContext context)
        {
            return new IngredientImportContext();
        }
    }
    
    public class IngredientImportContext: GLTFImportPluginContext
    {
        public override void OnAfterImportNode(Node node, int nodeIndex, GameObject nodeObject)
        {
            var mesh = node.Mesh?.Value;
            if (mesh == null)
                return;

            if (mesh.Extras == null)
            {
                return;
            }
            
            var reader = mesh.Extras.CreateReader();
            reader.Read();

            int? prop = -1;
            
            while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
            {
                var extraProperty = reader.Value.ToString();
                switch (extraProperty)
                {
                    case "ItemId":
                        prop = reader.ReadAsInt32();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Close();

            if (prop == null || prop.Value < 1)
            {
                return;
            }

            if(GLTFSceneImporter.DebugLogging) RLog.Msg(Color.Pink, $"Found ItemId: {prop.Value}");

            var comp = nodeObject.AddComponentInternal("Sons.Crafting.Structures.StructureCraftingNodeIngredient");
            var p1 = comp.GetIl2CppType().GetField("_itemId", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty);
            p1.SetValue(comp, prop.Value);
        }
    }
}