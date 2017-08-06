using System;
using System.Collections.Generic;

namespace LobbyServer.load_balancing
{
    internal class Loadbalancer
    {
        #region Fields

        private Dictionary<string, GameNode> servers_ = new Dictionary<string, GameNode>();
        private List<GameNode> flatList_ = new List<GameNode>();

        #endregion

        #region Public Methods

        public void AddGameNode(GameNode node)
        {
            lock (servers_)
            {
                GameNode possiblyExistentNode;
                if (servers_.TryGetValue(node.Id, out possiblyExistentNode))
                {
                    // smth is wrong, disconnect what just connected 
                    node.Controller.Disconnect();
                }
                else
                {
                    servers_.Add(node.Id, node);
                    flatList_.Add(node);
                }
            }
        }

        public void RemoveGameNode(GameNode node)
        {
            lock (servers_)
            {
                node.Controller.Disconnect();
                servers_.Remove(node.Id);
                flatList_.Remove(node);
            }
        }

        public void UpdateStatus(string nodeId, proto_server.GameNodeStatus status)
        {
            lock (servers_)
            {
                GameNode node;
                if (servers_.TryGetValue(nodeId, out node))
                {
                    node.FeedbackLevel = status.workload_level;
                    node.GameSessions = status.active_games;
                    node.PlayersConnected = status.players_connected;

                    flatList_.Sort((GameNode n1, GameNode n2) => 
                    { 
                        int lvl1 = (int)n1.FeedbackLevel;
                        int lvl2 = (int)n2.FeedbackLevel;
                        return lvl1.CompareTo(lvl2);
                    });
                }
            }
        }

        /// <summary>
        /// Find most suitable game node
        /// </summary>
        /// <returns>Returns Ip of Game Server</returns>
        public string GetFreeNodeIp()
        {
            string ip = null;

            lock (servers_)
            {
                if (flatList_.Count > 0)
                {
                    ip = flatList_[0].Ip;
                }
            }

            return ip;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
