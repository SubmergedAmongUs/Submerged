using System;
using UnityEngine;

public class ElevatorRendererSetup : MonoBehaviour
{
    public Sprite Sprite;

    public string Path;
    [ContextMenu("Setup")]
    public void Setup()
    {
        #if UNITY_EDITOR
        
        UnityEditor.AssetDatabase.CreateAsset(SpriteToMesh(Sprite), Path);
        
        #endif
    }
    
    Mesh SpriteToMesh(Sprite sprite)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Array.ConvertAll(sprite.vertices, i => (Vector3)i);
        mesh.uv = sprite.uv;
        mesh.triangles = Array.ConvertAll(sprite.triangles, i => (int)i);
 
        return mesh;
    }
}
