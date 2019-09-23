using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoxelBrush))]
public class EditorInfo : Editor
{
    void OnSceneGUI()
    {
        if (!EditorApplication.isPlaying)
            return;

        var cam = Camera.main;
        var ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, cam.farClipPlane))
        {
            Handles.color = Color.cyan;
            Handles.Label(hit.point, hit.point.ToString());
        }
    }
}
