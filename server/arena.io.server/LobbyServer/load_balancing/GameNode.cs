using System;
using System.Collections.Generic;

namespace LobbyServer.load_balancing
{
    using controller;

    class GameNode
    {
        public GameNode(string id, string ip)
        {
            Id = id;
            Ip = ip;
        }

        #region Properties

        public int GameSessions { get; set; }
        public int PlayersConnected { get; set; }
        public proto_server.FeedbackLevel FeedbackLevel { get; set; }

        public string Id { get; private set; }
        public string Ip { get; private set; }

        public GameNodeController Controller { get; private set; }

        #endregion

        #region Public Methods

        public void UpdateStatus(proto_server.GameNodeStatus status)
        {
            FeedbackLevel = status.workload_level;
            GameSessions = status.active_games;
            PlayersConnected = status.players_connected;
        }

        #endregion
    }
}
