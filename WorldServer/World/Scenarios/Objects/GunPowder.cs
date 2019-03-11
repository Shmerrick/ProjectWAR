using System.Collections.Generic;
using SystemData;
using FrameWork;
using WorldServer.NetWork.Handler;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios.Objects
{
    public class GunPowder : Object
    {
        public int ObjectiveID;
        public string ObjectiveName;
        private int _x, _y, _z;
        public int Owner;
        public int Ownership;
        public List<Player>[] playersInRange = new List<Player>[2];
        public int DeltaOwnership;
        public int _modelID;
        public byte CapturePoints { get; private set; }
        public byte TickPoints { get; private set; }
        public Player InteractPlayer { get; set; }
        public List<FlagGuard> Guards = new List<FlagGuard>();
        public delegate void InteractPileAction(GunPowder pile, Player player);

        protected InteractPileAction _onInteractWithPowder;
        protected InteractPileAction _onBombPlanted;

        public GunPowder(string name, int identifier, int modelID, int x, int y, int z, int o, InteractPileAction powederInteract, InteractPileAction onBombPlanted)
        {
            ObjectiveID = identifier;
            ObjectiveName = name;
            _x = x;
            _y = y;
            _z = z;
            _modelID = modelID;
            _onInteractWithPowder = powederInteract;
            _onBombPlanted = onBombPlanted;
        }


        public override void OnLoad()
        {
            X = Zone.CalculPin((uint)(_x), true);
            Y = Zone.CalculPin((uint)(_y), false);
            Z = _z;

            base.OnLoad();

            WorldPosition.X = _x;
            WorldPosition.Y = _y;
            WorldPosition.Z = Z;

            SetOffset((ushort)(_x >> 12), (ushort)(_y >> 12));

            IsActive = true;
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
            if (InteractPlayer != null && _onBombPlanted != null)
                _onBombPlanted(this, InteractPlayer);
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (player.StealthLevel > 0)
            {
                player.SendClientMessage("You can't interact with objects while in stealth.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }


            try
            {
                if (_onInteractWithPowder != null)
                    _onInteractWithPowder(this, player);
            }
            finally
            {
            }
        }

        public override void SendMeTo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)_z);
            Out.WriteUInt32((uint)_x);
            Out.WriteUInt32((uint)_y);

            Out.WriteUInt16((ushort)_modelID);

            Out.WriteUInt16(0x1E00);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            // flags
            Out.WriteUInt16(0x24);

            Out.WriteByte(0);

            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteByte(100);

            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteUInt32(0);

            Out.WritePascalString(ObjectiveName);
            Out.WriteByte(0);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

    }
}
