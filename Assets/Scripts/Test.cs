using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(1,0,0), 0.05f);
        Gizmos.DrawSphere(new Vector3(0,1,0), 0.05f);
        Gizmos.DrawSphere(Vector3.up, 0.05f);
    }
}
