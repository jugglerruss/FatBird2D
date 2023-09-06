using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Sinewave : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    public float spacing = .1f;
    public float resolution = 1;
    private float _xStart;
    public void Draw()
    {
        Vector2[] points = FindObjectOfType<PathCreator>().Path.CalculateEvenlySpacedPoints(spacing, resolution);
        _lineRenderer.positionCount = points.Length;
        for (int currentPoint = 0; currentPoint < points.Length; currentPoint++)
        {
            _lineRenderer.SetPosition(currentPoint, new Vector3(points[currentPoint].x,points[currentPoint].y,0));
        }
    }
}
