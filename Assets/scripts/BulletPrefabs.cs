using UnityEngine;
using System.Collections;

[System.Serializable]
public class BulletDict : SerializableDictionaryBase<proto_game.Bullets, GameObject> {}

public class BulletPrefabs : SingletonMonobehaviour<BulletPrefabs>
{
    [SerializeField]
    private BulletDict prefabs;

	void Awake () 
    {
	    instance_ = this;
	}

    public GameObject Get(proto_game.Bullets blt)
    {
        return prefabs[blt].GetPooled();
    }
}
