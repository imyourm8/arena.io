using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using Nito;
using DG.Tweening;

public class MovementInterpolator
{
    struct MoveData
    {
        public long time;
        public Vector2 pos;
        public bool stop;

        public MoveData(long t, Vector2 p, bool s)
        {
            time = t;
            pos = p;
            stop = s;
        }
    }

    private const int MaxMovements = 5;
    private long interpolationTime_ = 0;
    private long defaultInterpolationTime_ = 0;
    private Entity owner_ = null;
    private Deque<MoveData> positions_; 
    private long lastPacketTime_ = 0;
    private Vector2 snapPos_ = Vector2.zero;
    private Vector2 position_ = Vector2.zero;
    private Tweener moveTweener_;

    public MovementInterpolator(Entity owner)
    {
        interpolationTime_ = (long)(GameApp.Instance.MovementInterpolationTime * 1000.0f);
        defaultInterpolationTime_ = interpolationTime_;
        owner_ = owner;
        positions_ = new Deque<MoveData>(MaxMovements);
    }

    public void PushNextMovement(long time, Vector2 position, bool stop)
    {
        if (time <= lastPacketTime_) 
            return;

        if (positions_.Count == MaxMovements)
            positions_.RemoveFromFront();

        positions_.AddToBack(new MoveData(time, position, stop));

        lastPacketTime_ = time;

        SnapToNextPosition();
    }

    public void Reset(Vector2 position)
    {
        position_ = position;
    }

    private void SnapToNextPosition()
    {
        long renderTime = GameApp.Instance.TimeMs() - interpolationTime_;
        long snapDuration = 0;
        long startTime = 0;
        Vector2 startPos = Vector2.zero;
        int count = positions_.Count;

        for(int i = 0; i < count; ++i)
        {
            var data = positions_[i];
            if (data.time <= renderTime && i != count-1)
            {
                startPos = data.pos;
                startTime = data.time;

                var nextData = positions_[i+1];
                snapPos_ = nextData.pos;
                snapDuration = nextData.time - data.time;
            }
        }

        if (snapDuration > 0)
        {
            //we found smth to interpolate between
            //calculate position on render time 
            long timeElapsed = renderTime - startTime;
            float dtElapsed = (float)timeElapsed / (float)snapDuration;
            //correct left snap duration
            //snapDuration -= timeElapsed;

            //startPos.x = Mathf.Lerp(startPos.x, snapPos_.x, dtElapsed);
            //startPos.y = Mathf.Lerp(startPos.y, snapPos_.y, dtElapsed);

            startPos = position_;

            if (moveTweener_ != null)
                moveTweener_.Kill();

            position_ = startPos;
            moveTweener_ = DOTween.To(()=>position_, x=>position_=x,snapPos_,(float)snapDuration/1000.0f);
            moveTweener_.OnComplete(SnapToNextPosition);
        }
    }

    private void Estimates(long newPacketTime)
    {
        if (positions_.Count == 0)
        {
            lastPacketTime_ = newPacketTime;
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

        interpolationTime_ = System.Math.Max(defaultInterpolationTime_, interpolationTime_);
    }

    public Vector2 GetPosition()
    {
        return position_;
    }
}
