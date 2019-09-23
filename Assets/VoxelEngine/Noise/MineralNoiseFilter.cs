using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    enum MineralType : byte
    {
        STONE = 0,
        COPPER = 1,
        IRON = 2
    }

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
            return noise.GetNoise(x, y, z);
        }
    }
}
