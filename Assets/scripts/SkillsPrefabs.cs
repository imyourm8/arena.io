using UnityEngine;
using System.Collections;

public class SkillsPrefabs : SingletonMonobehaviour<SkillsPrefabs>
{
    [SerializeField]
    private SkillsDict skills = null;

    void Awake()
    {
        instance_ = this;
    }

    public Skill GetSkill(proto_game.Skills skillId)
    {
        return skills[skillId].GetPooled().GetComponent<Skill>();
    }
}
