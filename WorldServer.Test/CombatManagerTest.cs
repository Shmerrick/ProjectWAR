using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Components;

namespace WorldServer.Test
{
	[TestClass]
	public class CombatManagerTest
	{
		// https://www.returnofreckoning.com/forum/viewtopic.php?t=971

		[TestMethod]
		public void CheckDefenceTest()
		{
			// init values
			AbilityDamageInfo dmgInfo = new AbilityDamageInfo
			{
				Defensibility = 0
			};

			// BLOCK

			// 357 is block rating of sc 39 shield
			// 1050 is primary stat on soft cap
			double block1 = CombatManager.CalculateBlockRoll(357, 1050, dmgInfo, 0, 0);
			Assert.IsTrue(block1 >= 0 && block1 <= 100);
			// 484 is block rating of souvereign shield
			// 1050 is primary stat on soft cap
			double block2 = CombatManager.CalculateBlockRoll(484, 1050, dmgInfo, 0, 0);
			Assert.IsTrue(block2 >= 0 && block2 <= 100);
			// 577 is block rating of 100 shield
			// 1050 is primary stat on soft cap
			double block3 = CombatManager.CalculateBlockRoll(577, 1050, dmgInfo, 0, 0);
			Assert.IsTrue(block3 >= 0 && block3 <= 100);

			// REST (PARRY, DODGE, DISRUPT)

			double rest = CombatManager.CalculatePDDRoll(900, 1050, dmgInfo, 10, 3);
			Assert.IsTrue(rest >= 0 && rest <= 100);
		}
	}
}
