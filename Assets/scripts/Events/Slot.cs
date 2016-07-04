using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Events 
{
    public struct Slot<KeyType> : ISubscriber<KeyType>, IDisposable
    {
        private INotifier<KeyType> notifier_;
        private Dictionary<KeyType, List<ISubscription<KeyType>>> subscriptions_;
        
        public Slot(INotifier<KeyType> notifier)
        {
            subscriptions_ = new Dictionary<KeyType, List<ISubscription<KeyType>>>();
            notifier_ = notifier;
        }

        public void UnSubscribeFor(KeyType evt, Action<IEvent<KeyType>> listener)
        {
            GetSubscriptions(evt).ForEach(x=>
            {
                if (x.Action == listener)
                    x.Cancel();
            });
        }

        public void UnSubscribeAllOn(KeyType evt)
        {
            GetSubscriptions(evt).ForEach(x=>x.Cancel());
        }
        
        public ISubscription<KeyType> SubscribeOn(KeyType evt, Action<IEvent<KeyType>> listener)
        {
            if (notifier_ == null)
            {
                Debug.LogError("Trying to subscribe with disposed slot!");
                return null;
            }
            var subscription = notifier_.SubscribeOn(evt, listener);
            GetSubscriptions(evt).Add(subscription);
            return subscription;
        }
        
        public void CancelAll()
        {
            if (subscriptions_ == null) 
                return;
                
            subscriptions_.ForEachValue((List<ISubscription<KeyType>> obj)=>
            {
                obj.ForEach(x=>x.Cancel());
            });

            subscriptions_.Clear();
        }
        
        public void Dispose()
        {
            CancelAll();

            subscriptions_ = null;
            notifier_ = null;
        }

        private List<ISubscription<KeyType>> GetSubscriptions(KeyType evt)
        {
            List<ISubscription<KeyType>> events = null;
            if (!subscriptions_.TryGetValue(evt, out events))
            {
                events = new List<ISubscription<KeyType>>();
                subscriptions_.Add(evt, events);
            }

            return events;
        }
    }
}