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

        private Room CreateRoom()
        {
            return new Room();
        }

        public void Debug()
        {
            var room = CreateRoom();
        }
    }
}
