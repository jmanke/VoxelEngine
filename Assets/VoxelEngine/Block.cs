using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Toast.Voxels
{
    public class Block
    {
        public string id;
        public readonly int x;
        public readonly int y;
        public readonly int z;
        public int lod;
        public int size;
        public int spacing;
        public Block parent;
        public Block[] children;
        public GameObject go;
        public MeshRenderer renderer;
        public MeshCollider collider;
        public MeshFilter meshFilter;
        public VoxelObject voxelObject;

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

        public Block() { }

        public Block(int x, int y, int z, int lod, int size, VoxelObject voxelObject)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.lod = lod;
            this.size = size;
            this.id = GenerateId(x, y, z, lod);
            this.spacing = (int)Mathf.Pow(2, lod);
            this.voxelObject = voxelObject;
        }

        public static string GenerateId(int x, int y, int z, int lod)
        {
            return $"{x}_{y}_{z}_{lod}";
        }

        public Block Search(Vector3 position)
        {
            float halfLength = AxisLength / 2f;
            byte index = 0;

            if (position.x >= x * size + halfLength) index |= 1;
            if (position.y >= y * size + halfLength) index |= 2;
            if (position.z >= z * size + halfLength) index |= 4;

            return children[index];
        }

        public Vector3 Centre
        {
            get
            {
                float halfLength = AxisLength / 2f;
                return new Vector3(x * size + halfLength, y * size + halfLength, z * size + halfLength);
            }
        }

        public float AxisLength
        {
            get
            {
                return Mathf.Pow(2, lod) * size;
            }
        }
    }
}
