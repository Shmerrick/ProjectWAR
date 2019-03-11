using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scripting.Lairs.Beastlords
{
    public abstract class BasicBeastlord : AGeneralScript
    {
        Object Obj; // This is creature
        List<Object> stuffInRange = new List<Object>(); // This is list of stuff that entered range

        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, SlayLowbies);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, SlayLowbies);
            Obj.EvtInterface.AddEventNotify(EventName.OnDealDamage, SlayLowbies);
        }

        public bool SlayLowbies(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (!c.IsDead)
            {
                foreach (Player player in Obj.PlayersInRange.ToList())
                {
                    if (player != null)
                    {
                        if (player.Level < 31 && !player.IsDead && !player.IsInvulnerable && player.StealthLevel != 2 && c.PointWithinRadiusFeet(player, 150))// - removed
                        {
                            // c.Say("Don't waste my time " + player.Name + "!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY); // Banter

                            // Nuking player in progress...
                            player.SendClientMessage("You were killed by " + Obj.Name + " because you approached his lair with insufficient level.");
                            player.ReceiveDamage(c, int.MaxValue);

                            PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

                            damageOut.WriteUInt16(player.Oid);
                            damageOut.WriteUInt16(player.Oid);
                            damageOut.WriteUInt16(23584); // Terminate

                            damageOut.WriteByte(0);
                            damageOut.WriteByte(0); // DAMAGE EVENT
                            damageOut.WriteByte(7);

                            damageOut.WriteZigZag(-30000);
                            damageOut.WriteByte(0);

                            player.DispatchPacketUnreliable(damageOut, true, player);
                        }
                    }
                }
            }
            return false;
        }
    }
}
