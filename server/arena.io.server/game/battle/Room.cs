using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class Room
    {
        private List<Player> players_ = new List<Player>(); 
        private Game game_;
        private bool closed_ = false; 

        public Room() 
        {
            game_ = new Game(new GameModes.FFA(), this);  
        }

        public bool Closed { get { return closed_; } }

        public int PlayersCount
        {
            get { return players_.Count; }
        }

        public void Add(Player player)
        {
            players_.Add(player);
            player.Room = this;
            game_.Add(player);
        }

        public void Remove(Player player)
        {
            players_.Remove(player);
            game_.Remove(player);
            player.Room = null;
        }

        public void OnGameFinished()
        {
            closed_ = true;
            game_.Close();

            foreach(var player in players_)
            {
                player.Controller.OnGameFinished();
            }
        }
    }
}
