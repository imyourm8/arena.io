using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class PickUp : Entity
    {
        public PickUp()
        {
            Category = PhysicsDefs.Category.PICKUPS;
            TrackSpatially = false;
            ReplicateOnClients = true;
            sensorBody_ = true;
            additionalCollisionMask_ = (ushort)PhysicsDefs.Category.PLAYER;
        }

        public proto_game.Pickups PickupType
        { get; set; }

        public PickUpEntry Entry
        { get; private set; }

        public virtual void OnPickUpBy(Player player)
        { }

        public override void InitPhysics()
        {
            Entry = Factories.PickUpFactory.Instance.GetEntry(PickupType);
            Radius = Entry.CollisionRadius;

            base.InitPhysics();
        }
    }
}
