using UnityEngine;
using System.Collections;

namespace Events 
{
    public class Event<EventIDType> : IEvent<EventIDType>
    {
        private EventIDType id_;
        public EventIDType ID
        { 
            get { return id_; } 
            set { id_ = value; }
        }
        
        public object UserData
        { get; set; }
        
        public Event(EventIDType id)
        {
            id_ = id;
        }
    }
}