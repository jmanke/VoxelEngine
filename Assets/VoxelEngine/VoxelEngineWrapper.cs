﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Toast.Voxels
{
    public unsafe class VoxelEngineWrapper
    {
        const string NATIVE_LIB = "VoxelEngine";

        [DllImport(NATIVE_LIB, CallingConvention = CallingConvention.StdCall)]
        public static extern System.IntPtr new_VoxelObject();

        [DllImport(NATIVE_LIB, CallingConvention = CallingConvention.StdCall)]
        public static extern void delete_VoxelObject(System.IntPtr instance);

        [DllImport(NATIVE_LIB, CallingConvention = CallingConvention.StdCall)]
        public static extern void ComputeMesh(System.IntPtr instance, sbyte[] isovalues, int blockSize, int lod, Vector3[] vertices, int[] vertexCount, int[] triangles, int[] triangleCount, Vector3[] normals);

        private readonly System.IntPtr instance;

        public VoxelEngineWrapper()
        {
            this.instance = new_VoxelObject();
        }

        public void ComputeMesh(sbyte[] isovalues, int blockSize, int lod, Vector3[] vertices, int[] vertexCount, int[] triangles, int[] triangleCount, Vector3[] normals)
        {
            ComputeMesh(instance, isovalues, blockSize, lod, vertices, vertexCount, triangles, triangleCount, normals);
        }

        ~VoxelEngineWrapper()
        {
            delete_VoxelObject(instance);
        }
    }
}

