using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Toast.Voxels
{
    public class VoxelObject
    {
        public readonly Block[] blocks;

        public readonly int dimX;
        public readonly int dimY;
        public readonly int dimZ;

        public readonly int blockSize;

        private readonly int isoDimX;
        private readonly int isoDimY;
        private readonly int isoDimZ;

        private readonly sbyte[] isovalues;
        private VoxelEngineWrapper voxelObjWrapper;
        private FastNoiseSIMD fastNoise;

        public Transform root;

        public VoxelObject(int dimX, int dimY, int dimZ, int blockSize)
        {
            this.voxelObjWrapper = new VoxelEngineWrapper();

            this.dimX = dimX;
            this.dimY = dimY;
            this.dimZ = dimZ;
            this.blockSize = blockSize;

            this.isoDimX = dimX * blockSize;
            this.isoDimY = dimY * blockSize;
            this.isoDimZ = dimZ * blockSize;

            this.isovalues = new sbyte[isoDimX * isoDimY * isoDimZ];
            this.blocks = new Block[dimX * dimY * dimZ];

            root = new GameObject("Voxel Object").transform;
            root.position = Vector3.zero;
            root.rotation = Quaternion.identity;

            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        blocks[x + y * dimX + z * dimY * dimY] = new Block(x, y, z, 0, blockSize);
                    }
                }
            }

            fastNoise = new FastNoiseSIMD();
            fastNoise.SetFrequency(0.02f);
        }

        /// <summary>
        /// TODO: generate noise somewhere else
        /// </summary>
        public void GenerateIsovalues()
        {
            float[] noiseSet = fastNoise.GetNoiseSet(0, 0, 0, isoDimX, isoDimY, isoDimZ);
            int index = 0;

            for (int x = 0; x < isoDimX; x++)
            {
                for (int y = 0; y < isoDimY; y++)
                {
                    for (int z = 0; z < isoDimZ; z++)
                    {
                        isovalues[x + y * isoDimX + z * isoDimY * isoDimY] = (sbyte)(noiseSet[index++] * 128f); // (noiseSet[index++] < 0) ? (sbyte)-1 : (sbyte)1;
                    }
                }
            }

            Debug.Log("Filled");
        }

        public void FillIsoValues(Block block, sbyte[] filledIsoValues)
        {
            int isoSize = block.size + 3;

            for (int x = 0, isoX = block.x * blockSize; x < isoSize; x++, isoX++)
            {
                for (int y = 0, isoY = block.y * blockSize; y < isoSize; y++, isoY++)
                {
                    for (int z = 0, isoZ = block.z * blockSize; z < isoSize; z++, isoZ++)
                    {
                        if (isoX < 0)
                            isoX = 0;
                        else if (isoX > isoDimX - 1)
                            isoX = isoDimX - 1;
                        if (isoY < 0)
                            isoY = 0;
                        else if (isoY > isoDimY - 1)
                            isoY = isoDimY - 1;
                        if (isoZ < 0)
                            isoZ = 0;
                        else if (isoZ > isoDimZ - 1)
                            isoZ = isoDimZ - 1;

                        filledIsoValues[x + y * isoSize + z * isoSize * isoSize] = isovalues[isoX + isoY * isoDimX + isoZ * isoDimY * isoDimY];
                    }
                }
            }
        }

        public VoxelMesh ComputeMesh(Block block)
        {
            int numVoxels = blockSize * blockSize * blockSize;
            int maxTriCount = numVoxels * 5;
            int numIsovalues = (blockSize + 3) * (blockSize + 3) * (blockSize + 3);

            var isovalues = new sbyte[numIsovalues];
            var vertices = new Vector3[maxTriCount * 3];
            var normals = new Vector3[maxTriCount * 3];
            var triangles = new int[maxTriCount * 3];
            var numVert = new int[1] { 0 };
            var numTri = new int[1] { 0 };

            FillIsoValues(block, isovalues);

            voxelObjWrapper.ComputeMesh(isovalues,
                                        blockSize,
                                        0,
                                        vertices,
                                        numVert,
                                        triangles,
                                        numTri,
                                        normals);

            System.Array.Resize(ref vertices, numVert[0]);
            System.Array.Resize(ref normals, numVert[0]);
            System.Array.Resize(ref triangles, numTri[0]);

            var voxelMesh = new VoxelMesh();
            voxelMesh.vertices = vertices;
            voxelMesh.triangles = triangles;
            voxelMesh.normals = normals;

            return voxelMesh;
        }
    }
}
