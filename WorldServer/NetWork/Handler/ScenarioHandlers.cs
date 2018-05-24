/*
 * Copyright (C) 2014 WarEmu
 *	http://WarEmu.com
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Scenarios;

namespace WorldServer
{
    public class ScenarioHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_INTERACT_QUEUE, "onInteractQueue")]
        public static void F_INTERACT_QUEUE(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player player = cclient.Plr;
            packet.Skip(5);
            byte command = packet.GetUint8();
            ushort scenarioId = packet.GetUint16();
            switch (command)
            {
                case 1: // signed up
                    if (!player.ScnInterface.IsBlocked())
                        WorldMgr.ScenarioMgr.EnqueuePlayer(player, scenarioId);
                    break;
                case 2: // leave queue
                    player.ScnInterface.PendingScenario?.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.RemovePendingPlayer, player));
                    if (scenarioId != 0)
                    {
                        if (player.WorldGroup != null)
                            WorldMgr.ScenarioMgr.DequeueGroup(player.WorldGroup, scenarioId);
                        WorldMgr.ScenarioMgr.DequeuePlayer(player, scenarioId);
                    }
                    break;
                case 3: // click join scenario
                    if (player.ScnInterface.PendingScenario == null)
                        player.SendClientMessage("Scenario join failure: No pending scenario.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    else
                        player.ScnInterface.PendingScenario?.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.AddPlayer, player));
                    break;
                case 4: //give me more time
                    player.ScnInterface.PendingScenario?.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.AddPendingPlayer, player));
                    break;
                case 7: // group queue

                    if (player.WorldGroup == null)
                        break;

                    WorldMgr.ScenarioMgr.EnqueueGroup(player.WorldGroup, scenarioId);

                    // 7: "No bracket for your current level."
                    // 9: "Not all players same bracket."
                    // 10: "Just Leveled"
                    // 11: "Just Leveled with prompt"
                    // 12: "Rank too low"
                    // 13: "Rank too high"
                    // 14: "RR too low"
                    // 16: Instance full
                    // 17:
                    // 19: Trial Account Blocked

                    break;

            }


        }
    }
}