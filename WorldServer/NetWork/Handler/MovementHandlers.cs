using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Scenarios;

//using WorldServer.NetWork.Handler;    //this is needed for state2

namespace WorldServer.NetWork.Handler
{
    public class MovementHandlers : IPacketHandler
    {
        public enum MovementFlag
        {
            Retreat = 84,            // 001010100, 0x54
            Advance = 192,           // 011000000, 0xC0
            Immobile = 254,         // 111111100, 0xFE

            Jump = 4,
            GDBOUGE = 8,            // 000001000
            OffHand = 32,            // 000010000, 0x20 (Left?)
            MainHand = 64,            // 000100000, 0x40 (Right?)

            OutOfCombat = 31,   // 000011111, 0x1F
            InCombat = 95        // 001011111, 0x5F
        }

		public enum GROUNDTYPE
		{
			Solid = 0,
			ShallowWater = 1,
			ShallowSludge = 7,
			DeepWater = 17,
			DeepSludge = 23,
			DeepLava = 3,
			DeepLava2 = 19,
			LOTDShallowWater = 5,
			LOTDDeepWater = 21
		}
		
		enum MovementTypes
        {
            GroundForward = 0xC0,
            GroundBackward = 0x54,
            FlyModeForward = 0x88,
            FlyModeBackward = 0x00,
            NotMoving = 0xFE
        }

        enum Strafe  // 
        {
            left1 = 0x20,
            left2 = 0xa0,

            leftforward1 = 0x30,
            leftforward2 = 0xb0,

            right1 = 0x40,
            right2 = 0xc0,

            rightforward1 = 0x50,
            rightforward2 = 0xd0,

            jumpright = 0x44,
            jumpleft = 0x24
        }

        private static long _lavatimer = 0;
        private static readonly Dictionary<Unit, NewBuff> _lavaBuffs = new Dictionary<Unit, NewBuff>();

        private static void AddLavaBuff(NewBuff lavaBuff)
        {
            if (lavaBuff == null)
                return;

            if (_lavaBuffs.ContainsKey(lavaBuff.Caster))
                _lavaBuffs[lavaBuff.Caster] = lavaBuff;

            else _lavaBuffs.Add(lavaBuff.Caster, lavaBuff);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_ZONEJUMP, (int)eClientState.Playing, "F_JUMPZONE")]
        public static void F_ZONEJUMP(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            //Log.Dump("Jump", packet, true);
            uint destinationId = packet.GetUint32();
            byte responseType = packet.GetUint8();
            //Log.Info("Jump", "Jump to :" + Id);

            if (cclient.Plr.Zone.Info.Type == 1)
            {
                if (responseType == 0)
                    cclient.Plr.SendDialog(Dialog.ScenarioLeave, 586);
                else
                {
                    Scenario sc = cclient.Plr.ScnInterface.Scenario;
                    if (sc == null)
                        cclient.Plr.SendClientMessage("No active scenario.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    else
                        sc.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.RemovePlayer, cclient.Plr));
                }
                return;
            }

			Zone_jump Jump = null;

			// ZARU: zone jump out hackaround for LV leave
			if (destinationId == 272804328)
			{
				Instance_Info II;
				InstanceService._InstanceInfo.TryGetValue(260, out II);
				
				if (cclient.Plr.Realm == Realms.REALMS_REALM_ORDER)
					Jump = ZoneService.GetZoneJump(II.OrderExitZoneJumpID);
				else if (cclient.Plr.Realm == Realms.REALMS_REALM_DESTRUCTION)
					Jump = ZoneService.GetZoneJump(II.DestrExitZoneJumpID);

				if (Jump == null)
					Jump = ZoneService.GetZoneJump(destinationId);
			}
			else
				Jump = ZoneService.GetZoneJump(destinationId);

