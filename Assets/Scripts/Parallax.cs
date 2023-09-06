using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private Player _target;
    [SerializeField, Range(0f, 1f)] private float _parallaxStrength;
    [SerializeField] private bool _disableVertical;
    private Vector3 _targetPrevPosition;
    private Transform _targetTransform;
    void Start()
    {
        _targetTransform = _target.transform;
        _targetPrevPosition = _targetTransform.position;
    }

    void Update()
    {
        var delta = _targetTransform.position - _targetPrevPosition;
        if (_disableVertical) 
            delta.y = 0;
        else
            delta.y /= 2;
        _targetPrevPosition = _targetTransform.position;
        transform.position -= delta * _parallaxStrength;
    }
}
