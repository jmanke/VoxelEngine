using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public interface INoiseFilter
    {
        float Evaluate(float x, float y, float z);
    }
}

