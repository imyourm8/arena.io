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
    private float moveSpeed = -1.0f;

    [SerializeField]
    private float timeAlive = 1.4f;

    [SerializeField]
    protected bool penetrate = false;

    protected Entity owner_;
    private float speed_;
    private Vector3 direction_;
    private int collsionCount_ = 0;
    private float damage_ = 0;
    private Transform transform_;
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

    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    #if UNITY_EDITOR
    public proto_game.Bullets Type
    {
        get { return bulletType; }
    }

    public float TimeAlive
    { get { return timeAlive; } }
    #endif

    public bool Penetrate
    { get { return penetrate; } }

    public void Init(Entity owner, Vector3 direction, float speed, Vector3 spawnPoint, float damage, float alpha = -1)
    {
        destroyed_ = false;
        owner_ = owner;
        transform_ = transform;
        oldSize_ = transform.localScale;
        var newSize = oldSize_;
        var scale = owner_.Stats.GetFinValue(proto_game.Stats.BulletSize);
        newSize.Scale(new Vector3(scale,scale,scale));
        transform.localScale = newSize;

        direction_ = direction;
        direction_.Normalize();

        speed_ = speed;
        collsionCount_ = 0;
        damage_ = damage;

        var maxDistanceToTravel = timeAlive * speed_;
        maxDistanceToTravel = owner_.AttackRange > 0.0f ? owner_.AttackRange : maxDistanceToTravel;
        var move = direction_;
        move.x *= maxDistanceToTravel;
        move.y *= maxDistanceToTravel;

        var correctedLifeTime = timeAlive;
        if (alpha > 0)
        {
            //adjust bullet to server time spawn, move slightly forward, since we predict player's movement
            //remove difference in time from total bullet lifetime
            correctedLifeTime -= alpha;
            //advance bullet's spawn point by alpha
            var advance = direction_;
            advance.x *= alpha;
            advance.y *= alpha;
            spawnPoint += advance;
            move -= advance;
        }

        transform_.localPosition = spawnPoint;

        //if lifetime enough spawn bullet otherwise just remove it
        if (correctedLifeTime > 0.01f)
        {
            moveTween_ = transform_.DOLocalMove(move+spawnPoint, correctedLifeTime);
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
        else 
        {
            RemoveFromBattle();
        }
    }

    protected virtual void OnInit()
    {
        penetrate = false;
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
            var enemyMask = LayerMask.NameToLayer("Enemy");
            if ((owner_.gameObject.layer & enemyMask) != 0 &&
                (entity.gameObject.layer & enemyMask) == 0)
            {
                collsionCount_++;
                HandleCollision(entity);
            }
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
        //safe to use owner here, in case owner already pooled back
        if (rm)
            owner_.Controller.ReturnBullet(this);

        gameObject.ReturnPooled();
        transform_.localScale = oldSize_;
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
    }
}
