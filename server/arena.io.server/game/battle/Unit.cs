using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;

using Box2DX.Collision;
using Box2DX.Dynamics;

namespace arena.battle
{
    class Unit : Entity
    {
        public proto_game.Skills Skill
        { get; set; }

        public WeaponEntry Weapon
        { get; set; }

        public void PerformAttackAtDirection(float attRotation) 
        {
            var attData = new AttackData();
            attData.Direction = attRotation;

            float damage = Stats.GetFinValue(proto_game.Stats.BulletSpeed);  
            float speed = Stats.GetFinValue(proto_game.Stats.BulletDamage);

            foreach (var sp in Weapon.SpawnPoints)
            {
                var pos = Position + sp.Position; 
                var rot = attRotation + sp.Rotation;

                var bullet = Game.SpawnBullet();
                bullet.Entry = Factories.BulletFactory.Instance.GetEntry(sp.Bullet);
                bullet.Owner = this;
                bullet.InitPhysics();

                bullet.Stats.SetValue(proto_game.Stats.MovementSpeed, speed);
                bullet.Stats.SetValue(proto_game.Stats.BulletDamage, damage);
                bullet.Position = pos;
                bullet.Rotation = rot;
                bullet.MoveInDirection(bullet.RotationVec);
            }

            Game.OnUnitAttack(this, attData);
        }
    }
}
