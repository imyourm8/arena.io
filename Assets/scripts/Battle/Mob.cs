using UnityEngine;
using System.Collections;

public class Mob : Unit 
{
    [SerializeField]
    private proto_game.MobType type = proto_game.MobType.Basic;

    public proto_game.MobType Type
    { get { return type; } }

    private Mob ghost_;

    public void CreateGhost()
    {
        ghost_ = gameObject.GetPooled().GetComponent<Mob>();
        ghost_.GetComponent<SpriteRenderer>().color = Color.green;
        Controller.gameObject.AddChild(ghost_.gameObject);
    }

    public override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        if (ghost_ != null)
        {
            ghost_.Position = stateInterpolator_.GetRecentPosition();
            ghost_.Rotation = Rotation;
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (ghost_ != null)
        {
            ghost_.gameObject.ReturnPooled();
        }
    }
}
