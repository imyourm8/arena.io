using UnityEngine;
using System.Collections;

public class ExpBlock : Entity 
{
    [SerializeField]
    private proto_game.ExpBlocks blockType;

    #if UNITY_EDITOR
    public proto_game.ExpBlocks BlockType
    {
        get { return blockType; }
    }
    #endif
}
