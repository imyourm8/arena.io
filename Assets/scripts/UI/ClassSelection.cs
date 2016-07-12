using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

public class ClassSelection : MonoBehaviour 
{
    public static string UlockEvent = "evt_unlock_class";

    [SerializeField]
    private List<ClassHolder> classes = null;

	void Start () 
    {
        foreach(var t in classes)
        {
            t.OnSelect = HandleSelect;
            t.OnUnlock = HandleUnlock;
        }
	}

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        foreach(var t in classes)
        {
            t.Refresh();
        }
    }

    private void HandleSelect(proto_profile.PlayerClasses cl)
    {
        User.Instance.ClassSelected = cl;
    }

    private void HandleUnlock(proto_profile.PlayerClasses cl, ClassHolder callee)
    {
        var info = User.Instance.Classes.Find((proto_profile.ClassInfo i)=>
        {
            return i.@class == cl;
        });

        if (info != null)
        {
            
        }
    }
}
