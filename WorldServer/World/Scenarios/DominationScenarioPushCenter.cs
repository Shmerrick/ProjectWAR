using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Scenarios.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    public class DominationScenarioPushCenter : Scenario
    {
        public List<ClickFlag> Flags = new List<ClickFlag>();
        public ClickFlag CurrentFlag = null;
        private GameObject _glowObject = null;

        public DominationScenarioPushCenter(Scenario_Info info, int tier)
            : base(info, tier)
        {
            int i = 0;
            int flagCount = info.ScenObjects.Where(e => e.Type == "Flag").ToList().Count;

            foreach (Scenario_Object scenarioObject in info.ScenObjects.OrderBy(e=>e.Identifier).ToList())
            {
                if (scenarioObject.Type == "Flag")
                {
                    ClickFlag clickFlag = new ClickFlag(scenarioObject.Identifier, scenarioObject.ObjectiveName,
                        scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading,
                        scenarioObject.PointGain, scenarioObject.PointOverTimeGain, new ClickFlag.ClickFlagDelegate(OnHold), new ClickFlag.ClickFlagDelegate(OnCaptured));
                    Flags.Add(clickFlag);
                    Region.AddObject(clickFlag, info.MapId);

                    clickFlag.Open = false;

                    if (i < (flagCount / 2) )
                    {
                        clickFlag.Open = false;
                        clickFlag.Owner = 1;
                    }
                    else if (i == flagCount / 2)
                    {
                        clickFlag.Open = true;
                        clickFlag.Owner = 0;
                        CurrentFlag = clickFlag;
                        CreateGlow(CurrentFlag);
                    }
                    else
                    {
                        clickFlag.Open = false;
                        clickFlag.Owner = 2;
                    }
                    i++;
                    
                }

                else
                    LoadScenarioObject(scenarioObject);
            }
        }

        private void CreateGlow(ClickFlag flag)
        {
            if (_glowObject != null)
            {
                _glowObject.RemoveFromWorld();
            }
            // Spawn objective glow (Big Poofy Glowy Nuet)
            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(99858);

            if (glowProto != null)
            {
                GameObject_spawn spawn = new GameObject_spawn
                {
                    Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                    WorldO = flag.Heading,
                    WorldX = flag.WorldPosition.X,
                    WorldY = flag.WorldPosition.Y,
                    WorldZ = (ushort)flag.WorldPosition.Z,
                    ZoneId = Region.RegionId,
                };
                spawn.BuildFromProto(glowProto);

                _glowObject = new GameObject(spawn);
                _glowObject.VfxState = (byte)flag.Owner;
                Region.AddObject(_glowObject, spawn.ZoneId);
            }
        }

        private void OnHold(ClickFlag flag)
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null || plr.IsDisposed || !plr.IsInWorld())
                        continue;

                    flag.SendFlagInfo(plr);
                    flag.SendMeTo(plr);
                    if (_glowObject != null)
                    {
                        _glowObject.VfxState = (byte)flag.HoldOwner;
                    }
                }
            }
        }

        private void OnCaptured(ClickFlag flag)
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null || plr.IsDisposed || !plr.IsInWorld())
                        continue;

                    flag.SendFlagInfo(plr);
                    flag.SendFlagState(plr);
                    flag.SendMeTo(plr);
                }
            }

            if (flag.Owner == 0)
                return; // Nothing to do, probably a canceled capture

            GivePoints(flag.Owner, flag.CapturePoints);
            int index = Flags.IndexOf(flag);

            //leave the last flag open for capture
            if (index == 0 || index == Flags.Count - 1)
                flag.Open = true;
            else
                flag.Open = false;

            //unlock foward flag
            if (flag.Owner == 1)
            {

                for (int i = index + 1; i < Flags.Count; i++)
                {
                    Flags[i].Open = true;
                    CurrentFlag = Flags[i];
                    Flags[i].HoldOwner = 0;
                    Flags[i].Owner = 0;
                    break;
                }
            }
            else if (flag.Owner == 2)
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    Flags[i].Open = true;
                    CurrentFlag = Flags[i];
                    Flags[i].HoldOwner = 0;
                    Flags[i].Owner = 0;
                    break;
                }

            }

            CreateGlow(CurrentFlag);

            string packetString =
                $"{flag.ObjectiveName} is now {(flag.Owner == 1 ? "Order" : "Destruction")} controlled!";

            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    SendObjectiveStates(plr);
                    CurrentFlag.SendMeTo(plr);
                    plr.SendLocalizeString(packetString, SystemData.ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                }
            }
        }

        public override void OnStart()
        {
            //PlaySoundToAll(SOUND_BATTLEGROUND_BEGIN);
            CreateGlow(CurrentFlag);

            EvtInterface.AddEvent(GeneratePoints, 6000, 0);
        }

        public override void SendObjectiveStates(Player plr)
        {
            foreach (ClickFlag flag in Flags)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
                Out.WriteUInt32((uint)flag.ObjectiveID);
                Out.Fill(0xFF, 6);
                Out.WriteUInt16(0);
                Out.WriteByte((byte)flag.Owner);
                Out.Fill(0, 3);
                plr.SendPacket(Out);
            }
        }

        public void GeneratePoints()
        {
            byte[] points = new byte[2];

            foreach (ClickFlag flag in Flags)
            {
                if (flag.Owner == 0)
                    continue;

                if (flag.Owner == 1 && Flags.IndexOf(flag) >= Flags.Count / 2)
                    points[0] += flag.TickPoints;

                if (flag.Owner == 2 && Flags.IndexOf(flag) <= (Flags.Count / 2))
                    points[1] += flag.TickPoints;

            }

            GivePoints(1, points[0]);
            GivePoints(2, points[1]);
        }

        public override void OnClose()
        {
            EvtInterface.RemoveEvent(GeneratePoints);
        }
    }
}
