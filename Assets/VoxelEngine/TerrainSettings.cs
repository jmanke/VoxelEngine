using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toast.Voxels
{
    public enum TerrainType : byte
    {
        STONE = 0,
        COPPER = 1,
        IRON = 2,

        EMPTY = 255
    }

    [CreateAssetMenu(fileName = "TerrainTextures", menuName = "Voxels/TerrainTextures", order = 1)]
    public class TerrainSettings : ScriptableObject
    {
        public List<Texture2D> textures;

        public Texture2D GetTexture(TerrainType terrainType)
        {
            return textures[(byte)terrainType];
        }
    }
}
