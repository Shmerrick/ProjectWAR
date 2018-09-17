using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.AI
{
	public class InstanceBossBrain : ABrain
	{
		#region Constructors

		public InstanceBossBrain(Unit unit) : base(unit)
		{

		}

		#endregion Constructors

		#region Attributes

		#endregion Attributes

		#region Overrides
		
		public override void Think()
		{
			base.Think();
		}

		public override bool Start(Dictionary<ushort, AggroInfo> aggros)
		{
			return base.Start(aggros);
		}

		public override bool Stop()
		{
			return base.Stop();
		}

		public override void OnTaunt(Unit taunter, byte lvl)
		{
			base.OnTaunt(taunter, lvl);
		}

		public override void Fight()
		{
			base.Fight();
		}

		public override bool StartCombat(Unit fighter)
		{
			return base.StartCombat(fighter);
		}

		public override bool EndCombat()
		{
			return base.EndCombat();
		}
		
		public override Unit GetNextTarget()
		{
			return base.GetNextTarget();
		}

		public override void AddHatred(Unit fighter, bool isPlayer, long hatred)
		{
			base.AddHatred(fighter, isPlayer, hatred);
		}

		public override void AddHealReceive(ushort oid, bool isPlayer, uint count)
		{
			base.AddHealReceive(oid, isPlayer, count);
		}

		#endregion Overrides

		#region Methods

		#endregion Methods
	}
}
