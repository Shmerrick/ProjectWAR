using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    class DominationScenarioKhaine : Scenario
    {
        public List<ClickFlag> Flags = new List<ClickFlag>();

        public DominationScenarioKhaine(Scenario_Info info, int tier)
            : base(info, tier)
        {
            foreach (Scenario_Object scenarioObject in info.ScenObjects.OrderBy(e => e.Identifier).ToList())
            {
                if (scenarioObject.Type == "Flag")
                {
                    ClickFlag clickFlag = new ClickFlag(scenarioObject.Identifier, scenarioObject.ObjectiveName,
                        scenarioObject.WorldPosX, scenarioObject.WorldPosY, scenarioObject.PosZ, scenarioObject.Heading,
                        scenarioObject.PointGain, scenarioObject.PointOverTimeGain, new ClickFlag.ClickFlagDelegate(OnHold), new ClickFlag.ClickFlagDelegate(OnCaptured));
                    Flags.Add(clickFlag);
                    Region.AddObject(clickFlag, info.MapId);

                    clickFlag.Open = true;
                    clickFlag.Owner = 0;
                    clickFlag.ShowGlow = true;
                    clickFlag.HoldDuration = 0;
                    clickFlag.CaptureCastTime  = 10;

                }
                else
                    LoadScenarioObject(scenarioObject);
            }
        }

        public override void OnStart()
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    SendObjectiveStates(plr);
                }
            }
            ResetFlags();
        }

        protected override void UpdateScenario()
        {
            base.UpdateScenario();
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
                }
            }
        }

        public void Broadcast(string[] Msgs, ChatLogFilters filter, Localized_text localizeEntry)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    plr.SendLocalizeString(Msgs, filter, localizeEntry);
                    SendObjectiveStates(plr);
                }
        }

        private void ResetFlags()
        {
            foreach (var flag in Flags)
            {
                flag.Owner = 0;
                flag.HoldOwner = 0;
                flag.GlowOwner = 0;

            }
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null || plr.IsDisposed || !plr.IsInWorld())
                        continue;

                    foreach (var flag in Flags)
                    {
                        flag.SendFlagInfo(plr);
                        flag.SendFlagState(plr);
                        flag.SendMeTo(plr);
                    }
                }
            }
        }

        private void Lock()
        {
            Broadcast(new[] { "The forces of " + (Flags[0].Owner == (int)Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " bring forth Khaine's Wrath!" }, 
                ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);

            foreach (var obj in Region.GetObjects<GameObject>().Where(e => e.Name == "Khaine Flames").ToList())
            {
                obj.VfxState = 1; //show red beam on braziers
            }

            PlaySoundToAll(305);
            EvtInterface.AddEvent(ExplodeFlags, 1000, 1);
            GivePoints((byte)Flags[0].Owner, 75);
        }

        private void ExplodeFlags()
        {
            GroundTarget gt1 = new GroundTarget(Flags[0].WorldPosition, GameObjectService.GetGameObjectProto(23));
            Region.AddObject(gt1, 230);

            GroundTarget gt2 = new GroundTarget(Flags[1].WorldPosition, GameObjectService.GetGameObjectProto(23));
            Region.AddObject(gt2, 230);

            GroundTarget gt3 = new GroundTarget(new Point3D(364572, 365590, 12036), GameObjectService.GetGameObjectProto(23));
            Region.AddObject(gt3, 230);


            EvtInterface.AddEvent(() =>
            {
                Explosion(gt1);
                Explosion(gt2);
                Explosion(gt3);

            }, 1000, 1);

            //remove ground targets after explosions
            EvtInterface.AddEvent(() =>
            {
                gt1.RemoveFromWorld();
                gt2.RemoveFromWorld();
                gt3.RemoveFromWorld();

            }, 10000, 1);

            EvtInterface.AddEvent(Unlock, 5000, 1);
        }

        private void Unlock()
        {
            ResetFlags();

            //turn off read beams on braziers
            foreach (var obj in Region.GetObjects<GameObject>().Where(e => e.Name == "Khaine Flames").ToList())
                obj.VfxState = 0;
        }

        public override void GmCommand(Player plr, ref List<string> values)
        {
            base.GmCommand(plr, ref values);
            ExplodeFlags();
        }

        private void Explosion(GroundTarget gt)
        {
            //play explosion effect
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 30);
            Out.WriteUInt16(gt.Oid);
            Out.WriteUInt16(gt.Oid);
            Out.WriteUInt16(14054);
            Out.WriteByte((byte)1);
            Out.WriteByte((byte)0);
            Out.WriteByte((byte)5);
            Out.WriteByte((byte)0);
            Region.DispatchPacket(Out, gt.WorldPosition, 1200);

            //since game objets are sent to players within 400feet, send explosion effect to  players further away (Explosion wont play to dead players)
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr == null || plr.IsDisposed || !plr.IsInWorld())
                        continue;

                    if (plr.GetDistanceTo(gt.WorldPosition) > 500)
                        plr.PlayEffect(1272, gt.WorldPosition);
                }
            }

            var startTime = DateTime.Now;

            AbilityDamageInfo damageThisPass = new AbilityDamageInfo
            {
                Entry = 14050,
                DisplayEntry = 0,
                DamageType = DamageTypes.RawDamage,
                MinDamage = (ushort)(30000),
                CastPlayerSubID = 0
            };

            double explosionTime = 5000;
            double explosionSize = 190;

            EvtInterface.AddEvent(() =>
            {
                var elapsed = DateTime.Now.Subtract(startTime).TotalMilliseconds;
                var expSize = explosionSize * elapsed / explosionTime;

                if (elapsed < explosionTime)
                {
                    foreach (var plr in Region.WorldQuery<Player>(gt.WorldPosition, (int)expSize))
                    {
                        if (plr == null || plr.IsDisposed || !plr.IsInWorld() || plr.IsDead)
                            continue;

                        CombatManager.InflictDamage(damageThisPass, 20, plr, plr);
                    }
                }

            }, (int)(explosionTime / 20), (int)(20));
        }

        private void OnCaptured(ClickFlag flag)
        {
            flag.GlowOwner = flag.Owner;
            EvtInterface.RemoveEvent(Lock);
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

            if (flag.Owner == (int)Realms.REALMS_REALM_ORDER)
                PlaySoundToAll(580);
            else if (flag.Owner == (int)Realms.REALMS_REALM_DESTRUCTION)
                PlaySoundToAll(818);


            if (Flags[0].Owner == Flags[1].Owner)
            {
                Broadcast(new[] { (Flags[0].Owner == (int)Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + " will lock down both control points in 15 seconds!"}, 
                    ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
                EvtInterface.AddEvent(Lock, 15000, 1);

            }
        }

    }
}
