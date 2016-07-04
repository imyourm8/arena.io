using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Events 
{
    public class Channel<EventIDType> : IChannel<EventIDType>
    where EventIDType : IComparable
    {
        private EventIDType id_;
        private List<ISubscription<EventIDType>> subscriptions_;
        
        public Channel(EventIDType id)
        {
            subscriptions_ = new List<ISubscription<EventIDType>>();
            id_ = id;
        }

        ISubscription<EventIDType> IChannel<EventIDType>.Subscribe(System.Action<IEvent<EventIDType>> listener)
        {
            ISubscription<EventIDType> subscription = new Subscription<EventIDType>();
            subscriptions_.Add(subscription);
            subscription.Action = listener;
            subscription.EventID = id_;
            subscription.Channel = this;
            return subscription;
        }

        void IChannel<EventIDType>.UnSubscribe(System.Action<IEvent<EventIDType>> listener)
        {
            int count = subscriptions_.Count;
            for (int i = count - 1; i >= 0; --i)
            {
                if (subscriptions_[i].Action == listener)
                {
                    subscriptions_.RemoveAt(i);
                    break;
                }
            }
        }

        void IChannel<EventIDType>.UnSubscribeAll()
        {
            subscriptions_.Clear();
        }
        
        void IChannel<EventIDType>.Remove(ISubscription<EventIDType> subscription)
        {
            subscriptions_.Remove(subscription);
        }
        
        void IChannel<EventIDType>.Trigger(IEvent<EventIDType> evt)
        {
            for (int i = subscriptions_.Count - 1; i >= 0 && i < subscriptions_.Count; --i)
            {
                var subscription = subscriptions_[i];
                if ( !subscription.Invoke(evt) )
                    subscriptions_.Remove(subscription);
            }
        }
    }
}