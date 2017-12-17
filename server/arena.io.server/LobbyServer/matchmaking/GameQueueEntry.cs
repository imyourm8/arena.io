using System;

using shared.account;

namespace LobbyServer.matchmaking
{
    struct GameQueueEntry
    {
        public GameQueueEntry(Action<GameSession, string> cb, Profile profile, string ip)
        {
            Callback = cb;
            playerProfile = profile;
            Ip = ip;
        }

        public Action<GameSession, string> Callback;
        public Profile playerProfile;
        public string Ip;
    }
}
