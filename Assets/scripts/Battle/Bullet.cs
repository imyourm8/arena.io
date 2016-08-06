using UnityEngine;

using System;
using System.Collections;

using DG.Tweening;

public class Bullet : MonoBehaviour 
{
    [SerializeField]
    private new Collider2D collider = null;

    [SerializeField]
    private SpriteRenderer sprite = null;

    [SerializeField]
    private proto_game.Bullets bulletType = proto_game.Bullets.Common;

    [SerializeField]
    private float maxDistance = 0.0f;

    [SerializeField]
    private float moveSpeed = -1.0f;

    protected Entity owner_;
    private float speed_;
    private Vector3 direction_;
    private int collsionCount_ = 0;
    private float damage_ = 0;
    private Transform transform_;
    private const float timeAlive = 1.4f;
    private Tweener moveTween_;
    private Tweener fadeTweener_;
    private Vector3 oldSize_;
    private bool destroyed_ = false;

    public Collider2D Collider
    { get { return collider; } }

    public float Radius
    {
        get { return (collider as CircleCollider2D).radius; }
    }
    #if UNITY_EDITOR
    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    public proto_game.Bullets Type
    {
        get { return bulletType; }
    }
    #endif
    public float MaxDistance
    {
        get { return maxDistance; } 
    }

    public bool Penetrate
    { get; set; }

    public void Init(Entity owner, Vector3 direction, float speed, Vector3 spawnPoint, float damage)
    {
        destroyed_ = false;
        owner_ = owner;
        transform_ = transform;
        oldSize_ = transform.localScale;
        var newSize = oldSize_;
        var scale = owner_.Stats.GetFinValue(proto_game.Stats.BulletSize);
        newSize.Scale(new Vector3(scale,scale,scale));
        transform.localScale = newSize;
        transform_.localPosition = spawnPoint;
        direction_ = direction;
        direction_.Normalize();

        speed_ = speed;
        collsionCount_ = 0;
        damage_ = damage;

        var maxDistanceToTravel = maxDistance > 0.0f ? maxDistance : timeAlive * speed_;
        maxDistanceToTravel = owner_.AttackRange > 0.0f ? owner_.AttackRange : maxDistanceToTravel;
        var move = direction_;
        move.x *= maxDistanceToTravel;
        move.y *= maxDistanceToTravel;

        moveTween_ = transform_.DOLocalMove(move+spawnPoint, timeAlive);
        moveTween_.SetEase(Ease.InSine);
        moveTween_.OnComplete(()=>
        {
            moveTween_ = null;
            HandleDestroyBullet();
        });

        var color = sprite.color;
        color.a = 1.0f;
        sprite.color = color;

        OnInit();
    }

    protected virtual void OnInit()
    {
        Penetrate = false;
    }

    protected virtual void HandleCollision(Entity entity)
    {
        if (!Penetrate)
            HandleDestroyBullet();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed_) return;
        if (other.gameObject == owner_.gameObject) return;

        Entity entity = other.gameObject.GetComponent<Entity>();

        if (entity != null)
        {
            collsionCount_++;

            HandleCollision(entity);
        }
    }

    public void RemoveFromBattle(bool rm = true)
    {
        destroyed_ = true;
        if (moveTween_ != null)
        {
            moveTween_.Kill(false);
            moveTween_ = null;
        }
        if (fadeTweener_ == null)
        {
            fadeTweener_.Kill(false);
            fadeTweener_ = null;
        }
        //safe to use owner here
        if (rm)
            owner_.Controller.ReturnBullet(this);

        gameObject.ReturnPooled();
    }

    void HandleDestroyBullet()
    {
        if (destroyed_) return;
        destroyed_ = true;
        fadeTweener_ = sprite.DOFade(0.0f, 0.1f).OnComplete(()=>
        {   
            fadeTweener_ = null;
            RemoveFromBattle();
        });
        transform_.localScale = oldSize_;
    }
}
