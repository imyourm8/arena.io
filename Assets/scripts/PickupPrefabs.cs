using UnityEngine;
using System.Collections;

[System.Serializable]
public class PickupsDict : SerializableDictionaryBase<proto_game.Pickups, GameObject>
{};

public class PickupPrefabs : SingletonMonobehaviour<PickupPrefabs>
{
    [SerializeField]
    private PickupsDict pickups = null;

    void Awake()
    {
        instance_ = this;
    }

    public PickUp GetPickup(proto_game.Pickups id)
    {
        return pickups[id].GetPooled().GetComponent<PickUp>();
    }
}
