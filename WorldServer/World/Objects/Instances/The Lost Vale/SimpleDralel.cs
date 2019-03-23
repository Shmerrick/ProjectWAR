using Common;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances.The_Lost_Vale
{
    public class SimpleDralel : InstanceBossSpawn
    {
        #region Constructors

        public SimpleDralel(Creature_spawn spawn, uint bossId, ushort Instanceid, Instance instance) : base(spawn, bossId, Instanceid, instance)
        {

        }

        #endregion Constructors

        #region Attributes

        #endregion Attributes

        #region Overrides

        public override void OnLoad()
        {
            base.OnLoad();

            AiInterface.SetBrain(new InstanceBossBrain(this));
        }
        
        public override bool OnEnterCombat(Object mob, object args)
        {
            bool res = base.OnEnterCombat(mob, args);
            EvtInterface.AddEvent(RemoveAllCCImmunitiesFromTanks, 1000, 0);
            return res;
        }

        public override bool OnLeaveCombat(Object mob, object args)
        {
            bool res = base.OnLeaveCombat(mob, args);
            EvtInterface.RemoveEvent(RemoveAllCCImmunitiesFromTanks);
            return res;
        }

        #endregion Overrides

        #region Methods
        
        /// <summary>
        /// 402, 403 = Unstoppable
        /// 408 = Immovable
        /// </summary>
        private void RemoveAllCCImmunitiesFromTanks()
        {
            foreach (var plr in PlayersInRange)
            {
                // only clean from tanks
                if (plr.CrrInterface.GetArchetype() == EArchetype.ARCHETYPE_Tank)
                {
                    plr.BuffInterface.RemoveBuffByEntry(408); // Removing Immunity
                    NewBuff newBuff = plr.BuffInterface.GetBuff(408, null);
                    if (newBuff != null)
                        newBuff.RemoveBuff(true);

                    plr.BuffInterface.RemoveBuffByEntry(402); // Removing Immunity
                    newBuff = plr.BuffInterface.GetBuff(402, null);
                    if (newBuff != null)
                        newBuff.RemoveBuff(true);

                    plr.BuffInterface.RemoveBuffByEntry(403); // Removing Immunity
                    newBuff = plr.BuffInterface.GetBuff(403, null);
                    if (newBuff != null)
                        newBuff.RemoveBuff(true);

                    plr.BuffInterface.RemoveBuffByEntry(4384); // Removing Whitefire Webbing
                    newBuff = plr.BuffInterface.GetBuff(4384, null);
                    if (newBuff != null)
                        newBuff.RemoveBuff(true);
                }
            }
        }

        #endregion Methods
    }
}
