using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CoinsCreator : MonoBehaviour
{
    [SerializeField] private Coin CoinPrefabSF;
    [SerializeField] private float RangeSF;
    [SerializeField] [Range(0,10)] private int CoinsCountBy10PointsSF;
    [SerializeField] private int CountSpacesFromStartSF;
    private List<Coin> _coinsList = new();
    private List<Coin> _coinsListDisabled = new();
    public Action OnPlayerCollectCoin;

    public void ClearCoins()
    {
        foreach (var coin in _coinsList)
            coin.Delete();
        foreach (var coin in _coinsListDisabled)
            coin.Delete();
        _coinsList = new();
        _coinsListDisabled = new();
    }
    public void CreateCoinsOnRandomPoints(List<Vector2> allPoints)
    {
        InstantiateDisabledCoins();
        List<Vector2> dividedPosList = DividePathByRange(allPoints);
        var i = 0;
        List<Vector2> posCoinsList = new List<Vector2>();
        foreach (var pos in dividedPosList)
        {
            i++;
            if (CountSpacesFromStartSF > i) continue;
            posCoinsList.Add(pos);
            if ((i - CountSpacesFromStartSF) % 10 != 0) continue;
            InstantiateCoins(GetRandomList(posCoinsList));
            posCoinsList = new List<Vector2>();
        }
    }

    public void DisableCoinsBeforePoint(Vector2 point)
    {
        foreach (var coin in _coinsList.Where(coin => coin.Position.x < point.x))
            coin.SetAsDisabled();
    }
    public void MoveCoinsFromDisabledOnRandomPoints(List<Vector2> allPoints)
    {
        List<Vector2> posList = DividePathByRange(allPoints);
        var i = 0;
        List<Vector2> posCoinsList = new List<Vector2>();
        foreach (var pos in posList)
        {
            i++;
            posCoinsList.Add(pos);
            if (i % 10 != 0) continue;
            var rndList = GetRandomList(posCoinsList);
            foreach (var posRnd in rndList)
            {
                MoveCoinFromDisableList(posRnd);
            }
            posCoinsList = new List<Vector2>();
        }
    }

    private List<Vector2> GetRandomList(List<Vector2> posCoinsList)
    {
        List<Vector2> posRandomList = new List<Vector2>();
        int coinsCount = 0;
        foreach (var posCoin in posCoinsList)
        {
            if (CoinsCountBy10PointsSF > coinsCount)
                posRandomList.Add(posCoin);
            else if (Random.Range(0f, CoinsCountBy10PointsSF) < (float)CoinsCountBy10PointsSF / coinsCount)
                posRandomList[Random.Range(0, CoinsCountBy10PointsSF - 1)] = posCoin;
            coinsCount++;
        }

        return posRandomList;
    }

    private void InstantiateCoins(List<Vector2> posRandomList)
    {
        foreach (var coin in posRandomList.Select(posCoin => Instantiate(CoinPrefabSF, posCoin, Quaternion.identity, transform)))
        {
            _coinsList.Add(coin);
            coin.OnPlayerTrigger += OnPlayerTrigger;
        }
    }
    private void InstantiateDisabledCoins()
    {
        for (int i = 0; i < 20; i++)
        {
            var coin = Instantiate(CoinPrefabSF, transform);
            _coinsListDisabled.Add(coin);
            coin.OnPlayerTrigger += OnPlayerTrigger;
            coin.SetAsDisabled();
        }
    }

    private void MoveCoinFromDisableList(Vector2 pos)
    {
        Coin coin = _coinsListDisabled.Last();
        _coinsListDisabled.Remove(coin);
        coin.SetAsActive(pos);
        _coinsList.Add(coin);
    }
    private void OnPlayerTrigger(Coin coin)
    {
        _coinsList.Remove(coin);
        _coinsListDisabled.Add(coin);
        OnPlayerCollectCoin.Invoke();
    }

    private List<Vector2> DividePathByRange(List<Vector2> allPoints)
    {
        List<Vector2> posList = new List<Vector2>();
        var lastPoint = allPoints.First();
        foreach (var point in allPoints)
        {
            if (RangeSF < point.x - lastPoint.x)
            {
                var newPoint = point + Vector2.up * 4;
                lastPoint = point;
                posList.Add(newPoint);
            }
        }

        return posList;
    }
}
