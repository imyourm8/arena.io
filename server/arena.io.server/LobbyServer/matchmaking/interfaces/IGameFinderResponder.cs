using proto_server;

namespace LobbyServer.matchmaking.interfaces
{
    interface IGameFinderResponder
    {
        void OnGameFound(GameSession game);
        void OnNoGameFound();
    }
}
