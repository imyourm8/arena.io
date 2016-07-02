using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Worker : SingletonMonobehaviour<Worker> {
	public delegate bool Work ();

	private List<Work> workers_ = new List<Work>();
	private System.Object lock_ = new System.Object();

	// Use this for initialization
	void Start () {

	}

	void Awake() {
		if(Instance == null)
		{
			//If I am the first instance, make me the Singleton
			Instance = this;
			DontDestroyOnLoad(this);
		}
		else
		{
			//If a Singleton already exists and you find
			//another reference in scene, destroy it!
			if(this != Instance)
				Destroy(this.gameObject);
		}
	}

	// Update is called once per frame
	void Update () {
		lock (lock_) 
		{
			Work work;
			int index = workers_.Count - 1;
			while (index >= 0)
			{
				work = workers_[index--];
				if (work()) {
					workers_.Remove(work);
				}
			}
			workers_.Clear ();
		}
	}

	public void Add(Work work)
	{
		lock(lock_)
		{
			workers_.Add (work);
		}
	}
}
