using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

public class StatsPanel : MonoBehaviour 
{
    [SerializeField]
    private List<StatControl> stats = null;

    [SerializeField]
    private float hideDelay = 2.0f;

    [SerializeField]
    private float hideDuration = 0.3f;

    [SerializeField]
    private Ease hideEase = Ease.OutCubic;

    [SerializeField]
    private CanvasGroup canvasGroup = null;

    private Tweener hideTweener_;
    private int statsPointsLeft_ = 0;

	void Start () 
    {
        foreach(var control in stats)
        {
            control.OnStatUpgrade = HandleStatUpgrade;
        }
        SetHidden(false);
	}

    public Player Player
    { get; set; }

    public void Reset()
    {
        foreach(var control in stats)
        {
            control.Reset();
        }
    }

    public void AddPoints(int value)
    {
        statsPointsLeft_ += value;
        RefreshState();
    }

    private void HandleStatUpgrade(proto_game.Stats stat)
    {
        statsPointsLeft_--;
        RefreshState();
        Player.Stats.Get(stat).IncreaseByStep();
    }

    private void SetHidden(bool animated)
    {
        bool show = statsPointsLeft_ > 0;
        if (!animated)
        {
            canvasGroup.alpha = show ? 1.0f : 0.0f;
            gameObject.SetActive(show);
        }
        else 
        {
            gameObject.SetActive(true);

            if (hideTweener_ != null)
                hideTweener_.Kill();

            hideTweener_ = canvasGroup.DOFade(show?1.0f:0.0f, show?0.05f:hideDuration);
            hideTweener_.SetEase(hideEase);
            hideTweener_.OnComplete(()=>
            {
                hideTweener_ = null;
                gameObject.SetActive(show);
            });

            if (!show)
                hideTweener_.SetDelay(hideDelay);
        }
    }

    private void RefreshState()
    {
        SetHidden(true);

        foreach(var control in stats)
        {
            control.HandleStatPointsCount(statsPointsLeft_);
        }
    }
}
