using UnityEngine;
using System.Collections;

public class GunnerSkill : Skill 
{
    [SerializeField]
    private GameObject core = null;

    [SerializeField]
    private float coreSpeed = 0.0f;

    [SerializeField]
    private float recoil = 0.0f;

    protected override void OnCast()
    {
        SkillBullet bullet = core.GetPooled().GetComponent<SkillBullet>();

        var ownerAttRot = Owner.AttackDirection;
        var ownerAttPosition = Owner.AttackPosition;
        bullet.Init(
            Owner, 
                new Vector3(ownerAttRot.x, ownerAttRot.y, 0), 
                coreSpeed, 
                ownerAttPosition, 
                Owner.Stats.GetFinValue(proto_game.Stats.SkillDamage)
            );

        Owner.ApplyRecoil(recoil);
        Physics2D.IgnoreCollision(Owner.Collider, bullet.Collider);
    }
}
