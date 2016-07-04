using UnityEngine;
using System;
using System.Collections;

namespace Events 
{
    public interface ISubscription<EventIDType> : System.IDisposable
    {
        void Cancel();
        bool Invoke(IEvent<EventIDType> evt);
        Action<IEvent<EventIDType>> Action
        { get; set; }
        EventIDType EventID
        { get; set; }
        IChannel<EventIDType> Channel
        { get; set; }
    }
}