using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextureArrayWizard : ScriptableWizard
{
    public Texture2D[] textures;

    [MenuItem("Assets/Create/Texture Array")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<TextureArrayWizard>(
            "Create Texture Array", "Create"
        );
    }

    public static Texture2DArray CreateTexture2DArray(Texture2D[] textures)
    {
        var t = textures[0];
        var textureArray = new Texture2DArray(
            t.width, t.height, textures.Length, t.format, t.mipmapCount > 1
        )
        {
            anisoLevel = t.anisoLevel,
            filterMode = t.filterMode,
            wrapMode = t.wrapMode
        };

        for (int i = 0; i < textures.Length; i++)
        {
            for (int m = 0; m < t.mipmapCount; m++)
            {
                Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
            }
        }

        return textureArray;
    }

    void OnWizardCreate()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Texture Array", "Texture Array", "asset", "Save Texture Array"
        );

        if (path.Length == 0)
        {
            return;
        }

        var textureArray = CreateTexture2DArray(textures);

        AssetDatabase.CreateAsset(textureArray, path);
    }
}