            if (Jump == null)
            {
                cclient.Plr.SendClientMessage("This portal's jump destination (" + destinationId + ") does not exist.");
                SendJumpFailed(cclient.Plr);
                return;
            }
            if (cclient.Plr.GmLevel > 0)
            {
                cclient.Plr.SendClientMessage("Portal Id: " + destinationId);
            }
            if (cclient.Plr.CbtInterface.IsInCombat)
            {
                cclient.Plr.SendClientMessage("You can't use a portal while in combat." + destinationId);
                cclient.Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_PLAYER_REGION_NOT_AVAILABLE);
                SendJumpFailed(cclient.Plr);
                return;
            }

            if (Jump.Type == 1 && cclient.Plr.WorldGroup == null)
            {
                cclient.Plr.SendClientMessage("You must be a member of a group in order to use this portal." + destinationId);
                cclient.Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_PLAYER_REGION_NOT_AVAILABLE);
                SendJumpFailed(cclient.Plr);
                return;
            }
            if (Jump.Type == 2 && cclient.Plr.Level < 30)
            {
                cclient.Plr.SendClientMessage("A Career Rank of 30 is required to use this portal." + destinationId);
                cclient.Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_PLAYER_REGION_NOT_AVAILABLE);
                SendJumpFailed(cclient.Plr);
                return;
            }
            if (Jump.Type == 3 && cclient.Plr.GldInterface.GetGuildLevel() < 6)
            {
                cclient.Plr.SendClientMessage("A Guild Rank of 6 is required to use this portal." + destinationId);
                cclient.Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_PLAYER_REGION_NOT_AVAILABLE);
                SendJumpFailed(cclient.Plr);
                return;
            }
            if (Jump.Type >= 4 && Jump.Type <= 6)
            {
#pragma warning disable CS0642 // Возможно, ошибочный пустой оператор
                if (!WorldMgr.InstanceMgr.ZoneIn(cclient.Plr, Jump.Type, Jump)) ;
#pragma warning restore CS0642 // Возможно, ошибочный пустой оператор
                SendJumpFailed(cclient.Plr);
                return;
            }

            if (Jump.Enabled || cclient.Plr.GmLevel > 1)
                cclient.Plr.Teleport(Jump.ZoneID, Jump.WorldX, Jump.WorldY, Jump.WorldZ, Jump.WorldO);
            else
            {
                cclient.Plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_PLAYER_REGION_NOT_AVAILABLE);
                SendJumpFailed(cclient.Plr);
            }
        }


        public static void SendJumpFailed(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_ZONEJUMP_FAILED, 1);
            Out.WriteByte(0);
            plr.SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_PLAYER_STATE2, (int)eClientState.WorldEnter, "F_PLAYER_STATE2")]
        public static void F_PLAYER_STATE2(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            Player player = cclient.Plr;

            if (player == null || !player.IsInWorld())
                return;

            

            bool skipSend = false;

            long pos = packet.Position;

            //Comments below are for testing State2

            //byte[] data = packet.ToArray();
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_STATE2, (int)packet.Size + 1);
            //Out.Write(data, (int)packet.Position, (int)packet.Size); // instead of the line below for testing
            Out.Write(packet.ToArray(), (int)packet.Position, (int)packet.Size);
            Out.WriteByte(0);
