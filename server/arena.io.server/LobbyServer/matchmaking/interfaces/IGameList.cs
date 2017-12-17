using System.Collections.Generic;
using proto_game;

namespace LobbyServer.matchmaking.interfaces
{
    interface IGameList
    {
        void AddGame(GameSession session);
        void RemoveGame(GameSession session);
        GameSession FindGame(string id);
        bool HasAnyNonEmptyGame(GameMode mode);
        IReadOnlyList<GameSession> GetGames(GameMode mode);
        IReadOnlyList<GameSession> GetJoinableGames(GameMode mode);
    }
}
