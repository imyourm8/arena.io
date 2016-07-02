using UnityEngine;

using System;
using System.Collections;

using DG.Tweening;

public class Bullet : MonoBehaviour 
{
    private Entity owner_;
    private float speed_;
    private Vector3 direction_;
    private int collsionCount_ = 0;
    private float damage_ = 0;

    private const float MaxDistanceToTravel = 12;
    private Tweener moveTween_;

    [SerializeField]
    private Collider2D collider_ = null;

    public void Init(Entity owner, Vector3 direction, float speed, Vector3 spawnPoint, float damage)
    {
        owner_ = owner;
        transform.localPosition = spawnPoint;
        direction_ = direction;
        direction_.Normalize();

        speed_ = speed;
        collsionCount_ = 0;
        damage_ = damage;

        var move = direction_;
        move.x *= MaxDistanceToTravel;
        move.y *= MaxDistanceToTravel;

        moveTween_ = transform.DOLocalMove(move+transform.localPosition, MaxDistanceToTravel / speed);
        moveTween_.SetEase(Ease.InSine);
        moveTween_.OnComplete(()=>
        {
            HandleDestroyBullet();
        });
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner_.gameObject) return;

        Entity entity = other.gameObject.GetComponent<Entity>();

        if (entity != null)
        {
            owner_.DealDamage(entity, damage_);
            collsionCount_++;

            //if (owner_.MaxPenetration == collsionCount_)
            {
                HandleDestroyBullet();
            }
        }
    }

    void HandleDestroyBullet()
    {
        owner_.Controller.ReturnBullet(this);
        moveTween_.Kill(false);
        moveTween_ = null;
    }
}
