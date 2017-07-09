using UnityEngine;
using System.Collections;

public class GunnerSkill : Skill 
{
    [SerializeField]
    private GameObject core = null;

    protected override void OnCast(long serverSpawnTime = -1, int firstBulletID = -1)
    {
        SkillBullet bullet = core.GetPooled().GetComponent<SkillBullet>();

        var timeElapsedSinceServerSpawn = 0.0f;

        if (serverSpawnTime > -1)
        {
            timeElapsedSinceServerSpawn = Owner.Controller.GameTime - (float)serverSpawnTime / 1000;
        }

        var alpha = Owner.Local ? 0.0f : timeElapsedSinceServerSpawn / bullet.TimeAlive;

        if (alpha < 0.9f)
        {
            var ownerAttRot = Owner.AttackDirection;
            var ownerAttPosition = Owner.AttackPosition;
            bullet.Init(Owner.Controller);
            bullet.ID = firstBulletID;
            bullet.SetOwner(Owner);
            bullet.SetMoveSpeed(bullet.MoveSpeed);
            bullet.SetDirection(new Vector3(ownerAttRot.x, ownerAttRot.y, 0));
            bullet.SetStartPoint(ownerAttPosition);
            bullet.SetDamage(Owner.Stats.GetFinValue(proto_game.Stats.SkillDamage));
            if (alpha > 0.0f)
                bullet.AdjustPositionToServerTime(alpha);
            bullet.Prepare();

            var ownerIsLocalPlayer = Owner.Controller.IsLocalPlayer(Owner);
            Owner.Controller.Add(bullet);
            Owner.RegisterBullet(bullet);
        }
    }
}
