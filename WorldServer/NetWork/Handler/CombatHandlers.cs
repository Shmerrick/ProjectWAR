using System;
using SystemData;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.NetWork.Handler
{
    public struct InteractMenu
    {
        public ushort Oid;
        public ushort Menu;
        public byte Page;
        public byte Num;
        public ushort Unk;
        public ushort Count;

        public PacketIn Packet;
    }

    public class CombatHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_PLAYER_INFO, (int)eClientState.Playing, "onPlayerInfo")]
        public static void F_PLAYER_INFO(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            packet.GetUint16();
            ushort Oid = packet.GetUint16();
            byte LOS = packet.GetUint8();                   // line of sight updates   96 if in los 32 if targeting someone out of los  0 if channiling a spell and target runs out of los
            byte TargetType = packet.GetUint8();
            //cclient.Plr.DebugMessage("F_PLAYER_INFO: SetTarget: "+Oid);
            if (TargetType == (byte) TargetTypes.TARGETTYPES_TARGET_SELF)
                TargetType = (byte) TargetTypes.TARGETTYPES_TARGET_ALLY;

            cclient.Plr.CbtInterface.SetTarget(Oid, (TargetTypes)TargetType);

            if (LOS == 0)
                cclient.Plr.AbtInterface.Cancel(true, (ushort)AbilityResult.ABILITYRESULT_NOTVISIBLECLIENT);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_SWITCH_ATTACK_MODE, (int)eClientState.Playing, "onSwitchAttackMode")]
        public static void F_SWITCH_ATTACK_MODE(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient) client;
            if (!cclient.HasPlayer())
                return;

            if (cclient.Plr.WeaponStance != WeaponStance.Standard)
            {
                packet.Skip(1);
                cclient.Plr.WeaponStance = (WeaponStance) packet.ReadByte();
            }

            cclient.Plr.CbtInterface.IsAttacking = !cclient.Plr.CbtInterface.IsAttacking;
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_INTERACT, (int)eClientState.Playing, "onInteract")]
        public static void F_INTERACT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null || !cclient.Plr.IsInWorld())
                return;

            if (cclient.Plr.IsDead)
                return;

            // don't change anything here or it will brake loot dyes and alot of other stuff !!!!!!!!!!!!!!!!

            InteractMenu Menu = new InteractMenu();
            Menu.Unk = packet.GetUint16();
            Menu.Oid = packet.GetUint16();
            Menu.Menu = packet.GetUint16();
            Menu.Page = packet.GetUint8();
            Menu.Num = packet.GetUint8();
            //ushort unk1 = packet.GetUint16();
            Menu.Count = packet.GetUint16();

            // don't change anything here or it will brake loot dyes and alot of other stuff !!!!!!!!!!!!!!!!

            // loot need greed pass
            if (Menu.Oid == 4207 && cclient.Plr.PriorityGroup != null)
            {
                Menu.Packet = packet;
                cclient.Plr.PriorityGroup.LootVote(cclient.Plr, Menu.Num, Menu.Packet.GetUint16());
            }
            
            Object Obj = cclient.Plr.Region.GetObject(Menu.Oid);
            if (Obj == null)
                return;
            
            if (!Obj.AllowInteract(cclient.Plr))
            {
                //Log.Error("F_INTERACT", "Distance = " + Obj.GetDistanceBetweenObjects(cclient.Plr));
                return;
            }

            //this is to prevent double clicking on doors and getting ported back to original clicking position
            if (cclient.Plr.LastInteractTime.HasValue && DateTime.Now.Subtract(cclient.Plr.LastInteractTime.Value).TotalMilliseconds < 500)
            {
                //Log.Error("F_INTERACT", "InteractTime < 1500ms");
                return;
            }

            Menu.Packet = packet;
            Obj.SendInteract(cclient.Plr, Menu);
            if (Obj is GameObject)
                cclient.Plr.LastInteractTime = DateTime.Now;
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_DO_ABILITY, (int)eClientState.Playing, "F_DO_ABILITY")]
        public static void F_DO_ABILITY(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null)
                return;

            byte LOS = packet.GetUint8();
            bool Moving = Convert.ToBoolean(packet.GetUint8());

            ushort Heading = packet.GetUint16();
            ushort X = packet.GetUint16();
            ushort Y = packet.GetUint16();
            ushort ZoneID = packet.GetUint16();
            ushort Z = packet.GetUint16();
            ushort AbilityID = packet.GetUint16();

            // prevent ability when banner is out

            if (cclient.Plr.WeaponStance == WeaponStance.Standard && !(AbilityID == 14508 || AbilityID == 14524 || AbilityID == 14527 || AbilityID == 14526))
                return;

            byte abGroup = packet.GetUint8();

            byte unk = packet.GetUint8();

            uint unk2 = packet.GetUint32();

            bool enemyVisible = Convert.ToBoolean(LOS & 128);
            bool friendlyVisible = Convert.ToBoolean(LOS & 8);

            cclient.Plr.SetPosition(X, Y, Z, Heading, ZoneID);

            cclient.Plr.AbtInterface.StartCast(cclient.Plr, AbilityID, abGroup, 0, 0, enemyVisible, friendlyVisible, Moving);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_DO_ABILITY_AT_POS, (int)eClientState.Playing, "F_DO_ABILITY_AT_POS")]
        public static void F_DO_ABILITY_AT_POS(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (cclient.Plr == null)
                return;

            ushort unk = packet.GetUint16();
            ushort Oid = packet.GetUint16();
            ushort CastPx = packet.GetUint16();
            ushort CastPy = packet.GetUint16();
            ushort CastZoneId = packet.GetUint16();

            ushort CastPz = packet.GetUint16();
            ushort AbilityId = packet.GetUint16();
            ushort Px = packet.GetUint16();
            ushort Py = packet.GetUint16();
            ushort Pz = packet.GetUint16();
            ushort targetZoneId = packet.GetUint16();
            byte abGroup = packet.GetUint8();

            if (Px == 0 && Py == 0 && Pz == 0)
                cclient.Plr.SendClientMessage("Target position out of range", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);

            cclient.Plr.AbtInterface.StartCastAtPos(cclient.Plr, AbilityId, ZoneService.GetWorldPosition(cclient.Plr.Zone.Info, Px, Py, Pz), CastZoneId, abGroup);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_INTERRUPT, (int)eClientState.Playing, "F_INTERRUPT")]
        public static void F_INTERRUPT(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            //cclient.Plr?.AbtInterface.Cancel(false, (ushort)GameData.AbilityResult.ABILITYRESULT_INTERRUPTED);
            cclient.Plr?.AbtInterface.NotifyCancelled();
        }
    }
}
