using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlacer : MonoBehaviour {

    public float spacing = .1f;
    public float resolution = 1;

    public void PlacePrimitive(Vector2 pos)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.position = pos;
        g.transform.localScale = Vector3.one * spacing * .5f;
    }
	
}
