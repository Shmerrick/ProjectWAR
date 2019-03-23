using Common;
using WorldServer.World.AI;

namespace WorldServer.World.Objects.Instances.The_Lost_Vale
{
	public class SimpleMalghorGreathorn : InstanceBossSpawn
	{
		#region Constructors

		public SimpleMalghorGreathorn(Creature_spawn spawn, uint bossId, ushort Instanceid, Instance instance) : base(spawn, bossId, Instanceid, instance)
		{
			
		}

        #endregion Constructors

        #region Attributes

        //private List<List<object>> _AddSpawns = new List<List<object>>()
        //{
        //    // shaman: 6861, rager: 6828, crusher: 6836
        //    new List<object> { new List<uint> { 6861, 6828, 6836 }, 1399748, 1582516, 7849, 3362 },
        //    new List<object> { new List<uint> { 6861, 6828, 6836 }, 1400719, 1582162, 8019, 88 },
        //    new List<object> { new List<uint> { 6861, 6828, 6836 }, 1401783, 1581864, 8140, 580 },
        //    new List<object> { new List<uint> { 6861, 6828, 6836 }, 1402250, 1582949, 8019, 1066 },
        //    new List<object> { new List<uint> { 6861, 6828, 6836 }, 1401582, 1583676, 7891, 1212 }
        //};

        #endregion Attributes

        #region Overrides

        public override void OnLoad()
		{
			base.OnLoad();

			AiInterface.SetBrain(new InstanceBossBrain(this));
		}

        //public override bool OnEnterCombat(Object mob, object args)
        //{
        //    bool res = base.OnEnterCombat(mob, args);
        //    EvtInterface.AddEvent(SpawnAdds, 10 * 1000, 0);
        //    return res;
        //}

        //public override bool OnLeaveCombat(Object mob, object args)
        //{
        //    bool res = base.OnLeaveCombat(mob, args);
        //    EvtInterface.RemoveEvent(SpawnAdds);
        //    return res;
        //}

        #endregion Overrides

        #region Methods

        //private void SpawnAdds()
        //{
        //    try
        //    {
        //        SpawnAdds(_AddSpawns);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex.Message + "\r\n" + ex.StackTrace);
        //        EvtInterface.RemoveEvent(SpawnAdds);
        //        return;
        //    }
        //}

        #endregion Methods
    }
}
