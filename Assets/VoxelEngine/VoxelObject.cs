﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Toast.Voxels
{
    public class VoxelObject
    {
        public readonly Transform root;

        public readonly Block[] blocks;

        public readonly int dimX;
        public readonly int dimY;
        public readonly int dimZ;
        public readonly int blockSize;

        private readonly int isoDimX;
        private readonly int isoDimY;
        private readonly int isoDimZ;
        private readonly sbyte[] isovalues;
        private readonly byte[] materialValues;
        private VoxelEngineWrapper voxelObjWrapper;
        private FastNoiseSIMD fastNoise;
        private VoxelEngine voxelEngine;
        private VoxelObjectSettings settings;
        private INoiseFilter[] noiseFilters;

        public VoxelObject(VoxelObjectSettings settings, VoxelEngine voxelEngine)
        {
            this.voxelObjWrapper = new VoxelEngineWrapper();
            this.settings = settings;

            dimX = settings.dimensions.x;
            dimY = settings.dimensions.y;
            dimZ = settings.dimensions.z;
            blockSize = settings.blockSize;

            isoDimX = dimX * blockSize;
            isoDimY = dimY * blockSize;
            isoDimZ = dimZ * blockSize;
            isovalues = new sbyte[isoDimX * isoDimY * isoDimZ];
            materialValues = new byte[isoDimX * isoDimY * isoDimZ];
            blocks = new Block[dimX * dimY * dimZ];

            this.voxelEngine = voxelEngine;

            noiseFilters = new INoiseFilter[settings.noiseLayers.Length];

            for (int i = 0; i < noiseFilters.Length; i++)
            {
                noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
            }

            root = new GameObject("Voxel Object").transform;
            root.position = Vector3.zero;
            root.rotation = Quaternion.identity;
            root.gameObject.AddComponent<VoxelObjectUnity>().voxelObject = this;

            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        blocks[x + y * dimX + z * dimY * dimY] = new Block(x, y, z, 0, blockSize, this);
                    }
                }
            }

            fastNoise = new FastNoiseSIMD();
            fastNoise.SetFrequency(0.02f);
        }

        public void Destroy()
        {
            Object.Destroy(root.gameObject);
        }

        /// <summary>
        /// Converts a float to Sbyte
        /// </summary>
        /// <param name="val">Must be between -1.0f and 1.0f</param>
        /// <returns></returns>
        public static sbyte FloatToSbyte(float val)
        {
            return (sbyte)(val * 128f);
        }

        static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        bool BlockAt(int x, int y, int z, out Block block)
        {
            if (x < 0 || x > dimX - 1 || y < 0 || y > dimY - 1 || z < 0 || z > dimZ - 1)
            {
                block = null;
                return false;
            }

            block = blocks[x + y * dimX + z * dimY * dimY];

            return true;
        }

        /// <summary>
        /// Updates an isovalue
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="value">Must be between -1.0 and 1.0</param>
        public void UpdateIsovalue(int x, int y, int z, sbyte value)
        {
            if (x < 0 || x > isoDimX - 1 || y < 0 || y > isoDimY - 1 || z < 0 || z > isoDimZ - 1)
                return;

            //local block coords
            int blockX = x / blockSize;
            int blockY = y / blockSize;
            int blockZ = z / blockSize;

            byte neighbourFlags = 0;

            if (blockX < 3) neighbourFlags += 1;
            else if (blockX > blockSize - 3) neighbourFlags += 8;
            if (blockY < 3) neighbourFlags += 2;
            else if (blockY > blockSize - 3) neighbourFlags += 16;
            if (blockZ < 3) neighbourFlags += 4;
            else if (blockZ > blockSize - 3) neighbourFlags += 32;

            int blockIndX = x / blockSize;
            int blockIndY = y / blockSize;
            int blockIndZ = z / blockSize;

            //Block neighbour;
            byte neighbourMask = 0;

            for (int i = blockIndX - 1; i < blockIndX + 2; i++)
            {
                for (int j = blockIndY - 1; j < blockIndY + 2; j++)
                {
                    for (int k = blockIndZ - 1; k < blockIndZ + 2; k++)
                    {
                        neighbourMask = 0;

                        if (i == blockIndX - 1) neighbourMask += 1;
                        if (j == blockIndY - 1) neighbourMask += 2;
                        if (k == blockIndZ - 1) neighbourMask += 4;

                        if (i == blockIndX + 1) neighbourMask += 8;
                        if (j == blockIndY + 1) neighbourMask += 16;
                        if (k == blockIndZ + 1) neighbourMask += 32;

                        if ((neighbourMask & neighbourFlags) == neighbourMask && BlockAt(i, j, k, out var neighbour))
                        {
                            voxelEngine.UpdateBlock(neighbour);
                        }
                    }
                }
            }

            isovalues[x + y * isoDimX + z * isoDimY * isoDimY] = value;

            if (BlockAt(blockIndX, blockIndY, blockIndZ, out var block))
            {
                voxelEngine.UpdateBlock(block);
            }
        }

        public sbyte IsovalueAt(int x, int y, int z)
        {
            return isovalues[x + y * isoDimX + z * isoDimY * isoDimY];
        }

        /// <summary>
        /// Updates isovalues in a sphere around the origin
        /// </summary>
        /// <param name="origin">Origin in world space</param>
        /// <param name="radius"></param>
        public void UpdateIsovalues(Vector3 origin, float radius, sbyte delta)
        {
            origin = root.worldToLocalMatrix * origin;

            for (int x = (int)(origin.x - radius); x < (int)(origin.x + radius); x++)
            {
                if (x < 0) continue;
                if (x > isoDimX - 1) break;

                for (int y = (int)(origin.y - radius); y < (int)(origin.y + radius); y++)
                {
                    if (y < 0) continue;
                    if (y > isoDimY - 1) break;

                    for (int z = (int)(origin.z - radius); z < (int)(origin.z + radius); z++)
                    {
                        if (z < 0) continue;
                        if (z > isoDimZ - 1) break;

                        //if (Vector3.Distance(origin, new Vector3(x, y, z)) < radius)
                        //{
                            int res = IsovalueAt(x, y, z) + delta;

                            if (res < -128)
                                res = -128;
                            else if (res > 127)
                                res = 127;

                            UpdateIsovalue(x, y, z, (sbyte)res);
                        //}
                    }
                }
            }
        }

        public float CalculateIsovalue(float x, float y, float z)
        {
            float[] computedValues = new float[noiseFilters.Length];
            float val = 0f;

            for (int i = 0; i < noiseFilters.Length; i++)
            {
                if (settings.noiseLayers[i].enabled)
                {
                    bool useLayerAsMask = settings.noiseLayers[i].useLayerAsMask;

                    if (useLayerAsMask)
                    {
                        int maskingLayer = settings.noiseLayers[i].maskingLayer;

                        if (settings.noiseLayers[maskingLayer].enabled)
                        {
                            if ((settings.noiseLayers[i].comparator == VoxelObjectSettings.NoiseLayer.Comparator.LessThan && computedValues[maskingLayer] < settings.noiseLayers[i].compareAgainst) ||
                                (settings.noiseLayers[i].comparator == VoxelObjectSettings.NoiseLayer.Comparator.GreaterThan && computedValues[maskingLayer] > settings.noiseLayers[i].compareAgainst))
                            {
                                continue;
                            }

                            if (settings.noiseLayers[i].overrideValue)
                                val = 0f;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    float computedVal = noiseFilters[i].Evaluate(x, y, z) * settings.noiseLayers[i].noiseSettings.strength * ((settings.noiseLayers[i].noiseSettings.invertValue) ? -1 : 1);
                    computedValues[i] = computedVal;

                    if (!settings.noiseLayers[i].ignoreValue)
                        val += computedVal;
                }
            }

            return val;
        }

        /// <summary>
        /// TODO: generate noise somewhere else
        /// </summary>
        public void GenerateIsovalues()
        {
            //var noiseFilter = new AsteroidANoiseProfile();
            //noiseFilter.centre = new Vector3(isoDimX, isoDimY, isoDimZ) / 2f;
            //noiseFilter.radius = 20f;
            //noiseFilter.amp = 0.5f;
            //noiseFilter.noise.frequency = 0.2f;
            //noiseFilter.noise.SaveSettings();

            for (int x = 0; x < isoDimX; x++)
            {
                for (int y = 0; y < isoDimY; y++)
                {
                    for (int z = 0; z < isoDimZ; z++)
                    {
                        int ind = x + y * isoDimX + z * isoDimY * isoDimY;
                        float isoval = CalculateIsovalue(x, y, z);//noiseFilter.Evaluate(x, y, z);
                        isovalues[ind] = (isoval < 0) ? (sbyte)-1 : (sbyte)1; /*FloatToSbyte(Mathf.Clamp(isoval, -0.9999999f, 0.9999999f));*/
                        materialValues[ind] = (Mathf.Abs(isoval) < 0.25f) ? (byte)0 : (byte)1;
                    }
                }
            }

            //float[] noiseSet = fastNoise.GetNoiseSet(0, 0, 0, isoDimX, isoDimY, isoDimZ);
            //int index = 0;

            //for (int x = 0; x < isoDimX; x++)
            //{
            //    for (int y = 0; y < isoDimY; y++)
            //    {
            //        for (int z = 0; z < isoDimZ; z++)
            //        {
            //            float isoval = noiseSet[index++];
            //            isovalues[x + y * isoDimX + z * isoDimY * isoDimY] =  (isoval < 0) ? (sbyte)-1 : (sbyte)1; //FloatToSbyte(isoval);
            //            materialValues[x + y * isoDimX + z * isoDimY * isoDimY] = (Mathf.Abs(isoval) < 0.25f) ? (byte)0 : (byte)1;
            //        }
            //    }
            //}
        }

        public void FillIsoValues(Block block, sbyte[] filledIsoValues, byte[] filledMaterialIndices)
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

                        int filledInd = x + y * isoSize + z * isoSize * isoSize;
                        int ind = isoX + isoY * isoDimX + isoZ * isoDimY * isoDimY;
                        filledIsoValues[filledInd] = isovalues[isoX + isoY * isoDimX + isoZ * isoDimY * isoDimY];
                        filledMaterialIndices[filledInd] = materialValues[ind];
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
            var materialIndicies = new byte[numIsovalues];
            var vertices = new Vector3[maxTriCount * 3];
            var triangles = new int[maxTriCount * 3];
            var vertexMaterialIndices = new Color32[maxTriCount * 3];
            var numVert = new int[1] { 0 };
            var numTri = new int[1] { 0 };

            FillIsoValues(block, isovalues, materialIndicies);

            voxelObjWrapper.ComputeMesh(isovalues,
                                        blockSize,
                                        0,
                                        vertices,
                                        numVert,
                                        triangles,
                                        numTri,
                                        materialIndicies,
                                        vertexMaterialIndices);

            System.Array.Resize(ref vertices, numVert[0]);
            System.Array.Resize(ref triangles, numTri[0]);
            System.Array.Resize(ref vertexMaterialIndices, numVert[0]);

            var voxelMesh = new VoxelMesh();
            voxelMesh.vertices = vertices;
            voxelMesh.triangles = triangles;
            voxelMesh.vertexMaterialIndices = vertexMaterialIndices;

            return voxelMesh;
        }
    }
}
