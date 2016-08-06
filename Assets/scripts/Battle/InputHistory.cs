using UnityEngine;
using System.Collections;

using Nito;

public class InputHistory
{
    struct Move
    {
        public float force_x;
        public float force_y;
        public int tick;
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

        var move = new Move();
        move.force_x = input.force_x;
        move.force_y = input.force_y;
        move.tick = input.tick;
        move.state = state;

        //Debug.LogFormat("Add History {0} {1} {2} {3} {4}", state.Position.x, state.Position.y, input.force_x, input.force_y, input.tick);

        inputs_.AddToBack(move);
    }

    public void Correction(proto_game.PlayerInput.Response input, Player player)
    {
        //Debug.LogFormat("Input {0} {1} {2} {3} {4} {5}", input.x, input.y, input.force_x, input.force_y, input.tick, input.time);

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
            //Debug.Log("Start fixing........................................");
            //Debug.LogWarningFormat("Fix {2} from {0} {1} {3} {4}", oldestMove.state.Position.x, 
            //    oldestMove.state.Position.y, oldestMove.tick,
            //    oldestMove.force_x, oldestMove.force_y);
            //Debug.LogErrorFormat("Fix {2} to {0} {1} {3} {4}", input.x, input.y, input.tick, input.force_x, input.force_y);
            //Debug.LogErrorFormat("Prev pos {0} {1} {2} {3} {4}", input.prevx, input.prevy, input.prevfx, input.prevfy, input.ptick);
           // var a1 = !Mathf.Approximately(oldestMove.state.Position.x,input.x);
            //var a2 = !Mathf.Approximately(oldestMove.state.Position.y,input.y);
           /* var a3 = !Mathf.Approximately(oldestMove.state.Velocity.x,input.vel_x);
            var a4 = !Mathf.Approximately(oldestMove.state.Velocity.y,input.vel_y);*/

            inputs_.RemoveFromFront();
            var originalPosition = player.GetState().Position;
            player.Force = new Vector2(input.force_x, input.force_y);
            player.Snap(input.x, input.y);
            player.Replaying = true;
            player.CancelMoving();

            int ticks = input.tick;
            int i = 0;
            float inputDt = GameApp.Instance.MovementUpdateDT;

            while (i <= inputs_.Count - 1)
            {
                var replayedInput = inputs_[i];
                while (ticks < replayedInput.tick)
                {
                    player.OnFixedUpdate(inputDt);
                    ticks++;
                }

                replayedInput.state = player.GetState();
                inputs_[i] = replayedInput;
                player.Force = new Vector2(replayedInput.force_x, replayedInput.force_y);
                //Debug.LogFormat("Fix {2} {0} {1} {3} {4}", replayedInput.state.Position.x, replayedInput.state.Position.y, replayedInput.tick,
                //    replayedInput.force_x, replayedInput.force_y);
                i++;
            }
            player.OnFixedUpdate(inputDt);
            player.Replaying = false;
            var dist = player.GetState().Position - originalPosition;
            if (dist.magnitude > 0.01f)
            {
                player.Smooth();
            }
        }
    }
}
