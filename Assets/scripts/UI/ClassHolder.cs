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

    private Events.Slot<string> evtSlot_;

    public void Start()
    {
        if (unlocked)
        {
            SetUnlocked();
        }

        evtSlot_ = new Events.Slot<string>(Events.GlobalNotifier.Instance);
        evtSlot_.SubscribeOn(ClassSelection.UlockEvent, HandleUnlock);  

        if (playerClass == User.Instance.ClassSelected)
        {
            StartCoroutine(SwitchOn());
        }
    }

    private IEnumerator SwitchOn()
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
        SetUnlocked();
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
