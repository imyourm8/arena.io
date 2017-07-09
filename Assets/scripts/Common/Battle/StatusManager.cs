using System.Collections.Generic;

#if !UNITY_5
using arena.battle;
#endif

namespace arena.common.battle
{
    class StatusManager
    {
        private List<IStatus> statuses_ = new List<IStatus>();
        private Entity owner_;

        public StatusManager(Entity owner)
        {
            owner_ = owner;
        }

        public void Add(IStatus status)
        {
            status.Owner = owner_;
            statuses_.Add(status); 
            status.Apply();   
        }

        public void Remove(IStatus status)
        {
            statuses_.Remove(status);
            status.Remove();
        }

        public void Update(float dt)
        {
            List<IStatus> toRemove = ListPool<IStatus>.Get(5);
            foreach(var s in statuses_)
            {
                if (s.Update(dt))
                {
                    toRemove.Add(s);
                }
            }
            foreach(var r in toRemove)
            {
                r.Remove();
                statuses_.Remove(r);
            }
            ListPool<IStatus>.Release(toRemove);
        }
    }
}