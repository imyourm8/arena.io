using System.Collections.Generic;

namespace arena.serv.load_balancing.configuration
{
    internal class Default
    {
        internal static List<FeedbackController> GetDefaultControllers()
        {
            var cpuController = new FeedbackController(
            FeedbackName.CpuUsage,
            new Dictionary<FeedbackLevel, int>
                    {
                        { FeedbackLevel.Lowest, 20 },
                        { FeedbackLevel.Low, 35 },
                        { FeedbackLevel.Normal, 50 },
                        { FeedbackLevel.High, 70 },
                        { FeedbackLevel.Highest, 90 }
                    },
            0,
            FeedbackLevel.Lowest);

            const int megaByte = 1024 * 1024;
            var thresholdValues = new Dictionary<FeedbackLevel, int> 
                {
                    { FeedbackLevel.Lowest, megaByte }, 
                    { FeedbackLevel.Normal, 4 * megaByte }, 
                    { FeedbackLevel.High, 8 * megaByte }, 
                    { FeedbackLevel.Highest, 10 * megaByte }
                };
            var bandwidthController = new FeedbackController(FeedbackName.Bandwidth, thresholdValues, 0, FeedbackLevel.Lowest);

            return new List<FeedbackController> { cpuController, bandwidthController };
        }
    }
}
