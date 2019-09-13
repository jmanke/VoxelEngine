using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FixedVoxelObject 
{
    public sbyte[] isovalues;
    public Vector3[] vertices;
    public Vector3[] normals;
    public int[] triangles;
    public int[] numVert;
    public int[] numTri;

    public FixedVoxelObject(int blockSize)
    {
        int maxCount = blockSize * blockSize * blockSize * 5 * 3;
        int numIsovalues = (blockSize + 3) * (blockSize + 3) * (blockSize + 3);

        isovalues = new sbyte[numIsovalues];
        vertices = new Vector3[maxCount];
        normals = new Vector3[maxCount];
        triangles = new int[maxCount];
        numVert = new int[1] { 0 };
        numTri = new int[1] { 0 };
    }
}

internal unsafe struct FixedBlock
{
    public fixed sbyte isovalues[35 * 35 * 35];
    public fixed float vertices[32 * 32 * 32 * 5 * 3 * 3];
    public fixed float normals[32 * 32 * 32 * 5 * 3 * 3];
    public fixed int triangles[32 * 32 * 32 * 5 * 3 * 3];
    public fixed int numVert[1];
    public fixed int numTri[1];
}
