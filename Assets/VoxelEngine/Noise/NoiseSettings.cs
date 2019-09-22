using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType
    {
        Simple,
        Sphere
    }

    public FilterType filterType;
    public string noiseName = "Default Noise";
    public bool invertValue = false;

    [Header("Sphere")]
    public Vector3 centre = Vector3.zero;
    public float radius = 50f;
    public float amplitude = 1f;

    [Header("Simple")]
    [Range(0f, 5f)]
    public float strength = 1f;
    public int seed = 1337;
    public float frequency = 0.01f;
    public FastNoise.Interp interp = FastNoise.Interp.Quintic;
    public FastNoise.NoiseType noiseType = FastNoise.NoiseType.Simplex;

    public int octaves = 3;
    public float lacunarity = 2.0f;
    public float gain = 0.5f;
    public FastNoise.FractalType fractalType = FastNoise.FractalType.FBM;

    public FastNoise.CellularDistanceFunction cellularDistanceFunction = FastNoise.CellularDistanceFunction.Euclidean;
    public FastNoise.CellularReturnType cellularReturnType = FastNoise.CellularReturnType.CellValue;
    public FastNoiseUnity cellularNoiseLookup = null;
    public int cellularDistanceIndex0 = 0;
    public int cellularDistanceIndex1 = 1;
    public float cellularJitter = 0.45f;

    public float gradientPerturbAmp = 1.0f;
}

