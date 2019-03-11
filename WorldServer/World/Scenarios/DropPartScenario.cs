using System.Text;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    public class DropPartScenario : Scenario
    {
        private GameObject _centerGlow;
        private Part _salvage;
        private GameObject _orderCart;
        private GameObject _destroCart;
        private Player _carrier;
        private bool _salvageMoved = true;
        private Point3D _salvageSpawn = new Point3D();
        public DropPartScenario(Scenario_Info info, int tier)
            : base(info, tier)
        {
            foreach (Scenario_Object scBall in info.ScenObjects)
            {
                if (scBall.ObjectiveName == "Salvage")
                {
                    CreateSalvage(scBall.WorldPosX, scBall.WorldPosY, scBall.PosZ, scBall.Heading);
                    _salvageSpawn = new Point3D(scBall.WorldPosX, scBall.WorldPosY, scBall.PosZ);
                }

                if (scBall.ObjectiveName == "Order Parts Wagon")
                    _orderCart = AddObject(scBall.WorldPosX, scBall.WorldPosY, scBall.PosZ, scBall.Heading, 100595);

                if (scBall.ObjectiveName == "Destruction Parts Wagon")
                    _destroCart = AddObject(scBall.WorldPosX, scBall.WorldPosY, scBall.PosZ, scBall.Heading, 98822);

                if (scBall.ObjectiveName == "Center Glow")
                    _centerGlow = AddObject(scBall.WorldPosX, scBall.WorldPosY, scBall.PosZ, scBall.Heading, 99858);

            }
            _orderCart.Flags = 0x24;
            _destroCart.Flags = 0x24;
            _salvage.Flags = 0x24;
        }

        private void CreateSalvage(int x, int y, int z, int o)
        {
            if (_salvage != null)
            {
                _salvage.RemoveFromWorld();
            }

            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(100598);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = 0,
                WorldX = x,
                WorldY = y,
                WorldZ = (ushort)z,
                ZoneId = Region.RegionId,
            };
            spawn.BuildFromProto(glowProto);

            _salvage = new Part(spawn, FLAG_EFFECTS.Mball1, 2000, 2000)
            {
                PickedUp = new Part.PartDelegate(SalvagePickedUp),
                DroppedOff = new Part.PartDelegate(SalvageDroppedOff),
                Lost = new Part.PartDelegate(SalvageLost)
            };
            Region.AddObject(_salvage, spawn.ZoneId);
           
        }

        public override void OnClose()
        {
            if (_carrier != null)
            {
                _carrier.OSInterface.RemoveEffect(0xB);
                _carrier.CanMount = true;
                _carrier = null;
            }
        }
        public override void RemovePlayer(Player plr, bool logout)
        {
            base.RemovePlayer(plr, logout);
            if (_carrier == plr)
            {
               ResetPart();
            }
        }
        private void SalvagePickedUp(Player plr, Part part)
        {
            _salvage.RemoveFromWorld();

            _carrier = plr;

            EvtInterface.RemoveEvent(PartTimer);
            EvtInterface.AddEvent(PartTimer, 120000, 1);

            for (int i = 0; i < 2; ++i)
                foreach (Player player in Players[i])
                {

                    PacketOut Out = new PacketOut((byte)Opcodes.F_LOCALIZED_STRING, 64);

                    Out.WriteUInt16((ushort)256);

                    Out.WriteUInt16(0);
                    Out.WriteUInt16((ushort)Localized_text.TEXT_FLAG_PICKUP);
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(0x0003);
                    Out.WriteUInt16(0x0100);


                    byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(plr.Name);
                    Out.WriteByte((byte)(bytes.Length + 1));
                    Out.Write(bytes, 0, bytes.Length);


                    Out.Fill(0, 5);
                    if (plr.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        Out.WriteByte(0x9C);
                    else
                        Out.WriteByte(0x9B);

                    Out.WriteUInt32(0);
                    Out.WriteUInt16(0x0100);

                    bytes = Encoding.GetEncoding("iso-8859-1").GetBytes("Salvage Part");
                    Out.WriteByte((byte)(bytes.Length + 1));
                    Out.Write(bytes, 0, bytes.Length);


                    Out.WriteByte(0);

                    player.SendPacket(Out);
                    SendObjectiveStates(player);
                }

        }

        public void PartTimer()
        {
            if (_carrier != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    foreach (Player player in Players[i])
                    {
                        player.SendClientMessage(_carrier.Name + " took too long to capture the salvage, salvage reset", ChatLogFilters.CHATLOGFILTERS_C_WHITE_L);
                    }
                }
                ResetPart();
            }
        }

        private void SalvageDroppedOff(Player plr, Part part)
        {
            for (int i = 0; i < 2; ++i)
                foreach (Player player in Players[i])
                {
                    PacketOut Out = new PacketOut((byte)Opcodes.F_LOCALIZED_STRING, 64);

                    Out.WriteUInt16((ushort)256);

                    Out.WriteUInt16(0);
                    Out.WriteUInt16((ushort)Localized_text.TEXT_FLAG_CAPTURE);
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(0x0003);
                    Out.WriteUInt16(0x0100);


                    byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(plr.Name);
                    Out.WriteByte((byte)(bytes.Length + 1));
                    Out.Write(bytes, 0, bytes.Length);


                    Out.Fill(0, 5);
                    if (plr.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        Out.WriteByte(0x9C);
                    else
                        Out.WriteByte(0x9B);

                    Out.WriteUInt32(0);
                    Out.WriteUInt16(0x0100);

                    bytes = Encoding.GetEncoding("iso-8859-1").GetBytes("Salvage Part");
                    Out.WriteByte((byte)(bytes.Length + 1));
                    Out.Write(bytes, 0, bytes.Length);


                    Out.WriteByte(0);

                    player.SendPacket(Out);

                    SendObjectiveStates(player);
                }
            ResetPart();
            GivePoints((plr.Realm == Realms.REALMS_REALM_ORDER ? 1 : 2), 75);
            //rewarding the player turning in the salvage so it gets more focused on objectives
            plr.AddRenown(70, true);
            if (plr.Realm == Realms.REALMS_REALM_ORDER)
                _orderCart.PlayEffect(1136, new Point3D(_orderCart.Spawn.WorldX, _orderCart.Spawn.WorldY, _orderCart.Spawn.WorldZ+50));
            else
                _destroCart.PlayEffect(1136, new Point3D(_destroCart.Spawn.WorldX, _destroCart.Spawn.WorldY, _destroCart.Spawn.WorldZ+50));
        }

        private void SalvageLost(Player plr, Part part)
        {
            plr.OSInterface.RemoveEffect(0xB);
            _carrier = null;
            CreateSalvage(plr.WorldPosition.X, plr.WorldPosition.Y, plr.WorldPosition.Z, 0);
        }

        private GameObject AddObject(int x, int y, int z, int o, uint entry)
        {
            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(entry);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = o,
                WorldX = x,
                WorldY = y,
                WorldZ = (ushort)z,
                ZoneId = Region.RegionId,
            };
            spawn.BuildFromProto(glowProto);

            var obj = new GameObject(spawn);
            Region.AddObject(obj, spawn.ZoneId);

            return obj;
        }

        public override void Interact(GameObject obj, Player plr, InteractMenu menu)
        {
            base.Interact(obj, plr, menu);

            if ((obj == _orderCart && plr.Realm == Realms.REALMS_REALM_ORDER && plr == _carrier)
                || (obj == _destroCart && plr.Realm == Realms.REALMS_REALM_DESTRUCTION && plr == _carrier))
                PutPartInCart(plr, obj);

            else if (_carrier == null && obj == _salvage)
            {
                if (plr.StealthLevel > 0)
                {
                    plr.SendClientMessage("You can't interact with objects while in stealth.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }
                PickupPart(plr);
            }
        }

        public void PickupPart(Player plr)
        {
            if (_carrier == null)
                _salvage.BeginPickup(plr);
        }

        public void ResetPart()
        {
            if (_carrier != null)
            {
                _carrier.OSInterface.RemoveEffect(0xB);
                _carrier.CanMount = true;
            }
            _carrier = null;
            CreateSalvage(_salvageSpawn.X, _salvageSpawn.Y, _salvageSpawn.Z, 0);
        }

        public void DropPart()
        {
        }

        public void PutPartInCart(Player plr, GameObject targetCart)
        {
            if (_carrier == plr)
                _salvage.BeginDropOff(plr, targetCart);

        }

        public override bool OnPlayerKilled(Object pkilled, object instigator)
        {
            if (pkilled == _carrier)
            {
                if(_carrier != null)
                    _carrier.CanMount = true;

                _salvage.Lost(_carrier, _salvage);
            }
            return base.OnPlayerKilled(pkilled, instigator);
        }

        protected override void UpdateScenario()
        {
            if (_salvageMoved)
            {
                Point3D pos = _salvage.Position;

                for (int i = 0; i < 2; ++i)
                    foreach (Player plr in Players[i])
                    {
                        PacketOut Out = new PacketOut((byte)Opcodes.F_FLAG_OBJECT_LOCATION, 20);
                        Out.WriteUInt32(0x19);
                        Out.WriteByte(_carrier != null ? (byte)_carrier.Realm : (byte)0);
                        Out.Fill(0, 3);
                        Out.WriteUInt32((uint)pos.X);
                        Out.WriteUInt32((uint)pos.Y);
                        Out.WriteUInt32((uint)pos.Z);
                        plr.SendPacket(Out);
                    }
                _salvageMoved = false;         
            }

            if (_carrier != null)
            {

                if (_carrier.StealthLevel > 0)
                {
                    _carrier.Uncloak();
                    _carrier.OSInterface.RemoveEffect(0x0C);
                }
                if (_carrier.WorldPosition.X != _salvage.Position.X || _carrier.WorldPosition.Y != _salvage.Position.Y)
                {
                    _salvageMoved = true;
                    _salvage.Position = new Point3D(_carrier.WorldPosition.X, _carrier.WorldPosition.Y, _carrier.WorldPosition.Z);

                }
                for (int i = 0; i < 2; i++)
                {
                    var distance = _carrier.GetDistanceToWorldPoint(new Point3D(RespawnLocations[i].X, RespawnLocations[i].Y, RespawnLocations[i].Z));
                    if (distance < 190)
                    {
                        ResetPart();
                        break;
                    }
                }

            }


            base.UpdateScenario();
        }


    }
}
