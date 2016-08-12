using UnityEngine;
using System.Collections;
using CircularBuffer;

public class StateInterpolator 
{
    private static readonly int BufferSize = 10;
    struct StateData
    {
        public long time;
        public proto_game.UnitState state;
    }

    private Entity owner_;
    private long lastPacketTime_ = 0;
    private CircularBuffer<StateData> states_ = new CircularBuffer<StateData>(BufferSize);
    private StateData fromState_ = new StateData();
    private StateData toState_;
    private int updateCount_ = 0;

    public StateInterpolator(Entity owner)
    {
        owner_ = owner;
    }

    public void PushState(proto_game.UnitState state, long timestamp)
    {
        if (timestamp <= lastPacketTime_)
            return;

        lastPacketTime_ = timestamp;

        var stateData = new StateData();
        stateData.state = state;
        stateData.time = timestamp;
        states_.PushBack(stateData);
        updateCount_++;
        UpdateInterpolationStates();
    }

    private void UpdateInterpolationStates()
    {
        if (updateCount_ < 2)
        {
            return;
        }

        var renderTime = GetRenderTime();
        for (var i = states_.Size-1; i > 0; i--)
        {
            var state = states_[i];
            if (state.time > renderTime)
            {
                toState_ = state;
                fromState_ = states_[i-1];
            }
        }
    }

    private long GetRenderTime()
    {
        return GameApp.Instance.ClientTimeMs() - GameApp.Instance.MovementInterpolationTime;
    }

    public void Reset()
    {
        updateCount_ = 0;
    }

    public void Update()
    {
        if (updateCount_ < 2)
        {
            return;
        }

        var totalFrameTime = toState_.time - fromState_.time;
        var advancedFrameTime = GetRenderTime() - fromState_.time;
        var alpha = (float)advancedFrameTime / (float)totalFrameTime;

        var rotation = Mathf.LerpAngle(fromState_.state.rotation*Mathf.Rad2Deg, toState_.state.rotation*Mathf.Rad2Deg, alpha);
        var x = Mathf.Lerp(fromState_.state.x, toState_.state.x, alpha);
        var y = Mathf.Lerp(fromState_.state.y, toState_.state.y, alpha);

        owner_.Position = new Vector2(x, y);
        owner_.Rotation = rotation;

        if (alpha >= 1.0f)
        {
            UpdateInterpolationStates();
        }
    }
}
