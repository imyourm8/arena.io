using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared;
using arena.battle;

namespace arena.matchmaking
{
    class RoomManager : Singleton<RoomManager>
    {
        private const int MaxPlayersPerRoom = 10;

        private List<Room> rooms_ = new List<Room>();
        private Room debugRoom_;

        public void AssignPlayerToRandomRoom(Player player)
        {
            Room room = null;

            foreach (var r in rooms_)
            {
                if (!r.Closed && r.PlayersCount < MaxPlayersPerRoom)
                {
                    room = r;
                    break;
                }
            }

            if (room == null)
            {
                room = CreateRoom(); 
                rooms_.Add(room);
            }

            room.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            if (player.Room != null)
            {
                player.Room.Remove(player);
            }
        }

        public void CreateDebugRoom()
        {
            debugRoom_ = CreateRoom();
        }

        private Room CreateRoom()
        {
            return new Room(); 
        }
    }
}