/*
#if DEBUG
            State2 stateTest = new State2();
            stateTest.Read(data, data.Length);
            Log.Info("state2.HasEnemyTarget", stateTest.HasEnemyTarget.ToString());
            Log.Info("state2.FreeFall", stateTest.FreeFall.ToString());
            Log.Info("state2.Falltime", stateTest.FallTime.ToString());
#endif
*/
//End of the testing of state2

            packet.Position = pos;

            byte[] data = packet.ToArray();

            // Experimental throttling if too many players in range. This can be overridden later if the position update's forced
            if (player.PlayersInRange.Count > 150 && TCPManager.GetTimeStampMS() - player.LastStateRecvTime < 200)
                skipSend = true;

            ushort currentHeading = player.Heading;

            if (packet.Size > 9 && packet.Size < 18)
            {
                long state = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 2));
                long state2 = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 10));

                ushort x = ((ushort)(((state2 >> 56 & 0x1) << 15) | ((state >> 0 & 0xFF) << 7) | ((state >> 9 & 0x7F))));
                ushort y = ((ushort)(((state2 >> 40 & 0x1) << 15) | ((state2 >> 48 & 0xFF) << 7) | ((state2 >> 57 & 0x7F))));
                ushort z = ((ushort)(((state2 >> 16 & 0x3) << 14) | ((state2 >> 24 & 0xFF) << 6) | ((state2 >> 34 & 0x3F))));

                ushort direction = ((ushort)(((state >> 16 & 0x7F) << 5) | ((state >> 27 & 0x1F))));
                ushort zoneID = ((byte)(((state2 >> 32 & 0x1) << 7) | ((state2 >> 41 & 0x7F))));
                bool grounded = ((((state >> 8 & 0x1))) == 1);
                byte fallState = ((byte)(((state >> 40 & 0x1F))));
                bool walking = ((((state >> 48 & 0x1))) == 1);
                bool moving = ((((state >> 49 & 0x1))) == 1);
                bool notMoving = ((((state >> 63 & 0x1))) == 1);
                byte groundtype = ((byte)(((state2 >> 82 & 0x1F))));

                //Hack Zone ID should be ushort but we only read a byte
                if (cclient.Plr.ZoneId > 255)
                    zoneID = (ushort)Utils.setBit(zoneID,8,true);

                //hardcode to not allow players into gunbad in case we miss to invalidate the zone on push
#if (!DEBUG)
                /*
                if (zoneID == 60 && player.Client.IsPlaying())
                {
                    if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        player.Teleport(161, 439815, 134493, 16865, 0);

                    else if (player.Realm == Realms.REALMS_REALM_ORDER)
                        player.Teleport(162, 124084, 130213, 12572, 0);
                }
                */
               
               if (zoneID == 111 && player.Client.IsPlaying())
               {
                   if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                       player.Teleport(202, 1411789, 1454421, 3516, 0);

                   else if (player.Realm == Realms.REALMS_REALM_ORDER)
                       player.Teleport(202, 1449783, 1459746, 3549, 0);
                }
               

