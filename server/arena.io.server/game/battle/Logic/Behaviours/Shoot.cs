using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.battle.logic.behaviours
{
    class Shoot : Behaviour
    {
        public static float Predict(Entity host, Entity target)
        {
            var prevTargetPos = target.PrevPosition;
            var currentTargetPos = target.Position;
            var currentHostPos = host.Position;

            float originalAngle = (float)Math.Atan2(prevTargetPos.y - currentHostPos.y, prevTargetPos.x - currentHostPos.x);
            float newAngle = (float)Math.Atan2(currentTargetPos.y - currentHostPos.y, currentTargetPos.x - currentHostPos.x);

            float bulletSpeed = host.Stats.GetFinValue(proto_game.Stats.BulletSpeed) / (float)GlobalDefs.MainTickInterval;
            float angularVelocity = (newAngle - originalAngle) / ((float)GlobalDefs.MainTickInterval / 1000.0f);
            return angularVelocity * bulletSpeed;
        }

        class ShootState
        {
            public float cooldown;
        }

        private readonly float radius_;
        private readonly int shootCount_;
        private readonly float angleStep_;
        private readonly float predictiveCoeff_;
        private readonly float cooldownOffset_;
        private readonly float? fixedAngle_;
        private readonly float? angleOffset_;
        private readonly float cooldown_;
        private readonly float damage_;
        private readonly proto_game.Bullets bulletType_;

        public Shoot(
            float radius, 
            float cooldown, 
            int shootCount = 1, 
            float angleStep = 0,
            float predictive = 0, 
            float cooldownOffset = 0, 
            float? fixedAngle = null, 
            float damage = 0,
            proto_game.Bullets bulletType = proto_game.Bullets.MonSlowBullet,
            float? angleOffset = null)
        {
            bulletType_ = bulletType;
            radius_ = radius;
            shootCount_ = shootCount;
            angleStep_ = angleStep * MathHelper.Deg2Rad;
            predictiveCoeff_ = predictive;
            cooldownOffset_ = cooldownOffset;
            cooldown_ = cooldown;
            damage_ = damage;
            angleOffset_ = angleOffset != null ? angleOffset.Value * MathHelper.Deg2Rad : angleOffset;
            fixedAngle_ = fixedAngle * MathHelper.Deg2Rad;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            //non units have no ability to shoot anyway
            if (!(holder is Unit)) 
                return;

            var state = new ShootState();
            state.cooldown = cooldownOffset_;
            behaviorState = state;
        }

        protected override void HandleUpdate(Entity holder, float dt, ref object state)
        {
            if (state == null) 
                return;

            ShootState shootState = state as ShootState;

            if (shootState.cooldown <= 0)
            {
                shootState.cooldown = cooldown_ + cooldownOffset_;
                Entity target = holder.Game.Map
                    .GetNearestEntities(holder.Position, radius_, PhysicsDefs.Category.PLAYER).FirstOrDefault();
                if (target == null)
                    return;

                var unit = holder as Unit;
                var dir = target.Position - holder.Position;
                float startAngle = fixedAngle_ != null ? fixedAngle_.Value : (float)Math.Atan2(dir.y, dir.x);

                if (predictiveCoeff_ > 0.0f)
                {
                    startAngle += Predict(holder, target) * predictiveCoeff_;
                } 

                var offset = angleOffset_ != null ? angleOffset_.Value : 0.0f;
                startAngle = offset + startAngle - angleStep_ * (shootCount_ - 1) / 2;
                var startBulletID = holder.Game.GetCurrentEntityID();
                for (int i = 0; i < shootCount_; ++i)
                {
                    unit.RegisterBullet(holder.Game.SpawnBullet(bulletType_, unit));
                }

                var bulletDamage = damage_ > 0.0f ? damage_ : unit.Stats.GetFinValue(proto_game.Stats.BulletDamage);
                holder.Game.OnSpawnBullets(unit.ID, startAngle, angleStep_, shootCount_, unit.Position, bulletDamage, startBulletID, bulletType_);
            }
            else
            {
                shootState.cooldown -= dt;
            }
        }
    }
}
