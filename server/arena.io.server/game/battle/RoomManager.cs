using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class RoomManager : TapCommon.Singleton<RoomManager>
    {
        private const int MaxPlayersPerRoom = 10;

        private List<Room> rooms_ = new List<Room>();

        public void AssignPlayerToRandomRoom(Player player)
        {
            if (rooms_.Count == 0)
            {
                rooms_.Add(CreateRoom());
            }

            foreach (var room in rooms_)
            {
                if (room.PlayersCount < MaxPlayersPerRoom)
                {
                    room.Add(player);
                }
            }
        }

        public void RemovePlayer(Player player)
        {
            if (player.Room != null)
            {
                player.Room.Remove(player);
            }
        }

        private Room CreateRoom()
        {
            return new Room();
        }
    }
}
