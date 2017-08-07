using System;
using proto_game;

namespace LobbyServer.matchmaking
{

    class GameSession
    {
        #region Constructors

		public GameSession(GameMode mode, string id, int maxPlayers)
        {
            Mode = mode;
            MaxPlayersAllowed = maxPlayers;
            Id = id;
        } 

	    #endregion

        #region Properties

        public string Id { get; private set; }
        public GameMode Mode { get; private set; }
        public int PlayersConnected { get; set; }
        public int MaxPlayersAllowed { get; private set; }
        public bool IsFull { get { return PlayersConnected >= MaxPlayersAllowed; } }

        #endregion
    }
}
