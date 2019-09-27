using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public enum NodeIndex
    {
        /// 3 bits
        X = 1,
        Y = 2,
        Z = 4
    }

    public enum Direction
    {
        Right = 0,
        Left = 1,
        Top = 2,
        Bottom = 3,
        Front = 4,
        Back = 5
    }

    public class Octree
    {
        private static readonly Vector3[] VectorDirection = new Vector3[]
        {
            Vector3.right,
            -Vector3.right,
            Vector3.up,
            -Vector3.up,
            Vector3.forward,
            -Vector3.forward
        };

        public readonly int depth = 4;
        public readonly int size = 16;

        private Block root;
        private List<Block>[] blockLod;

        public Octree(int depth, int size, VoxelObject voxelObject)
        {
            root = new Block(0, 0, 0, depth-1, size, voxelObject);
            blockLod = new List<Block>[depth];

            for (int i = 0; i < blockLod.Length; i++)
            {
                blockLod[i] = new List<Block>();
            }

            blockLod[depth - 1].Add(root);

            BuildTreeRecursive(root);
        }

        public Block Search(Vector3 position, int lod)
        {
            return Search(root, position, lod);
        }

        public List<Block> BlocksAtLod(int lod)
        {
            return blockLod[lod]; 
        }

        public Block GetNeighbour(Block block, Direction direction)
        {
            var blockPos = new Vector3(block.x, block.y, block.z);
            var neightbourPos = (blockPos + VectorDirection[(int)direction]) * block.spacing;

            return Search(neightbourPos, block.lod);
        }

        private void BuildTreeRecursive(Block block)
        {
            if (block.lod < 1)
                return;

            block.children = new Block[8];
            int halfSize = (int)Mathf.Pow(2, block.lod) / 2;

            block.children[0] = new Block(block.x, block.y, block.z, block.lod - 1, size, block.voxelObject);
            block.children[1] = new Block(block.x + halfSize, block.y, block.z, block.lod - 1, size, block.voxelObject);
            block.children[2] = new Block(block.x, block.y + halfSize, block.z, block.lod - 1, size, block.voxelObject);
            block.children[3] = new Block(block.x + halfSize, block.y + halfSize, block.z, block.lod - 1, size, block.voxelObject);
            block.children[4] = new Block(block.x, block.y, block.z + halfSize, block.lod - 1, size, block.voxelObject);
            block.children[5] = new Block(block.x + halfSize, block.y, block.z + halfSize, block.lod - 1, size, block.voxelObject);
            block.children[6] = new Block(block.x, block.y + halfSize, block.z + halfSize, block.lod - 1, size, block.voxelObject);
            block.children[7] = new Block(block.x + halfSize, block.y + halfSize, block.z + halfSize, block.lod - 1, size, block.voxelObject);

            foreach (var child in block.children)
            {
                Debug.Log($"blocks[{child.lod}]");
                blockLod[child.lod].Add(child);
                child.parent = block;
                BuildTreeRecursive(child);
            }
        }

        private Block Search(Block block, Vector3 position, int lod)
        {
            if (block.lod == lod)
            {
                return block;
            }

            return Search(block.Search(position), position, lod);
        }
    }
}

