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

        private ConcurrentQueue<System.Tuple<Block, VoxelMesh>> meshToRender = new ConcurrentQueue<System.Tuple<Block, VoxelMesh>>();

        private VoxelObject voxelObject;

        public void Start()
        {
            voxelObject = new VoxelObject(dim.x, dim.y, dim.z, blockSize);
            voxelObject.GenerateIsovalues();

            System.Threading.Tasks.Parallel.For(0, voxelObject.dimX, x =>
            {
                for (int y = 0; y < voxelObject.dimY; y++)
                {
                    for (int z = 0; z < voxelObject.dimZ; z++)
                    {
                        var block = voxelObject.blocks[x + y * dim.x + z * dim.y * dim.y];
                        var voxelMesh = voxelObject.ComputeMesh(block);
                        meshToRender.Enqueue(new System.Tuple<Block, VoxelMesh>(block, voxelMesh));
                    }
                }
            });

            //var voxelObject = new VoxelObject(dim.x, dim.y, dim.z, blockSize);
            //voxelObject.GenerateIsovalues();

            //foreach (var block in voxelObject.blocks)
            //{
            //    var voxelMesh = voxelObject.ComputeMesh(block);
            //    meshToRender.Enqueue(new System.Tuple<Block, VoxelMesh>(block, voxelMesh));
            //}
        }

        public void OnDestroy()
        {
            if (voxelObject != null)
                voxelObject.Delete();
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < meshToRender.Count && i < 8; i++)
            {
                System.Tuple<Block, VoxelMesh> res;

                if (meshToRender.TryDequeue(out res))
                {
                    var block = res.Item1;
                    var voxelMesh = res.Item2;
                    var go = new GameObject($"{block.x}_{block.y}_{block.z}");
                    go.transform.SetParent(transform);
                    go.transform.position = new Vector3(block.x, block.y, block.z) * block.size;
                    var rend = go.AddComponent<MeshRenderer>();
                    rend.material = mat;
                    var mf = go.AddComponent<MeshFilter>();

                    var mesh = new Mesh();
                    mesh.vertices = voxelMesh.vertices;
                    mesh.triangles = voxelMesh.triangles;
                    mesh.normals = voxelMesh.normals;

                    mesh.RecalculateNormals();

                    mf.mesh = mesh;
                }
            }
        }
    }
}
