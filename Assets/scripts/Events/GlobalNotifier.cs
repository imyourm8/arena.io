using UnityEngine;
using System.Collections;

namespace Events 
{
    public sealed class GlobalNotifier : shared.Singleton<GlobalNotifier>, INotifier<string>
    {
        private INotifier<string> notifier_;
        
        public GlobalNotifier()
        {
            notifier_ = new Notifier<string>();
        }

        ISubscription<string> ISubscriber<string>.SubscribeOn(string evt, System.Action<IEvent<string>> listener)
        {
            return notifier_.SubscribeOn(evt, listener);
        }

        public void Trigger(string evtKey)
        {
            notifier_.Trigger(evtKey);
        }

        public void Trigger(IEvent<string> evt)
        {
            notifier_.Trigger(evt);
        }

        void ISubscriber<string>.UnSubscribeFor(string evt, System.Action<IEvent<string>> listener)
        {
            notifier_.UnSubscribeFor(evt, listener);
        }

        void ISubscriber<string>.UnSubscribeAllOn(string evt)
        {
            notifier_.UnSubscribeAllOn(evt);
        }
    }
}