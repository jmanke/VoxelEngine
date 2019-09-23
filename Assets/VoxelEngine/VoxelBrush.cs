using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Toast.Voxels;

public class VoxelBrush : MonoBehaviour
{
    public sbyte strength = 1;
    public float radius = 5f;
    public Transform cubeCol;
    public Transform cube;
    public Transform sphere;

    public float rate = 0.2f;
    private float timer;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, Camera.main.farClipPlane))
        {
            sphere.position = hit.point;
            var hitPoint = hit.point;
            if (hitPoint.x < -0.5f || hitPoint.x > 0.5f)
                hitPoint.x += (hitPoint.x > 0) ? 0.5f : -0.5f;
            if (hitPoint.y < -0.5f || hitPoint.y > 0.5f)
                hitPoint.y += (hitPoint.y > 0) ? 0.5f : -0.5f;
            if (hitPoint.z < -0.5f || hitPoint.z > 0.5f)
                hitPoint.z += (hitPoint.z > 0) ? 0.5f : -0.5f;

            var point = new Vector3Int((int)hitPoint.x, (int)hitPoint.y, (int)hitPoint.z);
            var voxelObject = hit.collider.GetComponentInParent<VoxelObjectUnity>();

            if (voxelObject && voxelObject.voxelObject.VoxelFilled(point))
            {
                point.y += 1;
            }

            //if (voxelObject && !voxelObject.voxelObject.VoxelFilled(point))
            //{
            //    var norm = hit.normal;

            //    if (norm == Vector3.up || norm == -Vector3.up)
            //    {
            //        point.y -= (int)norm.y;
            //    }
            //    else
            //    {
            //        float closestDist = float.MaxValue;

            //        for (int x = point.x - 1; x < point.x + 2; x++)
            //        {
            //            for (int z = point.z - 1; z < point.z + 2; z++)
            //            {
            //                if (voxelObject.voxelObject.VoxelFilled(new Vector3Int( x, point.y, z)))
            //                {
            //                    var voxelPoint = voxelObject.voxelObject.blockRoot.InverseTransformDirection(new Vector3(x, point.y, z));
            //                    var dist = Vector3.Distance(hitPoint, voxelPoint);

            //                    if (dist < closestDist)
            //                    {
            //                        closestDist = dist;
            //                        point.x = x;
            //                        point.z = z;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //Debug.Log($"{hit.point.ToString()} : {point.ToString()}");

            if (Input.GetMouseButton(0) && timer < 0f && voxelObject)
            {
                timer = rate;

                voxelObject.UpdateIsovalues(point, radius, strength);
                cube.position = point;
                
            }

            cube.position = point;
        }
    }
}
