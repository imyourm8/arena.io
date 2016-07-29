using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

using System.Collections.Generic;

public class AnimatedProgress : MonoBehaviour 
{
    public enum FillType
    {
        Image,
        Transform
    }
    
    public enum ScaleDirection
    {
        ToRight,
        ToLeft,
        ToTop,
        ToBottom
    }

    [SerializeField]
    private CanvasGroup alphaGroup_;

    [SerializeField]
    private float fillDuration_ = 0.2f;
    
    [SerializeField]
    private Ease fillEase_ = Ease.OutCubic;
    
    [SerializeField]
    private FillType fill_ = FillType.Image;

    [SerializeField]
    private Image imageFill_;
    
    [SerializeField]
    private Transform transformFill_;
    
    [SerializeField]
    private ScaleDirection scaleDirection_;

    [SerializeField]
    private float showSmoothDuration_ = 0.1f;

    [SerializeField]
    private Ease showEase_ = Ease.OutCubic;

    [SerializeField]
    private float hideSmoothDuration_ = 0.35f;

    [SerializeField]
    private Ease hideEase_ = Ease.OutCubic;

    [SerializeField]
    private bool startHidden = true;

    private float progress_ = 0.5f;
    private float maxProgress_ = 1.0f;
    private Tweener hideTweener_ = null;
    private bool hidden_ = false;
    
    private class ProgressPointInfo
    {
        public float progressValue;
        public bool eventTriggered;
        
        public ProgressPointInfo(float pV, bool eT)
        {
            progressValue = pV;
            eventTriggered = eT;
        }
    }

    private HashSet<ProgressPointInfo> progressPointsInfo_ = null;
    
    public event System.Action<float> OnProgressPointReached = delegate { };

    public bool Hidden
    {
        get { return hidden_; }
    }

    public float MaxProgress
    {
        get { return maxProgress_; }
        set { maxProgress_ = value; }
    }
    
    public float Progress
    {            
        get { return progress_; }
        set { Fill(value, true); }
    }
    
    public void SetProgressPoints(HashSet<float> progressPoints)
    {
        progressPointsInfo_ = new HashSet<ProgressPointInfo>();
        
        foreach (var progressPoint in progressPoints)
        {
            var info = new ProgressPointInfo(progressPoint, false);
            progressPointsInfo_.Add(info);
        }
    }

    void Start()
    {
        if (startHidden)
        {
            HideSmooth();
        }
        else 
        {
            ShowSmooth();
        }
    }

    public void Hide()
    {
        alphaGroup_.alpha = 0.0f;
    }

    public void SetProgreessNotAnimated(float progress)
    {
        Fill(progress, false);
    }

    public AnimatedProgress ShowSmooth()
    {
        if (!Hidden) return this;
        hidden_ = false;
        if (alphaGroup_.alpha > 0.99f) return this;
        if (hideTweener_ != null)
            hideTweener_.Kill();

        hideTweener_ = alphaGroup_.DOFade(1.0f, showSmoothDuration_);

        return this;
    }

    public AnimatedProgress HideSmooth()
    {
        if (Hidden) return this;
        hidden_ = true;
        if (alphaGroup_.alpha < 0.001f) return this;
        if (hideTweener_ != null)
            hideTweener_.Kill();

        hideTweener_ = alphaGroup_.DOFade(0.0f, hideSmoothDuration_);

        return this;
    }

    private void Fill(float progress, bool animated)
    {
        progress_ = Mathf.Min(progress, maxProgress_);
        float fillValue = progress_ / maxProgress_;
        
        if (animated)
        {
            if (fill_ == FillType.Image)
            {
                FillImageAnimated(fillValue);
            }
            else 
            {
                FillTransformAnimated(fillValue);
            }
        }
        else
        {
            if (fill_ == FillType.Image)
            {
                imageFill_.fillAmount = fillValue;
            }
            else
            {
                transformFill_.localScale = CalcTransformScale(fillValue);
            }
        }
    }
    
    private void FillImageAnimated(float fill)
    {
        var tweener = imageFill_.DOFillAmount(fill, fillDuration_).SetEase(fillEase_);
        
        if (progressPointsInfo_ != null)
        {
            tweener.OnUpdate(CheckProgressPoints);
        }
    }
    
    private void FillTransformAnimated(float fill)
    {
        var scale = CalcTransformScale(fill);            
        var tweener = transformFill_.DOScale(scale, fillDuration_).SetEase(fillEase_);
        
        if (progressPointsInfo_ != null)
        {
            tweener.OnUpdate(CheckProgressPoints);
        }
    }
    
    private Vector3 CalcTransformScale(float fillValue)
    {
        Vector3 scale = transformFill_.localScale;
                    
        switch (scaleDirection_)
        {
            case ScaleDirection.ToRight:
                scale = new Vector3(fillValue, scale.y, scale.z);
                break;
            case ScaleDirection.ToLeft:
                scale = new Vector3(-fillValue, scale.y, scale.z);
                break;
            case ScaleDirection.ToTop:
                scale = new Vector3(scale.x, fillValue, scale.z);
                break;
            case ScaleDirection.ToBottom:
                scale = new Vector3(scale.x, -fillValue, scale.z);
                break;
        }
        
        return scale;
    }
    
    private float GetCurrentFill()
    {
        var currentFill = 0.0f;
        
        switch (fill_)
        {
            case FillType.Image:
                currentFill = imageFill_.fillAmount;
                break;
                
            case FillType.Transform:
                var currentScale = transformFill_.localScale;
           
                switch (scaleDirection_)
                {
                    case ScaleDirection.ToRight:
                        currentFill = currentScale.x;
                        break;
                    case ScaleDirection.ToLeft:
                        currentFill = -currentScale.x;
                        break;
                    case ScaleDirection.ToTop:
                        currentFill = currentScale.y;
                        break;
                    case ScaleDirection.ToBottom:
                        currentFill = -currentScale.y;
                        break;
                }
                break;
        }
        
        return currentFill;
    }
    
    private void CheckProgressPoints()
    {
        if (progressPointsInfo_ != null)
        {
            var currentFill = GetCurrentFill();
            
            foreach (var progressPointInfo in progressPointsInfo_)
            {
                if (!progressPointInfo.eventTriggered && currentFill >= progressPointInfo.progressValue)
                {
                    progressPointInfo.eventTriggered = true;
                    OnProgressPointReached(progressPointInfo.progressValue);
                }
            }
        }
    }
}