using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TapCommon
{
    class Collisions
    {
        static public float FindImpactTime(float ObjPos, float ObjPos2, float ObjSpeed, float ObjSpeed2)
        {
            return (ObjPos2 - ObjPos) / (ObjSpeed - ObjSpeed2);
        }
    }
}
