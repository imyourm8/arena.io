using UnityEngine;

using System;
using System.Collections;

public class ClassHolder : MonoBehaviour 
{
    public Action<proto_profile.PlayerClasses> OnUnlock;

    [SerializeField]
    private proto_profile.PlayerClasses playerClass;

    public void Unlock()
    {
        if (OnUnlock != null)
        {
            OnUnlock(playerClass);
        }
    }
}
