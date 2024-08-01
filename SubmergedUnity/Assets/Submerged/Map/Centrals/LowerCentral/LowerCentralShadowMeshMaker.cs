using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
public class LowerCentralShadowMeshMaker : MonoBehaviour
{
    public MeshFilter Filter;
    public BoxCollider2D Collider2D;
    public RenderTexture RenderTexture;

    public void UpdateMesh()
    {
        Filter.mesh = Collider2D.CreateMesh(false, false);
        
        var points = new List<Vector3>();
        var mesh = Filter.mesh;
        mesh.GetVertices(points);
        
        points = points.OrderBy(v => v.x).ToList();
        var xMin = points[0].x;
        var xMax = points[points.Count - 1].x;
        
        points = points.OrderBy(v => v.y).ToList();
        var yMin = points[0].y;
        var yMax = points[points.Count - 1].y;

        var yRange = yMax - yMin;
        var xRange = xMax - xMin;

        var largestRange = yRange >= xRange ? yRange : xRange;

        Vector2 getRelavtivePos(Vector2 vec)
        {
            return new Vector2((vec.x - xMin) / largestRange, (vec.y - yMin) / largestRange);;
        }

        Debug.LogError(xRange);
        Debug.LogError(yRange);
        Debug.LogError(largestRange);
        
        Debug.LogError(getRelavtivePos(points.OrderBy(v => v.y).ToArray()[points.Count - 1]));
    }

    public void SetupMesh()
    {
        Vector2 renderTextureSize = new Vector2(RenderTexture.width, RenderTexture.height);
        var points = new List<Vector3>();

        var mesh = Filter.mesh;
        mesh.GetVertices(points);
        
        points = points.OrderBy(v => v.x).ToList();
        var xMin = points[0].x;
        var xMax = points[points.Count - 1].x;
        
        points = points.OrderBy(v => v.y).ToList();
        var yMin = points[0].y;
        var yMax = points[points.Count - 1].y;

        var yRange = yMax - yMin;
        var xRange = xMax - xMin;

        var largestRange = yRange >= xRange ? yRange : xRange;

        Vector2 getRelavtivePos(Vector2 vec)
        {
            return new Vector2((vec.x - xMin) / largestRange, (vec.y - yMin) / largestRange);;
        }

        Debug.LogError(xRange);
        Debug.LogError(yRange);
        Debug.LogError(largestRange);
        
        Debug.LogError(getRelavtivePos(points.OrderBy(v => v.y).ToArray()[points.Count - 1]));
        Debug.LogError(getRelavtivePos(points.OrderBy(v => v.x).ToArray()[points.Count - 1]));

        points.Clear();
        mesh.GetVertices(points);
        var uvs = points.Select(v => getRelavtivePos(v)).ToList();
        mesh.SetUVs(0, uvs);
    }


    public void CreateMesh()
    {
        Collider2D.CreateMesh(true, true);
    }
    
    
#if UNITY_EDITOR
    [ContextMenu("SaveMesh")]
    public void SaveMesh()
    { 
        UpdateMesh(); 
        UnityEditor.AssetDatabase.CreateAsset(Filter.mesh, "Assets/Map/Centrals/UpperCentral/UpperCentralMesh.asset");
    }
#endif
}

