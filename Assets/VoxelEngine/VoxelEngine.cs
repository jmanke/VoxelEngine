using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Toast.Voxels
{
    public class VoxelEngine : MonoBehaviour
    {
        public Material mat;
        public VoxelObjectSettings voxelObjectSettings;
        public int currLod = 0;
        // must be in same order as the texture2D array for the shader!
        // TODO: create this from the material itself
        public TerrainSettings terrainSettings;
        public VoxelItem voxelItem;
        public float rotationSpeed = 2f;

        private List<VoxelObject> voxelObjects = new List<VoxelObject>();

        /// <summary>
        /// Queue to update blocks
        /// </summary>
        private Queue<Block> blocksToUpdate = new Queue<Block>();

        /// <summary>
        /// queue for rendering meshes on a block
        /// </summary>
        //private ConcurrentQueue<System.Tuple<Block, VoxelMesh>> renderMesh = new ConcurrentQueue<System.Tuple<Block, VoxelMesh>>();
        private List<RenderGroup> renderGroups = new List<RenderGroup>();

        /// <summary>
        /// Queue for generating colliders on a block
        /// </summary>
        private Queue<Block> generateCollider = new Queue<Block>();

        private class RenderGroup
        {
            public int numBlocks;
            public ConcurrentBag<System.Tuple<Block, VoxelMesh>> completedBlocks = new ConcurrentBag<System.Tuple<Block, VoxelMesh>>();
        }

        public void Start()
        {
            var textureArray = TextureArrayWizard.CreateTexture2DArray(terrainSettings.textures.ToArray());
            mat.SetTexture("_TexArr", textureArray);

            for (int i = 0; i < 1; i++)
            {
                GenerateVoxelObject(voxelObjectSettings, new Vector3(i * 150, 0f, 0f));
            }
        }

        public void Regenerate()
        {
            foreach (var voxelObject in voxelObjects)
            {
                voxelObject.SetLod(currLod);
            }
        }

        public void Delete()
        {
            foreach (var vo in voxelObjects)
            {
                vo.Destroy();
            }

            voxelObjects.Clear();
        }

        public void GenerateVoxelObject(VoxelObjectSettings voxelObjectSettings, Vector3 position)
        {
            var voxelObject = new VoxelObject(voxelObjectSettings, this);
            voxelObject.root.position = position;
            voxelObjects.Add(voxelObject);

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                voxelObject.GenerateIsovalues();
                MainThread.ExecuteOnMainThread(() => voxelObject.SetLod(currLod));
            });
        }

        /// <summary>
        /// Update the given block
        /// </summary>
        /// <param name="block"></param>
        public void UpdateBlock(Block block)
        {
            lock (blocksToUpdate)
            {
                block.isProcessing = true;
                blocksToUpdate.Enqueue(block);
            }
        }

        private void QueueRenderMeshes()
        {
            lock (blocksToUpdate)
            {
                var renderGroup = new RenderGroup();
                renderGroup.numBlocks = blocksToUpdate.Count;
                renderGroups.Add(renderGroup);

                if (blocksToUpdate.Count > 1)
                {
                    var blocksToUpdate = new Block[this.blocksToUpdate.Count];
                    int i = 0;

                    foreach (var block in this.blocksToUpdate)
                    {
                        blocksToUpdate[i] = block;
                        i++;
                    }

                    this.blocksToUpdate.Clear();

                    System.Threading.ThreadPool.QueueUserWorkItem(o =>
                    {
                        foreach (var block in blocksToUpdate)
                        {
                            block.isDirty = false;
                            var voxelMesh = block.voxelObject.ComputeMesh(block);
                            renderGroup.completedBlocks.Add(new System.Tuple<Block, VoxelMesh>(block, voxelMesh));
                        }
                    });
                }
                else
                {
                    foreach (var block in blocksToUpdate)
                    {
                        block.isDirty = false;
                        var voxelMesh = block.voxelObject.ComputeMesh(block);
                        renderGroup.completedBlocks.Add(new System.Tuple<Block, VoxelMesh>(block, voxelMesh));
                    }

                    blocksToUpdate.Clear();
                }
            }
        }

        private void RenderMeshes()
        {
            for (int i = renderGroups.Count - 1; i >= 0; i--)
            {
                var renderGroup = renderGroups[i];
                // none are incomplete, render the group
                if (renderGroup.completedBlocks.Count == renderGroup.numBlocks)
                {
                    foreach (var res in renderGroup.completedBlocks)
                    {
                        var block = res.Item1;
                        var voxelMesh = res.Item2;
                        var voxelObject = block.voxelObject;

                        if (!block.isGenerated)
                        {
                            block.go = new GameObject($"{block.x}_{block.y}_{block.z}");
                            block.go.layer = LayerMask.NameToLayer("Terrain");
                            block.go.transform.SetParent(voxelObject.blockRoots[block.lod]);
                            block.go.transform.localPosition = new Vector3(block.x, block.y, block.z) * block.size * block.spacing;
                            block.go.transform.localRotation = Quaternion.identity;

                            block.renderer = block.go.AddComponent<MeshRenderer>();
                            block.renderer.material = mat;

                            block.meshFilter = block.go.AddComponent<MeshFilter>();
                            block.isGenerated = true;
                        }

                        var mesh = new Mesh();
                        mesh.vertices = voxelMesh.vertices;
                        mesh.triangles = voxelMesh.triangles;
                        mesh.colors32 = voxelMesh.vertexMaterialIndices;

                        block.meshFilter.sharedMesh = mesh;
                        block.renderer.enabled = false;
                        block.renderer.enabled = true;
                        block.isProcessing = false;

                        generateCollider.Enqueue(block);
                    }

                    renderGroups.RemoveAt(i);
                }
            }
        }

        private void GenerateColliders()
        {
            for (int i = 0; i < generateCollider.Count && i < 2; i++)
            {
                //Debug.Log("Generate collider");
                var block = generateCollider.Dequeue();

                if (block.collider == null)
                {
                    block.collider = block.go.AddComponent<MeshCollider>();
                    block.collider.sharedMesh = block.meshFilter.sharedMesh;
                }
                else
                {
                    block.collider.sharedMesh = block.meshFilter.sharedMesh;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var voxelObject in voxelObjects)
            {
                voxelObject.Update();
            }

            QueueRenderMeshes();

            RenderMeshes();

            GenerateColliders();
        }
    }
}
