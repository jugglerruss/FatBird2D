using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


public class Paths : MonoBehaviour
{
    [HideInInspector] public Path PathSF;
    [HideInInspector] public CoinsCreator CoinCreatorSF;
    [SerializeField] public List<PathCreator> PathsListSF;
    [SerializeField] private MeshCreator MeshCreatorSF;
    private List<Vector2> _points;
    private List<Vector2> _controlPointList;
    private List<MeshCreator> _meshesList;
    private int _pathIndex;
    public Action OnAddSegment;
    public Action OnPlayerCollectCoin;

    public void Init(CoinsCreator coinsCreator)
    {
        CoinCreatorSF = coinsCreator;
        CoinCreatorSF.OnPlayerCollectCoin += PlayerCollectCoin;
    }
    private void Start()
    {
        MakeStartPath();
    }
    private void MakeStartPath()
    {
        _pathIndex = 0;
        _points = new List<Vector2>();
        _meshesList = new List<MeshCreator>();
        _controlPointList = new List<Vector2>();
        foreach (var pathCreator in PathsListSF)
        {
            Path currentPath = pathCreator.Path;
            MeshCreator newMeshCreator = Instantiate(MeshCreatorSF);
            _meshesList.Add(newMeshCreator);
            var pointsList = _points.Count > 0 ? GenerateNextSegment(currentPath, newMeshCreator) : GenerateFirstSegment(currentPath, newMeshCreator);
            currentPath.SetPathPoints();
            _points.AddRange(pointsList);
        }

        _controlPointList.Add(PathsListSF.Last().Path.Points.Last());
        PathSF = new Path(transform, _points);
        PathSF.SetPathPoints();
        _meshesList[0].ToggleVisible(true);
        CoinCreatorSF.CreateCoinsOnRandomPoints(PathSF.EvenlySpacedPoints.ToList());
    }

    private List<Vector2> GenerateNextSegment(Path currentPath, MeshCreator newMeshCreator)
    {
        var pointsList = currentPath.GetPathFromPoint(_points[^1]);
        newMeshCreator.CreateAllMesh(currentPath, _points[^1]);
        _controlPointList.Add(currentPath.Points.First());
        _points.Remove(_points[^1]);
        return pointsList;
    }

    private List<Vector2> GenerateFirstSegment(Path currentPath, MeshCreator newMeshCreator)
    {
        _controlPointList.Add(currentPath.Points.First());
        newMeshCreator.CreateAllMesh(currentPath, Vector2.zero);
        var pointsList = currentPath.Points;
        return pointsList;
    }

    public void MovePathSegmentToEnd(Vector2 playerPos)
    {
        if (_controlPointList.Count - 2 > _pathIndex && _controlPointList[_pathIndex].x < playerPos.x)
        {
            _pathIndex++;
            _meshesList[_pathIndex].ToggleVisible(true);
            if(_pathIndex - 3 >= 0)_meshesList[_pathIndex - 3].ToggleVisible(false);
            return;
        }
        if (_controlPointList[^2].x > playerPos.x) return;
        
        Vector2 start = _controlPointList[0];
        Vector2 end = _controlPointList[1];
        Vector2 lastPoint = _controlPointList.Last();
            
        Vector2 newPoint = end - start + lastPoint;
        _controlPointList.Remove(_controlPointList[0]);
        _controlPointList.Add(newPoint);
        _pathIndex++;
        var nextIndex = _pathIndex % PathsListSF.Count;
        var newPath = (Path)PathsListSF[nextIndex].Path.Clone();
        //_meshesList[(_pathIndex-3) % PathsListSF.Count].ToggleVisible(false); 
        MeshCreator selectedMesh = _meshesList[nextIndex];
        
        newPath.SetFirstSegmentPoints(lastPoint);
        PathSF.AddEvenlySpacedPointsToEnd(newPath);
        CoinCreatorSF.DisableCoinsBeforePoint(end);
        CoinCreatorSF.MoveCoinsFromDisabledOnRandomPoints(newPath.EvenlySpacedPoints.ToList());
            
        selectedMesh.ChangeStartMesh(newPath, lastPoint);
        OnAddSegment.Invoke();
    }

    private void PlayerCollectCoin()
    {
        OnPlayerCollectCoin();
    }

    public void ToggleVisibleGround(bool toggle)
    {
        foreach (var creator in _meshesList)
        {
            creator.ToggleVisible(toggle);
        }
    }
    public void Restart()
    {
        ClearMeshes();
        ClearPaths();
        CoinCreatorSF.ClearCoins();
        MakeStartPath();
    }

    private void ClearPaths()
    {
        foreach (var pathCreator in PathsListSF)
        {
            pathCreator.Path.SetPosition(Vector2.zero);
        }
    }

    private void ClearMeshes()
    {
        foreach (var meshCreator in _meshesList)
        {
            meshCreator.Delete();
        }
    }
}