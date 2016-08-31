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
        private float weaponCooldown_;

        public WeaponEntry Weapon
        { get; set; }

        public Skills.Skill Skill
        { get; set; }

        public Vector2 RecoilVelocity
        { get; set; }

        public void PerformAttackAtDirection(float attRotation) 
        {
            if (!IsWeaponAttackAvailable())
                return;

            var attData = new AttackData();
            attData.Direction = attRotation;
            attData.FirstBulletID = Game.GetCurrentEntityID();

            foreach (var sp in Weapon.SpawnPoints) 
            {
                Game.GenerateID();
            }
            
            double angleInRadians = attRotation;
            /*
            float cosTheta = (float)Math.Cos(angleInRadians);
            float sinTheta = (float)Math.Sin(angleInRadians);

            float damage = Stats.GetFinValue(proto_game.Stats.BulletDamage);  
            float speed = Stats.GetFinValue(proto_game.Stats.BulletSpeed);

            foreach (var sp in Weapon.SpawnPoints)
            {
                var pos = sp.Position; 
                //rotate point 
                var rotatedPoint = new Vector2(
                        cosTheta * pos.x - sinTheta * pos.y,
                        sinTheta * pos.x + cosTheta * pos.y
                    );
                var rot = attRotation + sp.Rotation;
                pos = Position + rotatedPoint;
                var bullet = Game.SpawnBullet(sp.Bullet, this);
                bullet.Stats.SetValue(proto_game.Stats.MovementSpeed, speed);
                bullet.Stats.SetValue(proto_game.Stats.BulletDamage, damage); 
                bullet.Position = pos;
                bullet.Rotation = rot;
                bullet.MoveInDirection(bullet.RotationVec);
            }
            */
            ApplyRecoil(Weapon.Recoil, (float)angleInRadians);
            Game.OnUnitAttack(this, attData);
            SetWeaponCooldown();
            OnWeaponAttack(attData);
        }

        protected virtual void OnWeaponAttack(AttackData attData)
        { }

        protected void AddSkill(proto_game.Skills skill)
        {
            Skill = Skills.Skill.Create(skill);
            Skill.Owner = this;
        }

        private void SetWeaponCooldown()
        {
            weaponCooldown_ = 0.0f;
        }

        public bool IsWeaponAttackAvailable()
        {
            return weaponCooldown_ >= Stats.GetFinValue(proto_game.Stats.ReloadSpeed);
        }

        public void CastSkill()
        {
            if (Skill != null && Skill.Cast())
            {
                Game.OnSkillCast(this);
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            Body.SetLinearVelocity(Vector2.zero); 
        }

        public void ApplyRecoil(float recoil, float attRotation)
        {
            return;
            if (Body != null)
            {
                recoil *= -1.0f; 
                float x = (float)Math.Cos(attRotation) * recoil;
                float y = (float)Math.Sin(attRotation) * recoil;
                RecoilVelocity = new Vector2(x, y);
            }
        }

        public override void InitPhysics(bool dynamicBody = true, bool isSensor = false)
        {
            base.InitPhysics(dynamicBody, isSensor);
            AddToCollisionMask((ushort)PhysicsDefs.Category.BULLET);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            
            weaponCooldown_ += dt;
        }
    }
}
