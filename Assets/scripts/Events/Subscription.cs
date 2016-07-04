using UnityEngine;
using System;
using System.Collections;

namespace Events 
{
    public class Subscription<EventIDType> : ISubscription<EventIDType>
    where EventIDType : IComparable
    {
        private Action<IEvent<EventIDType>> action_;
        private EventIDType eventID_;
        private IChannel<EventIDType> channel_;
        
        public Subscription()
        {
        }
    
        IChannel<EventIDType> ISubscription<EventIDType>.Channel
        { 
            get { return channel_; }
            set { channel_ = value; }
        }
    
        Action<IEvent<EventIDType>> ISubscription<EventIDType>.Action
        { 
            get { return action_; }
            set { action_ = value; }
        }
        
        EventIDType ISubscription<EventIDType>.EventID
        { 
            get { return eventID_; }
            set { eventID_ = value; }
        }
        
        public void Cancel()
        {
            if (channel_ != null)
            {
                channel_.Remove(this);
            }
        }
        
        public bool Invoke(IEvent<EventIDType> evt)
        {
            if (action_ == null || eventID_.CompareTo(evt.ID)!=0)
            {
                return false;
            }
            action_(evt);
            return true;
        }

        void IDisposable.Dispose()
        {
            Cancel();
        }
        
    }
}