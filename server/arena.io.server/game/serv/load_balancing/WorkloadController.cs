using System;
using ExitGames.Concurrency.Fibers;
using ExitGames.Diagnostics.Counter;
using ExitGames.Logging;
using Photon.SocketServer;

using arena.serv.perfomance;

namespace arena.serv.load_balancing
{
    using FeedbackLevel = proto_server.FeedbackLevel;

    public class WorkloadController
    {
        #region Constants and Fields

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private const int AverageHistoryLength = 100;

        private readonly ApplicationBase application_;

        private readonly string applicationName;

        private readonly AverageCounterReader businessLogicQueueCounter_;
        private readonly AverageCounterReader bytesInCounter_;
        private readonly AverageCounterReader bytesOutCounter;
        private readonly AverageCounterReader cpuCounter;
        private readonly AverageCounterReader enetQueueCounter;
        private readonly AverageCounterReader timeSpentInServerInCounter;
        private readonly AverageCounterReader timeSpentInServerOutCounter;
        private readonly AverageCounterReader enetThreadsProcessingCounter;
        private readonly AverageCounterReader tcpPeersCounter;
        private readonly AverageCounterReader tcpDisconnectsPerSecondCounter;
        private readonly AverageCounterReader tcpClientDisconnectsPerSecondCounter;
        private readonly AverageCounterReader udpPeersCounter;
        private readonly AverageCounterReader udpDisconnectsPerSecondCounter;
        private readonly AverageCounterReader udpClientDisconnectsPerSecondCounter;
        private readonly PerformanceCounterReader enetThreadsActiveCounter;
        private readonly IFeedbackControlSystem feedbackControlSystem;

        private readonly PoolFiber fiber;

        private readonly long updateIntervalInMs_;

        private IDisposable timerControl;

        private ServerState serverState = ServerState.Normal;

        #endregion

        #region Constructors and Destructors

        public WorkloadController(ApplicationBase application, string instanceName, long updateIntervalInMs, string workLoadConfigFile)
        {
            try
            {
                updateIntervalInMs_ = updateIntervalInMs;
                FeedbackLevel = FeedbackLevel.Normal;
                application_ = application;

                fiber = new PoolFiber();
                fiber.Start();

                cpuCounter = new AverageCounterReader(AverageHistoryLength, "Processor", "% Processor Time", "_Total");
                if (!cpuCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", cpuCounter.Name);
                }

                businessLogicQueueCounter_ = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: Threads and Queues", "Business Logic Queue", instanceName);
                if (!businessLogicQueueCounter_.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", businessLogicQueueCounter_.Name);
                }

                enetQueueCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: Threads and Queues", "ENet Queue", instanceName);
                if (!enetQueueCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", enetQueueCounter.Name);
                }

                // amazon instances do not have counter for network interfaces
                bytesInCounter_ = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server", "bytes in/sec", instanceName);
                if (!bytesInCounter_.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", bytesInCounter_.Name);
                }

                bytesOutCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server", "bytes out/sec", instanceName);
                if (!bytesOutCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", bytesOutCounter.Name);
                }

                enetThreadsProcessingCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: Threads and Queues", "ENet Threads Processing", instanceName);
                if (!enetThreadsProcessingCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", enetThreadsProcessingCounter.Name);
                }

                enetThreadsActiveCounter = new PerformanceCounterReader("Photon Socket Server: Threads and Queues", "ENet Threads Active", instanceName);
                if (!enetThreadsActiveCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", enetThreadsActiveCounter.Name);
                }

                timeSpentInServerInCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: ENet", "Time Spent In Server: In (ms)", instanceName);
                if (!timeSpentInServerInCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", timeSpentInServerInCounter.Name);
                }

                timeSpentInServerOutCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: ENet", "Time Spent In Server: Out (ms)", instanceName);
                if (!timeSpentInServerOutCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", timeSpentInServerOutCounter.Name);
                }

                tcpDisconnectsPerSecondCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: TCP", "TCP: Disconnected Peers +/sec", instanceName);
                if (!tcpDisconnectsPerSecondCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", tcpDisconnectsPerSecondCounter.Name);
                }

                tcpClientDisconnectsPerSecondCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: TCP", "TCP: Disconnected Peers (C) +/sec", instanceName);
                if (!tcpClientDisconnectsPerSecondCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", tcpClientDisconnectsPerSecondCounter.Name);
                }

                tcpPeersCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: TCP", "TCP: Peers", instanceName);
                if (!tcpPeersCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", tcpPeersCounter.Name);
                }

                udpDisconnectsPerSecondCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: UDP", "UDP: Disconnected Peers +/sec", instanceName);
                if (!udpDisconnectsPerSecondCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", udpDisconnectsPerSecondCounter.Name);
                }

                udpClientDisconnectsPerSecondCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: UDP", "UDP: Disconnected Peers (C) +/sec", instanceName);
                if (!udpClientDisconnectsPerSecondCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", udpClientDisconnectsPerSecondCounter.Name);
                }

                udpPeersCounter = new AverageCounterReader(AverageHistoryLength, "Photon Socket Server: UDP", "UDP: Peers", instanceName);
                if (!udpPeersCounter.InstanceExists)
                {
                    log.WarnFormat("Did not find counter {0}", udpPeersCounter.Name);
                }

                feedbackControlSystem = new FeedbackControlSystem(2000);

                IsInitialized = true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception during WorkloadController construction. Exception: {0}", e);
            }
        }

