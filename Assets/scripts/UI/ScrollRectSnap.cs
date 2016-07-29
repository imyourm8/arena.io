using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;


[RequireComponent(typeof(ScrollRect))]
public class ScrollRectSnap : MonoBehaviour
{
    public float snapSpeed = 0.3f;
    float[] points_;
    float stepSize_;
    Vector3 startPoint_;
    bool findNearest_ = false;
    ScrollRect scroll_;

    float targetH_;
    int currentPage_ = 0;

    private Sequence hSequence_;
    private Sequence vSequence_;

    public bool snapInH = true;

    float targetV_;
    public bool snapInV = false;
    public float dragThreshold = 40f;
    public float maxDrag = 200f;

    private Action<int> focusedOnCallback_;
    public Action<int> OnFocused
    {
        set { focusedOnCallback_ = value; }
    }

    // Use this for initialization
    void Awake()
    {
        if (snapInV == true && snapInH == true)
        {
            throw new UnityException("Can't snap on double axis!");
        }

        FindScroll();
    }

    void FindScroll()
    {
        if (scroll_ == null)
        {
            scroll_ = gameObject.GetComponent<ScrollRect>();
        }

        scroll_.inertia = false;
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        FindScroll();

        var screens = scroll_.content.transform.childCount;
        if (screens > 0)
        {
            points_ = new float[screens];

            if (screens == 1)
                stepSize_ = 1.0f;
            else
                stepSize_ = 1.0f / (float)(screens - 1);

            for (int i = 0; i < screens; i++)
            {
                points_[i] = i * stepSize_;
            }
        }
        else
        {
            points_ = new float[] { 0 };
        }
    }

    public void ResetPosition(bool toEnd = false)
    {
        if (scroll_.horizontal && snapInH)
        {
            scroll_.horizontalNormalizedPosition = toEnd ? 1 : 0;
        }
        else if (scroll_.vertical && snapInV)
        {
            scroll_.verticalNormalizedPosition = toEnd ? 1 : 0;
        }
    }

    public int ScrollToPage
    {
        set
        {
            if (points_ == null)
            {
                return;
            }

            currentPage_ = value;

            if (scroll_.horizontal && snapInH)
            {
                hSequence_ = DOTween.Sequence();

                targetH_ = points_[currentPage_];
                var tween = DOTween.To(() => scroll_.horizontalNormalizedPosition,
                            (x) => scroll_.horizontalNormalizedPosition = x,
                            targetH_, snapSpeed);

                hSequence_.Insert(0, tween);

                if (focusedOnCallback_ != null)
                {
                    focusedOnCallback_(currentPage_);
                }
            }
            else if (scroll_.vertical && snapInV)
            {
                vSequence_ = DOTween.Sequence();

                targetH_ = points_[currentPage_];
                var tween = DOTween.To(() => scroll_.verticalNormalizedPosition,
                            (x) => scroll_.verticalNormalizedPosition = x,
                            targetH_, snapSpeed);

                vSequence_.Insert(0, tween);

                if (focusedOnCallback_ != null)
                {
                    focusedOnCallback_(currentPage_);
                }
            }
        }
    }

    public int ScrollToPageForce
    {
        set
        {
            if (points_ == null)
            {
                return;
            }

            currentPage_ = value;

            if (scroll_.horizontal && snapInH)
            {
                scroll_.horizontalNormalizedPosition = points_[currentPage_];

                if (focusedOnCallback_ != null)
                {
                    focusedOnCallback_(currentPage_);
                }
            }
            else if (scroll_.vertical && snapInV)
            {
                scroll_.verticalNormalizedPosition = points_[currentPage_];

                if (focusedOnCallback_ != null)
                {
                    focusedOnCallback_(currentPage_);
                }
            }
        }
    }

    private Vector3 GetContentPos()
    {
        return scroll_.content.localPosition;
    }

    public void DragBegin()
    {
        startPoint_ = GetContentPos();

        if (hSequence_ != null)
        {
            hSequence_.Kill();
        }

        if (vSequence_ != null)
        {
            vSequence_.Kill();
        }
    }

    public void Drag()
    {
        var endPoint = GetContentPos();
        var distance = Vector3.Distance(endPoint, startPoint_);
        if (distance > maxDrag)
        {
            findNearest_ = true;
        }

        int prevPage = currentPage_;
        currentPage_ = GetNearestPage();

        if (currentPage_ != prevPage && focusedOnCallback_ != null)
            focusedOnCallback_(currentPage_);
    }

    public void DragEnd()
    {
        var endPoint = GetContentPos();
        var distance = Vector3.Distance(endPoint, startPoint_);

        if (distance > dragThreshold)
        {
            if (findNearest_)
            {
                currentPage_ = GetNearestPage();
            }
            else
            {
                var dir = (endPoint - startPoint_);
                dir.y += 0.000001f;
                dir.x += 0.000001f;

                currentPage_ -= Mathf.RoundToInt(Mathf.Sign(dir.x * dir.y));
                currentPage_ = Mathf.Min(currentPage_, points_.Length - 1);
                currentPage_ = Mathf.Max(0, currentPage_);
            }
        }

        findNearest_ = false;
        ScrollToPage = currentPage_;
    }

    private int GetNearestPage()
    {
        int currentPage = currentPage_;

        if (scroll_.horizontal && snapInH)
        {
            currentPage = FindNearest(scroll_.horizontalNormalizedPosition, points_);
        }
        else if (scroll_.vertical && snapInV)
        {
            currentPage = FindNearest(scroll_.verticalNormalizedPosition, points_);
        }

        return currentPage;
    }

    int FindNearest(float f, float[] array)
    {
        float distance = Mathf.Infinity;
        int output = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if (Mathf.Abs(array[index] - f) < distance)
            {
                distance = Mathf.Abs(array[index] - f);
                output = index;
            }
        }
        return output;
    }
}
