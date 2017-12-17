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

        public bool AddGameNode(GameNode node)
        {
            GameNode possiblyExistentNode;
            if (servers_.TryGetValue(node.Id, out possiblyExistentNode))
            {
                // smth is wrong, skip this node 
                return false;
            }
            else
            {
                servers_.Add(node.Id, node);
                flatList_.Add(node);
            }
            return true;
        }

        public void RemoveGameNode(GameNode node)
        {
            servers_.Remove(node.Id);
            flatList_.Remove(node);
        }

        public void UpdateStatus(string nodeId, proto_server.GameNodeStatus status)
        {
            GameNode node;
            if (servers_.TryGetValue(nodeId, out node))
            {
                node.FeedbackLevel = status.workload_level;
                node.PlayersConnected = status.players_connected;

                flatList_.Sort((GameNode n1, GameNode n2) => 
                { 
                    int lvl1 = (int)n1.FeedbackLevel;
                    int lvl2 = (int)n2.FeedbackLevel;
                    // from highest to lowest
                    return lvl2.CompareTo(lvl1);
                });
            }
        }

        public IReadOnlyList<GameNode> GetNodes()
        {
            return flatList_;
        }

        #endregion
    }
}
