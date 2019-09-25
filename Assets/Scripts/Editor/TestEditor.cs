using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var test = (Test)target;

        if (GUILayout.Button("TestFun"))
        {
            test.TestFun();
        }
    }
}
