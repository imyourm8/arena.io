using UnityEngine;
using System.Collections;

namespace Events
{
    public interface IEvent<EventIDType>
    {
        EventIDType ID
        { 
            get;
        }
        
        object UserData
        { get; set; }
    }
}
