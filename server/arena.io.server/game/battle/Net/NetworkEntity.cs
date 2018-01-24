using proto_common;

namespace arena.battle.Net
{
    public struct EventPacket
    {
        public EventPacket(Events eventID, object packet):this()
        {
            Packet = packet;
            EventID = eventID;
        }
        public object Packet
        {
            get;
            private set;
        }
        public Events EventID
        {
            get;
            private set;
        }
        public bool IsValid
        {
            get { return Packet != null; }
        }
    }

    public class NetworkEntity
    {
        public virtual EventPacket GetAppearedPacket()
        {
            return ConstructPacket(Events.UNKNOWN_EVT, null);
        }

        protected EventPacket ConstructPacket(Events evt, object packet)
        {
            return new EventPacket(evt, packet);
        }
    }
}
