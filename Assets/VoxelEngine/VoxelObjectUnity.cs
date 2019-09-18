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
        public void UpdateIsovalues(Vector3 origin, float radius, sbyte delta)
        {
            voxelObject.UpdateIsovalues(origin, radius, delta);
        }
    }
}
