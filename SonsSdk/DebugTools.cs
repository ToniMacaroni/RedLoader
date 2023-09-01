using Il2CppInterop.Runtime.Injection;
using RedLoader;
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
    
    public static Material GetHdrpMaterial(Color? color = null)
    {
        var material = new Material(Shader.Find("HDRP/Lit"));
        if (color.HasValue)
        {
            material.SetColor("_BaseColor", color.Value);
        }
        
        return material;
    }

    public static void DrawGlLine(Vector3 start, Vector3 end, float thickness = 1)
    {
        _glMaterial.SetPass(0);
        GL.PushMatrix();
        
        Vector3 direction = (end - start).normalized;
        
        Vector3 normal = Vector3.Cross(direction, Camera.main.transform.forward).normalized;
        
        float halfThickness = thickness * 0.5f;
        
        Vector3 v1 = start - normal * halfThickness;
        Vector3 v2 = start + normal * halfThickness;
        Vector3 v3 = end + normal * halfThickness;
        Vector3 v4 = end - normal * halfThickness;
        
        GL.Begin(0x0007);
        GL.Color(Color.red);
        
        GL.Vertex(v1);
        GL.Vertex(v2);
        GL.Vertex(v3);
        GL.Vertex(v4);
        
        GL.End();
        GL.PopMatrix();
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
    /// Class for drawing a line in the world.
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
        
        public LineDrawer()
        {
            LineRenderer = GetNewLineRenderer();
            Transform = LineRenderer.transform;
        }

        public void SetLine(Vector3 start, Vector3 end)
        {
            SetPosition(start);
            
            LineRenderer.SetPosition(0, start);
            LineRenderer.SetPosition(1, end);
        }

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

public class ImLineDrawer
{
    public Vector3 Start;
    public Vector3 End;
    
    private bool _isRendering;
    
    public static ImLineDrawer StartNew()
    {
        var drawer = new ImLineDrawer();
        drawer.BeginRendering();
        return drawer;
    }

    public void SetLine(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    public void BeginRendering()
    {
        if (_isRendering)
            return;
        
        _isRendering = true;
        
        SdkEvents.OnCameraRender.Subscribe(OnRender);
    }
    
    public void EndRendering()
    {
        if (!_isRendering)
            return;
        
        _isRendering = false;
        
        SdkEvents.OnCameraRender.Unsubscribe(OnRender);
    }
    
    private void OnRender(ScriptableRenderContext context, Il2CppSystem.Collections.Generic.List<Camera> cams)
    {
        DebugTools.DrawGlLine(Start, End);
    }
}