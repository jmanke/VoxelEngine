using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Toast.Voxels
{
    public class VoxelEngine : MonoBehaviour
    {
        public Vector3Int dimensions = new Vector3Int(4, 4, 4);
        public int size = 32;
        public Material mat;

        private ConcurrentQueue<System.Tuple<Block, VoxelMesh>> meshToRender = new ConcurrentQueue<System.Tuple<Block, VoxelMesh>>();

        // Start is called before the first frame update
        void Start()
        {
            var voxelObject = new VoxelObject(dimensions.x, dimensions.y, dimensions.z, size);

            voxelObject.GenerateIsovalues();

            System.Threading.ThreadPool.QueueUserWorkItem((o) =>
            {
                foreach (var block in voxelObject.blocks)
                {
                    var voxelmesh = voxelObject.GenerateMesh(block);
                    meshToRender.Enqueue(new System.Tuple<Block, VoxelMesh>(block, voxelmesh));
                }
            });
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < meshToRender.Count && i < 3; i++)
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
