using UnityEngine;

namespace Toast.Voxels
{
    public class VoxelObject
    {
        public readonly Transform root;
        public readonly Transform[] blockRoots;
        public readonly Block[][] blocks;
        public readonly int depth;
        public readonly int blockSize;

        private readonly int isoDim;
        private readonly sbyte[] isovalues;
        private readonly byte[] materialValues;
        private VoxelEngineWrapper voxelObjWrapper;
        private VoxelEngine voxelEngine;
        private VoxelObjectSettings settings;
        private INoiseFilter[] noiseFilters;
        private MineralNoiseFilter mineralNoiseFilter;
        private int currLod = 0;
        private int currBlockDim;

        public VoxelObject(VoxelObjectSettings settings, VoxelEngine voxelEngine)
        {
            this.voxelObjWrapper = new VoxelEngineWrapper();
            this.settings = settings;

            depth = settings.depth;
            blockSize = settings.blockSize;
            isoDim = (int)Mathf.Pow(2, depth) * blockSize;
            isovalues = new sbyte[isoDim * isoDim * isoDim];
            materialValues = new byte[isoDim * isoDim * isoDim];
            blocks = new Block[depth][];
            blockRoots = new Transform[depth];

            for (int i = 0; i < depth; i++)
            {
                int dim = (int)Mathf.Pow(2, depth - i);
                blocks[i] = new Block[dim * dim * dim];

                for (int x = 0; x < dim; x++)
                {
                    for (int y = 0; y < dim; y++)
                    {
                        for (int z = 0; z < dim; z++)
                        {
                            blocks[i][x + y * dim + z * dim * dim] = new Block(x, y, z, i, blockSize, this);
                        }
                    }
                }
            }

            this.voxelEngine = voxelEngine;

            mineralNoiseFilter = new MineralNoiseFilter(settings.mineralNoiseSettings);
            noiseFilters = new INoiseFilter[settings.noiseLayers.Length];

            for (int i = 0; i < noiseFilters.Length; i++)
            {
                noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
            }

            root = new GameObject("Voxel Centre").transform;
            root.position = Vector3.zero;
            root.position = root.TransformPoint(new Vector3(isoDim, isoDim, isoDim) / 2f);
            root.rotation = Quaternion.identity;

            for (int i = 0; i < depth; i++)
            {
                var blockRoot = new GameObject($"lod_{i}").transform;
                blockRoot.position = Vector3.zero;
                blockRoot.rotation = Quaternion.identity;
                blockRoot.gameObject.AddComponent<VoxelObjectUnity>().voxelObject = this;
                blockRoot.SetParent(root);
                blockRoots[i] = blockRoot;
                blockRoot.gameObject.SetActive(false);
            }
        }

        public void SetLod(int lod)
        {
            blockRoots[currLod].gameObject.SetActive(false);
            currLod = lod;
            currBlockDim = (int)Mathf.Pow(2, depth - lod);
            blockRoots[lod].gameObject.SetActive(true);

            for (int i = 0; i < blocks[currLod].Length; i++)
            {
                if (!blocks[currLod][i].isGenerated)
                    blocks[currLod][i].isDirty = true;
            }
        }

        public void Destroy()
        {
            Object.Destroy(root.gameObject);
            voxelObjWrapper.Delete();
        }

        public Vector3 Centre
        {
            get
            {
                return root.position;
            }
        }

