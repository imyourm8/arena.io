using UnityEngine;
using System;
using System.Collections;

public class CancelableActionExecutor 
{
	private bool canceled_ = false;
	private Action action_;

	public void Cancel()
	{
		canceled_ = true;
	}

	public void Reset(Action action)
	{
		action_ = action;
	}

	public bool Executed()
	{
		return action_ == null;
	}

	public void Execute()
	{
		Action toExe = action_;
		action_ = null;
		if (canceled_) return;
		toExe ();
	}
}
