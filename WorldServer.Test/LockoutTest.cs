using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace WorldServer.Test
{
	[TestClass]
	public class LockoutTest
	{
		[TestMethod]
		public void MyLockoutTest()
		{
			// test init
			Character_value charValue = new Character_value();

			// Test 1:
			// Player Lockouts empty, add new Lockout
			Instance_Lockouts Lockout = new Instance_Lockouts
			{
				InstanceID = "~260:10000",
				Bosseskilled = "1:2:5"
			};
			charValue.AddLockout(Lockout);
			Assert.AreEqual(charValue.Lockouts, "~260:10000:1:2:5");

			// Test 2:
			// Player Lockouts filled, add new Lockout of same zoneID
			Lockout = new Instance_Lockouts
			{
				InstanceID = "~260:10000",
				Bosseskilled = "3:4"
			};
			charValue.AddLockout(Lockout);
			Assert.AreEqual(charValue.Lockouts, "~260:10000:1:2:3:4:5");

			// Test 3:
			// Player Lockouts filled, add new Lockout of other zoneID
			Lockout = new Instance_Lockouts
			{
				InstanceID = "~197:20000",
				Bosseskilled = "6:9"
			};
			charValue.AddLockout(Lockout);
			Assert.AreEqual(charValue.Lockouts, "~260:10000:1:2:3:4:5~197:20000:6:9");
			Lockout = new Instance_Lockouts
			{
				InstanceID = "~100:30000",
				Bosseskilled = "331:789"
			};
			charValue.AddLockout(Lockout);
			Assert.AreEqual(charValue.Lockouts, "~260:10000:1:2:3:4:5~197:20000:6:9~100:30000:331:789");

			// Test 4:
			// Player get all Lockouts
			List<string> allLockouts = charValue.GetAllLockouts();
			string allLo = string.Empty;
			foreach (string lo in allLockouts)
				allLo += lo;
			Assert.AreEqual(allLo, "~260:10000:1:2:3:4:5~197:20000:6:9~100:30000:331:789");

			// Test 5:
			// Player Lockout filled, remove a Lockout of containing zoneID
			charValue.RemoveLockout("~100:30000:331:789");
			Assert.AreEqual(charValue.Lockouts, "~260:10000:1:2:3:4:5~197:20000:6:9");

			// Test 6:
			// Player Lockout filled, try to remove a Lockout of not-containing zoneID
			charValue.RemoveLockout("~42:9999:12:43");
			Assert.AreEqual(charValue.Lockouts, "~260:10000:1:2:3:4:5~197:20000:6:9");

			// Test 7:
			// Player get Lockout positive
			string validLo = charValue.GetLockout(260);
			Assert.AreEqual(validLo, "~260:10000:1:2:3:4:5");

			// Test 8:
			// Player get Lockout negative
			string invalidLo = charValue.GetLockout(42);
			Assert.IsNull(invalidLo);
		}
	}
}
