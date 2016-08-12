using UnityEngine;
using System.Collections;

public class Skill : MonoBehaviour 
{
    [SerializeField]
    protected float recoil = 0.0f;

    [SerializeField]
    protected proto_game.Skills type;

    private long lastCastTime_ = 0;

    #if UNITY_EDITOR
    public float Recoil
    {
        get { return recoil; }
    }

    public proto_game.Skills Type
    { get { return type; } }
    #endif

    public long Cooldown
    { get; set; }

    public Entity Owner
    { get; set; }

    public void Cast()
    {
        OnCast();
        lastCastTime_ = GameApp.Instance.TimeMs();
    }

    public bool CanCast()
    {
        var time = GameApp.Instance.TimeMs();
        if (time - lastCastTime_ < Cooldown)
        {
            return false;
        }
        return true;
    }

    protected virtual void OnCast()
    {}
}
