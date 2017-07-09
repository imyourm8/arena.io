//#define LOG
//#define USE_HERMIT

using UnityEngine;
using System.Collections;
using CircularBuffer;

public class StateInterpolator 
{
    private static readonly int BufferSize = 20;
    struct StateData
    {
        public float time;
        public int tick;
        public proto_game.UnitState state;
    }

    private readonly float DefaultInterpolationSmoothValue = 0.01f;

    private Entity owner_;
    private CircularBuffer<StateData> states_;
    private StateData fromState_ = new StateData();
    private StateData toState_ = new StateData();
    private float timeWhenExtrapolationStarted_ = 0.0f;
    private float interpolationExtension_ = 0.0f;
    private float prevUpdateTime_ = 0.0f;
    private float interpolationSmoothValue_ = 0.0f;
    private float latestUpdateTime_ = 0.0f;
    private bool newPacketReceived_ = false;

    #if USE_HERMIT
    private StateData prevState_ = new StateData();
    #endif
    private int updateCount_ = 0;

    public StateInterpolator(Entity owner)
    {
        owner_ = owner;
        Reset();
    }

    private float DefaultInterpolationValue
    {
        get { return (float)GameApp.Instance.MovementInterpolationTime/1000.0f; }
    } 

    public void PushState(proto_game.UnitState state, int tick)
    {
        updateCount_++;

        var stateData = new StateData();
        stateData.state = state;
        stateData.tick = tick;
        stateData.time = owner_.Controller.TickToFloatTime(tick);
        states_.PushBack(stateData);

        newPacketReceived_ = true;
        #if LOG
        Debug.LogErrorFormat("Push state at time diff {0} Position {3} {4} Interpolatin Time {1} Latency {2}", 
            stateData.time - GetRenderTime(), 
            GameApp.Instance.MovementInterpolationTime,
            GameApp.Instance.Latency,
            state.x, state.y);

        Debug.LogErrorFormat("GameTime: {0} State time {1}", owner_.Controller.GameTime, stateData.time);
        #endif
        //Debug.LogWarningFormat("Push new state with tick {0}", tick);
        interpolationSmoothValue_ += (DefaultInterpolationSmoothValue - interpolationSmoothValue_) * 0.1f;
    }

    private void UpdateInterpolationStates()
    {
        if (updateCount_ < 2)
        {
            return;
        }

        var renderTime = GetRenderTime();
        var size = states_.Size;
        var i = 0;
        for (; i < size; i++)
        {
            var state = states_[i];
            #if LOG
            //Debug.LogWarningFormat("State index {0} diff {1}", i, state.time - renderTime);
            #endif
            if (state.time > renderTime && state.time >= latestUpdateTime_)
            {
                var toIndex = Mathf.Max(0, i-1);
                toState_ = state;
                fromState_ = states_[toIndex];
                #if USE_HERMIT
                prevState_ = states_[Mathf.Max(0, toIndex-1)];
                #endif
                break;
            }
        }

        //Debug.LogFormat("latestUpdateTime_ {0} picked time with {1}", latestUpdateTime_, fromState_.time);

        if (toState_.tick == fromState_.tick && interpolationExtension_ < 1.0f)
        {
            if (newPacketReceived_)
            {
                //we offset time back in past to extend future interpolations
                timeWhenExtrapolationStarted_ = owner_.Controller.GameTime - DefaultInterpolationValue * 0.25f;
                newPacketReceived_ = false;
            }
            //so we have packet delay, lets try to push interpolation window for this entity
            interpolationExtension_ = (owner_.Controller.GameTime - timeWhenExtrapolationStarted_);

            //also low down smoothing value
            interpolationSmoothValue_ *= 0.5f;
            //Debug.LogErrorFormat("GameTime: {0} saved time {1}", owner_.Controller.GameTime, timeWhenExtrapolationStarted_);
            //Debug.LogErrorFormat("Push lerp window to {0} and smoothing to {1} on tick {2}", interpolationExtension_, interpolationSmoothValue_, toState_.tick);
        }


        latestUpdateTime_ = toState_.time;
   }

