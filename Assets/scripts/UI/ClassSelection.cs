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
        }
	}

    private void HandleSelect(proto_profile.PlayerClasses cl)
    {
        User.Instance.ClassSelected = cl;
    }
}
