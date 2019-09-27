using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Toast.Voxels;

public class VoxelBrush : MonoBehaviour
{
    public enum QueryMode
    {
        Removal,
        Placement
    }

    public QueryMode mode = QueryMode.Removal;
    public TerrainType mineralType = TerrainType.COPPER;

    public sbyte strength = 1;
    public float radius = 5f;
    public Transform cube;
    public Transform sphere;
    public float rate = 0.2f;

    private float timer;
    private Collider cubeCol;
    private LayerMask terrainMask;

    private void Start()
    {
        cubeCol = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<Collider>();
        cubeCol.transform.SetParent(transform);
        cubeCol.GetComponent<MeshRenderer>().enabled = false;
        cubeCol.enabled = false;
        terrainMask = LayerMask.GetMask("Terrain");
    }

    private Vector3Int GetRemovalPoint(Vector3Int point, RaycastHit hit, VoxelObjectUnity voxelObject)
    {
        if (voxelObject && !voxelObject.voxelObject.VoxelFilled(point))
        {
            var norm = hit.normal;

            if (norm == Vector3.up || norm == -Vector3.up)
            {
                point.y -= (int)norm.y;
            }
            else
            {
                float closestDist = float.MaxValue;

                for (int x = point.x - 1; x < point.x + 2; x++)
                {
                    for (int z = point.z - 1; z < point.z + 2; z++)
                    {
                        if (voxelObject.voxelObject.VoxelFilled(new Vector3Int(x, point.y, z)))
                        {
                            var voxelPoint = voxelObject.voxelObject.blockRoots[0].InverseTransformDirection(new Vector3(x, point.y, z));
                            var dist = Vector3.Distance(hit.point, voxelPoint);

                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                point.x = x;
                                point.z = z;
                            }
                        }
                    }
                }
            }
        }

        return point;
    }

    private Vector3Int GetPlacementPoint(Vector3Int point, VoxelObjectUnity voxelObject)
    {
        if (voxelObject && voxelObject.voxelObject.VoxelFilled(point))
        {
            cubeCol.transform.position = point;
            cubeCol.enabled = true;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // hits the cube now
            if (Physics.Raycast(ray, out var hit, Camera.main.farClipPlane, terrainMask))
            {
                var norm = hit.normal;

                point += new Vector3Int((int)norm.x, (int)norm.y, (int)norm.z);
            }

            cubeCol.enabled = false;
        }

        return point;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (mode == QueryMode.Placement)
                mode = QueryMode.Removal;
            else
                mode = QueryMode.Placement;
        }

        timer -= Time.deltaTime;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, Camera.main.farClipPlane, terrainMask))
        {
            var hitPoint = hit.point;
            sphere.position = hitPoint;

            if (hitPoint.x < -0.5f || hitPoint.x > 0.5f)
                hitPoint.x += (hitPoint.x > 0) ? 0.5f : -0.5f;
            if (hitPoint.y < -0.5f || hitPoint.y > 0.5f)
                hitPoint.y += (hitPoint.y > 0) ? 0.5f : -0.5f;
            if (hitPoint.z < -0.5f || hitPoint.z > 0.5f)
                hitPoint.z += (hitPoint.z > 0) ? 0.5f : -0.5f;

            var point = new Vector3Int((int)hitPoint.x, (int)hitPoint.y, (int)hitPoint.z);
            var voxelObject = hit.collider.GetComponentInParent<VoxelObjectUnity>();

            if (mode == QueryMode.Removal)
            {
                point = GetRemovalPoint(point, hit, voxelObject);
            }
            else if (mode == QueryMode.Placement)
            {
                point = GetPlacementPoint(point, voxelObject);
            }

            if (Input.GetMouseButton(0) && timer < 0f && voxelObject)
            {
                timer = rate;
                
                if (mode == QueryMode.Removal)
                {
                    voxelObject.DeleteVoxel(point);
                } 
                else if (mode == QueryMode.Placement)
                {
                    voxelObject.FillVoxel(point, mineralType);
                }
            }

            cube.position = point;
        }
    }
}
