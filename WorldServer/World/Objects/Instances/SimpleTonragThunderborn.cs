using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances
{
    public class SimpleTonragThunderborn : InstanceBossSpawn
    {
        #region Constructors

        public SimpleTonragThunderborn(Creature_spawn spawn, uint instancegroupspawnid, uint bossid, ushort Instanceid, Instance instance) : base(spawn, instancegroupspawnid, bossid, Instanceid, instance)
        {
            EvtInterface.AddEvent(CheckDistanceAndUpdateAbilitiyDmgHeal, 500, 0);
        }

        #endregion Constructors

        #region Attributes

        private const float DMGHEAL_SCALE = 0.1f;

        #endregion Attributes

        #region Overrides

        public override void OnLoad()
        {
            base.OnLoad();

            AiInterface.SetBrain(new InstanceBossBrain(this));
        }
        
        #endregion Overrides

        #region Methods

        private void CheckDistanceAndUpdateAbilitiyDmgHeal()
        {
            try
            {
                // get your brother!
                Unit brother = (Unit)ObjectsInRange.Where(x => x is SimpleFulgurThunderborn).FirstOrDefault();
                if (brother == null || brother.IsDead || IsDead)
                {
                    EvtInterface.RemoveEvent(CheckDistanceAndUpdateAbilitiyDmgHeal);
                    return;
                }
                int distance = GetDistanceToObject(brother);

                // modify the scaler that is used in combatmanager
                brother.ModifyDmgHealScaler = ModifyDmgHealScaler = DMGHEAL_SCALE * distance;
                
                List<Player> plrs = GetPlayersInRange(300, false);
                foreach (Player plr in plrs)
                {
                    plr.ModifyDmgHealScaler = DMGHEAL_SCALE * distance;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                EvtInterface.RemoveEvent(CheckDistanceAndUpdateAbilitiyDmgHeal);
                return;
            }
        }

        #endregion Methods
    }
}
