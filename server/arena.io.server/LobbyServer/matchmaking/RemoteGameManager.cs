using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

using proto_server;
using proto_game;
using shared.account;

namespace LobbyServer.matchmaking
{
    using load_balancing;
    using AwaitingQueue = Queue<GameQueueEntry>;

    class RemoteGameManager
    {
        #region Fields

        private LobbyApplication application_;
        private Dictionary<GameMode, AwaitingQueue> awaitingQueues_ = new Dictionary<GameMode, AwaitingQueue>();

        #endregion

        #region Constructors

        public RemoteGameManager(LobbyApplication app)
        {
            application_ = app;
        }

        #endregion

        #region Private Methods

        private AwaitingQueue GetQueue(GameMode mode)
        {
            AwaitingQueue queue = null;
            if (!awaitingQueues_.TryGetValue(mode, out queue))
            {
                queue = new AwaitingQueue(100);
                awaitingQueues_.Add(mode, queue);
            }
            return queue;
        }

        private void AddPlayerToGame(GameSession game, Profile profile)
        {
            game.PlayersConnected++;
        }

        private void TryMatchPlayers(AwaitingQueue queue, IReadOnlyCollection<GameSession> games)
        {
            foreach (var game in games)
            {
                if (game.IsFull || (game.PlayersConnected == 0 && queue.Count < game.MinPlayersToStart))
                {
                    return;
                }

                int playersToFetch = game.MaxPlayersAllowed - game.PlayersConnected;
                List<string> playersJoined = new List<string>(playersToFetch);
                List<GameQueueEntry> playersBatch = new List<GameQueueEntry>(playersToFetch);
                while (playersToFetch > 0)
                {
                    playersToFetch--;

                    GameQueueEntry entry = queue.Dequeue();
                    playersJoined.Add(entry.playerProfile.UniqueID);
                    AddPlayerToGame(game, entry.playerProfile);
                }

                game.Node.Controller.SendPlayersJoin(playersJoined, game.Id, (bool success) =>
                    {
                        if (success)
                        {
                            foreach (var entry in playersBatch)
                            {
                                entry.Callback(game, entry.Ip);
                            }
                        }
                    });
            }
        }

        /// <summary>
        /// Try to match existing players in queues with games
        /// </summary>
        /// <returns>Returns False if no games were found</returns>
        private void MatchPlayersInGames()
        {
            var nodes = application_.Loadbalancer.GetNodes();
            if (nodes.Count == 0)
            {
                // smth is terribly wrong
                return;
            }

            // iterate over every queue
            foreach (var queueEntry in awaitingQueues_)
            {
                var mode = queueEntry.Key;
                // nodes are sorted from highest to lowest workload level
                GameNode bestNode = nodes[0];
                foreach (var node in nodes)
                {
                    if (!node.IsJoinable)
                        continue;

                    if (node.GameList.HasAnyNonEmptyGame(mode))
                    {
                        bestNode = node;
                        break;
                    }
                }

                if (bestNode.GameList.HasAnyNonEmptyGame(mode))
                {
                    var games = bestNode.GameList.GetJoinableGames(mode);
                    // first element with highest amount of connected players
                    TryMatchPlayers(queueEntry.Value, games);
                }
                else 
                {
                    bestNode.CreateGame(mode);
                }
            }
        }

        #endregion

        #region Public Methods

        public void Add(GameSession game)
        {
            game.Node.GameList.AddGame(game);
            MatchPlayersInGames();
        }

        public void Remove(GameNode node, string gameId)
        {
            var game = node.GameList.FindGame(gameId);
            Debug.Assert(game != null);
            node.GameList.RemoveGame(game);
        }

        public void RemoveOnePlayer(GameNode node, string gameId)
        {
            GameSession game = node.GameList.FindGame(gameId);
            Debug.Assert(game != null);
            Debug.Assert(game.PlayersConnected != 0);
            game.PlayersConnected--;
            MatchPlayersInGames();
        }


        /// <summary>
        /// Find free game or create a new one
        /// </summary>
        /// <returns>Task with Game Session of currently running game</returns>
        public void FindGame(proto_game.GameMode modeID, Profile profile, Action<GameSession, string> callback)
        {
            var nodes = application_.Loadbalancer.GetNodes();
            if (nodes.Count == 0)
            {
                // smth is terribly wrong
                callback(null, null);
                return;
            }
            // enqueue request, to call it later as part of matching process
            GetQueue(modeID).Enqueue(new GameQueueEntry(callback, profile, ""));
        } 

        #endregion
    }
}
