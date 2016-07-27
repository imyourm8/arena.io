using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour 
{
    [SerializeField]
    private GameObject target = null;

    [SerializeField]
    private float smoothTime = 1.0f;

    private Vector2 velocity;
    private Transform thisTransform;
    private Transform targetTransform;
    private bool snapNextTime_ = false;

	void Start () 
    {
	    thisTransform = transform;
	}

    public void SetTarget(GameObject target)
    {
        this.target = target;
        targetTransform = target.transform;
    }

	void FixedUpdate () 
    {
        if (target == null) return;
        if (snapNextTime_)
        {
            float x = Mathf.SmoothDamp( thisTransform.position.x, 
                targetTransform.position.x, ref velocity.x, smoothTime);
            float y = Mathf.SmoothDamp( thisTransform.position.y, 
                targetTransform.position.y, ref velocity.y, smoothTime);

            thisTransform.position = new Vector3(x, y, thisTransform.position.z);
        }
        else 
        {
            thisTransform.position = new Vector3(targetTransform.position.x, targetTransform.position.y, thisTransform.position.z);
            snapNextTime_ = false;
        }
	}

    public void SnapNextTick()
    {
        snapNextTime_ = true;
    }
}
