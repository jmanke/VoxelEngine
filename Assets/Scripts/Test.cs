using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    public float x = 0.1456f;
    public sbyte y = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnValidate()
    {
        y = (sbyte)(x*128f);
    }
}
