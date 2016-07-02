using System;
using System.Collections.Generic;

namespace TapCommon
{
    public class CooldownManager<KeyType>
    {
        private Dictionary<KeyType, long> cooldowns_ = new Dictionary<KeyType, long>();
        private Func<long> time_;

        public CooldownManager(Func<long> timeFunc)
        {
            time_ = timeFunc;
        }

        public void SetCooldown(KeyType key, long cooldown)
        {
            long cd = time_() + cooldown;
            if (cooldowns_.ContainsKey(key))
            {
                cooldowns_[key] = cd;
            }
            else
            {
                cooldowns_.Add(key, cd);
            }
        }

        public bool HasCooldown(KeyType key)
        {
            return cooldowns_.ContainsKey(key) && cooldowns_[key] - time_() > 0;
        }

        public long GetCooldown(KeyType key)
        {
            if (!cooldowns_.ContainsKey(key))
            {
                return 0;
            }
            return Math.Max(cooldowns_[key] - time_(), 0);
        }

        public void Remove(KeyType key)
        {
            cooldowns_.Remove(key);
        }

        public void Reset()
        {
            cooldowns_.Clear();
        }
    }
}
