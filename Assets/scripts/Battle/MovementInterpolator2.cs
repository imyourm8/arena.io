using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using DG.Tweening;
using DG.Tweening.Core;

using CircularBuffer;
using Nito;

public class MovementInterpolator2
{
    struct MoveData
    {
        public long time;
        public Vector2 pos;
    }

    private const int MaxMovements = 5;
    private long lastPacketTime_ = 0;
    private long defaultInterpolationTime_ = 0;
    private long interpolationTime_ = 0;
    private Entity owner_ = null;
    private Deque<MoveData> positions_;
    private Vector2 snapVelocity_ = Vector2.zero;
    private Vector2 velocity_ = Vector2.zero;
    private Vector2 snapPos_ = Vector2.zero;
    private Vector2 aimPos_ = Vector2.zero;
    private Vector2 lastPacketPos_ = Vector2.zero;
    private long snapTime_ = 0;
    private long aimTime_ = 0;
    private Vector2 stopPos_ = Vector2.zero;
    private bool stop_ = true;
    private Tweener stopTweener_;

    public MovementInterpolator2(Entity owner)
    {
        interpolationTime_ = (long)(GameApp.Instance.MovementInterpolationTime * 1000.0f);
        defaultInterpolationTime_ = interpolationTime_;
        owner_ = owner;
        positions_ = new Deque<MoveData>(MaxMovements);
    }

    public void PushNextMovement(long time, Vector2 position, bool stop)
    {
        if (lastPacketTime_ >= time) return;
        Estimates(time);

        long currentTime = GameApp.Instance.ServerTimeMs();

        if (stop_ && !stop)
        {
            if (stopTweener_ != null)
                stopTweener_.Kill();
            stopTweener_ = null;
            Reset();
        } 
        else if (!stop_ && stop)
        {
            stopPos_ = snapPos_;
            stopTweener_ = DOTween.To(()=>stopPos_, x=>stopPos_ = x, position, 0.2f);
        }


        float dt = 1.0f / ((float)(time - lastPacketTime_) / 1000.0f);

        stop_ = stop;
        Vector2 estimatedVelocity = Vector2.zero;
        estimatedVelocity.x = (position.x - lastPacketPos_.x) * dt;
        estimatedVelocity.y = (position.y - lastPacketPos_.y) * dt;

        lastPacketPos_ = position;
        lastPacketTime_ = time;

        snapPos_ = ReadPosition(currentTime);
        aimTime_ = currentTime + interpolationTime_;

        float aimDT = (float)(aimTime_ - time) / 1000.0f;
        snapTime_ = currentTime;
        aimPos_.x = position.x + estimatedVelocity.x * aimDT;
        aimPos_.y = position.y + estimatedVelocity.y * aimDT;

        if (Mathf.Abs(aimTime_ - snapTime_) < 1e-4)
        {
            snapVelocity_ = estimatedVelocity;
        } 
        else 
        {
            float velDt = 1.0f / ((float)(aimTime_ - snapTime_) / 1000.0f);
            snapVelocity_.x = (aimPos_.x - snapPos_.x) * velDt;
            snapVelocity_.y = (aimPos_.y - snapPos_.y) * velDt;
        }

        /*
        if (positions_.Count == 0 && !localPush)
            PushNextMovement(currentTime, owner_.Position, true);

        if (positions_.Count == MaxMovements) 
            positions_.RemoveFromFront();

        

        var moveData = new MoveData();
        moveData.time = time;
        moveData.pos = position;
        positions_.AddToBack(moveData);
        */
    }

    public void Reset()
    {
        positions_.Clear();
        lastPacketTime_ = GameApp.Instance.ServerTimeMs();
        interpolationTime_ = defaultInterpolationTime_;
        snapTime_ = GameApp.Instance.ServerTimeMs();
        aimTime_ = snapTime_ + interpolationTime_;
        velocity_ = Vector2.zero;
        aimPos_ = Vector2.zero;
        snapPos_ = owner_.Position;
        lastPacketPos_ = snapPos_;
    }

    private void Estimates(long newPacketTime)
    {
        if (positions_.Count == 0)
        {
            //lastPacketTime_ = newPacketTime;
        }

        var tick = newPacketTime - lastPacketTime_;
        if (tick > interpolationTime_)
        {
            interpolationTime_ = (interpolationTime_ + tick) / 2;
        }
        else 
        {
            interpolationTime_ = (interpolationTime_ * 7 + tick) / 8;
        }
    }

    public Vector2 ReadPosition(long time)
    {
        bool ok = true;
        if (time < snapTime_)
        {
            ok = false;
            time = snapTime_;
        }

        long maxRange = aimTime_ + interpolationTime_;
        if (time > maxRange)
        {
            ok = false;
            time = maxRange;
        }

        velocity_ = snapVelocity_;

        Vector2 pos = Vector2.zero;

        if (!stop_)
        {
            float dt = (float)(time - snapTime_) / 1000.0f;
            pos.x = snapPos_.x + velocity_.x * dt;
            pos.y = snapPos_.y + velocity_.y * dt;
        }
        else 
        {
            pos = stopPos_;
        }

        if (!ok)
        {
            velocity_ = Vector2.zero;
        }

        return pos;
    }

    public Vector2 GetPosition()
    {
        return ReadPosition(GameApp.Instance.ServerTimeMs());
        /*
        if (positions_.Count < 2) 
            return owner_.Position;

        var renderTime = GameApp.Instance.TimeMs() - interpolationTime_;
        //now find packets with this time

        MoveData startPos = positions_[0];
        MoveData finalPos = startPos;
        foreach(var moveData in positions_)
        {
            if (moveData.time <= renderTime)
            {
                startPos = moveData;
                finalPos = moveData;
            } 
            else if (moveData.time > renderTime)
            {
                finalPos = moveData;
                break;
            }
        }

        float interpolationTime = 0.0f;

        if (finalPos.time == startPos.time)
        {
            return new Vector2(finalPos.pos.x, finalPos.pos.y);
        }
        else
        {
            long startTime = startPos.time;
            long timeDiff = (finalPos.time - startTime);

            //timeDiff = timeDiff > interpolationTime_ ? interpolationTime_ : timeDiff;
            //startTime = finalPos.time - timeDiff;
            interpolationTime = (float)(renderTime-startTime) / (float)timeDiff;
        }

        return new Vector2(Mathf.Lerp(startPos.pos.x, finalPos.pos.x, interpolationTime), 
            Mathf.Lerp(startPos.pos.y, finalPos.pos.y, interpolationTime)); */
    }
}
