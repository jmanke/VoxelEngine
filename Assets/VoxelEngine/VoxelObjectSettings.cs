using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    [CreateAssetMenu(menuName = "Noise/Voxel Object Settings")]
    public class VoxelObjectSettings : ScriptableObject
    {
        public int depth = 3;
        public int blockSize = 16;
        public Vector2 copperRange = new Vector2(0.4f, 0.7f);
        public Vector2 ironRange = new Vector2(-0.6f, -0.3f);
        public NoiseSettings mineralNoiseSettings;
        public NoiseLayer[] noiseLayers;

        [System.Serializable]
        public class NoiseLayer
        {
            public enum Comparator
            {
                LessThan,
                GreaterThan
            }

            public bool enabled = true;
            public bool useLayerAsMask = false;
            public int maskingLayer = 0;
            public Comparator comparator;
            public float compareAgainst = 0f;
            public bool overrideValue = false;
            /// <summary>
            /// Do not add this layer to the result. Used for layers that are purely for masking.
            /// </summary>
            public bool ignoreValue = false;
            public NoiseSettings noiseSettings;
        }
    }
}

