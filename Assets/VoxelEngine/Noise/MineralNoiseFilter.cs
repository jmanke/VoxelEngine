using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public class MineralNoiseFilter : INoiseFilter
    {
        NoiseSettings settings;
        FastNoise noise;

        public MineralNoiseFilter(NoiseSettings settings)
        {
            this.settings = settings;
            noise = new FastNoise();
            noise.ApplySettings(settings);
        }

        public float Evaluate(float x, float y, float z)
        {
            return noise.GetSimplex(x, y, z);
        }
    }
}
