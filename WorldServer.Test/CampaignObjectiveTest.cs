using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.Services.World;
using Common;
using WorldServer.World.Objects;

namespace WorldServer.Test
{
	[TestClass]
	public class CampaignObjectiveTest
	{
		#region TestMembers

		#endregion

		#region TestMethods

		[TestMethod]
		public void CampaignObjectiveRewardVPTest()
		{
			// arrange
			var flag = new BattlefieldObjective();

			// act
			var VP = flag.RewardManager.RewardCaptureTick(new HashSet<Player>(), GameData.Realms.REALMS_REALM_ORDER, 4, "thisFlag", 1f, BORewardType.CAPTURING);
			// assert
			Assert.AreEqual(VP.OrderVictoryPoints, 15f, 0f);
			Assert.AreEqual(VP.DestructionVictoryPoints, 0f, 0f);
			// act
			VP = flag.RewardManager.RewardCaptureTick(new HashSet<Player>(), GameData.Realms.REALMS_REALM_DESTRUCTION, 4, "thisFlag", 1f, BORewardType.CAPTURING);
			// assert
			Assert.AreEqual(VP.DestructionVictoryPoints, 15f, 0f);
			Assert.AreEqual(VP.OrderVictoryPoints, 0f, 0f);
			// act
			VP = flag.RewardManager.RewardCaptureTick(new HashSet<Player>(), GameData.Realms.REALMS_REALM_ORDER, 4, "thisFlag", 1f, BORewardType.CAPTURED);
			// assert
			Assert.AreEqual(VP.OrderVictoryPoints, 200f, 0f);
			Assert.AreEqual(VP.DestructionVictoryPoints, 0f, 0f);
			// act
			VP = flag.RewardManager.RewardCaptureTick(new HashSet<Player>(), GameData.Realms.REALMS_REALM_DESTRUCTION, 4, "thisFlag", 1f, BORewardType.CAPTURED);
			// assert
			Assert.AreEqual(VP.DestructionVictoryPoints, 200f, 0f);
			Assert.AreEqual(VP.OrderVictoryPoints, 0f, 0f);
			// act
			VP = flag.RewardManager.RewardCaptureTick(new HashSet<Player>(), GameData.Realms.REALMS_REALM_ORDER, 4, "thisFlag", 1f, BORewardType.GUARDED);
			// assert
			Assert.AreEqual(VP.OrderVictoryPoints, 30f, 0f);
			Assert.AreEqual(VP.DestructionVictoryPoints, 0f, 0f);
			// act
			VP = flag.RewardManager.RewardCaptureTick(new HashSet<Player>(), GameData.Realms.REALMS_REALM_DESTRUCTION, 4, "thisFlag", 1f, BORewardType.GUARDED);
			// assert
			Assert.AreEqual(VP.DestructionVictoryPoints, 30f, 0f);
			Assert.AreEqual(VP.OrderVictoryPoints, 0f, 0f);
		}

		#endregion
	}
}
