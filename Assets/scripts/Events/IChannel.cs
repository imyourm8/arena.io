using UnityEngine;
using System;
using System.Collections;

namespace Events 
{
    public interface IChannel<EventIDType>
    {
        ISubscription<EventIDType> Subscribe(Action<IEvent<EventIDType>> listener);
        void UnSubscribe(Action<IEvent<EventIDType>> listener);
        void UnSubscribeAll();
        void Trigger(IEvent<EventIDType> evt);
        void Remove(ISubscription<EventIDType> subscription);
    }
}