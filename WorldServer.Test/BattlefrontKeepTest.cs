using System.Collections.Generic;
using Appccelerate.StateMachine;
using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.BattleFronts.Keeps;
using FakeItEasy;

namespace WorldServer.Test
{
    [TestClass]
    public class BattlefrontKeepTest
    {
        public PassiveStateMachine<SM.ProcessState, SM.Command> fsm { get; set; }

        public List<BattlefieldObjective> Region1BOList { get; set; }
        public List<BattlefieldObjective> Region3BOList { get; set; }
        public List<RegionMgr> RegionMgrs { get; set; }
        public IKeepCommunications FakeKeepComms { get; set; }
        public IApocCommunications FakeApocComms { get; set; }
        public RegionMgr Region1 { get; set; }
        public RegionMgr Region3 { get; set; }



        [TestInitialize]
        public void Setup()
        {
            FakeKeepComms = A.Fake<IKeepCommunications>();
            FakeApocComms = A.Fake<IApocCommunications>();
            RegionMgrs = new List<RegionMgr>();

            var R1ZoneList = new List<Zone_Info>();
            R1ZoneList.Add(new Zone_Info { ZoneId = 200, Name = "R1Zone200 PR", Pairing = 2, Tier = 4 });
            R1ZoneList.Add(new Zone_Info { ZoneId = 201, Name = "R1Zone201 CW", Pairing = 2, Tier = 4 });

            var R3ZoneList = new List<Zone_Info>();
            R3ZoneList.Add(new Zone_Info { ZoneId = 400, Name = "R3Zone400 TM", Pairing = 1, Tier = 4 });
            R3ZoneList.Add(new Zone_Info { ZoneId = 401, Name = "R3Zone401 KV", Pairing = 1, Tier = 4 });

            Region1 = new RegionMgr(1, R1ZoneList, "Region1", FakeApocComms);
            Region3 = new RegionMgr(3, R3ZoneList, "Region3", FakeApocComms);


            RegionMgrs.Add(Region1);
            RegionMgrs.Add(Region3);
        }


        [TestMethod]
        public void CanStartFSM()
        {
            fsm = new PassiveStateMachine<SM.ProcessState, SM.Command>();

            fsm.Initialize(SM.ProcessState.Initial);
            fsm.Fire(SM.Command.OnOpenBattleFront);
            fsm.Start();

            Assert.IsTrue(fsm.IsRunning);
        }

        [TestMethod]
        public void SimpleTransition()
        {
            fsm = new PassiveStateMachine<SM.ProcessState, SM.Command>();

            fsm.Initialize(SM.ProcessState.Initial);
            fsm.Fire(SM.Command.OnOpenBattleFront);
            fsm.Start();

            fsm.Fire(SM.Command.OnOuterDoorDown);

            Assert.IsTrue(fsm.IsRunning);
        }

        [TestMethod]
        public void SimpleBFKTransition()
        {
            var keepInfo = new Keep_Info();
            keepInfo.KeepId = 1;
            keepInfo.Name = "test keep";

            var bfk = new BattleFrontKeep(keepInfo, 4, Region1, FakeKeepComms, false);

            bfk.fsm.Initialize(SM.ProcessState.Initial);
            bfk.fsm.Fire(SM.Command.OnOpenBattleFront);
            bfk.fsm.Start();

            Assert.IsTrue(bfk.fsm.IsRunning);
        }


    }
}
