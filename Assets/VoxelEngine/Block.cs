using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Toast.Voxels
{
    public class Block
    {
        public string id;
        public int x;
        public int y;
        public int z;
        public int lod;
        public int size;

        public VoxelObject voxelObject;

        public GameObject go;
        public MeshRenderer renderer;
        public MeshCollider collider;
        public MeshFilter meshFilter;

        public Block(int x, int y, int z, int lod, int size, VoxelObject voxelObject)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.lod = lod;
            this.size = size;
            this.id = GenerateId(x, y, z, lod);
            this.voxelObject = voxelObject;
        }

        public static string GenerateId(int x, int y, int z, int lod)
        {
            return $"{x}_{y}_{z}_{lod}";
        }
    }
}
