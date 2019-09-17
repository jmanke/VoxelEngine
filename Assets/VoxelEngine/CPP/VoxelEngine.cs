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

        private ConcurrentQueue<System.Tuple<Block, VoxelMesh, VoxelObject>> renderMesh = new ConcurrentQueue<System.Tuple<Block, VoxelMesh, VoxelObject>>();

        private Queue<Block> generateCollider = new Queue<Block>();

        private VoxelObject voxelObject;

        public void Start()
        {
            voxelObject = new VoxelObject(dim.x, dim.y, dim.z, blockSize);

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                voxelObject.GenerateIsovalues();

                System.Threading.Tasks.Parallel.For(0, voxelObject.dimX, x =>
                {
                    for (int y = 0; y < voxelObject.dimY; y++)
                    {
                        for (int z = 0; z < voxelObject.dimZ; z++)
                        {
                            var block = voxelObject.blocks[x + y * dim.x + z * dim.y * dim.y];
                            var voxelMesh = voxelObject.ComputeMesh(block);
                            renderMesh.Enqueue(new System.Tuple<Block, VoxelMesh, VoxelObject>(block, voxelMesh, voxelObject));
                        }
                    }
                });
            });
        }

        private void RenderMeshes()
        {
            for (int i = 0; i < renderMesh.Count && i < 8; i++)
            {
                System.Tuple<Block, VoxelMesh, VoxelObject> res;

                if (renderMesh.TryDequeue(out res))
                {
                    var block = res.Item1;
                    var voxelMesh = res.Item2;
                    var voxelObject = res.Item3;

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
            }
        }

        private void GenerateColliders()
        {
            for (int i = 0; i < generateCollider.Count && i < 1; i++)
            {
                var block = generateCollider.Dequeue();

                if (block.collider == null)
                    block.collider = block.go.AddComponent<MeshCollider>();
                else
                    block.collider.sharedMesh = block.meshFilter.sharedMesh;
            }
        }

        // Update is called once per frame
        void Update()
        {
            RenderMeshes();

            GenerateColliders();
        }
    }
}
