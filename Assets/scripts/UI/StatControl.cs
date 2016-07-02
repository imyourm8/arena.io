using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

public class StatControl : MonoBehaviour 
{
    [SerializeField]
    private List<GameObject> statPoints;

    [SerializeField]
    private Button addBtn;

    private int statsCountUpgraded_;

    public Action OnStatUpgrade;

    void Start()
    {
        foreach(var obj in statPoints)
        {
            obj.GetComponent<Image>().color = Color.grey;
        }
    }

    public void HandleStatPointsCount(int count)
    {
        if (count > 0 && statsCountUpgraded_ < statPoints.Count)
        {
            addBtn.interactable = true;
        }
        else 
        {
            addBtn.interactable = false;
        }
    }

    public void HandleUpgradeStatPoint()
    {
        if (statsCountUpgraded_ >= statPoints.Count) return;

        statPoints[statsCountUpgraded_++].GetComponent<Image>().color = Color.white;

        if (OnStatUpgrade != null)
            OnStatUpgrade();
    }
}
