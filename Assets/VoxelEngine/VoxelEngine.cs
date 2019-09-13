using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Toast.Voxels.Old
{
    public class VoxelEngine : MonoBehaviour
    {
        public Vector3Int dimensions = new Vector3Int(4, 4, 4);
        public int size = 32;
        public Material mat;

        private ConcurrentQueue<System.Tuple<Block, VoxelMesh>> meshToRender = new ConcurrentQueue<System.Tuple<Block, VoxelMesh>>();

        private sbyte[] isovalues;

        // Start is called before the first frame update
        void Start()
        {
            //var timer = new System.Diagnostics.Stopwatch();
            //timer.Start();

            ComputeMesh();
            //isovalues = VoxelObject.GetIsoValues(dimensions.x * size, dimensions.y * size, dimensions.z * size);
            //var test = new sbyte[35 * 35 * 35];

            //int ind;

            //for (int x = 0; x < 35; x++)
            //{
            //    for (int y = 0; y < 35; y++)
            //    {
            //        for (int z = 0; z < 35; z++)
            //        {
            //            ind = x + y * 35 + z * 35 * 35;
            //            test[ind] = isovalues[ind];
            //        }
            //    }
            //}

            //timer.Stop();
            ////Debug.Log(timer.ElapsedMilliseconds + ", " + test.Length);

            //var voxelObject = new VoxelObject(dimensions.x, dimensions.y, dimensions.z, size);

            //timer.Reset();
            //timer.Start();

            //var block = voxelObject.blocks[0];
            //var voxelmesh = voxelObject.GenerateMesh(block);
            //meshToRender.Enqueue(new System.Tuple<Block, VoxelMesh>(block, voxelmesh));

            //timer.Stop();
            //Debug.Log("Took " + timer.ElapsedMilliseconds);

            //ComputeMesh();

            //System.Threading.ThreadPool.QueueUserWorkItem((o) =>
            //{
            //    var voxelObject = new VoxelObject(dimensions.x, dimensions.y, dimensions.z, size);
            //    voxelObject.GenerateIsovalues();

            //    System.Threading.Tasks.Parallel.For(0, voxelObject.dimX, x =>
            //    {
            //        for (int y = 0; y < voxelObject.dimY; y++)
            //        {
            //            for (int z = 0; z < voxelObject.dimZ; z++)
            //            {
            //                var block = voxelObject.blocks[voxelObject.BlockCoordToIndex(x, y, z)];
            //                var voxelmesh = voxelObject.GenerateMesh(block);
            //                meshToRender.Enqueue(new System.Tuple<Block, VoxelMesh>(block, voxelmesh));
            //            }
            //        }
            //    });
            //});
        }

        private unsafe void ComputeMesh()
        {
            var timer = new System.Diagnostics.Stopwatch();
            var voxelObjectCpp = new VoxelEngineWrapper();
            var fixedVoxelObject = new FixedVoxelObject(size);

            fixedVoxelObject.isovalues = VoxelObject.GetIsoValues(35, 35, 35);

            var fixedObjHandle = GCHandle.Alloc(fixedVoxelObject);

            timer.Start();

            //voxelObjectCpp.ComputeMesh(fixedVoxelObject.isovalues,
            //                                                size,
            //                                                0,
            //                                                fixedVoxelObject.vertices,
            //                                                fixedVoxelObject.numVert,
            //                                                fixedVoxelObject.triangles,
            //                                                fixedVoxelObject.numTri,
            //                                                fixedVoxelObject.normals);

            timer.Stop();

            System.Array.Resize(ref fixedVoxelObject.vertices, fixedVoxelObject.numVert[0]);
            System.Array.Resize(ref fixedVoxelObject.normals, fixedVoxelObject.numVert[0]);
            System.Array.Resize(ref fixedVoxelObject.triangles, fixedVoxelObject.numTri[0]);

            Debug.Log($"Took {timer.ElapsedMilliseconds}");

            var mesh = new Mesh();
            mesh.vertices = fixedVoxelObject.vertices;
            mesh.triangles = fixedVoxelObject.triangles;
            mesh.normals = fixedVoxelObject.normals;

            var go = new GameObject($"test");
            go.transform.SetParent(transform);
            go.transform.position = Vector3.zero;
            var rend = go.AddComponent<MeshRenderer>();
            rend.material = mat;
            var mf = go.AddComponent<MeshFilter>();

            mf.mesh = mesh;

            fixedObjHandle.Free();
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < meshToRender.Count && i < 1; i++)
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

                    mf.mesh = mesh;
                }
            }
        }
    }
}
