using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public static class NoiseFilterFactory
    {
        public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
        {
            switch (settings.filterType)
            {
                case NoiseSettings.FilterType.Simple:
                    return new SimpleNoiseFilter(settings);
                case NoiseSettings.FilterType.Sphere:
                    return new SphereNoiseFilter(settings);
            }

            return null;
        }
    }
}
