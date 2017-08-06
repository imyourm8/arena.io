﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared.net.interfaces
{
    public interface IBaseConnection
    {
        void Disconnect();
    }

    public interface IConnection : IBaseConnection
    {
        
    }
}
