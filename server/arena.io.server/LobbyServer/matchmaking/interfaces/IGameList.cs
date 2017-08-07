using proto_game;

namespace LobbyServer.matchmaking.interfaces
{
    interface IGameList
    {
        void AddGame(GameSession session);
        void RemoveGame(GameSession session);

        /// <summary>
        /// Get most suitable game 
        /// </summary>
        /// <returns>Game session. Can be null.</returns>
        GameSession GetSuitableGame(GameMode mode);
        GameSession FindGame(string id);
        bool HasAnyNonEmptyGame(GameMode mode);
    }
}
