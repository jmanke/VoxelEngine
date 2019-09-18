using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FollowEditorCamera : MonoBehaviour
{
#if UNITY_EDITOR
    // Update is called once per frame
    void LateUpdate()
    {
        var cams = SceneView.GetAllSceneCameras();

        transform.position = cams[0].transform.position;
        transform.rotation = cams[0].transform.rotation;
    }
#endif
}
