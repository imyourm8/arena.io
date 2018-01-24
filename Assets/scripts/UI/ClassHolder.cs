using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

public class ClassHolder : MonoBehaviour 
{
    public Action<proto_profile.PlayerClasses, ClassHolder> OnUnlock;
    public Action<proto_profile.PlayerClasses> OnSelect;

    [SerializeField]
    private proto_profile.PlayerClasses playerClass = proto_profile.PlayerClasses.TypeA;

    [SerializeField]
    private bool unlocked = false;

    [SerializeField]
    private GameObject disabledOverlay = null;

    [SerializeField]
    private Text unlockPrice = null;

    private Events.Slot<string> evtSlot_;
    private proto_profile.ClassInfo info_;
    private Toggle toggle_ = null;

    public proto_profile.PlayerClasses Class
    { get { return playerClass; }}

    void Awake()
    {
        evtSlot_ = new Events.Slot<string>(Events.GlobalNotifier.Instance);
        evtSlot_.SubscribeOn(ClassSelection.UlockEvent, HandleUnlock);
        toggle_ = GetComponent<Toggle>();
        toggle_.onValueChanged.AddListener(HandleValueChanged);

        if (unlocked)
        {
            SetUnlocked();
        } 
    }

    void Start()
    {
    }

    public void Refresh()
    {
        info_ = User.Instance.Classes.Find((proto_profile.ClassInfo info)=>
        {
            return info.@class == Class;
        });

        //if (info_ != null)
        //{
        //    if (info_.levelrequired <= user.instance.level || info_.unlocked)
        //    {
        //        setunlocked();
        //    }
        //}

        if (Class == User.Instance.ClassSelected)
        {
            SwitchOn();
        }
    }

    private void SwitchOn()
    {
        if (toggle_ != null)
            toggle_.isOn = true;
    }

    private IEnumerator SwitchOnInternal()
    {
        yield return null;
        if (toggle_ != null)
            toggle_.isOn = true;
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
        if ((evt.UserData as ClassHolder) == this)
        {
            SetUnlocked();
        }
    }

    private void SetUnlocked()
    {
        if (disabledOverlay != null)
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
