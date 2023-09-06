using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator _creator;
    private Path _path => _creator.Path;
    
    const float segmentSelectDistanceThreshold = .1f;
    int selectedSegmentIndex = -1;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create new"))
        {
            Undo.RecordObject(_creator, "Create new");
            _creator.CreatePath();
        }
        bool autoSetControlPoints = GUILayout.Toggle(_path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != _path.AutoSetControlPoints)
        {
            Undo.RecordObject(_creator, "Toggle auto set controls");
            _path.AutoSetControlPoints = autoSetControlPoints;
        }
        _path.SholderMagnitude = _creator.SholderMagnitude;
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }
    private void OnSceneGUI()
    {
        Input();
        Draw();
    }
    private void OnEnable()
    {
        _creator = (PathCreator)target;
        if (_creator.Path == null)
        {
            _creator.CreatePath();
        }
        if (_creator.Path.ParentTransformSF == null)
            _creator.Path.ParentTransformSF = _creator.transform;
    }
    private void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
        
        if (guiEvent.type == EventType.KeyDown && guiEvent.keyCode == KeyCode.E)
        {
            _path.SetStartEnd();
        }
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if (selectedSegmentIndex != -1)
            {
                Undo.RecordObject(_creator, "Split segment");
                _path.SplitSegment(mousePos, selectedSegmentIndex);
            }
            else
            {
                Undo.RecordObject(_creator, "Add Segment");
                _path.AddSegment(mousePos);
            }
            return;
        }
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDstToAnchor = _creator.AnchorDiameter;
            int closestAnchorIndex = -1;

            for (int i = 0; i < _path.NumPoints; i+=3)
            {
                float dst = Vector2.Distance(mousePos, _path[i]);
                if (dst < minDstToAnchor)
                {
                    minDstToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(_creator, "Delete segment");
                _path.DeleteSegment(closestAnchorIndex);
            }
            return;
        }
        
        
        if (guiEvent.type == EventType.MouseMove)
        {
            float minDstToSegment = segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < _path.NumSegments; i++)
            {
                Vector2[] points = _path.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dst < minDstToSegment)
                {
                    minDstToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }
            if (newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
            return;
        }
        if (guiEvent.type == EventType.MouseDrag && selectedSegmentIndex == -1)
        {
            _path.SetCenter();
        }
    }
    private void Draw()
    {
        for (int i = 0; i < _path.NumSegments; i++)
        {
            Vector2[] points = _path.GetPointsInSegment(i);
            if (_creator.IsDisplayControlPoints)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]); 
            }
            Color segmentCol = (i == selectedSegmentIndex && Event.current.shift) ? _creator.SelectedSegmentCol : _creator.SegmentCol;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentCol, null, 2);
        }
        for (int i = 0; i < _path.NumPoints; i++)
        {
            if (i % 3 != 0 && !_creator.IsDisplayControlPoints) continue;
            Handles.color = i % 3 == 0 ? _creator.AnchorCol : _creator.ControlPointCol;
            float handleSize = i % 3 == 0 ? _creator.AnchorDiameter : _creator.ControlDiameter;
            Vector2 newPos = Handles.FreeMoveHandle(_path[i], Quaternion.identity, handleSize, Vector3.zero, Handles.CylinderHandleCap);
            if (_path[i] == newPos) continue;
            Undo.RecordObject(_creator, "Move Point");
            _path.MovePoint(i, newPos);
        }
    }
}
