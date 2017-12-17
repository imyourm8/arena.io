using System;
using proto_game;

namespace LobbyServer.matchmaking
{
    using load_balancing;

    class GameSession : IDisposable
    {
        #region Constructors

		public GameSession(GameMode mode, string id, int maxPlayers, int minPlayers, GameNode node)
        {
            Mode = mode;
            MaxPlayersAllowed = maxPlayers;
            MinPlayersToStart = minPlayers;
            Id = id;
            Node = node;
        } 

	    #endregion

        #region Properties

        public string Id { get; private set; }
        public GameMode Mode { get; private set; }
        public int PlayersConnected { get; set; }
        public int MaxPlayersAllowed { get; private set; }
        public int MinPlayersToStart { get; private set; }
        public bool IsFull { get { return PlayersConnected >= MaxPlayersAllowed; } }
        public GameNode Node { get; private set; }

        #endregion

        public void Dispose()
        {
            Node = null;
            Id = null;
        }
    }
}
