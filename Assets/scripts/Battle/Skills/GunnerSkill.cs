using UnityEngine;
using System.Collections;

public class GunnerSkill : Skill 
{
    [SerializeField]
    private GameObject core = null;

    protected override void OnCast()
    {
        SkillBullet bullet = core.GetPooled().GetComponent<SkillBullet>();

        var ownerAttRot = Owner.AttackDirection;
        var ownerAttPosition = Owner.AttackPosition;
        bullet.Init(Owner.Controller);
        bullet.SetOwner(Owner);
        bullet.SetMoveSpeed(bullet.MoveSpeed);
        bullet.SetDirection(new Vector3(ownerAttRot.x, ownerAttRot.y, 0));
        bullet.Position = ownerAttPosition;
        bullet.SetDamage(Owner.Stats.GetFinValue(proto_game.Stats.SkillDamage));
        bullet.Prepare();
        Owner.ApplyRecoil(recoil);

        if (Owner.Controller.IsLocalPlayer(Owner))
        {
            (Owner as Player).OnBulletShoot(bullet);
        }
    }
}
