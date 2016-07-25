using UnityEngine;
using System.Collections;

public class TimedComponent : MonoBehaviour {

    private float startTime_ = 0.0f;

    public float DestroyAfter
    { get; set; }

	void Start () 
    {
	    startTime_ = Time.time;
	}

	void Update () 
    {
	    if (Time.time - startTime_ >= DestroyAfter)
        {
            Destroy(this);
        }
	}
}
