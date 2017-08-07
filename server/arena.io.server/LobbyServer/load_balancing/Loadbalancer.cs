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

        /// <summary>
        /// Find most suitable game node
        /// </summary>
        /// <returns>Returns Game Server</returns>
        public GameNode GetBestNode(proto_game.GameMode mode)
        {
            if (flatList_.Count == 0)
            {
                return null;
            }

            //in case there are no games running at all, choose the most crowded one
            GameNode bestNode = flatList_[0];
            foreach (var node in flatList_)
            { 
                //if there are no empty games, try find on less crowded servers
                if (node.GameList.HasAnyNonEmptyGame(mode))
                {
                    bestNode = node;
                    break;
                }
            }

            return bestNode;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
