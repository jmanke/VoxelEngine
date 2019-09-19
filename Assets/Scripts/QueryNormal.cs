using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueryNormal : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, Camera.main.farClipPlane))
            {
                Debug.Log(hit.normal.ToString());
            }
        }
    }
}
