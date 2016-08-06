using UnityEngine;
using System.Collections;

public class Skill : MonoBehaviour 
{
    private long lastCastTime_ = 0;

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
