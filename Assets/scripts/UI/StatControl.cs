using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

public class StatControl : MonoBehaviour 
{
    [SerializeField]
    private List<StatPoint> statPoints = null;

    [SerializeField]
    private Button addBtn = null;

    [SerializeField]
    private proto_game.Stats stat = default(proto_game.Stats);

    [SerializeField]
    private Color upgradedStatColor;

    [SerializeField]
    private Color nonUpgradedStatColor;

    private int statsCountUpgraded_;

    public Action<proto_game.Stats> OnStatUpgrade;

    void Start()
    {
        Reset();
    }

    public proto_game.Stats Stat
    { get { return stat; }}

    public void StepBack()
    {
        statPoints[--statsCountUpgraded_].SwitchColor(nonUpgradedStatColor);
    }

    public void Reset()
    {
        foreach(var obj in statPoints)
        {
            obj.SwitchColor(nonUpgradedStatColor);
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

        statPoints[statsCountUpgraded_++].SwitchColor(upgradedStatColor);

        if (OnStatUpgrade != null)
            OnStatUpgrade(stat);
    }
}
