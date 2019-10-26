using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vectorf
{
    // Round a 2D vector
    [HideInInspector] public static Vector2 Round(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x),Mathf.Round(v.y));
    }

    // Round a 3D vector
    [HideInInspector] public static Vector3 Round(Vector3 v)
    {
        return new Vector3(Mathf.Round(v.x),Mathf.Round(v.y), Mathf.Round(v.z));
    }

}
