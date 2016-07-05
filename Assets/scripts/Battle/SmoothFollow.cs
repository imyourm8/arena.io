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
    private bool snapNextTime_ = false;

	void Start () 
    {
	    thisTransform = transform;
	}

    public void SetTarget(GameObject target)
    {
        this.target = target;
    }

	void Update () 
    {
        if (target == null) return;
        if (snapNextTime_)
        {
            float x = Mathf.SmoothDamp( thisTransform.position.x, 
                target.transform.position.x, ref velocity.x, smoothTime);
            float y = Mathf.SmoothDamp( thisTransform.position.y, 
                target.transform.position.y, ref velocity.y, smoothTime);

            thisTransform.position = new Vector3(x, y, thisTransform.position.z);
        }
        else 
        {
            thisTransform.position = new Vector3(target.transform.position.x, target.transform.position.y, thisTransform.position.z);
            snapNextTime_ = false;
        }
	}

    public void SnapNextTick()
    {
        snapNextTime_ = true;
    }
}
