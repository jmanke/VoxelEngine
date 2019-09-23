using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public class SimpleNoiseFilter : INoiseFilter
    {
        NoiseSettings settings;
        FastNoise noise;

        public SimpleNoiseFilter(NoiseSettings settings)
        {
            this.settings = settings;
            noise = new FastNoise();
            noise.ApplySettings(settings);
        }

        public float Evaluate(float x, float y, float z)
        {
            return noise.GetNoise(x, y, z);
        }
    }
}
