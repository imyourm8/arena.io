namespace arena.serv.load_balancing
{
    internal interface IFeedbackControlSystem
    {
        FeedbackLevel Output { get; }

        void SetPeerCount(int peerCount);

        void SetCpuUsage(int cpuUsage);

        void SetBandwidthUsage(int bytes);
        
        void SetOutOfRotation(bool isOutOfRotation);
    }
}