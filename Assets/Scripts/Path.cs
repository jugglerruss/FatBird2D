using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Path : ICloneable
{
    [SerializeField, HideInInspector] private List<Vector2> PointsSF;
    [SerializeField, HideInInspector] private bool AutoSetControlPointsSF;
    public Vector2 this[int i] => PointsSF[i];
    public int NumPoints => PointsSF.Count;
    public List<Vector2> Points => PointsSF;
    public Vector2 Centre => _centre;
    public int NumSegments => PointsSF.Count / 3;
    [FormerlySerializedAs("ParentTransform")] public Transform ParentTransformSF;
    public PathCreator Creator { get; set; }
    private Vector2 _prevPosOnCurve;
    private Vector2 _prevCasOnCurve;
    private Vector2 _centre;
    public Vector2[] EvenlySpacedPoints { get; private set; }
    private int _lastIndex; 
    private float _sholderMagnitude; 
    public bool AutoSetControlPoints
    {
        get => AutoSetControlPointsSF;
        set
        {
            if (AutoSetControlPointsSF == value)
                return;
            AutoSetControlPointsSF = value;
            if (AutoSetControlPointsSF)
            {
                AutoSetAllControls();
            }
        }
    }
    public float SholderMagnitude
    {
        get => _sholderMagnitude;
        set
        {
            if (_sholderMagnitude == value)
                return;
            _sholderMagnitude = value;
            SetAllSholders();
        }
    }

    public Path(PathCreator creator)
    {
        Creator = creator;
        _centre = creator.transform.position;
        PointsSF = new List<Vector2>()
        {
            _centre + Vector2.left,
            _centre + (Vector2.left + Vector2.up) * .5f,
            _centre + (Vector2.right + Vector2.down) * .5f,
            _centre + Vector2.right
        };
    }
    public Path(Transform parent, List<Vector2> points)
    {
        _centre = parent.position;
        PointsSF = points;
    }
    public void SetStartEnd()
    {
        PointsSF[NumPoints-1] = new Vector2(PointsSF[NumPoints-1].x, PointsSF[0].y);
        Debug.Log("SetStartEnd");
    }
    public void SetCenter()
    {
        if(_centre == (Vector2)ParentTransformSF.position) return;
        for (int i = 0; i < NumPoints; i++)
        {
            PointsSF[i] = PointsSF[i] + (Vector2)ParentTransformSF.position - _centre;
        }
        _centre = ParentTransformSF.position;
        Debug.Log(_centre);
    }
    public void SetPosition(Vector2 pos)
    {
        Vector2 firstPos = PointsSF[0];
        for (int i = 0; i < NumPoints; i++)
        {
            PointsSF[i] = PointsSF[i] - firstPos + pos;
        }
        _centre = -firstPos + pos;
    }
    public List<Vector2> GetPathFromPoint(Vector2 pos)
    {
        Vector2 firstPos = PointsSF[0];
        List<Vector2> newList = PointsSF;
        for (int i = 0; i < NumPoints; i++)
        {
            newList[i] = newList[i] - firstPos + pos;
        }
        return newList;
    }
    public void MoveSegmentEvenlySpacedPoints(Vector2 start, Vector2 end)
    {
        var segment = EvenlySpacedPoints.Where(p => p.x > start.x && p.x < end.x).ToList();
        if(segment.Count == 0) Debug.Log("segment.Count == 0");
        
        Vector2 firstPos = segment[0];
        for (int i = 0; i < segment.Count; i++)
        {
            segment[i] = segment[i] - firstPos + EvenlySpacedPoints[^1];
        }
        var tempList = EvenlySpacedPoints.ToList();
        tempList.AddRange(segment);
        EvenlySpacedPoints = tempList.ToArray();
    }
    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments < 3) return;
        if (anchorIndex == 0)
            PointsSF.RemoveRange(0, 3);
        else if (anchorIndex == NumPoints - 1)
            PointsSF.RemoveRange(anchorIndex - 2, 3);
        else
            PointsSF.RemoveRange(anchorIndex - 1, 3);
    }
    public void AddSegment(Vector2 anchorPos)
    {
        if (PointsSF.Any(varPoint => varPoint == anchorPos)) return;
        PointsSF.Add(PointsSF[NumPoints - 1] * 2 - PointsSF[NumPoints - 2]);
        PointsSF.Add((PointsSF[NumPoints - 1] + anchorPos) * .5f);
        PointsSF.Add(anchorPos);
        if (AutoSetControlPointsSF)
            AutoSetAllAffectedControlPoints(NumPoints - 1);
    }
    
    public void SplitSegment(Vector2 anchorPos, int segmentIndex)
    {
        PointsSF.InsertRange(segmentIndex * 3 + 2, new[] { Vector2.zero, anchorPos, Vector2.zero });
        if (AutoSetControlPointsSF)
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
        else
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
    }
    public Vector2[] GetPointsInSegment(int i)=> new[] { PointsSF[i * 3], PointsSF[i * 3 + 1], PointsSF[i * 3 + 2], PointsSF[i * 3 + 3] };
    
    private void SetAllSholders()
    {
        List<int> correspondingIndexes = new List<int>();
        for (int i = 0; i < NumPoints; i++)
        {
            if (i % 3 == 0 || correspondingIndexes.Contains(i)) continue;
            bool nextPointIsAnchor = (i + 1) % 3 == 0;
            int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
            correspondingIndexes.Add(i);
            correspondingIndexes.Add(correspondingControlIndex);
            int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;
            Vector2 anchorDeltaPoint = PointsSF[anchorIndex] - PointsSF[i];
            Vector2 anchorDelta = GetAnchorDelta(anchorDeltaPoint);
            if(anchorDeltaPoint.x * anchorDelta.x < 0) continue;
            if (correspondingControlIndex >= 0 && correspondingControlIndex < NumPoints)
                PointsSF[correspondingControlIndex] = PointsSF[anchorIndex] + anchorDelta;
            PointsSF[i] = PointsSF[anchorIndex] - anchorDelta;
        }
    }

    private Vector2 GetAnchorDelta(Vector2 anchorDelta)
    {
        var yToX = anchorDelta.y / anchorDelta.x;
        if (anchorDelta.x > 0)
        {
            if (yToX >= 1)
                anchorDelta = (Vector2.up + Vector2.right).normalized * SholderMagnitude;
            else if (yToX <= -1)
                anchorDelta = (Vector2.down + Vector2.right).normalized * SholderMagnitude;
            else
                anchorDelta = Vector2.right * SholderMagnitude;
        }
        else
        {
            if (yToX >= 1)
                anchorDelta = (Vector2.down + Vector2.left).normalized * SholderMagnitude;
            else if (yToX <= -1)
                anchorDelta = (Vector2.up + Vector2.left).normalized * SholderMagnitude;
            else
                anchorDelta = Vector2.left * SholderMagnitude;
        }
        return anchorDelta;
    }

    public void MovePoint(int i, Vector2 pos)
    {
        if (i % 3 != 0 && AutoSetControlPointsSF) return;
        Vector2 delta = pos - PointsSF[i];
        if (AutoSetControlPointsSF)
        {
            PointsSF[i] = pos;
            AutoSetAllAffectedControlPoints(i);   
        }
        else
        {
            if (i % 3 == 0)
            {
                if (i + 1 < NumPoints)
                {
                    PointsSF[i + 1] += delta;
                }
                if (i - 1 >= 0)
                {
                    PointsSF[i - 1] += delta;
                }
            }
            else
            {
                bool nextPointIsAnchor = (i + 1) % 3 == 0;
                int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
                int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;
                Vector2 anchorDeltaPoint = PointsSF[anchorIndex] - PointsSF[i];
                Vector2 anchorDelta = GetAnchorDelta(PointsSF[anchorIndex] - pos);
                if(anchorDeltaPoint.x * anchorDelta.x < 0) return;
                if (correspondingControlIndex >= 0 && correspondingControlIndex < NumPoints)
                    PointsSF[correspondingControlIndex] = PointsSF[anchorIndex] + anchorDelta;
                pos = PointsSF[anchorIndex] - anchorDelta;
            }
            PointsSF[i] = pos;
        }
    }
    public Vector2[] GetPositionOnLine(float x)
    {
        for (int pointIndex = _lastIndex+1; pointIndex < EvenlySpacedPoints.Length; pointIndex++)
        {
            if (x > EvenlySpacedPoints[pointIndex].x) continue;
            _lastIndex = pointIndex - 1;
            var casOnCurve = EvenlySpacedPoints[pointIndex] - EvenlySpacedPoints[pointIndex - 1];
            var t = (EvenlySpacedPoints[pointIndex].x - x) / (EvenlySpacedPoints[pointIndex].x - EvenlySpacedPoints[pointIndex - 1].x);
            var y = Mathf.Lerp(EvenlySpacedPoints[pointIndex - 1].y, EvenlySpacedPoints[pointIndex].y,t);
            var pointOnCurve = new Vector2(x, y);
            return new [] {  pointOnCurve , casOnCurve };
        }
        _lastIndex = 0;
        return new [] { Vector2.zero,Vector2.zero };
    }
    private static Vector2 FindPointOnCurve(float x, Vector2[] p, double t)
    {
        var preAy = -p[0].y + 3 * p[1].y - 3 * p[2].y + p[3].y;
        var cy = p[0].y;
        var by = 3 * (p[1].y - p[0].y);
        var ay = 3 * (p[0].y - 2 * p[1].y + p[2].y);
        var y = Math.Pow(t, 3) * preAy + Math.Pow(t, 2) * ay + t * by + cy;
        Vector2 pointOnCurve = new Vector2(x, (float)y);
        return pointOnCurve;
    }
    public void SetPathPoints() => EvenlySpacedPoints = CalculateEvenlySpacedPoints(0.01f, 2f);
    public void SetFirstSegmentPoints(Vector2 startPoint)
    {
        SetPosition(startPoint);
        var firstSegment = CalculateEvenlySpacedPoints(0.01f, 2f, 1).ToList();
        List<Vector2> secondSegment = new List<Vector2>();
        Vector2 firstPos = EvenlySpacedPoints[0];
        for (int i = 4; i < EvenlySpacedPoints.Length; i++)
        {
            secondSegment.Add( EvenlySpacedPoints[i] - firstPos + startPoint);
        }
        firstSegment.AddRange(secondSegment);
        EvenlySpacedPoints = firstSegment.ToArray();
    }

    public void AddEvenlySpacedPointsToEnd(Path path)
    {
        EvenlySpacedPoints = EvenlySpacedPoints.Concat(path.EvenlySpacedPoints).ToArray();
    }
    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1, int numSegments = 0)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2> { PointsSF[0] };
        Vector2 previousPoint = PointsSF[0];
        float dstSinceLastEvenPoint = 0;
        if (numSegments == 0) numSegments = NumSegments;
        Vector2 last = PointsSF[0];
        for (int segmentIndex = 0; segmentIndex < numSegments; segmentIndex++)
        {
            Vector2[] p = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f/divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;
            }
            last = p.Last();
        }
        if( !evenlySpacedPoints.Contains(last) ) evenlySpacedPoints.Add(last);
        return evenlySpacedPoints.ToArray();
    }
    
    private void AutoSetStartAndEndsControls()
    {
        PointsSF[1] = (PointsSF[0] + PointsSF[2]) * .5f;
        PointsSF[1] = new Vector2(PointsSF[1].x, 0);
        PointsSF[NumPoints - 2] = (PointsSF[NumPoints - 1] + PointsSF[NumPoints - 3]) * .5f;
        PointsSF[NumPoints - 2] = new Vector2(PointsSF[NumPoints - 2].x, 0);
    }
    private void AutoSetAllControls()
    {
        for (int i = 0; i < NumPoints; i+=3)
            AutoSetAnchorControlPoints(i);
        AutoSetStartAndEndsControls();
    }
    private void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex-3; i < updatedAnchorIndex+3; i+=3)
            if (InRange(i)) AutoSetAnchorControlPoints(i);
        AutoSetStartAndEndsControls();
    }
    private void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = PointsSF[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];
        if (InRange(anchorIndex - 3))
        {
            Vector2 offset = PointsSF[anchorIndex - 3] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (InRange(anchorIndex + 3))
        {
            Vector2 offset = PointsSF[anchorIndex + 3] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }
        dir.Normalize();
        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (InRange(controlIndex)) PointsSF[controlIndex] = anchorPos + dir * (neighbourDistances[i] * .5f);
        }
    }
    private bool InRange(int i) => i >= 0 && i < NumPoints;
    public void Restart()
    {
        _lastIndex = 0;
    }
    private Vector2 RotateTowards (Vector2 dir, int angle) => new Vector2((float)(dir.x * Math.Cos(angle) - dir.y * Math.Sin(angle)), (float)(dir.x * Math.Cos(angle) + dir.y * Math.Sin(angle)));
    public object Clone()
    {
        Path newPath = new Path(ParentTransformSF, PointsSF)
        {
            _centre = _centre
        };
        Debug.Log("_centre "+ _centre);
        Debug.Log("Parent.position "+ ParentTransformSF.position);
        newPath.EvenlySpacedPoints = EvenlySpacedPoints;
        return newPath;
    }
    public Vector2[] GetPositionOnLineOLD(float x)
    {
        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector2[] p = GetPointsInSegment(segmentIndex);
            if(p[0].x > x) return new [] { Vector2.zero,Vector2.zero };
            if(p[3].x < x) continue;
            var preA = -p[0].x + 3 * p[1].x - 3 * p[2].x + p[3].x;
            var c = (p[0].x - x) / preA;
            var b = 3*(p[1].x - p[0].x) / preA;
            var a = 3*(p[0].x - 2*p[1].x + p[2].x) / preA;
            var res = Bezier.GetRootsOfCubicEquations(a, b, c);
            foreach (var t in res)
            {
                if (!(t > 0) || !(t < 1)) continue;
                var pointOnCurve = FindPointOnCurve(x, p, t);
                var casOnCurve = pointOnCurve - _prevPosOnCurve;
                if (casOnCurve == Vector2.zero)
                    casOnCurve = _prevCasOnCurve;
                _prevPosOnCurve = pointOnCurve;
                _prevCasOnCurve = casOnCurve;
                return new [] {  pointOnCurve , casOnCurve };
            }
        }
        return new [] { Vector2.zero,Vector2.zero };
    }
}
