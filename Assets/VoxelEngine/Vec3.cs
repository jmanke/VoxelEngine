using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels.Old
{
    public readonly struct Vec3
    {
        public readonly float x;
        public readonly float y;
        public readonly float z;

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        // Vec3

        public static Vec3 operator +(in Vec3 vec3_1, in Vec3 vec3_2)
        {
            return new Vec3(vec3_1.x + vec3_2.x, vec3_1.y + vec3_2.y, vec3_1.z + vec3_2.z);
        }

        public static Vec3 operator -(in Vec3 vec3_1, in Vec3 vec3_2)
        {
            return new Vec3(vec3_1.x - vec3_2.x, vec3_1.y - vec3_2.y, vec3_1.z - vec3_2.z);
        }

        public static Vec3 operator *(in Vec3 vec3_1, in Vec3 vec3_2)
        {
            return new Vec3(vec3_1.x * vec3_2.x, vec3_1.y * vec3_2.y, vec3_1.z * vec3_2.z);
        }

        public static Vec3 operator /(in Vec3 vec3_1, in Vec3 vec3_2)
        {
            return new Vec3(vec3_1.x / vec3_2.x, vec3_1.y / vec3_2.y, vec3_1.z / vec3_2.z);
        }

        // floats

        public static Vec3 operator +(in Vec3 vec3_1, in float a)
        {
            return new Vec3(vec3_1.x + a, vec3_1.y + a, vec3_1.z + a);
        }

        public static Vec3 operator -(in Vec3 vec3_1, in float a)
        {
            return new Vec3(vec3_1.x - a, vec3_1.y - a, vec3_1.z - a);
        }

        public static Vec3 operator *(in Vec3 vec3_1, in float a)
        {
            return new Vec3(vec3_1.x * a, vec3_1.y * a, vec3_1.z * a);
        }

        public static Vec3 operator /(in Vec3 vec3_1, in float a)
        {
            return new Vec3(vec3_1.x / a, vec3_1.y / a, vec3_1.z / a);
        }

        // ints

        public static Vec3 operator +(in Vec3 vec3_1, in int a)
        {
            return new Vec3(vec3_1.x + a, vec3_1.y + a, vec3_1.z + a);
        }

        public static Vec3 operator -(in Vec3 vec3_1, in int a)
        {
            return new Vec3(vec3_1.x - a, vec3_1.y - a, vec3_1.z - a);
        }

        public static Vec3 operator *(in Vec3 vec3_1, in int a)
        {
            return new Vec3(vec3_1.x * a, vec3_1.y * a, vec3_1.z * a);
        }

        public static Vec3 operator /(in Vec3 vec3_1, in int a)
        {
            return new Vec3(vec3_1.x / a, vec3_1.y / a, vec3_1.z / a);
        }

        // long
        public static Vec3 operator +(in Vec3 vec3_1, in long a)
        {
            return new Vec3(vec3_1.x + a, vec3_1.y + a, vec3_1.z + a);
        }

        public static Vec3 operator -(in Vec3 vec3_1, in long a)
        {
            return new Vec3(vec3_1.x - a, vec3_1.y - a, vec3_1.z - a);
        }

        public static Vec3 operator *(in Vec3 vec3_1, in long a)
        {
            return new Vec3(vec3_1.x * a, vec3_1.y * a, vec3_1.z * a);
        }

        public static Vec3 operator /(in Vec3 vec3_1, in long a)
        {
            return new Vec3(vec3_1.x / a, vec3_1.y / a, vec3_1.z / a);
        }
    }
}
