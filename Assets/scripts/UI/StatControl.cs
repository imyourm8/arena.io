using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

public class StatControl : MonoBehaviour 
{
    [SerializeField]
    private List<GameObject> statPoints = null;

    [SerializeField]
    private Button addBtn = null;

    [SerializeField]
    private proto_game.Stats stat = default(proto_game.Stats);

    private int statsCountUpgraded_;

    public Action<proto_game.Stats> OnStatUpgrade;

    void Start()
    {
        Reset();
    }

    public void Reset()
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
            OnStatUpgrade(stat);
    }
}