#endif
                
                //lets move players instantly, if they are in a void (even staff)
                if ((zoneID == 0 && player.ZoneId.HasValue) && player.Client.IsPlaying())
                {
                    player.SendClientMessage("You managed to go outside of the worlds boundries, as such you have been forcefully moved to your capital", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        player.Teleport(161, 439815, 134493, 16865, 0);
                   


                    else if (player.Realm == Realms.REALMS_REALM_ORDER)
                        player.Teleport(162, 124084, 130213, 12572, 0);

                }
                
                //stop players from getting stuck below the world like below IC
                if (player.Z < 150 && !player.IsDead && player.Client.IsPlaying())
                {
                    player.SendClientMessage("You have fallen through the floor, instead of falling and getting stuck somewhere you get terminated.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    player.Terminate();
                }

                //player should not be able to cast while in the air.
                if (!grounded)
                {
                    if (player.AbtInterface.GetAbiityProcessor() != null && player.AbtInterface.GetAbiityProcessor().HasInfo() && player.AbtInterface.GetAbiityProcessor().AbInfo.Entry != 8090 && player.AbtInterface.GetAbiityProcessor().AbInfo.Entry != 9393)
                        player.AbtInterface.Cancel(true);
                }

                if (fallState != 31 || ((moving || walking || !notMoving || !grounded) && player.Speed > 0))
                    player.IsMoving = true;
                else
                    player.IsMoving = false;

                if (!player.WasGrounded)
                {
                    if (grounded)
                    {
                        player.CalculateFallDamage();
                        player.AirCount = -1;
                        player.ForceSendPosition = true;
                    }
                }

                player.WasGrounded = grounded;

                if (!grounded)
                    player.FallState = fallState;

                // Throttle pure rotation updates.
                if (player.X == x && player.Y == y && player.Z == z && TCPManager.GetTimeStampMS() - player.LastStateRecvTime < 150)
                {
                    if (player.AirCount == -1)
                        player.AirCount = 0;
                    else skipSend = true;
                }

                if (player.IsStaggered || player.IsDisabled)
                    direction = currentHeading;

                player.SetPosition(x, y, z, direction, zoneID);

				player.GroundType = (GROUNDTYPE) groundtype;

                //solid, exclude any zones that has to use a hardcode lava to not disable the damage instantly
                if (groundtype == 0)
                {
                    //release lava damage
                    if (_lavaBuffs.ContainsKey(player) && (zoneID != 190 || zoneID != 197))
                    {
                        _lavaBuffs[player].BuffHasExpired = true;
                        _lavaBuffs.Remove(player);
                    }
                    
                }
                //shallow water 1, have no use for it atm, shallow sludge 7
                /*
                if (groundtype == 1 || groundtype == 7)
                {
                    
                }*/

                //deep water 17 in current implementation should be 2 by londos decode, deep sludge 23
                if (groundtype == 17 || groundtype == 23)
                {
                    player.Dismount();
                }

                //lava and deep water/sludge, ref: ZARU
                long Now = TCPManager.GetTimeStampMS();
                if (groundtype == 3 || groundtype == 19 || (player.Zone.ZoneId == 260 && groundtype == 17 || groundtype == 23)) //19 is deep lava, 17 is deep water, 23 is deep sludge
                {
                    player.Dismount();
                    if (!player.IsDead )
                    {
                        //do lava dmg
                        if (_lavatimer < Now)
                        {
                            player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(14430), AddLavaBuff));
                            _lavatimer = Now + 1000;
                            player.ResetMorale();
                        } 
                        
                    }
                }
                
                //instadeath
                
                if (groundtype == 5 || groundtype == 21) //should be 4 according to londos notes 5=lotd shallow water 21=lotd deep water
                {
                    if (!player.IsDead)
                    {
                        Log.Notice("groundstate instadeath: ", player.Name + " managed to trigger groundtype instadeath in zone: " + player.ZoneId);
                        player.SendClientMessage("You have managed to trigger the instadeath code from river mortis, please screenshot your death and send to the devs on the bugtracker", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                        player.Terminate();
                    }
                }
                
                //bright wizard colleage lava
                if (zoneID == 197)
                {
                    if (!player.IsDead && player.Z <= 23791)
                    {
                        if (_lavatimer < Now)
                        {
                            player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(14430), AddLavaBuff));
                            _lavatimer = Now + 1000;
                            player.ResetMorale();
                        }
                    }
                    else
                    {
                       if (_lavaBuffs.ContainsKey(player))
                       {
                            _lavaBuffs[player].BuffHasExpired = true;
                            _lavaBuffs.Remove(player);
                       }
                    }
                }
                if (zoneID == 190)
                {
                    if (!player.IsDead && player.Z <= 7806)
                    {
                        if (_lavatimer < Now)
                        {
                            player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(14430), AddLavaBuff));
                            _lavatimer = Now + 1000;
                            player.ResetMorale();
                        }
                    }
                    else
                    {
                        if (_lavaBuffs.ContainsKey(player))
                        {
                            _lavaBuffs[player].BuffHasExpired = true;
                            _lavaBuffs.Remove(player);
                        }
                    }
                }

                if (player.Zone.Info.Illegal && player.GmLevel == 1)
                {
                    if (!player.IsDead && player.BuffInterface.GetBuff(27960, player) == null)
                        player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(27960)));
                 
                }
                else if (!player.Zone.Info.Illegal)
                {
                    if (player.BuffInterface.GetBuff(27960, player) != null)
                    {
                        player.BuffInterface.RemoveBuffByEntry(27960);
                    }
                }

                if (player.ForceSendPosition)
                {
                    skipSend = false;
                    player.ForceSendPosition = false;
                }

                //gunbad has low points where we want to kill the player
                if (zoneID == 60 && player.Z < 17900)
                    player.Terminate();
            }

            #region Long seems to be when the player has a hostile target
            else if (packet.Size > 17)
            {
                long state = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 2));
                long state2 = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 10));

                ushort x = ((ushort)(((state2 >> 56 & 0x3) << 14) | ((state >> 0 & 0xFF) << 6) | ((state >> 10 & 0x3F))));
                ushort y = ((ushort)(((state2 >> 40 & 0x3) << 14) | ((state2 >> 48 & 0xFF) << 6) | ((state2 >> 58 & 0x3F))));
                ushort z = ((ushort)(((state2 >> 16 & 0x7) << 12) | ((state2 >> 24 & 0xFF) << 4) | ((state2 >> 36 & 0x0F))));
                byte zoneID = ((byte)(((state2 >> 34 & 0x1) << 7) | ((state2 >> 32 & 0x3) << 5) | ((state2 >> 43 & 0x1F))));
                ushort direction = ((ushort)(((state >> 16 & 0xFF) << 4) | ((state >> 28 & 0x0F))));
                bool grounded = ((((state >> 9 & 0x1))) == 1);
                bool walking = ((((state >> 48 & 0x1))) == 1);
                bool moving = ((((state >> 49 & 0x1))) == 1);
                bool notMoving = ((((state >> 63 & 0x1))) == 1);
                byte fallState = ((byte)(((state >> 40 & 0x1F))));
                byte groundtype = ((byte)(((state2 >> 73 & 0x1F))));

                if (fallState != 31 || ((moving || walking || !notMoving || !grounded) && player.Speed > 0))
                    player.IsMoving = true;
                else
                    player.IsMoving = false;

                //stop players from getting stuck below the world like below IC
                if (player.Z < 150 && !player.IsDead && player.Client.IsPlaying())
                {
                    player.SendClientMessage("You have fallen through the floor, instead of falling and getting stuck somewhere you get terminated.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    player.Terminate();
                }
                //hardcode to not allow players into gunbad in case we miss to invalidate the zone on push
#if (!DEBUG)
                /*
                if (zoneID == 60 && player.Client.IsPlaying())
                {
                    if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        player.Teleport(161, 439815, 134493, 16865, 0);

                    else if (player.Realm == Realms.REALMS_REALM_ORDER)
                        player.Teleport(162, 124084, 130213, 12572, 0);
                }
                */
#endif

                //lets move players instantly, if they are in a void (even staff)
                if ((zoneID == 0 && player.ZoneId.HasValue) && player.Client.IsPlaying())
                {
                    player.SendClientMessage("You managed to go outside of the worlds boundries, as such you have been forcefully moved to your capital", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        player.Teleport(161, 439815, 134493, 16865, 0);

                    else if (player.Realm == Realms.REALMS_REALM_ORDER)
                        player.Teleport(162, 124084, 130213, 12572, 0);

                }

                //player should not be able to cast while in the air.
                if (!grounded)
                    player.AbtInterface.Cancel(true);

                if (!player.WasGrounded)
                {
                    if (grounded)
                    {
                        player.CalculateFallDamage();
                        player.AirCount = -1;
                        player.ForceSendPosition = true;
                    }
                }

                player.WasGrounded = grounded;
                if (!grounded)
                    player.FallState = fallState;

                if (player.IsStaggered || player.IsDisabled)
                    direction = currentHeading;

                player.SetPosition(x, y, z, direction, zoneID);

                //solid, exclude any zones that has to use a hardcode lava to not disable the damage instantly
                if (groundtype == 0)
                {
                    //release lava damage
                    if (_lavaBuffs.ContainsKey(player) && (zoneID != 190 || zoneID != 197))
                    {
                        _lavaBuffs[player].BuffHasExpired = true;
                        _lavaBuffs.Remove(player);
                    }

                }
                //shallow water 1, have no use for it atm... shallow sludge 7
                /*
                if (groundtype == 1 || groundtype == 7)
                {
                    
                }*/

                //deep water 17 in current implementation should be 2 by londos decode, deep sludge 23
                if (groundtype == 17 || groundtype == 23)
                {
                    player.Dismount();
                }

                //lava
                long Now = TCPManager.GetTimeStampMS();
                if (groundtype == 3 || groundtype == 19) //19 is deep lava
                {
                    player.Dismount();
                    if (!player.IsDead)
                    {
                        //do lava dmg
                        if (_lavatimer < Now)
                        {
                            player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(14430), AddLavaBuff));
                            _lavatimer = Now + 1000;
                            player.ResetMorale();
                        }
                    }
                }

                //instadeath
                
                if (groundtype == 5 || groundtype == 21) //should be 4 according to londos notes 5=lotd shallow water 21=lotd deep water
                {
                    if (!player.IsDead)
                    {
                        Log.Notice("groundstate instadeath: ", player.Name + " managed to trigger groundtype instadeath in zone: " + player.ZoneId);
                        player.SendClientMessage("You have managed to trigger the instadeath code from river mortis, please screenshot your death and send to the devs on the bugtracker", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

                        player.Terminate();
                    }
                }
                
                //bright wizard colleage lava
                if (zoneID == 197)
                {
                    if (!player.IsDead && player.Z <= 23791)
                    {
                        if (_lavatimer < Now)
                        {
                            player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(14430), AddLavaBuff));
                            _lavatimer = Now + 1000;
                            player.ResetMorale();
                        }
                    }
                    else
                    {
                        if (_lavaBuffs.ContainsKey(player))
                        {
                            _lavaBuffs[player].BuffHasExpired = true;
                            _lavaBuffs.Remove(player);
                        }
                    }
                }
                if (zoneID == 190)
                {
                    if (!player.IsDead && player.Z <= 7806)
                    {
                        if (_lavatimer < Now)
                        {
                            player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(14430), AddLavaBuff));
                            _lavatimer = Now + 1000;
                            player.ResetMorale();
                        }
                    }
                    else
                    {
                        if (_lavaBuffs.ContainsKey(player))
                        {
                            _lavaBuffs[player].BuffHasExpired = true;
                            _lavaBuffs.Remove(player);
                        }
                    }
                }

                if (player.Zone.Info.Illegal && player.GmLevel == 1)
                {
                    if (!player.IsDead && player.BuffInterface.GetBuff(27960, player) == null)
                        player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo(27960)));

                }
                else if (!player.Zone.Info.Illegal)
                {
                    if (player.BuffInterface.GetBuff(27960, player) != null)
                    {
                        player.BuffInterface.RemoveBuffByEntry(27960);
                    }
                }

                if (player.ForceSendPosition)
                {
                    skipSend = false;
                    player.ForceSendPosition = false;
                }

                //gunbad has low points where we want to kill the player
                if (zoneID == 60 && player.Z < 17900)
                    player.Terminate();
            }

