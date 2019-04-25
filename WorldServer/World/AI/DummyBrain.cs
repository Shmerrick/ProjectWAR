using WorldServer.World.Objects;

namespace WorldServer.World.AI
{ 
    class DummyBrain : ABrain
    {
        public DummyBrain(Unit myOwner)
            : base(myOwner)
        {

        }

        public override bool StartCombat(Unit fighter)
        {
            return false;
        }

        public override bool EndCombat()
        {
            return false;
        }

        public override void OnTaunt(Unit taunter, byte lvl)
        {

        }

        public override void AddHatred(Unit fighter, bool isPlayer, long hatred)
        {
            
        }

        public override void AddHealReceive(ushort oid, bool isPlayer, uint count)
        {
            
        }
    }
}
