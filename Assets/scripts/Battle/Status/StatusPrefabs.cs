using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class PowerUpsDict : SerializableDictionaryBase<proto_game.PowerUpType, GameObject> { }

public class StatusPrefabs : SingletonMonobehaviour<StatusPrefabs>
{
    [SerializeField]
    private PowerUpsDict powerUps;

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetPowerUp(proto_game.PowerUpType type)
    {
        return powerUps[type];
    }
}
