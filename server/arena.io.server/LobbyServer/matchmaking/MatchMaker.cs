using System;
using System.Collections.Generic;

using shared;
using shared.account;
using proto_game;

namespace LobbyServer.matchmaking
{
    using interfaces;
    using load_balancing;

    /// <summary>
    /// Assign players into running games
    /// </summary>
    class MatchMaker
    {
        #region Fields

        public delegate void OnGameFound(GameSession game);

        private LobbyApplication application_;

        #endregion

        #region Constructors

        public MatchMaker(LobbyApplication app)
        {
            application_ = app;
        }

        #endregion

        #region Public Methods

        public void FindGame(GameMode mode, OnGameFound onGameFound)
        {
            GameNode node = application_.Loadbalancer.GetBestNode(mode);
            node.GameList.GetSuitableGame(mode);
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
