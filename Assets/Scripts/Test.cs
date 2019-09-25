using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void TestFun()
    {
        var verts = new Vector3[]
            {
            new Vector3(-0.5f, 0.5f, 0f),                   // 0
            new Vector3(0f,0.5f,0.5f),             //1
            new Vector3(0.5f,0.5f,0f),             //2
            new Vector3(0f,0.5f,-0.5f),             //3
            new Vector3(0f, 1f, 0f),  //4
            new Vector3(0f, 0f, 0f)//5
            };

        var tris = new int[]
        {
            0,1,4,
            1,2,4,
            3,4,2,
            0,4,3,

            0,5,1,
            1,5,2,
            3,2,5,
            0,3,5
        };

        var uvs = new Vector2[]
        {
            new Vector2(0, 0.5f),          // 0
            new Vector2(0.5f, 1f),         //1
            new Vector2(1f,0.5f),          //2
            new Vector2(0.5f,0),           //3
            new Vector2(0.5f, 0.5f),       //4
            new Vector2(0.5f, 0.5f)        //5
        };

        var mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        var go = new GameObject("Voxel");
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        mr.material = mat;
    }
}
