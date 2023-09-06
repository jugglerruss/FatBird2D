using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private GameSettings SettingsSF;
    [SerializeField] private Player PlayerSF;
    [SerializeField] private Camera CameraSF;
    [SerializeField] private Paths PathsSF;
    [SerializeField] private InputCanvas InputSF;
    [SerializeField] private CoinsCreator CoinCreatorSF;
    private float _time;
    private Vector3 _cameraStart;
    private float _cameraStartSize;
    private float _cameraStartX;
    private float _cameraStartY;
    private Transform _cameraTransform;
    private int _frameCount;
    private bool _isDebug;
    private bool _isVisibleGround;
    private int _coinsCount;
    void Awake()
    {
        _coinsCount = 0;
        SettingsSF.UpdateSettings();
        PlayerSF.Init(SettingsSF);
        PlayerSF.OnMove += OnMove;
        PlayerSF.GetMultiplier += OnGetMultiplier;
        PathsSF.Init(CoinCreatorSF);
        PathsSF.OnAddSegment += ShowInfoAddSegment;
        PathsSF.OnPlayerCollectCoin += PlayerCollectCoin;
    }

    private void Start()
    {
        SettingsSF.SetSettingsOnStart();
        SetCamera();
    }
    private Vector3[] OnMove(Vector3 pos)
    {
        var res = PathsSF.PathSF.GetPositionOnLine(pos.x);
        PathsSF.MovePathSegmentToEnd(pos);
        Vector3 linePos = res[0];
        if (linePos == Vector3.zero) return new []{Vector3.zero, Vector3.zero};
        if (pos.y > 0) MoveCamera(pos);
        return linePos.y >= pos.y ? new[] { linePos, (Vector3)res[1] } : new []{Vector3.zero, Vector3.zero};
    }
    
    private void MoveCamera(Vector3 pos)
    {
        var localPosition = _cameraTransform.localPosition;
        var camPos = localPosition;
        var size = Mathf.Lerp(CameraSF.orthographicSize, _cameraStartSize + pos.y / 2, 0.1f);
        CameraSF.orthographicSize = size;
        localPosition = Vector3.MoveTowards(localPosition,
            new Vector3(_cameraStartX * size,
                _cameraStartY * size, camPos.z),
            0.1f);
        _cameraTransform.localPosition = localPosition;
    }
    private float OnGetMultiplier() => Input.GetMouseButton(0) ? SettingsSF.DebugSettings.MassMultiplier : 1f;
    private void Update()
    {
        DisplayFps();
    }
    private void DisplayFps()
    {
        _time += Time.deltaTime;
        _frameCount++;
        if (_time < 1f) return;
        var frameRate = Mathf.RoundToInt(_frameCount / _time);
        InputSF.SetTextFps(frameRate);
        _time = 0;
        _frameCount = 0;
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Restart()
    {
        PlayerSF.RestartToPos();
        PathsSF.Restart();
        ResetCoins();
    }
    public void DebugPlayer()
    {
        _isDebug = !_isDebug;
        PlayerSF.ToggleDebug();
        InputSF.SetDebug(_isDebug);
        if (!_isDebug) return;
        InputSF.SetInfo(SettingsSF.DebugSettings);
    }
    public void SetSettings()
    {
        SettingsSF.SetSettings(InputSF.GetDebugSettings());
    }
    public void ToggleVisibleGround()
    {
        PathsSF.ToggleVisibleGround(_isVisibleGround);
        _isVisibleGround = !_isVisibleGround;
    }
    private void ShowInfoAddSegment()
    {
        StartCoroutine(InputSF.ShowInfo());
    }
    private void SetCamera()
    {
        _cameraTransform = CameraSF.transform;
        _cameraStart = _cameraTransform.localPosition;
        _cameraStartSize = CameraSF.orthographicSize;
        _cameraStartX = _cameraStart.x / _cameraStartSize;
        _cameraStartY = _cameraStart.y / _cameraStartSize;
    }

    private void PlayerCollectCoin()
    {
        _coinsCount++;
        InputSF.UpdateCoinsCount(_coinsCount);
    }
    private void ResetCoins()
    {
        _coinsCount = 0;
        InputSF.UpdateCoinsCount(_coinsCount);
    }

    
}