using UnityEngine;
using System.Collections;

class Status : arena.common.battle.Status
{
    private GameObject statusObject_ = null;

    public Status(proto_game.PowerUpType t, float removeAfter)
    :base(t, removeAfter)
    {
        
    }

    protected override void HandleHugeBulletsCustom(bool apply)
    {
        if (apply)
        {
            var statusPrefab = StatusPrefabs.Instance.GetPowerUp(Type);
            if (statusPrefab != null)
            {
                statusObject_ = statusPrefab.GetPooled();
                Owner.gameObject.AddChild(statusObject_);
            }
        }
        else 
        {
            if (statusObject_ != null)
            {
                statusObject_.ReturnPooled();
            }
        }
    }

    protected override void HandleQuadDamageCustom(bool apply)
    {
        if (apply)
        {
            var statusPrefab = StatusPrefabs.Instance.GetPowerUp(Type);
            if (statusPrefab != null)
            {
                statusObject_ = statusPrefab.GetPooled();
                Owner.gameObject.AddChild(statusObject_);
            }
        }
        else
        {
            if (statusObject_ != null)
            {
                statusObject_.ReturnPooled();
            }
        }
    }
}
