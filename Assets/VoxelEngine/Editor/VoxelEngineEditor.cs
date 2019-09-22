using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Toast.Voxels
{
    [CustomEditor(typeof(VoxelEngine))]
    public class VoxelEngineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var voxelEngine = (VoxelEngine)target;

            if (GUILayout.Button("Regenerate"))
            {
                voxelEngine.Regenerate();
            }
        }
    }
}
