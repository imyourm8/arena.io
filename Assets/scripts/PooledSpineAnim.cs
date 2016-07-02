using UnityEngine;
using System.Collections;

public class PooledSpineAnim : MonoBehaviour 
{
	private SkeletonAnimation anim_;

	public void PrepareSelfDestruction()
	{
		if (anim_ == null) anim_ = gameObject.GetComponent<SkeletonAnimation> ();
		anim_.state.Complete += ReturnBack;
	}

	void ReturnBack(Spine.AnimationState state, int trackIndex, int loopCount) 
	{
		state.Complete -= ReturnBack;
        gameObject.ReturnPooled();
	}
}
