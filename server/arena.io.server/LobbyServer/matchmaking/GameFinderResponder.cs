using System;
using System.Collections.Generic;

using proto_game;

namespace LobbyServer.matchmaking
{
    using load_balancing;
    using interfaces;

    class GameFinderResponder : IGameFinderResponder
    {
        #region Fields

        private GameNode node_;
        private MatchMaker.OnGameFound callback_;
        private GameMode gameMode_;

        #endregion

        #region Constructors

        public GameFinderResponder(GameMode mode, GameNode node, MatchMaker.OnGameFound callback)
        {
            node_ = node;
            callback_ = callback;
            gameMode_ = mode;
        }

        #endregion

        #region IGameFinderResponder implemetation

        void IGameFinderResponder.OnGameFound(GameSession game)
        {
            if (callback_ != null)
            {
                callback_(game);
            }
        }

        void IGameFinderResponder.OnNoGameFound()
        {
            node_.CreateGame(this);
        } 
        #endregion
    }
}
