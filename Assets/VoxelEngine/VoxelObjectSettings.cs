﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    [CreateAssetMenu(menuName = "Noise/Voxel Object Settings")]
    public class VoxelObjectSettings : ScriptableObject
    {
        public Vector3Int dimensions = new Vector3Int(1,1,1);
        public int blockSize = 16;
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
