using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public class SphereNoiseFilter : INoiseFilter
    {
        NoiseSettings settings;
        FastNoise noise;

        public SphereNoiseFilter(NoiseSettings settings)
        {
            this.settings = settings;
            noise = new FastNoise();
            noise.ApplySettings(settings);
        }

        public float Evaluate(float x, float y, float z)
        {
            float dist = Vector3.Distance(settings.centre, new Vector3(x, y, z));
            float val = noise.GetNoise(x, y, z);
            return Mathf.Clamp(settings.radius - dist + val * settings.amplitude, -1f, 1f);
        }
    }
}
