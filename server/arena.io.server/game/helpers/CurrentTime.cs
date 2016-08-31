using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.helpers
{
    class CurrentTime : TapCommon.Singleton<CurrentTime>  
    {
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public long CurrentTimeInMs
        {
            get
            {
                return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
            }
        }
    }
}
