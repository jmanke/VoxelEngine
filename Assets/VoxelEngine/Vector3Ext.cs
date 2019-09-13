using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels.Old
{
    public static class Vector3Ext
    {
        public static Vector3 Vec3ToVector3(this Vector3 vector3, Vec3 vec3)
        {
            return new Vector3(vec3.x, vec3.y, vec3.z);
        }
    }
}
