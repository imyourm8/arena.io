using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System;

using DG.Tweening;

public class Entity : ArenaObject
{ 
    [SerializeField]
    private Vector3 hpBarOffset_ = Vector3.zero;

    protected StateInterpolator stateInterpolator_;
    private Tween rotationTweener_;
    private Vector2 moveImpulse_;
    private Tween colorTween_;

    #region getter & setters
    private Attributes.UnitAttributes stats_ = new Attributes.UnitAttributes();
    public Attributes.UnitAttributes Stats 
    {
        get { return stats_; } 
    }

    private int exp_;
    public int Exp
    {
        set { exp_ = value; }
        get { return exp_; }
    }

    public Vector2 AttackDirection
    {
        get; private set;
    }

    public bool IsNetworked
    { get; set; }

    public void SetAttackDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float x = Vector2.right.x * Mathf.Cos(rad) - Vector2.right.y * Mathf.Sin(rad);
        float y = Vector2.right.x * Mathf.Sin(rad) + Vector2.right.y * Mathf.Cos(rad);
        AttackDirection = new Vector2(x, y);
    }

    private float rotation_ = 0;
    private bool forceRotation_ = false;
    public float Rotation 
    {
        set 
        { 
            rotation_ = value; 
            if (rotationTweener_ != null)
                rotationTweener_.Kill(false);

            rotationTweener_ = transform_.DORotate(new Vector3(0,0,rotation_), forceRotation_?0.0f:0.1f, RotateMode.Fast);

            forceRotation_ = false;
            SetAttackDirection(rotation_);
        }

        get { return rotation_; }
    }

    public void ForceRotation()
    {
        forceRotation_ = true;
    }

    private float health_ = -1.0f;
    public float Health
    { 
        get { return health_; }  
        set 
        {
            if (health_ > -1.0f && !Mathf.Approximately(health_, value))
            {
                hpBar_.ShowSmooth();
            }

            health_ = value;

            if (Mathf.Approximately(Health, stats_.GetFinValue(proto_game.Stats.MaxHealth)))
            {
                hpBar_.HideSmooth();
            }

            hpBar_.Progress = Health / stats_.GetFinValue(proto_game.Stats.MaxHealth);
        }
    }

    public Vector2 AttackPosition
    { get; set; }
      
    public AnimatedProgress hpBar_;
    public AnimatedProgress HpBar
    {
        set { hpBar_ = value; }
    }
    #endregion

    public void ApplyDamage(float damage)
    {
        Health -= damage;
        OnDamageApplied();
    }

    private void OnDamageApplied()
    {
        if (colorTween_ != null)
            colorTween_.Kill(true);

        Sequence colorTween = DOTween.Sequence();
        colorTween.Append(sprite.DOColor(Color.red, 0.06f));
        colorTween.Append(sprite.DOColor(initialColor_, 0.06f));
        colorTween.OnComplete(()=>colorTween_ = null);
        colorTween_ = colorTween;
    }

    public void SetRotation(Vector2 rot)
    {
        Rotation = (float)Math.Atan2(rot.y, rot.x) * Mathf.Rad2Deg;
    }

    protected override void OnPositionSet()
    {
        AttackPosition = Position;
    }

    public override void Init(arena.ArenaController controller)
	{
        base.Init(controller);
        //critical for hp bar logic
        health_ = -1.0f;

        if (stateInterpolator_ == null)
            stateInterpolator_ = new StateInterpolator(this);
        if (!Local) 
            stateInterpolator_.Reset();
	}
    
	public override void OnUpdate (float dt) 
	{
        base.OnUpdate(dt);

        if (!Local && IsNetworked)
        {
            if (stateInterpolator_ != null)
            {
                stateInterpolator_.Update();
            }
        }

        UpdateHpBarPosition();
	}

    protected void UpdateHpBarPosition()
    {
        if (hpBar_ != null)
        {
            hpBar_.transform.position = transform_.localPosition + hpBarOffset_;
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (colorTween_ != null)
        {
            colorTween_.Kill();
        }

        if (hpBar_ != null)
        {
            hpBar_.gameObject.ReturnPooled();
            hpBar_ = null;      
        }
    }

	public void UpdateState(proto_game.UnitState state, int tick)
	{
        stateInterpolator_.PushState(state, tick);
	}

    public void ApplyStats(List<proto_game.StatValue> stats)
    {
        foreach(var s in stats)
        {
            stats_.Get(s.stat).SetStep(s.step).SetValue(s.value);
            stats_.Get(s.stat).ResetSteps();
        }
    }
}
