using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels.Old
{
    public struct Block
    {
        public int x;
        public int y;
        public int z;
        public int lod;
        public float spacing;
        public int size;
        public Cell[] cells;

        public Block(int x, int y, int z, int lod, int size)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.spacing = Mathf.Pow(2, lod);
            this.lod = lod;
            this.size = size;
            this.cells = new Cell[size * size * size];
        }

        public static Vector3 BlockGlobalCoord(Block block) {
		    return new Vector3(block.x, + block.y, + block.z) * block.size;
        }

        public static int CellInd(int x, int y, int z, int size)
        {
            return x + y * size + z * size * size;
        }
    }
}
