using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;

namespace WorldServer.World.Scenarios
{
    public class MurderballScenario : Scenario
    {
        private readonly Dictionary<HoldObject, int> _murderballTicks = new Dictionary<HoldObject, int>();

        public MurderballScenario(Scenario_Info info, int tier)
            : base(info, tier)
        {
            foreach (Scenario_Object obj in info.ScenObjects)
            {
                HoldObject ball;

                if (obj.ProtoEntry != 0)
                {
                    GameObject_proto proto = GameObjectService.GetGameObjectProto(obj.ProtoEntry);

                    ball = new HoldObject(obj.Identifier, obj.ObjectiveName, new Point3D(obj.WorldPosX, obj.WorldPosY, obj.PosZ), 14031, 30000, BallPickedUp, BallDropped, ObjectReset, BallBuffAssigned, proto.DisplayID, proto.DisplayID);
                }
                else
                    ball = new HoldObject(obj.Identifier, obj.ObjectiveName, new Point3D(obj.WorldPosX, obj.WorldPosY, obj.PosZ), 14031, 30000, BallPickedUp, BallDropped, ObjectReset, BallBuffAssigned, 235, 233);

                _murderballTicks.Add(ball, 0);
                Region.AddObject(ball, info.MapId);
            }
        }

        public override void OnStart()
        {
            EvtInterface.AddEvent(ActivateBall, 30000, 1);

            for (int i=0; i<2; ++i)
                foreach (Player plr in Players[i])
                    SendObjectiveStates(plr);
        }

        public void ActivateBall()
        {
            foreach (HoldObject ball in _murderballTicks.Keys)
                ball.SetActive(0);

            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                    foreach (HoldObject ball in _murderballTicks.Keys)
                        SendFlagObjectState(plr, ball);
        }

        protected override void UpdateScenario()
        {
            base.UpdateScenario();
        }
            
        #region Murderball Events

        public void BallPickedUp(HoldObject ball, Player pickedBy)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    SendObjectiveStates(plr);
                    plr.SendLocalizeString(new [] {pickedBy.GenderedName, (pickedBy.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"), ball.name}, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_FLAG_PICKUP);
                }

            EvtInterface.AddEvent(UpdateMapPosition, 1000, 0);
        }

        public void BallBuffAssigned(NewBuff buff)
        {
            _pointDelayTime = TCPManager.GetTimeStampMS() + 14500;
            ((HoldObjectBuff)buff).OnUpdate = TickPoints;
        }

        public void BallDropped(HoldObject ball)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                {
                    SendObjectiveStates(plr);
                    plr.SendLocalizeString(ball.name, ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_FLAG_DROP);
                }

            _murderballTicks[ball] = 0;

            EvtInterface.RemoveEvent(UpdateMapPosition);
        }

        #endregion

        #region Points

        private long _pointDelayTime;

        public void TickPoints(NewBuff hostBuff, HoldObject ball, long tick)
        {
            if (tick >= _pointDelayTime)
            {
                if (TooCloseToSpawn((Player) hostBuff.Caster))
                {
                    if (Info.MapId == 31 || Info.MapId == 232 || Info.MapId == 133)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            foreach (Player player in Players[i])
                            {
                                if (player.Realm == hostBuff.Caster.Realm)
                                    player.SendClientMessage("The gods are displeased with your cowardice and snatched " + ball.name + " back to its place", ChatLogFilters.CHATLOGFILTERS_C_WHITE_L);

                                else
                                    player.SendClientMessage("The enemy team displeased their gods, " + ball.name + " has been taken back to its resting place", ChatLogFilters.CHATLOGFILTERS_C_WHITE_L);
                            }
                        }
                        ball.ResetFromHeld();
                    }
                }
                else
                    GivePoints((hostBuff.Caster.Realm == Realms.REALMS_REALM_ORDER ? 1 : 2), (uint) (2*((_murderballTicks[ball]/3) + 1)));
            }

            ++_murderballTicks[ball];
        }

        #endregion
    
        public void UpdateMapPosition()
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player plr in Players[i])
                    foreach (HoldObject ball in _murderballTicks.Keys)
                        SendFlagObjectLocation(plr, ball);
        }

        readonly Point2D[][] torAnrocGuardPoints =
        {
            new[] { new Point2D(491347, 367492), new Point2D(494445, 364643) },
            new[] { new Point2D(489022, 356120), new Point2D(484365, 359500) }
        };

        readonly Point2D[][] mawGuardPoints =
        {
            new[] { new Point2D(550002, 365314), new Point2D(549499, 362996) },
            new[] { new Point2D(560592, 366453), new Point2D(560819, 365133) }
        };

        public override bool TooCloseToSpawn(Player plr)
        {
            if (Info.MapId == 232)
            {
                if (plr.Get2DDistanceToWorldPoint(torAnrocGuardPoints[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1][0]) < 250 ||
                    plr.Get2DDistanceToWorldPoint(torAnrocGuardPoints[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1][1]) < 250 ||
                    plr.Get2DDistanceToWorldPoint(torAnrocGuardPoints[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 0 : 1][0]) < 250 ||
                    plr.Get2DDistanceToWorldPoint(torAnrocGuardPoints[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 0 : 1][1]) < 250)
                {
                    return true;
                }
            }
            else if (Info.MapId == 31)
            {
                if (plr.Get2DDistanceToWorldPoint(RespawnLocations[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1]) < 150 ||
                    plr.Get2DDistanceToWorldPoint(RespawnLocations[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 0 : 1]) < 150)
                {
                    return true;
                }
            }
            else if (Info.MapId == 133)
            {
                if (plr.Get2DDistanceToWorldPoint(mawGuardPoints[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1][0]) < 250 ||
                    plr.Get2DDistanceToWorldPoint(mawGuardPoints[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1][1]) < 250 ||
                    plr.Get2DDistanceToWorldPoint(mawGuardPoints[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 0 : 1][0]) < 250 ||
                    plr.Get2DDistanceToWorldPoint(mawGuardPoints[plr.Realm == Realms.REALMS_REALM_DESTRUCTION ? 0 : 1][1]) < 250)
                {
                    return true;
                }
            }
            return plr.Get2DDistanceToWorldPoint(RespawnLocations[plr.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1]) < 100;
        }

        public override void SendObjectiveStates(Player plr)
        {
            foreach (HoldObject ball in _murderballTicks.Keys)
                SendFlagObjectState(plr, ball);
        }

        public override void OnClose()
        {
            EvtInterface.RemoveEvent(UpdateMapPosition);
        }
    }
}