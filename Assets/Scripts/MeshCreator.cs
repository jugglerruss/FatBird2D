using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshCreator : MonoBehaviour
{
    public float SpacingSF = .1f;
    public float ResolutionSF = 1;
    public float SizeMultyplierSF = 20;
    
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uv;
    private Vector2[] _evSpacedPoints;
    void Awake()
    {
        _mesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = _mesh;
        gameObject.SetActive(false);
    }
    public void ToggleVisible(bool toggle)
    {
        if(gameObject.activeSelf != toggle)  gameObject.SetActive(toggle); 
    }
    public void CreateAllMesh(Path path, Vector2 start)
    {
        _evSpacedPoints = path.CalculateEvenlySpacedPoints(SpacingSF, ResolutionSF);
        Debug.Log("Mesh points " + _evSpacedPoints.Length);
        CreateMesh(start, _evSpacedPoints);
    }
    public void ChangeStartMesh(Path path, Vector2 start)
    {
        Vector2[] startPoints = path.CalculateEvenlySpacedPoints(SpacingSF, ResolutionSF, 1);
        for (int i = 0; i < _evSpacedPoints.Length; i++)
        {
            if (i < startPoints.Length) 
                _evSpacedPoints[i] = startPoints[i];
            else
                _evSpacedPoints[i] += path.Centre;
        }

        ClearMesh();
        ToggleVisible(true);
        StartCoroutine(CreateMeshCoroutine(start, _evSpacedPoints));
    }
    private void CreateMesh(Vector2 start, Vector2[] points)
    {
        ClearMesh();
        SetStartPoint(start, points);
        var vectorDown = Vector2.down * SizeMultyplierSF * 2;
        var vectorLeft = Vector2.left * SizeMultyplierSF;
        for (int i = 0; i < points.Length - 1; i++)
            CreateShape(points[i] + vectorDown + vectorLeft, points[i + 1] + vectorDown + vectorLeft, points[i], points[i + 1]);
        SetMeshParameters();
    }
    private IEnumerator CreateMeshCoroutine(Vector2 start, Vector2[] points)
    {
        Debug.Log("ChangeStartMesh start" + points.Length);
        yield return new WaitForFixedUpdate();
        Vector2 firstPos = points[0];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = points[i] - firstPos + start;
            if (i % 100 != 0) continue;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        var vectorDown = Vector2.down * SizeMultyplierSF * 2;
        var vectorLeft = Vector2.left * SizeMultyplierSF;
        for (int i = 0; i < points.Length - 1; i++)
        {
            CreateShape(points[i] + vectorDown + vectorLeft, points[i + 1] + vectorDown + vectorLeft, points[i], points[i + 1]);
            if (i % 10 != 0) continue;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        SetMeshParameters();
        Debug.Log("ChangeStartMesh end");
    }
    private static void SetStartPoint(Vector2 start, Vector2[] points)
    {
        if (start == Vector2.zero) return;
        Vector2 firstPos = points[0];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = points[i] - firstPos + start;
        }
    }
    private void ClearMesh()
    {
        _mesh.Clear(false); 
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _uv = new List<Vector2>();
    }

    private void SetMeshParameters()
    {
        _mesh.SetVertices( _vertices.ToArray());
        _mesh.SetTriangles(_triangles.ToArray(),0);
        _mesh.SetUVs(1,_uv.ToArray());
    }
    private void CreateShape(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        var uvRange = 1500f;
        _vertices.AddRange(new List<Vector3> { p1, p2, p3, p4 });
        _triangles.AddRange(new List<int> 
        { 
            _vertices.Count - 4, _vertices.Count - 3, _vertices.Count - 2,
            _vertices.Count - 2, _vertices.Count - 1, _vertices.Count - 3 
        });
        var ost = _vertices.Count % uvRange;
        var uv0 = (ost - 4f )/ uvRange;
        var uv1 = ost / uvRange;
        var uv2 = uv0;
        var uv3 = uv1;
        _uv.AddRange(new List<Vector2> { new(uv0, 0), new (uv1, 0), new (uv2, 1), new (uv3, 1) });
    }

    public void Delete()
    {
        Destroy (_mesh);
        Destroy (_meshFilter);
        Destroy (gameObject);
    }
}
