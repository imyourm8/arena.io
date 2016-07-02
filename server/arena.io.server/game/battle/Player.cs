using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class Player : Entity, PlayerExperience.IExpProvider
    {
        private HashSet<Entity> visibleEntities_ = new HashSet<Entity>();
        private PlayerExperience exp_;

        public Player(player.PlayerController controller)
        {
            Controller = controller;

            Level = 1;
            exp_ = new PlayerExperience(this);
        }

        public string UniqueID
        { get; set; }

        public int Level
        { get; set; }

        public string Name
        { get; set; }

        public Room Room
        { get; set; }

        public Game Game
        { get; set; }

        public player.PlayerController Controller
        { get; private set; }

        public proto_game.PlayerAppeared GetAppearedPacket()
        {
            var appearData = new proto_game.PlayerAppeared();

            appearData.name = Name;
            appearData.guid = ID;
            appearData.hp = HP;
            appearData.stats = GetStatsPacket();
            appearData.position = new proto_game.Vector();
            appearData.position.x = X;
            appearData.position.y = Y;

            return appearData;
        }

        public proto_game.PlayerDisconnected GetDisconnectedPacket()
        {
            var disconnectPacket = new proto_game.PlayerDisconnected();
            disconnectPacket.who = ID;
            return disconnectPacket;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            HP = Math.Min(HP + Stats.GetFinValue(proto_game.Stats.HealthRegen) * dt, Stats.GetFinValue(proto_game.Stats.MaxHealth));
        }

        public proto_game.PlayerStatusChange GetStatusChanged()
        {
            proto_game.PlayerStatusChange statusPacket = new proto_game.PlayerStatusChange();
            statusPacket.guid = ID;
            statusPacket.hp = HP;
            return statusPacket;
        }

        public void AddExperience(int value)
        {
            exp_.AddExperience(value);
        }
    }
}
