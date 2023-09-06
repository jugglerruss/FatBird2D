using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Action<Coin> OnPlayerTrigger;
    public Vector3 Position => transform.position;
    private void OnTriggerEnter2D(Collider2D col)
    {
        OnPlayerTrigger.Invoke(this);
        SetAsDisabled();
    }

    public void SetAsDisabled()
    {
        gameObject.SetActive(false);
        transform.position = Vector3.down * 10000;
    }
    public void SetAsActive(Vector3 pos)
    {
        gameObject.SetActive(true);
        transform.position = pos;
    }
    public void Delete()
    {
        OnPlayerTrigger = null;
        Destroy(gameObject);
    }
}
