using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Logging;

using arena.serv.load_balancing.configuration;

namespace arena.serv.load_balancing
{
    using FeedbackLevel = proto_server.FeedbackLevel;

    /// <summary>
    /// Calculates estimated usage level of server instance
    /// </summary>
    internal sealed class FeedbackControlSystem : IFeedbackControlSystem
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        
        private readonly int maxCcu;
        private FeedbackControllerCollection controllerCollection;

        public FeedbackControlSystem(int maxCcu)
        {
            this.maxCcu = maxCcu;

            this.Initialize();
        }

        public FeedbackLevel Output
        {
            get
            {
                return this.controllerCollection.Output;
            }
        }

#region IFeedbackControlSystem implementation

        public void SetBandwidthUsage(int bytes)
        {
            this.controllerCollection.SetInput(FeedbackName.Bandwidth, bytes);
        }

        public void SetCpuUsage(int cpuUsage)
        {
            this.controllerCollection.SetInput(FeedbackName.CpuUsage, cpuUsage);
        }

        public void SetOutOfRotation(bool isOutOfRotation)
        {
            this.controllerCollection.SetInput(FeedbackName.OutOfRotation, isOutOfRotation ? 1 : 0);
        }

        public void SetPeerCount(int peerCount)
        {
            this.controllerCollection.SetInput(FeedbackName.PeerCount, peerCount);
        }
#endregion

#region Private Methods

        private static List<FeedbackController> GetNonConfigurableControllers(int maxCcu)
        {
            Dictionary<FeedbackLevel, int> peerCountThresholds = maxCcu == 0
                                                                     ? new Dictionary<FeedbackLevel, int>()
                                                                     : new Dictionary<FeedbackLevel, int> {
                                                                             { FeedbackLevel.Lowest, 1 }, 
                                                                             { FeedbackLevel.Low, 2 }, 
                                                                             { FeedbackLevel.Normal, maxCcu / 2 }, 
                                                                             { FeedbackLevel.High, maxCcu * 8 / 10 }, 
                                                                             { FeedbackLevel.Highest, maxCcu }
                                                                         };
            var peerCountController = new FeedbackController(FeedbackName.PeerCount, peerCountThresholds, 0, FeedbackLevel.Lowest);

            return new List<FeedbackController> { peerCountController };
        }

        private void Initialize()
        {
            // CCU, Out-of-Rotation
            var allControllers = GetNonConfigurableControllers(this.maxCcu);

            // TODO: load feedback controllers from file: 
            allControllers.AddRange(Default.GetDefaultControllers());

            this.controllerCollection = new FeedbackControllerCollection(allControllers.ToArray());
        }

#endregion
    }
}