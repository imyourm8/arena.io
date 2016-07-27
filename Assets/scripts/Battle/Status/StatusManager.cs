﻿using UnityEngine;
using System.Collections.Generic;

public class StatusManager
{
    private List<Status> statuses_ = new List<Status>();
    private Entity owner_;

    public StatusManager(Entity owner)
    {
        owner_ = owner;
    }

    public void Add(Status status)
    {
        status.Owner = owner_;
        statuses_.Add(status); 
        status.Apply();   
    }

    public void Update()
    {
        List<Status> toRemove = ListPool<Status>.Get(5);
        foreach(var s in statuses_)
        {
            if (s.Update())
            {
                toRemove.Add(s);
            }
        }
        foreach(var r in toRemove)
        {
            r.Remove();
            statuses_.Remove(r);
        }
        ListPool<Status>.Release(toRemove);
    }
}