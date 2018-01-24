using UnityEngine;
using System.Collections.Generic;

public class User : shared.Singleton<User>, PlayerExperience.IExpProvider
{
    public User()
    {
        exp_ = new PlayerProfileExperience(this);
        ClassSelected = proto_profile.PlayerClasses.TypeA;
    }

    private PlayerProfileExperience exp_;
    public PlayerProfileExperience ProfileExperience { get { return exp_; }}
    public int Coins { get; set; }
    public string Name { get; set; }
    public List<proto_profile.ClassInfo> Classes { get; set; }
    public proto_profile.PlayerClasses ClassSelected { get; set; }

    int PlayerExperience.IExpProvider.Exp
    {
        get; set;
    }

    public int Level
    {
        get; set;
    }
}
