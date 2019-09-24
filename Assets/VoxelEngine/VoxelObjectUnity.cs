using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public class VoxelObjectUnity : MonoBehaviour
    {
        public VoxelObject voxelObject;

        /// <summary>
        /// Modifies values of the isovalues in a sphere around the origin with the given radius
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="radius"></param>
        public void UpdateIsovalues(Vector3Int origin, float radius, sbyte delta)
        {
            //voxelObject.ModifyVoxel(origin, delta);
            //voxelObject.FillVoxel(origin);
            voxelObject.DeleteVoxel(origin);
            //voxelObject.UpdateIsovalues(origin, radius, delta);
        }

        public void DeleteVoxel(Vector3Int origin)
        {
            voxelObject.DeleteVoxel(origin);
        }

        public void FillVoxel(Vector3Int origin, MineralType mineralType)
        {
            voxelObject.FillVoxel(origin, mineralType);
        }
    }
}
