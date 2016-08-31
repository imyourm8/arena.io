using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Skills
{
    class GunnerSkill : Skill
    {
        protected override void OnCast()
        {
            base.OnCast();
            /*
            var bullet = Owner.Game.SpawnBullet(proto_game.Bullets.CanonCore, Owner);
            bullet.Stats.SetValue(proto_game.Stats.BulletDamage, Owner.Stats.GetFinValue(proto_game.Stats.SkillDamage));
            bullet.Position = Owner.Position;
            bullet.Rotation = Owner.Rotation;
            bullet.MoveInDirection(bullet.RotationVec);
             * */
            Owner.ApplyRecoil(Owner.Skill.Entry.Recoil, Owner.Rotation);
        }
    }
}
