using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System;

using DG.Tweening;

public class Entity : MonoBehaviour
{ 
	public delegate void OnDeathCallback (Entity entity);
    public event OnDeathCallback OnDie = null;

	public delegate void OnAttackCallback (Entity entity);
    public event OnAttackCallback OnAttack = null;

    public delegate void OnDamageCallback (Entity entity, float damage);
    public event OnDamageCallback OnDamage = null;

    [SerializeField]
    private bool local = false;

    [SerializeField]
    private Vector3 hpBarOffset_ = Vector3.zero;

    [SerializeField]
    private Rigidbody2D body_ = null;

    [SerializeField]
    private Transform transform_ = null;

    [SerializeField]
    private new Collider2D collider = null;

    [SerializeField]
    private SpriteRenderer sprite = null;

    private bool firstInit_ = true;
    private Color initialColor_;
    private Vector3 initialScale_;
    private StateInterpolator stateInterpolator_;
    private Vector2 previousPosition_;
    protected Weapon weapon_;
    private arena.ArenaController controller_;
    private Tween rotationTweener_;
    private Vector2 moveImpulse_;
    private Skill skill_;
    private bool destroyed_ = false;

    #region getter & setters
    public Rigidbody2D Body
    {
        get { return body_; }
    }

    private Vector2 force_;
    public Vector2 Force
    {
        set 
        {   
            force_ = value;
        }
        get { return force_; }
    }

    public Collider2D Collider
    { get { return collider; } }

    private int exp_;
    public int Exp
    {
        set { exp_ = value; }
        get { return exp_; }
    }

    public float AttackRange
    { get; set; }

    public Vector2 AttackPosition
    { get; set; }

    public bool Local
    { 
        get 
        { 
            return local; 
        } 
        set 
        { 
            local = value; 
        }
    }

    public Vector2 AttackDirection
    {
        get; private set;
    }

    public void Remove(bool animated)
    {   
        if (animated)
        {
            Sequence tweenSeq = DOTween.Sequence();

            var scale = transform_.localScale;
            scale.x *= 1.4f;
            scale.y *= 1.4f;

            tweenSeq.Append(sprite.DOFade(0.0f, 0.2f));
            tweenSeq.Join(transform_.DOScale(scale, 0.2f));

            tweenSeq.OnComplete(()=>
            {   
                OnRemove();
            });
        } 
        else
        {
            OnRemove();
        }
    }

    public void SetAttackDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float x = Vector2.right.x * Mathf.Cos(rad) - Vector2.right.y * Mathf.Sin(rad);
        float y = Vector2.right.x * Mathf.Sin(rad) + Vector2.right.y * Mathf.Cos(rad);
        AttackDirection = new Vector2(x, y);
    }

    private float prevRotation_ = 0;
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

    public void SetSkill(proto_game.Skills skillId)
    {
        skill_ = SkillsPrefabs.Instance.GetSkill(skillId);
        skill_.Owner = this;

        var cd = stats_.GetFinValue(proto_game.Stats.SkillCooldown);
        skill_.Cooldown = (long)cd;
    }

    public bool CanCast()
    {
        return skill_.CanCast();
    }

    public void CastSkill()
    {
        skill_.Cast();
    }

    public bool Rotated 
    {
        get { return !Mathf.Approximately(rotation_, prevRotation_); }
    }

    [SerializeField]
    private float currentSpeed_ = 0;
    public float CurrentSpeed
    {
        get { return currentSpeed_; }
    }

    public int ID
    { get; set; }

    private bool stopped_ = true;
    public bool Moved
    {
        get { return previousPosition_ != Position; }
    }

    private float nextAttack_ = 0;
    public float AutoAttackCooldown
    {
        get { return stats_.GetFinValue(proto_game.Stats.ReloadSpeed); }
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

    public Vector2 Position
    {
        get 
        { 
            return new Vector2(transform_.localPosition.x, transform_.localPosition.y); 
        }

        set 
        { 
            var prev = Position;
            transform_.localPosition = new Vector3(value.x, value.y, 0);

            var diff = Position - prev;
            currentSpeed_ = diff.magnitude;

            AttackPosition = value;
        }
    }

    public Vector2 RecoilVelocity
    { get; set; }

    public Vector2 Velocity
    { get; set; }
      
    public AnimatedProgress hpBar_;
    public AnimatedProgress HpBar
    {
        set 
        { 
            hpBar_ = value; 
        }
    }

    public arena.ArenaController Controller
    { get { return controller_; } }

    private Attributes.UnitAttributes stats_ = new Attributes.UnitAttributes();
    public Attributes.UnitAttributes Stats { get { return stats_; }}
    #endregion

    public void SetRotation(Vector2 rot)
    {
        Rotation = (float)Math.Atan2(rot.y, rot.x) * Mathf.Rad2Deg;
    }

	protected Entity() 
    {
        stats_.Add(new Attributes.Attribute<proto_game.Stats>().Init(proto_game.Stats.BulletSize, 1));
	}

    public virtual void Init(arena.ArenaController controller, Vector2 startPos)
	{
        //critical for hp bar logic
        health_ = -1.0f;

        if (firstInit_)
        {
            firstInit_ = false;
            initialScale_ = transform_.localScale;
            initialColor_ = sprite.color;
        }
        else 
        {
            transform_.localScale = initialScale_;
            sprite.color = initialColor_;
        }
        destroyed_ = false;
        AttackRange = 0.0f;
        controller_ = controller;
        Position = startPos;

        if (stateInterpolator_ == null)
            stateInterpolator_ = new StateInterpolator(this);
        if (!local) 
            stateInterpolator_.Reset();
	}

    public virtual void ApplyRecoil(float recoil)
    {
        
    }

    public bool PerformAttack(float direction, long time = -1)
    {
        Rotation = direction;

        if (Time.time < nextAttack_)
        {
            return false;
        }

        if (local)
            nextAttack_ = Time.time + AutoAttackCooldown;

        if (weapon_ != null)
        {
            weapon_.SpawnBullets(time);
        }

        return true;
    }

    public bool PerformAttack(Vector2 direction, long time = -1)
	{
        return PerformAttack((float)Math.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, time);
	}

	public virtual void OnUpdate (float dt) 
	{
        previousPosition_ = Position;
        prevRotation_ = rotation_; 

        if (!local)
        {
            if (stateInterpolator_ != null)
                stateInterpolator_.Update();
        }

        UpdateHpBarPosition();
	}

    protected void UpdateHpBarPosition()
    {
        if (hpBar_ != null)
            hpBar_.transform.position = transform_.localPosition + hpBarOffset_;
    }

    public virtual void OnRemove()
    {
        if (destroyed_) 
            return;
        destroyed_ = true;
        if (hpBar_ != null)
        {
            hpBar_.gameObject.ReturnPooled();
            hpBar_ = null;      
        }

        if (skill_ != null)
        {
            skill_.gameObject.ReturnPooled();
            skill_ = null;
        }

        gameObject.ReturnPooled();
    }

	public void UpdateState(proto_game.UnitState state, long time)
	{
        stateInterpolator_.PushState(state, time);
	}

    public void ApplyStats(proto_game.PlayerStats stats)
    {
        foreach(var s in stats.stats)
        {
            stats_.Get(s.stat).SetStep(s.step).SetValue(s.value);
            stats_.Get(s.stat).RawValue = 0;
        }
    }
}
