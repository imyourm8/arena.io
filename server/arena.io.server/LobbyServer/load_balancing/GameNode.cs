using System;
using System.Collections.Generic;

namespace LobbyServer.load_balancing
{
    using controller;
    using matchmaking.interfaces;

    class GameNode
    {
        public GameNode(string id, string ip, IGameList gameList)
        {
            Id = id;
            Ip = ip;
            GameList = gameList;
        }

        #region Properties

        public int PlayersConnected { get; set; }
        public proto_server.FeedbackLevel FeedbackLevel { get; set; }

        public string Id { get; private set; }
        public string Ip { get; private set; }

        public GameNodeController Controller { get; private set; }
        public IGameList GameList { get; private set; }

        public bool IsJoinable { get { return FeedbackLevel != proto_server.FeedbackLevel.Highest; } }

        #endregion

        #region Public Methods

        public void UpdateStatus(proto_server.GameNodeStatus status)
        {
            FeedbackLevel = status.workload_level;
            PlayersConnected = status.players_connected;
        }

        public void UpdateGameStatus(string gameId, int playersConnected)
        {
            var game = GameList.FindGame(gameId);
            game.PlayersConnected = playersConnected;
        }

        public void CreateGame(IGameFinderResponder responder)
        {
            Controller.CreateGame(responder);
        }

        #endregion
    }
}
