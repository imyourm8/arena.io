using UnityEngine;
using System;
using System.Collections;

public class GuiEventFire : MonoBehaviour {
	public event Action OnEvent;

	public void FireEvent()
	{
		if (OnEvent != null)
						OnEvent ();
		OnEvent = null;
	}
}
