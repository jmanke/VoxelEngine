using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public struct Block
    {
        public int x;
        public int y;
        public int z;
        public int lod;
        public int size;

        public Block(int x, int y, int z, int lod, int size)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.lod = lod;
            this.size = size;
        }
    }
}
