using System.Collections.Generic;

namespace arena.serv.load_balancing
{
    internal class ValueHistory : Queue<int>
    {
        private readonly int capacity;

        public ValueHistory(int capacity)
            : base(capacity)
        {
            this.capacity = capacity;
        }

        public void Add(int value)
        {
            if (this.Count == this.capacity)
            {
                this.Dequeue();
            }

            this.Enqueue(value);
        }
    }
}