using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Toast.Voxels;

public class VoxelBrush : MonoBehaviour
{
    public sbyte strength = 1;
    public float radius = 5f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, Camera.main.farClipPlane))
            {
                var voxelObject = hit.collider.GetComponentInParent<VoxelObjectUnity>();

                if (voxelObject)
                {
                    voxelObject.UpdateIsovalues(hit.point, radius, strength);
                }
            }
        }
    }
}
