using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PathCreator : MonoBehaviour
{
    [HideInInspector] public Path Path;
    [SerializeField] public float AnchorDiameter = .15f;
    [SerializeField] public float ControlDiameter = .1f;
    [SerializeField] public float SholderMagnitude = 2f;
    [SerializeField] public Color SelectedSegmentCol;
    [SerializeField] public Color SegmentCol;
    [SerializeField] public Color AnchorCol;
    [SerializeField] public Color ControlPointCol;
    [SerializeField] public bool IsDisplayControlPoints;
    public void CreatePath()
    {
        Path = new Path(this); 
    }
    private void Reset()
    {
        CreatePath();
    }
    private void OnEnable()
    {
        if (Path == null)
        {
            CreatePath();
        }
        if (Path.Creator == null)
        {
            Path.Creator = this;
        }
    }
}
