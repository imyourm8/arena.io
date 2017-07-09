using arena.battle;

namespace arena.battle.Logic
{
    interface IOnDestroyResponser
    {
        void WasDestroyed(ExpBlock expBlock);
        void WasDestroyed(Mob mob);
        void WasDestroyed(Entity entity);
        void WasDestroyed(Player player);
    }
}
