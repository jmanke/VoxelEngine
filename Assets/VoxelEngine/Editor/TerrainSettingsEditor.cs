using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Toast.Voxels
{
    [CustomEditor(typeof(TerrainSettings))]
    public class TerrainSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("textures"), true);
            var textures = serializedObject.FindProperty("textures");
            string[] terrainTypeNames = System.Enum.GetNames(typeof(TerrainType));

            for (int i = 0; i < textures.arraySize; i++)
            {
                EditorGUILayout.PropertyField(textures.GetArrayElementAtIndex(i), new GUIContent(terrainTypeNames[i]));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
