//#define LOG
using UnityEngine;
using System.Collections;

using Nito;

public class InputHistory
{
    public struct Move
    {
        public float force_x;
        public float force_y;
        public int tick;
        public float recoil;
        public PhysicalState state;
    }

    private Deque<Move> inputs_ = new Deque<Move>();

    public InputHistory(int size = 100)
    {
        inputs_.Capacity = size;
    }

    public void Reset()
    {
        inputs_.Clear();
    }

    public void Add(proto_game.PlayerInput.Request input, PhysicalState state)
    {
        if (inputs_.Count == inputs_.Capacity)
        {
            inputs_.RemoveFromFront();
        }
#if LOG
        Debug.LogFormat("ADD {0} {1} | {2} {3} | {4}=>{5} TICK: {6}  ================================", 
            state.Position.x,state.Position.y,state.RecoilVelocity.x,state.RecoilVelocity.y,
            state.Recoil, state.Rotation, input.tick);
#endif
        var move = new Move();
        move.force_x = input.force_x;
        move.force_y = input.force_y;
        move.tick = input.tick;
        move.state = state;
        move.recoil = state.Recoil;

        inputs_.AddToBack(move);
    }

    public void Correction(proto_game.PlayerInput.Response input, Player player)
    {
#if LOG
        Debug.LogFormat("{0} {1} Vel: {3} {4} Recoil: {5} {6} TICK: {2} Shoot or Skill {7}",
         input.x, input.y, input.tick, input.velx, input.vely, input.rvelx, input.rvely, input.shoot||input.skill);
#endif
        while (inputs_.Count > 0 && inputs_[0].tick < input.tick)
        {
            inputs_.RemoveFromFront();
        }

        if (inputs_.Count == 0)
            return;
        
        var oldestMove = inputs_[0];
        if (!Mathf.Approximately(oldestMove.state.Position.x,input.x) ||
            !Mathf.Approximately(oldestMove.state.Position.y,input.y))
        {
            inputs_.RemoveFromFront();
#if LOG
            Debug.Log("Start fixing........................................");
            Debug.LogWarningFormat("Fix {2} from {0} {1} Recoil Vel: {3} {4} ReCOIL: {5} Rotation {6}", oldestMove.state.Position.x, 
                oldestMove.state.Position.y, oldestMove.tick, oldestMove.state.RecoilVelocity.x,oldestMove.state.RecoilVelocity.y,
                oldestMove.recoil, oldestMove.state.Rotation);
            Debug.LogErrorFormat("Fix {2} to {0} {1} vel {3} {4} Recoil {5} {6} Shoot or Skill {7}", input.x, input.y, 
            input.tick, input.velx, input.vely, input.rvelx, input.rvely, input.shoot||input.skill);
#endif
            
            var originalPosition = player.GetState().Position;
            player.Force = new Vector2(input.force_x, input.force_y);
            player.Snap(input.x, input.y);
            //set current recoil velocity
            player.RecoilVelocity = oldestMove.state.RecoilVelocity;
            if (oldestMove.recoil > 0.0f)
            {
                player.ApplyRecoil(oldestMove.recoil);
                #if LOG 
                Debug.LogFormat("ApplyRecoil({0})", oldestMove.recoil);
                #endif
            }
            player.Replaying = true;

            int ticks = input.tick;
            int i = 0;
            float inputDt = GameApp.Instance.MovementUpdateDT;

            while (i <= inputs_.Count - 1)
            {
                var replayedInput = inputs_[i];
                //while (ticks < replayedInput.tick)
                {
                    player.ApplyInputs(inputDt);
                    #if LOG 
                    Debug.LogFormat("ApplyInputs ticks:", ticks);
                    #endif
                    ticks++;
                }

                player.ForceRotation();
                player.Rotation = replayedInput.state.Rotation;
                player.Force = new Vector2(replayedInput.force_x, replayedInput.force_y);

                replayedInput.state = player.GetState();
                inputs_[i] = replayedInput;

                if (replayedInput.recoil > 0.0f)
                {
                    player.ApplyRecoil(replayedInput.recoil);
                    #if LOG 
                    Debug.LogFormat("ApplyRecoil({0})", oldestMove.recoil);
                    #endif
                }
#if LOG
                Debug.LogFormat("FIX {0} {1} | {2} {3} | {4}=>{5} TICK: {6}", 
                    replayedInput.state.Position.x, replayedInput.state.Position.y,
                    player.RecoilVelocity.x,player.RecoilVelocity.y,
                    replayedInput.recoil, replayedInput.state.Rotation, replayedInput.tick);
#endif
                i++;
            }
            player.ApplyInputs(inputDt);
            player.Replaying = false;

            var posDiff = player.GetState().Position - originalPosition;
            if (posDiff.magnitude > 0.01f)
            {
                player.Smooth();
            }
#if LOG
            var s = player.GetState();
            Debug.LogErrorFormat("Final  result {0}, {1}  Original {2} {3} ", s.Position.x, s.Position.y,originalPosition.x,originalPosition.y);
#endif
        }
    }
}
