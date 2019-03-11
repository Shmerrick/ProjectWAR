using System.Collections.Generic;
using SystemData;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using WorldServer.World.Scenarios;
using Item = WorldServer.World.Objects.Item;

namespace WorldServer.NetWork.Handler
{
    public class InterfaceHandlers : IPacketHandler
    {
        private static void CommandSelectTitle(Player plr, PacketIn packet)
        {
            packet.ReadByte();

            // Read the titleId
            ushort titleId = packet.GetUint16();

            // Verify player has this title unlocked
            if (titleId !=0 && !plr.TokInterface.HasTok(titleId))
                plr.SendClientMessage("You attempted to select a title that you have not yet unlocked.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
            else
                plr.SetTitleId(titleId);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_INTERFACE_COMMAND, (int)eClientState.WorldEnter, "onInterfaceCommand")]
        public static void F_INTERFACE_COMMAND(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            Player Plr = cclient.Plr;

            if (Plr == null)
                return;

            byte CommandId = packet.GetUint8();

            switch (CommandId)
            {

                case 1: // ????
                    {
                    } break;

                case 2: // Resurrect Button
                    {
                        Plr.PreRespawnPlayer();
                    } break;
                case 3: // Buff remove
                    {
                        Plr.BuffInterface.RemoveBuffByID(packet.GetUint8());
                    } break;
                case 4: // Moral ability to slot
                {
                        if (Plr.CbtInterface.IsInCombat)
                        {
                            Plr.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_PLAYER_CANT_CHANGE_MORALE_IN_COMBAT);
                            return;
                        }

                        byte slotIndex = packet.GetUint8();
                        ushort abilityEntry = packet.GetUint16();

                        Plr._Value.SetMorale(slotIndex, abilityEntry);
                        CharMgr.Database.SaveObject(Plr._Value);

                        Plr.SendMoraleAbilities();
                    } break;
                case 7: // Select title
                    {
                        CommandSelectTitle(cclient.Plr, packet);
                    } break;

                case 10: // Talisman Fuse
                    {
                            byte command = packet.GetUint8();
                            byte unk2 = packet.GetUint8();
                            byte slot = packet.GetUint8();
                            ushort itemslot = packet.GetUint16();
                            byte unk4 = packet.GetUint8();

                            switch (command)
                            {
                                case 0:
                                    Plr.ItmInterface.MoveSlot(itemslot, 255,1);
                                    break;
                                case 1:

                                    Item modifiedItem = Plr.ItmInterface.GetItemInSlot(255);
                                    if(modifiedItem == null)
                                        break;

                                    List<uint> unfused = modifiedItem.AbortFuseTalisman();

                                    foreach (uint entry in unfused)
                                    {
                                        Plr.ItmInterface.CreateItem(entry, 1);
                                    }

                                    Plr.ItmInterface.MoveSlot(255, Plr.ItmInterface.GetFreeInventorySlot(Plr.ItmInterface.GetItemInSlot(255).Info,true),1);
                                    break;
                                case 2:    // add Talis
                                    if (Plr.ItmInterface.GetItemInSlot(255) != null && Plr.ItmInterface.GetItemInSlot(255).AddTalisman(Plr.ItmInterface.GetItemInSlot(itemslot).Info.Entry, slot))
                                    {
                                        Plr.ItmInterface.DeleteItem(itemslot, 1);
                                        Plr.ItmInterface.SendItemInSlot(Plr, 255);
                                    }
                                    break;
                                case 3:  //  remove talis
                                    Plr.ItmInterface.CreateItem(Plr.ItmInterface.GetItemInSlot(255).RemoveTalisman(slot), 1);
                                    Plr.ItmInterface.SendItemInSlot(Plr, 255);
                                    break;
                                case 4:   // fuse
                                    Plr.ItmInterface.GetItemInSlot(255).FuseTalisman();
                                    Plr.ItmInterface.MoveSlot(255, Plr.ItmInterface.GetFreeInventorySlot(Plr.ItmInterface.GetItemInSlot(255).Info, true),1);
                                    break;
                            }
                        } break;

                case 12: // cultivation
                {
                        if (Plr.CultivInterface == null)
                        { 
                            Plr.SendClientMessage("You don't have the Cultivating skill.", ChatLogFilters.CHATLOGFILTERS_CRAFTING);
                            return;
                        }

                        byte command = packet.GetUint8();
                        //Log.Info("command", "" + command);   // 0 plant seed soul   3 uproot 4 client request
                        byte unk2 = packet.GetUint8();
                        byte plotIndex = packet.GetUint8();
                        ushort itemSlot = packet.GetUint16();

                        switch (command)
                        {
                            case 0:
                                Plr.CultivInterface.AddComponentToPlot(plotIndex, itemSlot);
                                break;
                            case 1:
                                Plr.CultivInterface.Harvest(plotIndex);
                                break;
                            case 3:
                                Plr.CultivInterface.Uproot(plotIndex);
                                break;
                            case 4:
                                Plr.CultivInterface.SendPlotInfo(plotIndex);
                                break;
                        }
                    } break;
                case 13: // Resurrection handler
                    {
                        Plr.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnAcceptResurrection, null, packet.GetUint8() > 0 ? Plr : null);
                    } break;
                case 14: // apotekari
                    {
                        byte command = packet.GetUint8();
                        //Log.Info("command", "" + command);   // 0 plant seed soul   3 uproot 4 client request
                        byte unk2 = packet.GetUint8();
                        byte pot = packet.GetUint8();
                        ushort itemslot = packet.GetUint16();
                        //Log.Info("Pot", "" + pot);
                        //Log.Info("itemslot", "" + itemslot);
                        //Log.Info("unk2", "" + packet.GetUint8());

                        if(Plr._Value.CraftingSkill == 4)
                        switch (command)
                        {
                            case 1: Plr.CraftApoInterface.AddContainer(itemslot); break;
                            case 2: Plr.CraftApoInterface.AddContainer(itemslot); break;
                            case 3: Plr.CraftApoInterface.RemoveContainer(itemslot); break;
                            case 4: Plr.CraftApoInterface.Craft(); break;
                            case 5: Plr.CraftApoInterface.Reset(0); break;
                        }
                        if (Plr._Value.CraftingSkill == 5)
                            switch (command)
                            {
                                case 1: Plr.CraftTalInterface.AddContainer(itemslot); break;
                                case 2: Plr.CraftTalInterface.AddContainer(itemslot); break;
                                case 3: Plr.CraftTalInterface.RemoveContainer(itemslot); break;
                                case 4: Plr.CraftTalInterface.Craft(); break;
                                case 5: Plr.CraftTalInterface.Reset(); break;
                            }


                    } break;
                case 28: // Leave scenario
                {
                    Plr.ScnInterface.Scenario?.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.RemovePlayer, Plr));
                }
                    break;
            }

        }


        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_REQUEST_LASTNAME, (int)eClientState.WorldEnter, "onRequestLastName")]
        public static void F_REQUEST_LASTNAME(BaseClient client, PacketIn In)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null)
                return;

            Player Plr = cclient.Plr;

            string lastName = In.GetStringToZero();

            if (lastName == null)
                lastName = "";

            if (Plr.noSurname == 1)
            {
                Plr.SendClientMessage("You have been flagged by a GM to not be able to set a last name", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                // Send success, don't subtract gold
                SendUpdateLastNameSuccess(Plr);
                return;
            }

            // Check to see if the player actually changed their last name
            if (lastName == Plr.Info.Surname)
            {
                Plr.SendClientMessage("The new name is the same as the current name.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                // Send success, don't subtract gold
                SendUpdateLastNameSuccess(Plr);
                return;
            }

            // Check that the name does not exceed the character limit
            if (lastName.Length > GameData.Constants.LastNameCharacterLimit)
            {
                Plr.SendClientMessage("The new name is longer than the character limit.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                SendUpdateLastNameError(Plr);
                return;
            }

            // Check that lastName contains only valid characters
            string invalidCharsFound = FindInvalidCharsInLastName(lastName);
            if (invalidCharsFound.Length > 0)
            {
                string msg = string.Format(
                    "The requested name '{0}' contains the invalid characters [{1}].",
                    lastName, invalidCharsFound);
                Plr.SendClientMessage(msg, ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                SendUpdateLastNameError(Plr);
                return;
            }

            // Check for high enough level
            if (Plr._Value.Level < GameData.Constants.LastNameLevelRequirement)
            {
                Plr.SendClientMessage("You require Rank "+ GameData.Constants.LastNameLevelRequirement+" in order to set a last name.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                SendUpdateLastNameError(Plr);
                return;
            }

            // Check that the player has enough gold
            if (Plr._Value.Money < GameData.Constants.LastNameChangeCost)
            {
                Plr.SendClientMessage("You do not have enough money.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                SendUpdateLastNameError(Plr);
                return;
            }

            // Check for abuse
            // Kick straight off for using invalid name.
            if (!CharMgr.AllowName(lastName))
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_QUIT, 4);
                Out.WriteHexStringBytes("01000000");
                Plr.SendPacket(Out);
                return;
            }

            if (lastName.Length > 1)
                lastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower();

            // Update last name field
            Plr.SetLastName(lastName);
            Plr.RemoveMoney(Constants.LastNameChangeCost);

            SendUpdateLastNameSuccess(Plr);
        }
         /*
         * FindInvalidCharsInLastName
         */
        private static string FindInvalidCharsInLastName(string lastName)
        {
            // TODO: if special characters (like apostrophe) are allowed, then the name will need to be escaped before going into the db

            string result = "";

            for (int i = 0; i < lastName.Length; i++)
                if (!char.IsLetter(lastName[i]))
                    result = result + lastName[i];

            return result;
        }

        /*
         * SendUpdateLastNameSuccess
         */
        private static void SendUpdateLastNameSuccess(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_LASTNAME, 32);
            Out.WriteUInt16(plr.Oid);
            Out.WritePascalString(plr.CrrInterface.ExperimentalMode ? plr.Info.Surname + "*" : plr.Info.Surname);
            plr.SendPacket(Out);
        }

        /*
         * SendUpdateLastNameError
         */
        private static void SendUpdateLastNameError(Player plr)
        {
            // TODO: ???
         }

    }
}
