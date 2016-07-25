using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

public class ClassHolder : MonoBehaviour 
{
    public Action<proto_profile.PlayerClasses, ClassHolder> OnUnlock;
    public Action<proto_profile.PlayerClasses> OnSelect;

    [SerializeField]
    private proto_profile.PlayerClasses playerClass = proto_profile.PlayerClasses.Assault;

    [SerializeField]
    private bool unlocked = false;

    [SerializeField]
    private GameObject disabledOverlay = null;

    [SerializeField]
    private Text unlockPrice = null;

    private Events.Slot<string> evtSlot_;
    private proto_profile.ClassInfo info_;

    public proto_profile.PlayerClasses Class
    { get { return playerClass; }}

    void Awake()
    {
        evtSlot_ = new Events.Slot<string>(Events.GlobalNotifier.Instance);
        evtSlot_.SubscribeOn(ClassSelection.UlockEvent, HandleUnlock);

        if (unlocked)
        {
            SetUnlocked();
        } 
    }

    void Start()
    {
        if (unlockPrice != null)
            unlockPrice.text = info_.coinsPrice.ToString();
    }

    public void Refresh()
    {
        info_ = User.Instance.Classes.Find((proto_profile.ClassInfo info)=>
        {
            return info.@class == Class;
        });

        if (info_ != null)
        {
            if (info_.levelRequired <= User.Instance.Level || info_.unlocked)
            {
                SetUnlocked();
            }
        }

        if (Class == User.Instance.ClassSelected)
        {
            SwitchOn();
        }
    }

    private void SwitchOn()
    {
        //StartCoroutine(SwitchOnInternal());
        GetComponent<Toggle>().isOn = true;
    }

    private IEnumerator SwitchOnInternal()
    {
        yield return null;
        GetComponent<Toggle>().isOn = true;
    }

    public void HandleValueChanged(bool value)
    {
        if (value && OnSelect != null && unlocked)
        {
            OnSelect(playerClass);
        }
    }

    private void HandleUnlock(Events.IEvent<string> evt)
    {
        if (evt.UserData == this)
        {
            SetUnlocked();
        }
    }

    private void SetUnlocked()
    {
        disabledOverlay.SetActive(false);
        unlocked = true;
    }

    public void Unlock()
    {
        if (OnUnlock != null)
        {
            OnUnlock(playerClass, this);
        }
    }
}
