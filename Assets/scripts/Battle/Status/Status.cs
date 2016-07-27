using UnityEngine;
using System.Collections;

public class Status
{
    public proto_game.PowerUpType Type
    { get; private set; }

    public Entity Owner
    { get; set; }

    private float startTime_ = 0.0f;
    private float removeAfter_ = 0.0f;
    private GameObject statusObject_ = null;

    public Status(proto_game.PowerUpType t, float removeAfter)
    {
        startTime_ = Time.time;
        Type = t;
        removeAfter_ = removeAfter;
    }

    public void Apply()
    {
        ApplyOrRemove(true);
    }

    public void Remove()
    {
        ApplyOrRemove(false);
    }

    private void ApplyOrRemove(bool apply)
    {
        switch(Type)
        {
            case proto_game.PowerUpType.DoubleDamage:
                HandleQuadDamage(apply);
                break;
            case proto_game.PowerUpType.HugeBullets:
                HandleHugeBullets(apply);
                break;
        }
    }

    public bool Update()
    {
        return Time.time - startTime_ >= removeAfter_;
    }

    private void HandleHugeBullets(bool apply)
    {
        var stat = Owner.Stats.Get(proto_game.Stats.BulletSize);
        if (apply)
        {
            stat.SetMultiplier(2.5f);

            var statusPrefab = StatusPrefabs.Instance.GetPowerUp(Type);
            if (statusPrefab != null)
            {
                statusObject_ = statusPrefab.GetPooled();
                Owner.gameObject.AddChild(statusObject_);
            }
        }
        else 
        {
            stat.SetMultiplier(0.0f);
            if (statusObject_ != null)
            {
                statusObject_.ReturnPooled();
            }
        }
    }

    private void HandleQuadDamage(bool apply)
    {
        var stat = Owner.Stats.Get(proto_game.Stats.BulletDamage);
        if (apply)
        {
            stat.SetMultiplier(2.0f);

            var statusPrefab = StatusPrefabs.Instance.GetPowerUp(Type);
            if (statusPrefab != null)
            {
                statusObject_ = statusPrefab.GetPooled();
                Owner.gameObject.AddChild(statusObject_);
            }
        }
        else 
        {
            stat.SetMultiplier(0.0f);
            if (statusObject_ != null)
            {
                statusObject_.ReturnPooled();
            }
        }
    }
}
