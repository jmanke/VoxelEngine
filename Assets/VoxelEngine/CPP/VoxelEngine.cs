using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Toast.Voxels
{
    public class VoxelEngine : MonoBehaviour
    {
        public Vector3Int dim;
        public int blockSize;
        public Material mat;

        /// <summary>
        /// Queue to update blocks
        /// </summary>
        private HashSet<Block> updateBlocks = new HashSet<Block>();

        /// <summary>
        /// queue for rendering meshes on a block
        /// </summary>
        //private ConcurrentQueue<System.Tuple<Block, VoxelMesh>> renderMesh = new ConcurrentQueue<System.Tuple<Block, VoxelMesh>>();

        private List<RenderGroup> renderGroups = new List<RenderGroup>();

        /// <summary>
        /// Queue for generating colliders on a block
        /// </summary>
        private Queue<Block> generateCollider = new Queue<Block>();

        private VoxelObject voxelObject;

        private class RenderGroup
        {
            public int numBlocks;
            public ConcurrentBag<System.Tuple<Block, VoxelMesh>> completedBlocks = new ConcurrentBag<System.Tuple<Block, VoxelMesh>>();
        }

        public void Start()
        {
            voxelObject = new VoxelObject(dim.x, dim.y, dim.z, blockSize, this);

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                voxelObject.GenerateIsovalues();

                foreach (var block in voxelObject.blocks)
                {
                    updateBlocks.Add(block);
                }
            });
        }

        /// <summary>
        /// Update the given block
        /// </summary>
        /// <param name="block"></param>
        public void UpdateBlock(Block block)
        {
            updateBlocks.Add(block);
        }

        private void QueueRenderMeshes()
        {
            var renderGroup = new RenderGroup();
            renderGroup.numBlocks = updateBlocks.Count;
            renderGroups.Add(renderGroup);

            if (updateBlocks.Count > 1)
            {
                var blocksToUpdate = new Block[updateBlocks.Count];
                int i = 0;

                foreach (var block in updateBlocks)
                {
                    blocksToUpdate[i] = block;
                    i++;
                }

                updateBlocks.Clear();

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var block in blocksToUpdate)
                    {
                        var voxelMesh = block.voxelObject.ComputeMesh(block);
                        renderGroup.completedBlocks.Add(new System.Tuple<Block, VoxelMesh>(block, voxelMesh));
                    }
                });
            }
            else
            {
                foreach (var block in updateBlocks)
                {
                    var voxelMesh = block.voxelObject.ComputeMesh(block);
                    renderGroup.completedBlocks.Add(new System.Tuple<Block, VoxelMesh>(block, voxelMesh));
                }

                updateBlocks.Clear();
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

                        if (block.go == null)
                        {
                            block.go = new GameObject($"{block.x}_{block.y}_{block.z}");
                            block.go.transform.SetParent(voxelObject.root);
                            block.go.transform.position = new Vector3(block.x, block.y, block.z) * block.size;

                            block.renderer = block.go.AddComponent<MeshRenderer>();
                            block.renderer.material = mat;

                            block.meshFilter = block.go.AddComponent<MeshFilter>();
                        }

                        var mesh = new Mesh();
                        mesh.vertices = voxelMesh.vertices;
                        mesh.triangles = voxelMesh.triangles;
                        mesh.normals = voxelMesh.normals;

                        block.meshFilter.sharedMesh = mesh;

                        generateCollider.Enqueue(block);
                    }

                    renderGroups.RemoveAt(i);
                }
            }

            //for (int i = 0; i < renderMesh.Count && i < 8; i++)
            //{
            //    if (renderMesh.TryDequeue(out var res))
            //    {
            //        var block = res.Item1;
            //        var voxelMesh = res.Item2;
            //        var voxelObject = block.voxelObject;

            //        if (block.go == null)
            //        {
            //            block.go = new GameObject($"{block.x}_{block.y}_{block.z}");
            //            block.go.transform.SetParent(voxelObject.root);
            //            block.go.transform.position = new Vector3(block.x, block.y, block.z) * block.size;

            //            block.renderer = block.go.AddComponent<MeshRenderer>();
            //            block.renderer.material = mat;

            //            block.meshFilter = block.go.AddComponent<MeshFilter>();
            //        }

            //        var mesh = new Mesh();
            //        mesh.vertices = voxelMesh.vertices;
            //        mesh.triangles = voxelMesh.triangles;
            //        mesh.normals = voxelMesh.normals;

            //        block.meshFilter.sharedMesh = mesh;

            //        generateCollider.Enqueue(block);
            //    }
            //}
        }

        private void GenerateColliders()
        {
            for (int i = 0; i < generateCollider.Count && i < 2; i++)
            {

                Debug.Log("Generate collider");
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
            QueueRenderMeshes();

            RenderMeshes();

            GenerateColliders();
        }
    }
}