#endregion

            // Packets not conforming to the above are sent at 500ms intervals when the player is still.
            // I don't know their function, but it appears they're of no interest to the client.
            else
            {
                skipSend = true;
                player.ForceSendPosition = true;
            }

            // TODO: Conditional dispatch for Player State 2
            if (skipSend)
                return;
            player.SendCounter++;
            //player.DebugMessage("F_PLAYER_STATE2: "+ (TCPManager.GetTimeStampMS() - player.LastStateRecvTime));
            player.DispatchPacket(Out, false,true);
            player.LastStateRecvTime = TCPManager.GetTimeStampMS();
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_DUMP_STATICS, (int)eClientState.WorldEnter, "onDumpStatics")]
        public static void F_DUMP_STATICS(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            cclient.Plr.MoveBlock = false;

            uint Unk1 = packet.GetUint32();
            ushort Unk2 = packet.GetUint16();
            ushort OffX = packet.GetUint16();
            ushort Unk3 = packet.GetUint16();
            ushort OffY = packet.GetUint16();

            //cclient.Plr.DebugMessage("DUMP_STATICS : OFFSETS "+OffX+", "+OffY);
            cclient.Plr.SetOffset(OffX, OffY);

            if (!cclient.Plr.IsActive)
                cclient.Plr.IsActive = true;

            if (cclient.Plr.PendingDumpStatic)
                cclient.Plr.OnClientLoaded();
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_OPEN_GAME, (int)eClientState.CharScreen, "onOpenGame")]
        public static void F_OPEN_GAME(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            PacketOut Out = new PacketOut((byte)Opcodes.S_GAME_OPENED, 1);

            Out.WriteByte(cclient.Plr == null ? (byte)1 : (byte)0);

            cclient.Plr.PendingDumpStatic = true;

            cclient.SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_INIT_PLAYER, (int)eClientState.CharScreen, "onInitPlayer")]
        public static void F_INIT_PLAYER(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            Player Plr = cclient.Plr;
            if (Plr == null)
                return;

            // clear all lockouts if they are expired
            InstanceService.ClearLockouts(Plr);

            if (!Plr.IsInWorld()) // If the player is not on a map, then we add it to the map
            {
                ushort zoneId = Plr.Info.Value.ZoneId;
                ushort regionId = (ushort)Plr.Info.Value.RegionId;

                Zone_Info info = ZoneService.GetZone_Info(zoneId);
                if (info?.Type == 0)
                {
                    RegionMgr region = WorldMgr.GetRegion(regionId, true);
                    if (region.AddObject(Plr, zoneId, true))
                        return;
                }
                else if(info?.Type == 4 || info?.Type == 5|| info?.Type == 6)  // login into a instance results in teleport outside
                {
                    if (InstanceService._InstanceInfo.TryGetValue(zoneId, out Instance_Info II))
                    {
                        Zone_jump ExitJump = null;
                        if (Plr.Realm == Realms.REALMS_REALM_ORDER)
                            ExitJump = ZoneService.GetZoneJump(II.OrderExitZoneJumpID);
                        else if (Plr.Realm == Realms.REALMS_REALM_DESTRUCTION)
                            ExitJump = ZoneService.GetZoneJump(II.DestrExitZoneJumpID);

                        if (ExitJump == null)
                            Log.Error("Exit Jump in Instance", " " + zoneId + " missing!");
                        else
                            Plr.Teleport(ExitJump.ZoneID, ExitJump.WorldX, ExitJump.WorldY, ExitJump.WorldZ, ExitJump.WorldO);
                    }
                    return;
                }

                // Warp a player to their bind point if they attempt to load into a scenario map.
                RallyPoint rallyPoint = RallyPointService.GetRallyPoint(Plr.Info.Value.RallyPoint);

                if (rallyPoint != null)
                    Plr.Teleport(rallyPoint.ZoneID, rallyPoint.WorldX, rallyPoint.WorldY, rallyPoint.WorldZ, rallyPoint.WorldO);
                else
                {
                    CharacterInfo cInfo = CharMgr.GetCharacterInfo(Plr.Info.Career);
                    Plr.Teleport(cInfo.ZoneId, (uint)cInfo.WorldX, (uint)cInfo.WorldY, (ushort)cInfo.WorldZ,
                        (ushort)cInfo.WorldO);
                }
            }
            else
            {
                Plr.Loaded = false;
                Plr.StartInit();
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_PLAYER_ENTER_FULL, (int)eClientState.CharScreen, "onPlayerEnterFull")]
        public static void F_PLAYER_ENTER_FULL(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            ushort SID;
            byte unk1, serverID, characterSlot;

            SID = packet.GetUint16();
            unk1 = packet.GetUint8();
            serverID = packet.GetUint8();
            string CharName = packet.GetString(24);
            packet.Skip(2);
            string Language = packet.GetString(2);
            packet.Skip(4);
            characterSlot = packet.GetUint8();

            Log.Debug("F_PLAYER_ENTER_FULL", "Enter the game : " + CharName + ",Slot=" + characterSlot);

            if (Program.Rm.RealmId != serverID)
                cclient.Disconnect("Requested realm ID does not match this server's ID");
            else
            {
                PacketOut Out = new PacketOut((byte)Opcodes.S_PID_ASSIGN, 2);
                Out.WriteUInt16R((ushort)cclient.Id);
                cclient.SendPacket(Out);

                if (cclient.Plr != null)
                    cclient.Plr.DisconnectType = Player.EDisconnectType.Unclean;
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_REQUEST_WORLD_LARGE, (int)eClientState.CharScreen, "onRequestWorldLarge")]
        public static void F_REQUEST_WORLD_LARGE(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            if (cclient.Plr == null)
                return;

            if (!cclient.Plr.Loaded)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_SET_TIME, 8);
            Out.WriteUInt16((ushort)(DateTime.UtcNow.TimeOfDay.TotalSeconds / 65.5d));
            Out.WriteUInt16(0);
            Out.WriteUInt32(1); // Game seconds passing per second.
            cclient.Plr.SendPacket(Out);

            Out = new PacketOut((byte)Opcodes.S_WORLD_SENT, 1);
            Out.WriteByte(0);
            cclient.Plr.SendPacket(Out);

            if (cclient.Plr.InRegionChange)
            {
                cclient.Plr.ClearRange(true); // in case previous region was terminated and couldn't clear range for us
                cclient.Plr.InRegionChange = false;
            }

            cclient.Plr.CrrInterface.Notify_PlayerLoaded();
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_REQUEST_INIT_PLAYER, (int)eClientState.WorldEnter, "onRequestInitPlayer")]
        public static void F_REQUEST_INIT_PLAYER(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            if (cclient.Plr == null)
                return;

            ushort desiredClientId = packet.GetUint16();
            bool sent = false;
            List<Player> players;
            lock (cclient.Plr.PlayersInRange)
                players = new List<Player>(cclient.Plr.PlayersInRange);


            foreach (Player obj in players)
                if (obj != null && obj.Client != null && !obj.IsDisposed && obj.Client.Id == desiredClientId)
                {
                    sent = true;
                    obj.SendMeTo(cclient.Plr);
                }

            //request of party members can be outside of 400ft to show moving map pip
            if (!sent && cclient.Plr.PriorityGroup != null)
            {
                var member = cclient.Plr.PriorityGroup.GetMember(cclient.Plr.Zone.ZoneId, desiredClientId);
                if (member != null)
                    member.SendMeTo(cclient.Plr);
            }
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_REQUEST_INIT_OBJECT, (int)eClientState.WorldEnter, "onRequestInitObject")]
        public static void F_REQUEST_INIT_OBJECT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            if (cclient.Plr == null)
                return;

            ushort oid = packet.GetUint16();

            cclient.Plr.GetObjectInRange(oid)?.SendMeTo(cclient.Plr);
        }

        private const int FLIGHT_DISTANCE_TOLERANCE = 15;

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_FLIGHT, "F_FLIGHT")]
        public static void F_FLIGHT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            ushort targetOid = packet.GetUint16();
            ushort state = packet.GetUint16();

            if (state == 20) // Flight Master
            {
                Creature flightMaster = cclient.Plr.Region.GetObject(targetOid) as Creature;

                if (flightMaster == null || flightMaster.InteractType != InteractType.INTERACTTYPE_FLIGHT_MASTER || cclient.Plr.Get2DDistanceToObject(flightMaster, true) > FLIGHT_DISTANCE_TOLERANCE)
                {
                    cclient.Plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_FLIGHT_MASTER_RANGE_ERR);
                    return;
                }

                ushort destId = packet.GetUint16();

                List<Zone_Taxi> destinations = WorldMgr.GetTaxis(cclient.Plr);

                if (destinations.Count <= destId - 1)
                    return;

                Zone_Taxi destination = destinations[destId - 1];

                if (!cclient.Plr.RemoveMoney(destination.Info.Price))
                {
                    cclient.Plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_FLIGHT_LACK_FUNDS);
                    return;
                }
                
                Pet pet = cclient.Plr.CrrInterface.GetTargetOfInterest() as Pet;
                if (pet != null)
                    pet.Destroy();
                cclient.Plr.AbtInterface.ResetCooldowns();
                cclient.Plr.Teleport(destination.ZoneID, destination.WorldX, destination.WorldY, destination.WorldZ, destination.WorldO);
            }
        }
    }
}
