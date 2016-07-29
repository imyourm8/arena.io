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

    public delegate void OnStopCallback (Entity entity);
    public event OnStopCallback OnStop;
    public event OnStopCallback OnStartMove;

    [SerializeField]
    private bool local = false;

    [SerializeField]
    private Vector3 hpBarOffset_ = Vector3.zero;

    [SerializeField]
    private Rigidbody2D body_ = null;

    [SerializeField]
    private Transform transform_ = null;

    [SerializeField]
    private Collider2D collider = null;

    [SerializeField]
    private SpriteRenderer sprite;

    private bool firstInit_ = true;
    private Color initialColor_;
    private Vector3 initialScale_;
    private MovementInterpolator moveInterpolator_;
    private Vector2 previousPosition_;
    protected Weapon weapon_;
    private arena.ArenaController controller_;
    private Tween rotationTweener_;
    private Vector2 moveImpulse_;

    #region getter & setters
    private Vector2 force_;
    public Vector2 Force
    {
        set { force_ = value; }
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
            if (!local)
            { 
                gameObject.layer = LayerMask.NameToLayer("Enemy"); 
            }
            else 
            {
                gameObject.layer = LayerMask.NameToLayer("Default"); 
            }
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
                gameObject.ReturnPooled();
            });
        } 
        else
        {
            gameObject.ReturnPooled();
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
        }

        get { return rotation_; }
    }

    public void ForceRotation()
    {
        forceRotation_ = true;
    }

    public bool Rotated 
    {
        get { return !Mathf.Approximately(rotation_, prevRotation_); }
    }

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

    private float health_ = 0.0f;
    public float Health
    { 
        get { return health_; }  
        set 
        {
            if (!Mathf.Approximately(health_, value))
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
            transform_.localPosition = new Vector3(value.x, value.y, 0);
            AttackPosition = value;
        }
    }
      
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

	protected Entity() 
    {
        stats_.Add(new Attributes.Attribute<proto_game.Stats>().Init(proto_game.Stats.BulletSize, 1));
	}

    public virtual void Init(arena.ArenaController controller, Vector2 startPos)
	{
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

        controller_ = controller;
        if (moveInterpolator_ == null)
            moveInterpolator_ = new MovementInterpolator(this);
        Position = startPos;

        if (!local) 
            moveInterpolator_.Reset(Position);
	}

    public void ApplyRecoil(float recoil)
    {
        if (local && body_ != null)
        {
            recoil *= -1;
            var dir = AttackDirection; 
            dir.x *= recoil;
            dir.y *= recoil;
            body_.AddForce(dir, ForceMode2D.Impulse);
        }
    }

    public void PerformAttack(float direction)
    {
        Rotation = direction;
        SetAttackDirection(direction);

        if (Time.time < nextAttack_)
        {
            return;
        }

        if (local)
            nextAttack_ = Time.time + AutoAttackCooldown;

        if (weapon_ != null)
        {
            weapon_.SpawnBullets();

            if (local)
                OnAttack(this);
        }
    }

	public void PerformAttack(Vector2 direction)
	{
        PerformAttack(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
	}

    public virtual void OnFixedUpdate()
    {
        previousPosition_ = Position;
        prevRotation_ = rotation_; 

        if (local)
        {
            Vector2 totalImpulse = new Vector2(
                stats_.GetFinValue(proto_game.Stats.MovementSpeed), 
                stats_.GetFinValue(proto_game.Stats.MovementSpeed)
            );
            totalImpulse.Scale(force_);
            //totalImpulse_ = totalImpulse;
            currentSpeed_ = totalImpulse.magnitude;
            //totalImpulse.x *= Time.fixedDeltaTime;
            //totalImpulse.y *= Time.fixedDeltaTime;
            //totalImpulse.x *= 3.0f;
            //totalImpulse.y *= 3.0f;
            body_.AddForce(totalImpulse);
            //Position = previousPosition_ + totalImpulse;
        }
        else 
        {
            Position = moveInterpolator_.GetPosition();
        }

        UpdateHpBarPosition();
        
        if (!Moved && !stopped_ && local)
        {
            OnStop(this);
            stopped_ = true;
        }
        else if (Moved && stopped_ && local)
        {
            OnStartMove(this);
            stopped_ = false;
        }
    }

	public virtual void OnUpdate () 
	{
        AttackPosition = Position;
	}

    private void UpdateHpBarPosition()
    {
        hpBar_.transform.position = transform_.localPosition + hpBarOffset_;
    }

    public void DealDamage(Entity target, float damage)
    {
        Controller.SendDamageDone(target, damage);
    }

    public virtual void OnRemove()
    {
        Controller.ReturnHpBar(hpBar_.gameObject);        
    }

	public void SetNextPosition(long time, Vector2 pos, bool stop)
	{
        moveInterpolator_.PushNextMovement(time, pos, stop);
	}

    public void ApplyStats(proto_game.PlayerStats stats)
    {
        foreach(var s in stats.stats)
        {
            stats_.Get(s.stat).SetStep(s.step).SetValue(s.value);
        }
    }
}
