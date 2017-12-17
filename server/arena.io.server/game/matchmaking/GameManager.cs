using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using shared;
using arena.battle;
using arena.battle.modes;

namespace arena.matchmaking
{
    class GameManager
    {
        private List<Game> games_;
        private int activeGames_;
        private GameApplication app_;

        #region Constructors

        public GameManager(GameApplication app)
        {
            app_ = app;
            games_ = new List<Game>();
            activeGames_ = 0;
        }

        #endregion

        #region Getters & Setters

        public int ActiveGames { get { return activeGames_; } }

        #endregion

        #region Public Methods
        /// <summary>
        /// Returns current running game.
        /// Creates new List 
        /// </summary>
        /// <returns>Game list</returns>
        public List<Game> GetGameList(bool onlyOpened = false)
        {
            List<Game> gameList;
            lock (games_)
            {
                gameList = new List<Game>(games_.Count);
                foreach (var game in games_)
                {
                    if (onlyOpened && game.PlayersCanJoin || !onlyOpened)
                    { 
                        gameList.Add(game);
                    }
                }
            }
            return gameList;
        }

        public Game CreateGame(proto_game.GameMode mode)
        {
            Game game = new Game(mode);
            return game;
        }

        #endregion

        #region Private Methods

        private void HandleGameClosed(Game game)
        {
            Interlocked.Decrement(ref activeGames_);
            app_.Controller.OnGameFinished(game);
        }

        #endregion
    }
}
