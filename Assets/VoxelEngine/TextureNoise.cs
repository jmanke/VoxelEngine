using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureNoise : MonoBehaviour
{
    public Texture3D GetNoise(int size)
    {
        Color[] colorArray = new Color[size * size * size];
        var texture = new Texture3D(size, size, size, TextureFormat.RGBA32, true);
        float r = 1.0f / (size - 1.0f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    var c = new Color(x * r, y * r, z * r, 1.0f);
                    colorArray[x + (y * size) + (z * size * size)] = c;
                }
            }
        }
        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }
}
