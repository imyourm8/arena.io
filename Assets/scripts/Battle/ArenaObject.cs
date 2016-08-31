using UnityEngine;
using System.Collections;

using DG.Tweening;

public class ArenaObject : MonoBehaviour 
{
    [SerializeField]
    protected Rigidbody2D body_ = null;

    [SerializeField]
    protected new Collider2D collider = null;

    [SerializeField]
    protected Transform transform_ = null;

    [SerializeField]
    protected SpriteRenderer sprite = null;

    protected arena.ArenaController controller_;
    private bool destroyed_ = false;
    protected Color initialColor_;
    protected Vector3 initialScale_;
    private bool firstInit_ = true;

    protected bool Destroyed
    {
        get { return destroyed_; }   
    }

    public arena.ArenaController Controller
    { 
        get { return controller_; } 
    }

    public Rigidbody2D Body
    {
        get { return body_; }
    }

    public Collider2D Collider
    { 
        get { return collider; } 
    }

    protected bool DisableVelocityMovement
    { get; set; }

    public bool Local
    { 
        get; set;
    }

    public int ID
    { get; set; }

    public Vector2 Position
    {
        get 
        { 
            return new Vector2(transform_.localPosition.x, transform_.localPosition.y); 
        }

        set 
        { 
            transform_.localPosition = new Vector3(value.x, value.y, 0);
            OnPositionSet();
        }
    }

    protected virtual void OnPositionSet()
    {}

    public Vector2 Velocity
    { get; set; }

    public virtual void Init(arena.ArenaController controller)
    {
        Collider.enabled = true;
        destroyed_ = false;
        DisableVelocityMovement = false;
        controller_ = controller;

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
    }

    public virtual void OnUpdate(float dt)
    {
        if (!DisableVelocityMovement)
        {
            var newPosition = Position;
            newPosition.x += Velocity.x * dt;
            newPosition.y += Velocity.y * dt;
            Position = newPosition;
        }
    }

    public void Remove(bool animated)
    {   
        if (destroyed_) 
            return;

        destroyed_ = true;
        OnBeforeRemove();

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
                HandleRemove();
            });
        } 
        else
        {
            HandleRemove();
        }
    }

    private void HandleRemove()
    {
        OnRemove();
    }

    protected virtual void OnBeforeRemove()
    {
        Collider.enabled = false;
    }

    protected virtual void OnRemove()
    {
        gameObject.ReturnPooled();

        transform_.localScale = initialScale_;
        sprite.color = initialColor_;
    }
}