        static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        bool BlockAt(int x, int y, int z, out Block block)
        {
            if (x < 0 || x > currBlockDim - 1 || y < 0 || y > currBlockDim - 1 || z < 0 || z > currBlockDim - 1)
            {
                block = null;
                return false;
            }

            block = blocks[currLod][x + y * currBlockDim + z * currBlockDim * currBlockDim];

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
            if (x < 0 || x > isoDim - 1 || y < 0 || y > isoDim - 1 || z < 0 || z > isoDim - 1)
                return;

            //local block coords
            int blockX = x % blockSize;
            int blockY = y % blockSize;
            int blockZ = z % blockSize;

            byte neighbourFlags = 0;

            if (blockX < 1) neighbourFlags += 1;
            else if (blockX > blockSize - 1) neighbourFlags += 8;
            if (blockY < 1) neighbourFlags += 2;
            else if (blockY > blockSize - 1) neighbourFlags += 16;
            if (blockZ < 1) neighbourFlags += 4;
            else if (blockZ > blockSize - 1) neighbourFlags += 32;

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
                            neighbour.isDirty = true;
                            //voxelEngine.UpdateBlock(neighbour);
                        }
                    }
                }
            }

            isovalues[x + y * isoDim + z * isoDim * isoDim] = value;

            if (BlockAt(blockIndX, blockIndY, blockIndZ, out var block))
            {
                block.isDirty = true;
                //voxelEngine.UpdateBlock(block);
            }
        }

        public sbyte IsovalueAt(int x, int y, int z)
        {
            return isovalues[x + y * isoDim + z * isoDim * isoDim];
        }

        /// <summary>
        /// Safely gets an isovalue without worrying about index out of range exceptions. Returns false if no isovalue exists.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="isovalue"></param>
        /// <returns></returns>
        public bool SafeIsovalueAt(int x, int y, int z, out sbyte isovalue)
        {
            if (x < 0 || x > isoDim - 1 || y < 0 || y > isoDim - 1 || z < 0 || z > isoDim - 1)
            {
                isovalue = -1;
                return false;
            }

            isovalue = IsovalueAt(x, y, z);

            return true;
        }

        public bool VoxelFilled(int x, int y, int z)
        {
            if (SafeIsovalueAt(x, y, z, out sbyte isovalue))
            {
                return isovalue == 1;
            }

            return true;
        }

        public bool VoxelFilled(Vector3Int worldPos)
        {
            var origin = blockRoots[currLod].worldToLocalMatrix.MultiplyPoint(worldPos);
            return VoxelFilled((int)origin.x, (int)origin.y, (int)origin.z);
        }

        private int CoordToIsoInd(int x, int y, int z)
        {
            return x + y * isoDim + z * isoDim * isoDim;
        }

        private Vector3Int GetClosestFilledVoxel(int x, int y, int z)
        {
            float closestDist = float.MaxValue;
            var origin = new Vector3(x, y, z);
            var closestVoxelInd = new Vector3Int();

            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    for (int k = z - 1; k < z + 2; k++)
                    {
                        if (VoxelFilled(i, j, k))
                        {
                            var dist = Vector3.Distance(origin, new Vector3(i, j, k));
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closestVoxelInd = new Vector3Int(i, j, k);
                            }
                        }
                    }
                }
            }

            return closestVoxelInd;
        }

        public void FillVoxel(Vector3 worldPos, TerrainType terrainType)
        {
            // TODO: refactor this, very messy. Only for testing purposes currently
            var origin = blockRoots[currLod].worldToLocalMatrix.MultiplyPoint(worldPos);
            int x = (int)origin.x;
            int y = (int)origin.y;
            int z = (int)origin.z;
            //var intOrigin = new Vector3Int(x, y, z);

            UpdateIsovalue(x, y, z, 1);
            byte matInd = (byte)terrainType;
            materialValues[CoordToIsoInd(x, y, z)] = matInd;
        }

        public void DeleteVoxel(Vector3 worldPos)
        {
            var origin = blockRoots[currLod].worldToLocalMatrix.MultiplyPoint(worldPos);
            int xPos = (int)origin.x;
            int yPos = (int)origin.y;
            int zPos = (int)origin.z;
            int ind = CoordToIsoInd(xPos, yPos, zPos);

            if (VoxelFilled(xPos, yPos, zPos))
            {
                var voxelDrop = GameObject.Instantiate(voxelEngine.voxelItem.gameObject);
                voxelDrop.transform.position = worldPos;
                voxelDrop.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 4);
                voxelDrop.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", voxelEngine.terrainSettings.textures[materialValues[ind]]);
            }

            UpdateIsovalue(xPos, yPos, zPos, -1);
            materialValues[ind] = (byte)TerrainType.EMPTY;
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

        private byte CalculateBlockType(float x, float y, float z)
        {
            float noiseVal = mineralNoiseFilter.Evaluate(x, y, z);
            byte blockType;

            if (noiseVal >= settings.copperRange.x && noiseVal < settings.copperRange.y)
            {
                blockType = (byte)TerrainType.COPPER;
            }
            else if (noiseVal >= settings.ironRange.x && noiseVal < settings.ironRange.y)
            {
                blockType = (byte)TerrainType.IRON;
            }
            else
            {
                blockType = (byte)TerrainType.STONE;
            }

            return blockType;
        }

        /// <summary>
        /// TODO: generate noise somewhere else
        /// </summary>
        public void GenerateIsovalues()
        {
            int stone = 0;
            int copper = 0;
            int iron = 0;
            int empty = 0;

            for (int x = 0; x < isoDim; x++)
            {
                for (int y = 0; y < isoDim; y++)
                {
                    for (int z = 0; z < isoDim; z++)
                    {
                        int ind = x + y * isoDim + z * isoDim * isoDim;
                        float isoval = CalculateIsovalue(x, y, z);//noiseFilter.Evaluate(x, y, z);
                        sbyte isobyte = (isoval < 0) ? (sbyte)-1 : (sbyte)1;
                        isovalues[ind] = isobyte; 

                        materialValues[ind] = (isobyte == -1) ? (byte)TerrainType.EMPTY : CalculateBlockType(x, y, z); //(mineralNoiseFilter.Evaluate(x, y, z) < 0.7f) ? (byte)0 : (byte)1;
                        if (materialValues[ind] == (byte)TerrainType.EMPTY) empty++;
                        if (materialValues[ind] == (byte)TerrainType.STONE) stone++;
                        if (materialValues[ind] == (byte)TerrainType.COPPER) copper++;
                        if (materialValues[ind] == (byte)TerrainType.IRON) iron++;
                    }
                }
            }

            Debug.Log($"Iron = {iron}, Copper = {copper}");
        }

        public void FillIsoValues(Block block, sbyte[] filledIsoValues, byte[] filledMaterialIndices)
        {
            int isoSize = block.size + 1;
            int spacing = (int)Mathf.Pow(2, block.lod);

            for (int x = 0, isoX = block.x * blockSize * spacing; x < isoSize; x++, isoX+= spacing)
            {
                for (int y = 0, isoY = block.y * blockSize * spacing; y < isoSize; y++, isoY+= spacing)
                {
                    for (int z = 0, isoZ = block.z * blockSize * spacing; z < isoSize; z++, isoZ+= spacing)
                    {
                        if (isoX < 0)
                            isoX = 0;
                        else if (isoX > isoDim - 1)
                            isoX = isoDim - 1;
                        if (isoY < 0)
                            isoY = 0;
                        else if (isoY > isoDim - 1)
                            isoY = isoDim - 1;
                        if (isoZ < 0)
                            isoZ = 0;
                        else if (isoZ > isoDim - 1)
                            isoZ = isoDim - 1;

                        int filledInd = x + y * isoSize + z * isoSize * isoSize;
                        int ind = isoX + isoY * isoDim + isoZ * isoDim * isoDim;
                        filledIsoValues[filledInd] = isovalues[ind];
                        filledMaterialIndices[filledInd] = materialValues[ind];
                    }
                }
            }
        }

        public VoxelMesh ComputeMesh(Block block)
        {
            int numVoxels = blockSize * blockSize * blockSize;
            int maxTriCount = numVoxels * 5;
            int numIsovalues = (blockSize + 1) * (blockSize + 1) * (blockSize + 1);

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
                                        block.lod,
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

        public void Update()
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < blocks[i].Length; j++)
                {
                    var block = blocks[i][j];

                    if (block.isDirty && !block.isProcessing)
                    {
                        voxelEngine.UpdateBlock(block);
                    }
                }
            }
        }
    }
}
