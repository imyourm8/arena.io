using System.Collections;

#if !UNITY_5
using arena.battle;
#endif

namespace arena.common.battle
{
    interface IStatus
    {
        bool Update(float dt);
        void Remove();
        void Apply();
        Entity Owner { set; }
    }

    class Status : IStatus
    {
        private float timeElapsed = 0.0f;
        private float removeAfter_ = 0.0f;

        public Status(proto_game.PowerUpType t, float removeAfter)
        {
            Type = t;
            removeAfter_ = removeAfter;
        }

        public Entity Owner
        { get; set; }

        public proto_game.PowerUpType Type
        { get; private set; }

        public void Apply()
        {
            ApplyOrRemove(true);
        }

        public void Remove()
        {
            ApplyOrRemove(false);
        }

        public bool Update(float dt)
        {
            timeElapsed += dt;
            return timeElapsed >= removeAfter_;
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

        protected virtual void HandleQuadDamageCustom(bool apply)
        {}

        private void HandleQuadDamage(bool apply)
        {
            var stat = Owner.Stats.Get(proto_game.Stats.BulletDamage);
            if (apply)
            {
                stat.SetMultiplier(2.0f);
            }
            else 
            {
                stat.SetMultiplier(0.0f);
            }

            HandleQuadDamageCustom(apply);
        }

        protected virtual void HandleHugeBulletsCustom(bool apply) 
        {}

        private void HandleHugeBullets(bool apply)
        {
            var stat = Owner.Stats.Get(proto_game.Stats.BulletSize);
            if (apply)
            {
                stat.SetMultiplier(2.0f);
            }
            else 
            {
                stat.SetMultiplier(0.0f);
            }
            HandleHugeBulletsCustom(apply);
        }
    }
}