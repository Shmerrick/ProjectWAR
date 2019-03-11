using Common;
using System.Collections.Generic;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Creature equipment modification commands under .equip</summary>
    internal class EquipCommands
    {

        /// <summary>
        /// Add Equipement to target <Model,Slot,Save>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool EquipAdd(Player plr, ref List<string> values)
        {
            int model = GetInt(ref values);
            int slot = GetInt(ref values);
            int save = GetInt(ref values);

            Creature obj = GetObjectTarget(plr) as Creature;
            if (obj == null)
                return false;

            Creature_item item = new Creature_item();
            item.SlotId = (ushort)slot;
            item.ModelId = (ushort)model;
            item.Entry = obj.Entry;
            item.EffectId = 0;
            obj.ItmInterface.AddCreatureItem(item);
            plr.SendClientMessage("Item Added :" + (ushort)slot);
            if (save > 0)
            {
                CreatureService.AddCreatureItem(item);
            }

            return true;
        }

        /// <summary>
        /// Remove Equipement to target <Slot,Save>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool EquipRemove(Player plr, ref List<string> values)
        {
            int slot = GetInt(ref values);
            int save = GetInt(ref values);

            Creature obj = GetObjectTarget(plr) as Creature;
            if (obj == null)
                return false;

            if (obj.ItmInterface.RemoveCreatureItem((ushort)slot) != null)
            {
                plr.SendClientMessage("Item Removed :" + (ushort)slot);
                if (save > 0)
                {
                    CreatureService.RemoveCreatureItem(obj.Entry, (ushort)slot);
                }
            }

            return true;
        }

        /// <summary>
        /// Remove All Equipements to target <Save>
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool EquipClear(Player plr, ref List<string> values)
        {
            int save = GetInt(ref values);

            Creature obj = GetObjectTarget(plr) as Creature;
            if (obj == null)
                return false;

            for (int i = 0; i < ItemsInterface.MAX_EQUIPMENT_SLOT; ++i)
            {
                if (obj.ItmInterface.Items[i] != null)
                {
                    if (obj.ItmInterface.RemoveCreatureItem((ushort)i) != null)
                    {
                        plr.SendClientMessage("Item Removed :" + (ushort)i);
                        if (save > 0)
                        {
                            CreatureService.RemoveCreatureItem(obj.Entry, (ushort)i);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Draw Equipement list of target
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool EquipList(Player plr, ref List<string> values)
        {
            Creature obj = GetObjectTarget(plr) as Creature;
            if (obj == null)
                return false;

            for (int i = 0; i < ItemsInterface.MAX_EQUIPMENT_SLOT; ++i)
            {
                if (obj.ItmInterface.Items[i] != null)
                {
                    plr.SendClientMessage("<" + i + "," + obj.ItmInterface.Items[i].ModelId + ">");
                }
            }


            return true;
        }

    }
}