        #endregion

        #region Events

        public delegate void FeedbackLevelDelegate(FeedbackLevel level);
        public event FeedbackLevelDelegate FeedbacklevelChanged;

        #endregion

        #region Properties

        public proto_server.FeedbackLevel FeedbackLevel { get; private set; }

        public ServerState ServerState
        {
            get
            {
                return serverState;
            }

            set
            {
                if (value != serverState)
                {
                    var oldValue = serverState;
                    serverState = value;
                    Counter.ServerState.RawValue = (long)ServerState;
                    RaiseFeedbacklevelChanged();

                    if (log.IsInfoEnabled)
                    {
                        log.InfoFormat("ServerState changed: old={0}, new={1}", oldValue, serverState);
                    }

                }
            }
        }

        public bool IsInitialized { get; private set; }

        #endregion

        #region Public Methods


        /// <summary>
        ///   Starts the workload controller with a specified update interval in milliseconds.
        /// </summary>
        public void Start()
        {
            if (!IsInitialized)
            {
                return;
            }

            if (timerControl == null)
            {
                timerControl = fiber.ScheduleOnInterval(Update, 100, updateIntervalInMs_);
            }
        }

        public void Stop()
        {
            if (timerControl != null)
            {
                timerControl.Dispose();
            }
        }

        #endregion

        #region Methods

        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            FeedbackLevel oldValue = feedbackControlSystem.Output;

            if (cpuCounter.InstanceExists)
            {
                var cpuUsage = (int)cpuCounter.GetNextAverage();
                Counter.CpuAvg.RawValue = cpuUsage;
                feedbackControlSystem.SetCpuUsage(cpuUsage);
            }

            if (businessLogicQueueCounter_.InstanceExists)
            {
                var businessLogicQueue = (int)businessLogicQueueCounter_.GetNextAverage();
                Counter.BusinessQueueAvg.RawValue = businessLogicQueue;
            }

            if (enetQueueCounter.InstanceExists)
            {
                var enetQueue = (int)enetQueueCounter.GetNextAverage();
                Counter.EnetQueueAvg.RawValue = enetQueue;
            }

            if (bytesInCounter_.InstanceExists && bytesOutCounter.InstanceExists)
            {
                int bytes = (int)bytesInCounter_.GetNextAverage() + (int)bytesOutCounter.GetNextAverage();
                Counter.BytesInAndOutAvg.RawValue = bytes;
                feedbackControlSystem.SetBandwidthUsage(bytes);
            }

