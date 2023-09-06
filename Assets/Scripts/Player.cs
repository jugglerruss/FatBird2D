using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [SerializeField] private LineRenderer Force1SF;
    [SerializeField] private LineRenderer Force2SF;
    [SerializeField] private LineRenderer Force3SF;
    [SerializeField] private Transform StartSF;
    [SerializeField] private Transform BirdSF;
    [SerializeField] private Camera CameraSF;
    [SerializeField] private float MinVelocitySF;
    public Func<Vector3, Vector3[]> OnMove;
    public Func<float> GetMultiplier;
    private float _startSpeed;
    private bool _isDebug;
    private Transform _transformCached;
    private GameSettings _settings;
    private Vector3 _acceleration;
    private Vector3 _velocity;
    private Vector3 _pos;
    public void Init(GameSettings settings)
    {
        _settings = settings;
        _transformCached = transform;
        _startSpeed = _settings.StartSpeed;
        RestartToPos();
    }
    private void Update()
    {
        var currentPos = _transformCached.position;
        _acceleration = Vector3.zero;
        Vector3 gravityVector = Vector2.down * _settings.DebugSettings.Mass * GetMultiplier();
        var resMove = OnMove(currentPos + _pos);
        var frictionForce = resMove[0] != Vector3.zero ? MoveOnGround(resMove[0], resMove[1], gravityVector) : MoveOnSky(gravityVector);
        RotateBird();
        ShowForces(currentPos, gravityVector, frictionForce);
    }
    private Vector3 MoveOnSky(Vector3 gravityVector)
    {
        _transformCached.position += _pos;
        var frictionForce = _velocity * _settings.DebugSettings.SkyFriction * _settings.DebugSettings.Mass;
        _acceleration += gravityVector - frictionForce;
        _velocity += _acceleration * Time.deltaTime;
        _pos = _velocity * Time.deltaTime;
        return frictionForce;
    }
    private Vector3 MoveOnGround(Vector3 posOnGround, Vector3 casatel, Vector3 gravityVector)
    {
        casatel = casatel.normalized;
        _transformCached.position = posOnGround;
        var proect = casatel * casatel.y * gravityVector.y;
        var frictionForce = casatel * _settings.DebugSettings.GroundFriction;
        _acceleration += proect - frictionForce; 
        _velocity += _acceleration * Time.deltaTime;
        if (_velocity.magnitude < MinVelocitySF) _velocity = _velocity.normalized * MinVelocitySF;
        var positiveCas = casatel.x < 0 ? -casatel : casatel;
        _velocity = positiveCas * (positiveCas.x * _velocity.x + positiveCas.y * _velocity.y);
        if (_velocity.x < 0) _velocity = new Vector3(Math.Abs(_velocity.x), _velocity.y);
        _pos = _velocity * Time.deltaTime;
        return frictionForce;
    }
    private void RotateBird()
    {
        BirdSF.right = Vector3.MoveTowards(BirdSF.right,_velocity, 0.09f);
    }
    private void ShowForces(Vector3 currentPos, Vector3 gravityVector, Vector3 frictionForce)
    {
        if (!_isDebug) return;
        Force1SF.SetPositions(new[] { currentPos, currentPos + gravityVector });
        Force2SF.SetPositions(new[] { currentPos, currentPos - frictionForce });
        Force3SF.SetPositions(new[] { _transformCached.position, _transformCached.position + _velocity });
    }
    public void RestartToPos()
    {
        transform.position = StartSF.position;
        _velocity = Vector2.right * _startSpeed;
    }
    public void ToggleDebug()
    {
        _isDebug = !_isDebug;
        Force1SF.enabled = _isDebug;
        Force2SF.enabled = _isDebug;
        Force3SF.enabled = _isDebug;
    }
}
