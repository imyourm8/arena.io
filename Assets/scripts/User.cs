using UnityEngine;
using System.Collections.Generic;

public class User : TapCommon.Singleton<User> 
{
    public int Level { get; set; }
    public int Coins { get; set; }
    public string Name { get; set; }
    public List<proto_profile.ClassInfo> Classes { get; set; }
    public proto_profile.PlayerClasses ClassSelected { get; set; }
}
