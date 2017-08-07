using System;
using System.Collections.Generic;

using proto_server;
using proto_game;

namespace LobbyServer.matchmaking
{
    

    class RemoteGameManager
    {
        #region Fields

        private LobbyApplication application_;
        private Dictionary<GameMode, GameList> games_ = new Dictionary<GameMode, GameList>();

        #endregion

        #region Constructors

        public RemoteGameManager(LobbyApplication app)
        {
            application_ = app;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Find free game or create a new one
        /// </summary>
        /// <returns>Instance of currently running game</returns>
        public GameSession GetSuitableGame(proto_game.GameMode modeID)
        {
            GameSession suitableGame = null;
            lock (games_)
            {
                GameList gameList = null;
                if (games_.TryGetValue(modeID, out gameList) && gameList.Count > 0)
                {

                }
            }
            return suitableGame;
        } 

        #endregion
    }
}
