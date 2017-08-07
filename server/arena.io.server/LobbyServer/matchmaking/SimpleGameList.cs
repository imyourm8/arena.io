using System;
using System.Collections.Generic;

using proto_game;

namespace LobbyServer.matchmaking
{
    using interfaces;
    using GameList = List<GameSession>;

    class SimpleGameList : IGameList
    {
        #region Fields

        private Dictionary<GameMode, GameList> games_ = new Dictionary<GameMode, GameList>();
        private Dictionary<string, GameSession> gamesById_ = new Dictionary<string, GameSession>();

        #endregion

        #region Public Methods

        public void AddGame(GameSession session)
        {
            var list = GetList(session.Mode);
            list.Add(session);
            gamesById_.Add(session.Id, session);
        }

        public void RemoveGame(GameSession session)
        {
            var list = GetList(session.Mode);
            list.Remove(session);
            gamesById_.Remove(session.Id);
        }

        public void GetSuitableGame(GameMode mode, IGameFinderResponder responder)
        {
            var list = GetList(mode);
            list.Sort(SortGameSessions);
            if (list.Count > 0)
            {
                // if there are non full game, return it
                foreach (var game in list)
                {
                    if (game.IsFull)
                    {
                        responder.OnGameFound(game);
                        break;
                    }
                }
            }
            else
            {
                responder.OnNoGameFound();
            }
        }

        public GameSession FindGame(string id)
        {
            GameSession game;
            gamesById_.TryGetValue(id, out game);
            return game;
        }

        public bool HasAnyNonEmptyGame(GameMode mode)
        {
            var list = GetList(mode);
            foreach (var game in list)
            {
                if (!game.IsFull)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Private Methods

        private int SortGameSessions(GameSession game1, GameSession game2)
        {
            return game1.PlayersConnected.CompareTo(game2.PlayersConnected);
        }

        private GameList GetList(GameMode mode)
        {
            GameList list = null;
            if (!games_.TryGetValue(mode, out list))
            {
                list = new GameList(30);
                games_.Add(mode, list);
            }
            return list;
        }

        #endregion
    }
}