    private float GetRenderTime()
    {
        return (float)owner_.Controller.GameTime - DefaultInterpolationValue - interpolationExtension_;
    }

    public Vector2 GetRecentPosition()
    {
        if (states_.Size == 0) return Vector2.zero;
        var state = states_.Back();
        return state.state != null ? new Vector2(state.state.x, state.state.y) : Vector2.zero;
    }

    public void Reset()
    {
        updateCount_ = 0;
        interpolationExtension_ = 0;
        interpolationSmoothValue_ = DefaultInterpolationSmoothValue;
        latestUpdateTime_ = -1.0f;
        states_ = new CircularBuffer<StateData>(BufferSize);
    }

    private Vector2 HermitSpline(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 d1 = p1 - p0;
        Vector2 d2 = p2 - p1;

        Vector2 output;
        float tSqr = t*t;
        float tCube = t*tSqr;

        output = p1 * (2*tCube-3*tSqr+1);
        output += p2 * (-2*tCube+3*tSqr);
        output += d1 * (tCube-2*tSqr+t);
        output += d2 * (tCube-tSqr);

        return output;
    }

    public void Update()
    {
        UpdateInterpolationStates();

        if (fromState_.state == null || toState_.state == null)
        {
            return;
        }

        //we can't move in the past, only to present or future
        var currentRenderTime = Mathf.Max(GetRenderTime(), prevUpdateTime_);

        if (toState_.state == fromState_.state && GameApp.Instance.ExtrapolateNetworkedEntitiesTime > 0.01f && false)
        {
            fromState_ = states_[states_.Size - 2];
            var alpha = Mathf.Min(currentRenderTime - toState_.time, GameApp.Instance.ExtrapolateNetworkedEntitiesTime);
            var totalFrameTime = toState_.time - fromState_.time;
            alpha /= totalFrameTime;
            var x = Mathf.Lerp(fromState_.state.x, toState_.state.x, 1.0f + alpha);
            var y = Mathf.Lerp(fromState_.state.y, toState_.state.y, 1.0f + alpha);
            owner_.Position = new Vector2(x, y);

            #if LOG
            Debug.LogWarningFormat("Alpha {0} From pos {1} {2} To pos {3} {4} Pos {5} {6}", 
                alpha, fromState_.state.x,fromState_.state.y, toState_.state.x, toState_.state.y,
                owner_.Position.x,owner_.Position.y);
            #endif
        }
        else
        {
            var totalFrameTime = toState_.time - fromState_.time;
            var advancedFrameTime = currentRenderTime - fromState_.time;
            var alpha = totalFrameTime == 0.0f ? 0.0f : advancedFrameTime / totalFrameTime;
            alpha = Mathf.Min(alpha, 1.0f);
            #if USE_HERMIT
            var rotation = Mathf.LerpAngle(fromState_.state.rotation*Mathf.Rad2Deg, toState_.state.rotation*Mathf.Rad2Deg, alpha);
            var pos = HermitSpline(alpha, 
                new Vector2(prevState_.state.x,prevState_.state.y),
                new Vector2(fromState_.state.x, fromState_.state.y),
                new Vector2(toState_.state.x, toState_.state.y)
            );
            #else
            var rotation = Mathf.LerpAngle(fromState_.state.rotation*Mathf.Rad2Deg, toState_.state.rotation*Mathf.Rad2Deg, alpha);
            var x = Mathf.LerpUnclamped(fromState_.state.x, toState_.state.x, alpha);
            var y = Mathf.LerpUnclamped(fromState_.state.y, toState_.state.y, alpha);
            Vector2 pos = new Vector2(x,y);
            #endif
            owner_.Position = pos;
            owner_.Rotation = rotation;
        }

        prevUpdateTime_ = currentRenderTime;
        //smooth down interpolation extension back to normal
        interpolationExtension_ += -interpolationExtension_ * interpolationSmoothValue_;
        //Debug.LogFormat("Smooth lerp window to {0}", interpolationExtension_);
    }
}
