using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    class CaptureTheFlagScenario : Scenario
    {
        private readonly List<HoldObject> _flags = new List<HoldObject>();
        private readonly GameObject[] _flagBases = new GameObject[2];
        private readonly GameObject[] _flagDrops = new GameObject[2];

        private readonly uint _captureScore;

        public CaptureTheFlagScenario(Scenario_Info info, byte tier) : base(info, tier)
        {
            foreach (Scenario_Object obj in info.ScenObjects)
            {
                if (obj.Type == "Flag")
                {
                    HoldObject flag = new HoldObject(obj.Identifier, obj.ObjectiveName, new Point3D(obj.WorldPosX, obj.WorldPosY, obj.PosZ), 60001, 13000, ObjectPickedUp, FlagDropped, FlagReset, FlagBuffAssigned, 3442, 3442);

                    if (obj.ObjectiveName == "Order Flag")
                    {
                        flag.VfxState = 2;
                        _captureScore = obj.PointGain;
                        flag.SetActive(Realms.REALMS_REALM_DESTRUCTION);
                    }

                    else if (obj.ObjectiveName == "Destruction Flag")
                    {
                        flag.VfxState = 5;
                        _captureScore = obj.PointGain;
                        flag.SetActive(Realms.REALMS_REALM_ORDER);
                    }

                    else
                    {
                        flag.SetActive(0);
                        flag.HoldResetTimeSeconds = 180;
                        _captureScore = obj.PointGain;
                    }

                    flag.ObjectType = 1;

                    _flags.Add(flag);
                    Region.AddObject(flag, info.MapId);
                    AddTrackedObject(flag);
                }
                else
                    LoadScenarioObject(obj);

                ZoneMgr curZone = Region.GetZoneMgr(Info.MapId);

                Region.LoadCells((ushort)(curZone.Info.OffX + 8), (ushort)(curZone.Info.OffY + 8), 8);
            }
        }

        public override void OnStart()
        {
            base.OnStart();

            foreach (Object obj in Region.Objects)
            {
                GameObject go = obj as GameObject;

                if (go == null)
                    continue;

                switch (obj.Name)
                {
                    case "Order Capture":
                        _flagBases[0] = go;
                        break;
                    case "Destruction Capture":
                        _flagBases[1] = go;
                        break;
                    case "Order Flag Drop":
                        _flagDrops[0] = go;
                        break;
                    case "Destruction Flag Drop":
                        _flagDrops[1] = go;
                        break;
                }
                
            }
        }

        public void FlagBuffAssigned(NewBuff b)
        {
            HoldObjectBuff hB = (HoldObjectBuff) b;

            switch (hB.HeldObject.RealmCapturableFor)
            {
                case Realms.REALMS_REALM_ORDER:
                    hB.FlagEffect = FLAG_EFFECTS.Red;
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    hB.FlagEffect = FLAG_EFFECTS.Blue;
                    break;
                default:
                    hB.FlagEffect = FLAG_EFFECTS.Mball1;
                    break;
            }

            hB.Target.BuffInterface.QueueBuff(new BuffQueueInfo(hB.Target, 40, AbilityMgr.GetBuffInfo(14323)));
        }

        public void FlagDropped(HoldObject b)
        {
            ObjectDropped(b);

            if (b.Holder != null)
            {
                b.Holder.BuffInterface.RemoveBuffByEntry(14322);
                b.Holder.BuffInterface.RemoveBuffByEntry(14323);
            }
        }

        public void FlagReset(HoldObject b)
        {
            ObjectReset(b);

            if (b.Holder != null)
            {
                b.Holder.BuffInterface.RemoveBuffByEntry(14322);
                b.Holder.BuffInterface.RemoveBuffByEntry(14323);
            }
        }

        public override void Interact(GameObject obj, Player plr, InteractMenu menu)
        {
            if (obj == _flagBases[0] || obj == _flagBases[1])
                CheckCapture(obj, plr);
            else if (obj == _flagDrops[0] || obj == _flagDrops[1])
                CheckDropOff(obj, plr);
        }

        private void CheckCapture(GameObject flagBase, Player capturer)
        {
            if (flagBase == _flagBases[0])
            {
                if (capturer.Realm == Realms.REALMS_REALM_DESTRUCTION)
                {
                    capturer.SendClientMessage("This is the enemy realm's flag return point", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    return;
                }
            }

            else if (flagBase == _flagBases[1])
            {
                if (capturer.Realm == Realms.REALMS_REALM_ORDER)
                {
                    capturer.SendClientMessage("This is the enemy realm's flag return point", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    return;
                }
            }

            else
                return;

            HoldObject destFlag = null;

            foreach (HoldObject flag in _flags)
            {
                if (flag.Holder == capturer)
                    destFlag = flag;

                else if (flag.HeldState != EHeldState.Home)
                {
                    capturer.SendClientMessage("You cannot capture the enemy flag if your own is held", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    return;
                }
            }

            if (destFlag == null)
            {
                capturer.SendClientMessage("You must be holding the flag to capture", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return;
            }

            GivePoints((int)capturer.Realm, _captureScore);

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
            Out.WriteByte(0);
            Out.WriteUInt16(capturer.Realm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
            Out.Fill(0, 10);

            for (int i = 0; i < 2; ++i)
            {
                foreach (Player player in Players[i])
                {
                    player.SendLocalizeString(new[] { capturer.Name, capturer.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction", destFlag.name }, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_FLAG_CAPTURE);
                    player.SendPacket(Out);
                }
            }

            foreach (HoldObject flag in _flags)
                flag.ResetTo(EHeldState.Home);
        }

        private void CheckDropOff(GameObject flagDrop, Player capturer)
        {
            if (flagDrop == _flagDrops[0])
            {
                if (capturer.Realm == Realms.REALMS_REALM_DESTRUCTION)
                {
                    capturer.SendClientMessage("This is the enemy realm's flag drop off point", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    return;
                }
            }

            else if (flagDrop == _flagDrops[1])
            {
                if (capturer.Realm == Realms.REALMS_REALM_ORDER)
                {
                    capturer.SendClientMessage("This is the enemy realm's flag drop off point", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    return;
                }
            }

            else
                return;

            HoldObject destFlag = null;

            foreach (HoldObject flag in _flags)
            {
                if (flag.Holder == capturer)
                    destFlag = flag;

                else if (flag.HeldState != EHeldState.Carried)
                {
                    capturer.SendClientMessage("You can only drop off the enemy flag if both flags are held", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    return;
                }
            }

            if (destFlag == null)
            {
                capturer.SendClientMessage("You are not holding a flag", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return;
            }

            GivePoints((int) capturer.Realm, 50);

            PacketOut Out = new PacketOut((byte) Opcodes.F_PLAY_SOUND);
            Out.WriteByte(0);
            Out.WriteUInt16(capturer.Realm == Realms.REALMS_REALM_ORDER ? (ushort) 0x0C : (ushort) 0x332);
            Out.Fill(0, 10);

            for (int i = 0; i < 2; ++i)
            {
                foreach (Player player in Players[i])
                {
                    player.SendClientMessage($"{capturer.Name} of {(capturer.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")} has dropped off the {destFlag.name}!", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                    player.SendPacket(Out);
                }
            }

            foreach (HoldObject flag in _flags)
                flag.ResetTo(EHeldState.Home);
        }

        public override bool TooCloseToSpawn(Player plr)
        {
            return plr.Get2DDistanceToWorldPoint(RespawnLocations[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1]) < 100;
        }
    }
}
