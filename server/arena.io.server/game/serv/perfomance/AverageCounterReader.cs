using System.Linq;

using ExitGames.Diagnostics.Counter;

namespace arena.serv.perfomance
{
    public sealed class AverageCounterReader : PerformanceCounterReader
    {
        private readonly ValueHistory values;

        public AverageCounterReader(int capacity, string categoryName, string counterName)
            : base(categoryName, counterName)
        {
            this.values = new ValueHistory(capacity);
        }

        public AverageCounterReader(int capacity, string categoryName, string counterName, string instanceName)
            : base(categoryName, counterName, instanceName)
        {
            this.values = new ValueHistory(capacity);
        }

        public double GetNextAverage()
        {
            float value = this.GetNextValue();
            this.values.Add((int)value);
            return this.values.Average();
        }
    }
}
