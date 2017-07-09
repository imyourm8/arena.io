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
            var bullet = Owner.Game.SpawnBullet(proto_game.Bullets.CanonCore, Owner);
            Owner.Game.OnSkillCast(Owner, bullet.ID);
            Owner.RegisterBullet(bullet);
        }
    }
}
