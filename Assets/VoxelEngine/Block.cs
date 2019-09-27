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
        public int spacing;

        public VoxelObject voxelObject;

        public GameObject go;
        public MeshRenderer renderer;
        public MeshCollider collider;
        public MeshFilter meshFilter;

        /// <summary>
        /// True when this block has been rendered and initialized
        /// </summary>
        public bool isGenerated;

        /// <summary>
        /// Needs updating
        /// </summary>
        public bool isDirty;

        /// <summary>
        /// True when this block is being processed by the engine
        /// </summary>
        public bool isProcessing;

        public Block(int x, int y, int z, int lod, int size, VoxelObject voxelObject)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.lod = lod;
            this.size = size;
            this.id = GenerateId(x, y, z, lod);
            this.voxelObject = voxelObject;
            this.spacing = (int)Mathf.Pow(2, lod);
        }

        public static string GenerateId(int x, int y, int z, int lod)
        {
            return $"{x}_{y}_{z}_{lod}";
        }
    }
}
