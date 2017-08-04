using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using shared;
using arena.battle;
using arena.battle.modes;

namespace arena.serv
{
    class GameManager : Singleton<GameManager>
    {
        private List<Game> games_;
        private int activeGames_;
        private object lock_;

        #region Constructor & Destructor

        public GameManager()
        {
            games_ = new List<Game>();
            activeGames_ = 0;
        }

        #endregion

        #region Getters & Setters

        public int ActiveGames
        {
            get { return activeGames_; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Find free game or create a new one
        /// </summary>
        /// <returns>Instance of currently running game</returns>
        public Game GetSuitableGame(proto_game.GameMode modeID)
        {
            Game suitableGame = null;
            lock (lock_)
            {
                
                // if any available game
                if (games_.Count > 0 && ActiveGames > 0)
                {
                    //sort list first
                    games_.Sort((Game game1, Game game2) =>
                        {
                            return game1.PlayersConnected.CompareTo(game2.PlayersConnected);
                        });
                    suitableGame = games_[0];
                }
                else
                { 
                    // or start a new one
                    suitableGame = new Game(GameModeFactory.Instance.Get(modeID));
                    suitableGame.OnGameClosed += HandleGameClosed;
                    activeGames_++;
                }
            }
            return suitableGame;
        }

        #endregion

        #region Private Methods

        private void HandleGameClosed(Game game)
        {
            Interlocked.Decrement(ref activeGames_);
        }

        #endregion
    }
}
