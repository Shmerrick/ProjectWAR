using Common;
using FrameWork;
using GameData;
using System.Collections.Generic;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios.Objects
{
    public class GuardCreature : Creature
    {
        private FlagGuard FlagGrd;

        public GuardCreature(Creature_spawn spawn, FlagGuard FlagGrd) : base(spawn)
        {
            this.FlagGrd = FlagGrd;
        }

        public override void RezUnit()
        {
            FlagGrd.UpdateFlagOwningRealm();

            FlagGrd.Creature = new GuardCreature(Spawn, FlagGrd);
            Region.AddObject(FlagGrd.Creature, Spawn.ZoneId);
            Destroy();
        }

        public override void OnLoad()
        {
            base.OnLoad();
        }
    }

    public class FlagGuard
    {
        private uint OrderId;
        private uint DestroId;
        private int x;
        private int y;
        private int z;
        private int o;
        private ushort ZoneId;
        private RegionMgr Region;
        public GuardCreature Creature;
        private int team;

        public FlagGuard(BattlefieldObjective flag, RegionMgr Region, ushort ZoneId, uint OrderId, uint DestroId, int x, int y, int z, int o)
        {
            this.Region = Region;
            this.ZoneId = ZoneId;
            this.OrderId = OrderId;
            this.DestroId = DestroId;
            this.x = x;
            this.y = y;
            this.z = z;
            this.o = o;
        }

        public void SpawnGuard(int team)
        {
            if (Creature != null)
                Creature.Destroy();
            this.team = team;

            Creature_proto Proto = CreatureService.GetCreatureProto(team == 1 ? OrderId : DestroId);
            if (Proto == null)
            {
                Log.Error("FlagGuard", "No FlagGuard Proto");
                return;
            }

            Creature_spawn Spawn = new Creature_spawn();
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = o;
            Spawn.WorldY = y;
            Spawn.WorldZ = z;
            Spawn.WorldX = x;
            Spawn.ZoneId = ZoneId;
            Spawn.RespawnMinutes = 3;

            Creature = new GuardCreature(Spawn, this);
            Region.AddObject(Creature, Spawn.ZoneId);
        }

        public void DespawnGuard()
        {
            Region.RemoveObject(Creature);
        }

        public void UpdateFlagOwningRealm()
        {
            Creature_proto Proto = CreatureService.GetCreatureProto(team == 1 ? OrderId : DestroId);
            if (Proto == null)
            {
                Log.Error("FlagGuard", "No FlagGuard Proto");
                return;
            }

            Creature.Spawn = new Creature_spawn();
            Creature.Spawn.BuildFromProto(Proto);
            Creature.Spawn.WorldO = o;
            Creature.Spawn.WorldY = y;
            Creature.Spawn.WorldZ = z;
            Creature.Spawn.WorldX = x;
            Creature.Spawn.ZoneId = ZoneId;
            Creature.Spawn.RespawnMinutes = 3;
        }
    }

    public class ProximityFlag : Object
    {
        public int ObjectiveID;
        public string ObjectiveName;
        private int _x, _y, _z;
        public int OwningRealm;
        public int Ownership;
        public List<Player>[] playersInRange = new List<Player>[2];
        public int DeltaOwnership;
        public bool Open = true;
        public byte CapturePoints { get; private set; }
        public byte TickPoints { get; private set; }
        public Scenario_Object ScenarioObject { get; set; }
        public List<FlagGuard> Guards = new List<FlagGuard>();

        public ProximityFlag(int objectiveID, string objectiveName, int x, int y, int z, int o, Scenario_Object obj = null)
        {
            ObjectiveID = objectiveID;
            ObjectiveName = objectiveName;
            _x = x;
            _y = y;
            _z = z;
            ScenarioObject = obj;
            for (int i = 0; i < 2; i++)
            {
                playersInRange[i] = new List<Player>();
            }
        }

        public ProximityFlag(int objectiveID, string objectiveName, int x, int y, int z, int o, byte capturePoints, byte tickPoints, Scenario_Object obj = null)
        {
            ObjectiveID = objectiveID;
            ObjectiveName = objectiveName;
            _x = x;
            _y = y;
            _z = z;
            Heading = (ushort)o;

            CapturePoints = capturePoints;
            TickPoints = tickPoints;
            ScenarioObject = obj;
            for (int i = 0; i < 2; i++)
                playersInRange[i] = new List<Player>();
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

        public override void SendMeTo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC, 42 + ObjectiveName.Length);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)_z);
            Out.WriteUInt32((uint)_x);
            Out.WriteUInt32((uint)_y);

            int displayId = 3442;
            if (OwningRealm == 0)
                displayId = 3442;
            else if (OwningRealm == 1)
                displayId = 3443;
            else
                displayId = 3438;

            Out.WriteUInt16((ushort)displayId);

            Out.WriteUInt16(0x1E00);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            // flags
            Out.WriteUInt16(0x21);

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

        public void Capture()
        {
            EvtInterface.RemoveEvent(Capture);
            OwningRealm = Ownership;

            foreach (Object obj in ObjectsInRange)
            {
                if (obj.IsPlayer())
                {
                    SendMeTo(obj.GetPlayer());
                    SendFlagInfo(obj.GetPlayer());
                }
            }

            foreach (FlagGuard Guard in Guards)
            {
                Guard.SpawnGuard(OwningRealm);
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
            Out.WriteByte(0);
            Out.WriteUInt16(OwningRealm == (byte)Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
            Out.Fill(0, 10);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("The Forces of ");
            sb.Append(OwningRealm == (byte)Realms.REALMS_REALM_ORDER ? "Order" : "Destruction");
            sb.Append(" have captured ");
            sb.Append(ObjectiveName + "!");

            foreach (Player plr in Region.Players)
            {
                SendFlagState(plr);
                plr.SendPacket(Out);
                plr.SendLocalizeString(sb.ToString(), SystemData.ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.CHAT_TAG_DEFAULT);
            }
        }

        /// <summary>
        /// Call to update this flag, if it is a scenario flag.
        /// </summary>
        /// <returns>A boolean indicating whether the ownership of the flag changed this tick.</returns>
        public bool UpdateDominationProgress()
        {
            bool ownershipChanged = false;
            int oldDeltaOwnership = DeltaOwnership;

            if (playersInRange[0].Count == 0 ^ playersInRange[1].Count == 0)
            {
                DeltaOwnership = 0;
                // A single realm has exclusive control of the area, begin capturing.
                if (playersInRange[0].Count > playersInRange[1].Count)
                {
                    //DeltaOwnership = Math.Max(-6 - (playersInRange[0].Count - 1)*2, -12);

                    for (int i = 0; i < playersInRange[0].Count; ++i)
                    {
                        if (playersInRange[0][i].StealthLevel == 0)
                        {
                            DeltaOwnership -= 6;
                            if (DeltaOwnership == -24)
                                break;
                        }
                    }
                }
                else if (playersInRange[1].Count > playersInRange[0].Count)
                {
                    //DeltaOwnership = Math.Min(6 + (playersInRange[1].Count - 1)*2, 12);

                    for (int i = 0; i < playersInRange[1].Count; ++i)
                    {
                        if (playersInRange[1][i].StealthLevel == 0)
                        {
                            DeltaOwnership += 6;
                            if (DeltaOwnership == 24)
                                break;
                        }
                    }
                }

                Ownership += DeltaOwnership;

                if (Ownership > 100 && Ownership < 125)
                    Ownership = 100;
                else if (Ownership < 0)
                    Ownership = 255;
                else if (Ownership < 155 && Ownership > 125)
                    Ownership = 155;
                else if (Ownership > 255)
                    Ownership = 0;

                switch (Ownership)
                {
                    case 100:
                        if (OwningRealm != 2)
                        {
                            OwningRealm = 2;
                            ownershipChanged = true;
                        }
                        DeltaOwnership = 0;
                        break;

                    case 155:
                        if (OwningRealm != 1)
                        {
                            OwningRealm = 1;
                            ownershipChanged = true;
                        }
                        DeltaOwnership = 0;
                        break;

                    case 255:
                        goto case 0;
                    case 0:
                        if (OwningRealm != 0)
                        {
                            OwningRealm = 0;
                            ownershipChanged = true;
                        }
                        DeltaOwnership = 0;
                        break;
                }
            }
            else
                DeltaOwnership = 0;

            if (ownershipChanged)
            {
                foreach (Object obj in new List<Object>(ObjectsInRange))
                {
                    Player plr = obj as Player;
                    if (plr != null)
                        SendMeTo(plr);
                }
            }

            if (oldDeltaOwnership != DeltaOwnership)
            {
                // Capture rate changed, update clients.
                for (int i = 0; i < 2; i++)
                {
                    foreach (Player plr in playersInRange[i])
                        SendFlagControl(plr);
                }
            }

            return ownershipChanged;
        }

        public void SendFlagState(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
            Out.WriteUInt32((uint)ObjectiveID);
            Out.Fill(0xFF, 6);
            Out.WriteUInt16(0);
            Out.WriteByte((byte)OwningRealm);
            Out.Fill(0, 3); // 2nd byte can be meaningful!
            plr.SendPacket(Out);
        }

        public void SendFlagControl(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_CONTROL);
            //Out.WriteStringBytes("000013ED04040000");
            Out.WriteUInt32((uint)ObjectiveID);
            Out.WriteByte((byte)Ownership);
            Out.WriteByte((byte)DeltaOwnership);
            Out.WriteUInt16(0);
            plr.SendPacket(Out);
        }

        public void SendFlagInfo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO);
            Out.WriteUInt32((uint)ObjectiveID);
            Out.WriteByte(0);
            Out.WriteByte((byte)OwningRealm);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WritePascalString(ObjectiveName);
            Out.WriteByte(2);
            Out.WriteUInt32(0x0000348F);
            Out.WriteUInt32(0x0000FF00);

            if (OwningRealm == 0)
                Out.WritePascalString("Capturing");
            else if ((int)plr.Realm == OwningRealm)
                Out.WritePascalString("Defend");
            else
                Out.WritePascalString("Attack");

            Out.WriteByte(0);
            Out.WritePascalString(""); // Tooltip text
            Out.Fill(0, 12);
            Out.WriteUInt16(0x3101);
            Out.Fill(0, 3);
            plr.SendPacket(Out);
        }

        public void SendFlagLeft(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE);

            Out.WriteUInt32((uint)ObjectiveID);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }
    }
}