using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Toast.Voxels
{
    public class VoxelEngineWrapper
    {
        const string NATIVE_LIB = "VoxelEngine";

        [DllImport(NATIVE_LIB)]
        public static extern System.IntPtr new_VoxelObject();

        [DllImport(NATIVE_LIB)]
        public static extern void delete_TransVoxelImpl(System.IntPtr instance);

        private readonly System.IntPtr instance;

        public VoxelEngineWrapper()
        {
            this.instance = new_VoxelObject();
        }

        ~VoxelEngineWrapper()
        {
            delete_TransVoxelImpl(instance);
        }
    }
}

