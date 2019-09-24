using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        var mesh = new Mesh();

        Vector3[] verts = new Vector3[]
        {
            Vector3.zero,
            Vector3.one,
            Vector3.up
        };

        int[] triangles = new int[]
        {
            0,
            1,
            2
        };

        Color32[] colors = new Color32[]
        {
            new Color32(0, 0, 0, 0),
            new Color32(0, 0, 0, 0),
            new Color32(1, 0, 0, 0)
        };

        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.colors32 = colors;

        mesh.RecalculateNormals();

        var go = new GameObject();
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        mr.sharedMaterial = mat;
    }
}
