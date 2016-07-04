using UnityEngine;
using System;
using System.Collections;

namespace Events 
{
    public interface INotifier<EventIDType> : ISubscriber<EventIDType>
    {
        void Trigger(EventIDType evtKey);
        void Trigger(IEvent<EventIDType> evt);
    }
}