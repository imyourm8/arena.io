using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Events 
{
    public class Notifier<EventIDType> : INotifier<EventIDType>
    where EventIDType : IComparable
    {
        private Dictionary<EventIDType, IChannel<EventIDType>> channels_;
        private Event<EventIDType> defaultEvt_;
    
        public Notifier()
        {
            channels_ = new Dictionary<EventIDType, IChannel<EventIDType>>();
            defaultEvt_ = new Event<EventIDType>(default(EventIDType));
        }
    
        ISubscription<EventIDType> ISubscriber<EventIDType>.SubscribeOn(EventIDType evt, Action<IEvent<EventIDType>> listener)
        {
            var channel = GetChannelFor(evt);
            if (channel == null)
            {
                channel = new Channel<EventIDType>(evt);
                channels_.Add(evt, channel);
            }
            return channel.Subscribe(listener);
        }
        
        public void Trigger(EventIDType evtKey)
        {
            defaultEvt_.ID = evtKey;
            Trigger(defaultEvt_);
        }
        
        public void Trigger(IEvent<EventIDType> evt)
        {
            var channel = GetChannelFor(evt.ID);
            if (channel == null)
            {
                return;
            }
            channel.Trigger(evt);
        }
        
        void ISubscriber<EventIDType>.UnSubscribeFor(EventIDType evt, Action<IEvent<EventIDType>> listener)
        {
            var channel = GetChannelFor(evt);
            if (channel != null)
            {
                channel.UnSubscribe(listener);
            }
        }
        
        void ISubscriber<EventIDType>.UnSubscribeAllOn(EventIDType evt)
        {
            var channel = GetChannelFor(evt);
            if (channel != null)
            {
                channel.UnSubscribeAll();
            }
        }
        
        private IChannel<EventIDType> GetChannelFor(EventIDType evt)
        {
            IChannel<EventIDType> channel;
            channels_.TryGetValue(evt, out channel);
            return channel;   
        }
    }
}