            if (enetThreadsProcessingCounter.InstanceExists && enetThreadsActiveCounter.InstanceExists)
            {
                try
                {
                    var enetThreadsProcessingAvg = enetThreadsProcessingCounter.GetNextAverage();
                    var enetThreadsActiveAvg = enetThreadsActiveCounter.GetNextValue();

                    int enetThreadsProcessing;
                    if (enetThreadsActiveAvg > 0)
                    {
                        enetThreadsProcessing = (int)(enetThreadsProcessingAvg / enetThreadsActiveAvg * 100);
                    }
                    else
                    {
                        enetThreadsProcessing = 0;
                    }

                    Counter.EnetThreadsProcessingAvg.RawValue = enetThreadsProcessing;
                }
                catch (DivideByZeroException)
                {
                    log.WarnFormat("Could not calculate Enet Threads processing quotient: Enet Threads Active is 0");
                }
            }


            if (tcpPeersCounter.InstanceExists && tcpDisconnectsPerSecondCounter.InstanceExists 
                && tcpClientDisconnectsPerSecondCounter.InstanceExists)
            {
                try
                {
                    var tcpDisconnectsTotal = tcpDisconnectsPerSecondCounter.GetNextAverage();
                    var tcpDisconnectsClient = tcpClientDisconnectsPerSecondCounter.GetNextAverage();
                    var tcpDisconnectsWithoutClientDisconnects = tcpDisconnectsTotal - tcpDisconnectsClient;
                    var tcpPeerCount = tcpPeersCounter.GetNextAverage();

                    int tcpDisconnectRate;
                    if (tcpPeerCount > 0)
                    {
                        tcpDisconnectRate = (int)(tcpDisconnectsWithoutClientDisconnects / tcpPeerCount * 1000);
                    }
                    else
                    {
                        tcpDisconnectRate = 0;
                    }

                    int peerCount = (int)tcpPeerCount;
                    Counter.TcpDisconnectRateAvg.RawValue = tcpDisconnectRate;
                    Counter.PeerCount.RawValue = peerCount;
                    feedbackControlSystem.SetPeerCount(peerCount);
                }
                catch (DivideByZeroException)
                {
                    log.WarnFormat("Could not calculate TCP Disconnect Rate: TCP Peers is 0");
                }
            }

            if (udpPeersCounter.InstanceExists && udpDisconnectsPerSecondCounter.InstanceExists 
                && udpClientDisconnectsPerSecondCounter.InstanceExists)
            {
                try
                {
                    var udpDisconnectsTotal = udpDisconnectsPerSecondCounter.GetNextAverage();
                    var udpDisconnectsClient = udpClientDisconnectsPerSecondCounter.GetNextAverage();
                    var udpDisconnectsWithoutClientDisconnects = udpDisconnectsTotal - udpDisconnectsClient;
                    var udpPeerCount = udpPeersCounter.GetNextAverage();

                    int udpDisconnectRate;
                    if (udpPeerCount > 0)
                    {
                        udpDisconnectRate = (int)(udpDisconnectsWithoutClientDisconnects / udpPeerCount * 1000);
                    }
                    else
                    {
                        udpDisconnectRate = 0;
                    }

                    Counter.UdpDisconnectRateAvg.RawValue = udpDisconnectRate;
                }
                catch (DivideByZeroException)
                {
                    log.WarnFormat("Could not calculate UDP Disconnect Rate: UDP Peers is 0");
                }
            }

            if (timeSpentInServerInCounter.InstanceExists && timeSpentInServerOutCounter.InstanceExists)
            {
                var timeSpentInServer = (int)timeSpentInServerInCounter.GetNextAverage() + (int)timeSpentInServerOutCounter.GetNextAverage();
                Counter.TimeInServerInAndOutAvg.RawValue = timeSpentInServer;
            }

            FeedbackLevel = feedbackControlSystem.Output;
            Counter.LoadLevel.RawValue = (byte)FeedbackLevel;

            if (oldValue != FeedbackLevel)
            {
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("FeedbackLevel changed: old={0}, new={1}", oldValue, FeedbackLevel);
                }

                RaiseFeedbacklevelChanged();
            }
        }

        private void RaiseFeedbacklevelChanged()
        {
            var e = FeedbacklevelChanged;
            if (e != null)
            {
                e(FeedbackLevel);
            }
        }

        #endregion
    }
}


