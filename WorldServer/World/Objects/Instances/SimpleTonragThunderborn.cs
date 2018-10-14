using Common;
using System;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances
{
    public class SimpleTonragThunderborn : InstanceBossSpawn
    {
        #region Constructors

        public SimpleTonragThunderborn(Creature_spawn spawn, uint instancegroupspawnid, uint bossid, ushort Instanceid, Instance instance) : base(spawn, instancegroupspawnid, bossid, Instanceid, instance)
        {
            EvtInterface.AddEvent(CheckDistanceAndUpdateAbilitiyDmg, 500, 0);
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

        #endregion Overrides

        #region Methods

        private void CheckDistanceAndUpdateAbilitiyDmg()
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                EvtInterface.RemoveEvent(CheckDistanceAndUpdateAbilitiyDmg);
                return;
            }
        }

        #endregion Methods
    }
}
