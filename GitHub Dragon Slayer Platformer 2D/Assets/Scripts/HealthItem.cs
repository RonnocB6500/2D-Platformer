using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthItem : MonoBehaviour, IItem
{
    public int healAmount = 1;
    public static event Action<int> OnHealthCollect;

    void Start()
    {
        MenuButtonController.OnMenu += ResetItem;
        GameController.OnReset += ResetItem;
    }
    public void Collect()
    {
        OnHealthCollect.Invoke(healAmount);
        gameObject.SetActive(false);
    }

    public void ResetItem()
    {
        gameObject.SetActive(true);
    }
}
