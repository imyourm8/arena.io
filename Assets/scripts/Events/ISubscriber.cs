using UnityEngine;
using System;

namespace Events 
{
    public interface ISubscriber<EventIDType>
    {
        ISubscription<EventIDType> SubscribeOn(EventIDType evt, Action<IEvent<EventIDType>> listener);
        void UnSubscribeFor(EventIDType evt, Action<IEvent<EventIDType>> listener);
        void UnSubscribeAllOn(EventIDType evt);
    }
}