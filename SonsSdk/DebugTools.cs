using System.ComponentModel;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using RedLoader;
using Shapes;
using Sons.Items.Core;
using Sons.Weapon;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SonsSdk;

public class DebugTools
{
    private static Material _glMaterial = GetHdrpMaterial(Color.red);
    
    private static MethodBase _ueInspectMethod;
    private static PropertyInfo _ueShowMenuProperty;

    public static Material GetHdrpMaterial(Color? color = null)
    {
        var material = new Material(Shader.Find("HDRP/Lit"));
        if (color.HasValue)
        {
            material.SetColor("_BaseColor", color.Value);
        }
        
        return material;
    }

    public static GameObject CreatePrimitive(PrimitiveType type, Vector3? pos = null, Color? color = null)
    {
        var go = GameObject.CreatePrimitive(type);
        var meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.material = GetHdrpMaterial(color);
        if (pos.HasValue)
        {
            go.transform.position = pos.Value;
        }

        return go;
    }

    public static Disc CreateDisc(Vector3 position, float radius = 0.2f, Color? color = null, bool alwaysInFront = true, bool billboard = true)
    {
        var disc = new GameObject("Disc").AddComponent<Disc>();
        disc.transform.position = position;
        disc.Radius = radius;
        disc.Color = color ?? Color.red;
        if(alwaysInFront)
            disc.ZTest = CompareFunction.Always;
        if(billboard)
            disc.Geometry = DiscGeometry.Billboard;

        return disc;
    }
    
    public static Cuboid CreateCuboid(Vector3 position, Vector3? size = null, Color? color = null, bool alwaysInFront = true)
    {
        var cuboid = new GameObject("Cuboid").AddComponent<Cuboid>();
        cuboid.transform.position = position;
        cuboid.Size = size ?? new(0.5f, 0.5f, 0.5f);
        cuboid.Color = color ?? Color.red;
        if(alwaysInFront)
            cuboid.ZTest = CompareFunction.Always;

        return cuboid;
    }

    internal static LineRenderer GetNewLineRenderer(bool useWorldSpace = true, float width = 0.2f)
    {
        // Spear
        var rangedWeapon = ItemDatabaseManager.ItemById(474).HeldPrefab.GetComponent<RangedWeapon>();
        var prefab = rangedWeapon._trajectoryPathPrefab;
        var lineRenderer = Object.Instantiate(prefab).GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = useWorldSpace;
        lineRenderer.startWidth = 1;
        lineRenderer.endWidth = 1;
        lineRenderer.widthMultiplier = width;
        lineRenderer.SetVertexCount(2);

        lineRenderer.gameObject.SetActive(true);
        
        return lineRenderer;
    }

    /// <summary>
    /// Will inspect an object in Unity Explorer if installed.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="showExporer">if true opens Unity Explorer too</param>
    public static void Inspect(object obj, bool showExporer = false)
    {
        if (SonsMod.RegisteredMods.All(x => x.ID != "UnityExplorer"))
        {
            RLog.Msg(System.Drawing.Color.IndianRed, "Unity Explorer is not installed!");
            return;
        }

        if (_ueInspectMethod == null)
        {
            _ueInspectMethod = AccessTools.TypeByName("UnityExplorer.InspectorManager").GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x =>
            {
                return x.Name == "Inspect" && x.GetParameters().First().ParameterType == typeof(object);
            });
        }
        
        if (_ueInspectMethod == null)
        {
            RLog.Msg(System.Drawing.Color.IndianRed, "Couldn't get the inspect method for Unity Explorer!");
            return;
        }
        
        _ueInspectMethod.Invoke(null, new[] { obj, null });

        if (!showExporer)
            return;
        
        if (_ueShowMenuProperty == null)
        {
            _ueShowMenuProperty = AccessTools.TypeByName("UnityExplorer.UI.UIManager").GetProperty("ShowMenu");
        }
        
        if (_ueShowMenuProperty == null)
        {
            return;
        }
        
        _ueShowMenuProperty.SetValue(null, true);
    }
    
    /// <summary>
    /// Class for drawing a line in the world.
    /// The constructor will create a new line renderer.
    /// </summary>
    public class LineDrawer
    {
        public LineRenderer LineRenderer;
        public Transform Transform;
        
        public bool IsValid => _isDestroyed;

        public bool Active
        {
            get => IsValid && LineRenderer.gameObject.activeSelf;
            set
            {
                if(!IsValid)
                    return;
                
                LineRenderer.gameObject.SetActive(value);
            }
        }

        private bool _isDestroyed;
        
        public LineDrawer(bool useWorldSpace = true, float width = 0.2f)
        {
            LineRenderer = GetNewLineRenderer(useWorldSpace, width);
            Transform = LineRenderer.transform;
        }

        /// <summary>
        /// Sets the line start and end positions.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void SetLine(Vector3 start, Vector3 end)
        {
            SetPosition(start);
            
            LineRenderer.SetPosition(0, start);
            LineRenderer.SetPosition(1, end);
        }

        /// <summary>
        /// Sets the position of the line renderer gameobject.
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector3 pos)
        {
            Transform.position = pos;
        }

        public void Destroy()
        {
            if (_isDestroyed)
                return;
            
            Object.Destroy(Transform.gameObject);
            _isDestroyed = true;
        }
    }
}