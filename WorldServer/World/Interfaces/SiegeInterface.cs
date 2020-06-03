using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using CreatureSubTypes = GameData.CreatureSubTypes;
using CreatureTypes = GameData.CreatureTypes;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;
using Vector3 = FrameWork.Vector3;

namespace WorldServer.World.Interfaces
{
    public enum SiegeType
    {
        DIRECT = 1,
        OIL = 3,
        SNIPER = 2,
        RAM = 4,
        GTAOE = 5
    }

    public class SiegeInterface : BaseInterface
    {
        private enum MoveType
        {
            Static,
            Tow,
            Pickup
        }
        private ushort _strikePower;
        private ushort _strikeTarget;
        private ushort _abilityId;

        private byte _maxPlayers = 1;

        /// <summary>
        /// Indicates that the siege weapon is deployed and can fire.
        /// </summary>
        public bool IsDeployed = true;

        /// <summary>
        /// Indicates that the siege weapon is wheeled and can be towed by a player.
        /// </summary>
        private MoveType _moveType;

        /// <summary>
        /// Used to prevent issues which can result if an attempt is made to use the weapon too soon after its deployment.
        /// </summary>
        private long _nextUseTime;

        /// <summary>
        /// The Creature representing the siege weapon.
        /// </summary>
        private Creature _weapon;

        /// <summary>
        /// The type of siege weapon.
        /// </summary>
        public SiegeType Type { get; private set; }

        /// <summary>
        /// The handling state of the siege weapon with respect to the current player, if any.
        /// </summary>
        private SiegeObjectState _useState = SiegeObjectState.Empty;

        private long _deathTime;
        public long DeathTime {
            get { return _deathTime; }
            set {
                if (value > 0)
                    _deathTime = value;
                else
                    _deathTime = TCPManager.GetTimeStampMS();
            }
        }

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _weapon = (Creature) owner;

            switch (_weapon.Spawn.Proto.CreatureSubType)
            {
                case (byte)CreatureSubTypes.SIEGE_GTAOE:
                    Type = SiegeType.GTAOE;
                    break;
                case (byte)CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    Type = SiegeType.SNIPER;
                    break;
                case (byte)CreatureSubTypes.SIEGE_OIL:
                    Type = SiegeType.OIL;
                    break;
                case (byte)CreatureSubTypes.SIEGE_RAM:
                    Type = SiegeType.RAM;
                    _maxPlayers = 4;
                    break;
                default:
                    Type = SiegeType.DIRECT;
                    break;
            }
            
        }

        /// <summary>
        /// The player who originally created this weapon, if any, and who has priority over all other users.
        /// </summary>
        public Player Creator;

        /// <summary>
        /// The player currently acting as principal operator of this weapon.
        /// </summary>
        private Player _leader;

        #region Player Management

        public List<KeyValuePair<Player, byte>> Players { get; } = new List<KeyValuePair<Player, byte>>();

        public int PlayerCount
        {
            get
            {
                int count;
                lock (Players)
                {
                    count = Players.Count;
                }

                return count;
            }
        }

        public void Interact(Player player, byte menu)
        {
            if (TCPManager.GetTimeStampMS() < _nextUseTime)
                return;

            if (_weapon.Spawn.Proto.CreatureType == (byte)CreatureTypes.SIEGE && Type != SiegeType.DIRECT)
            {
                foreach (var plr in Players.ToList())
                {
                    if (!plr.Key.IsInWorld() || !plr.Key.ObjectWithinRadiusFeet(_weapon, 15))
                        RemovePlayer(plr.Key);
                }

                if (menu == 0)
                {
                    if (player.CurrentSiege != null)
                    {
                        if (player.CurrentSiege != _Owner)
                            player.SendClientMessage("You must first release your current siege weapon.");
                        return;
                    }

                    AddPlayer(player);
                }
                else if (menu == 34)
                {
                    RemovePlayer(player);
                }

                SendSiegeUserUpdate();
            }
        }

        private void AddPlayer(Player player)
        {
            if (_moveType == MoveType.Static)
            {
                HandleCannon(player);
                return;
            }

            if (IsDeployed)
            {
                if (!player.IsMounted)
                    HandleCannon(player);
                else if (_moveType == MoveType.Tow)
                    TryTow(player);
                else
                    TryPickup(player);
            }
            else
            {
                if (!player.IsMounted)
                    TryDeploy(player);
                else if (_moveType == MoveType.Tow)
                    HandleTow(player);
                // pickup siege can never enter a visible state where it is not deployed
            }
        }

        /// <summary>
        /// Attempts to change the state of a weapon from deployed to mobile and have the player tow it.
        /// </summary>
        /// <param name="player"></param>
        private void TryTow(Player player)
        {
            if (PlayerCount > 0)
            {
                if (player != Creator || (_leader != null && _leader.GmLevel > 1))
                {
                    player.SendClientMessage("A player is already towing this weapon.");
                    return;
                }
                
                if (player == _leader)
                    return;
                
                RemoveTower(_leader);
            }

            if (IsDeployed)
            {
                IsDeployed = false;

                foreach (Player p in _Owner.PlayersInRange)
                {
                    _Owner.SendRemove(p);
                    _Owner.SendMeTo(p);
                }
            }

            HandleTow(player);
        }

        /// <summary>
        /// Attempts to carry a siege weapon.
        /// </summary>
        /// <param name="player"></param>
        private void TryPickup(Player player)
        {
            if (player != Creator)
            {
                player.SendClientMessage("Only the purchaser of a pickup weapon can move it.");
                return;
            }

            if (PlayerCount > 0)
                return;

            if (IsDeployed)
            {
                IsDeployed = false;
                _Owner.IsActive = false;
                _Owner.ClearRange();
            }

            Players.Add(new KeyValuePair<Player, byte>(player, 0));
            player.CurrentSiege = _weapon;
            SetLeader(player);

            player.Speed = 90;
            player.UpdateSpeed();

            _weapon.Speed = (ushort)(235 * 1.44f);
            _weapon.StsInterface.Speed = _weapon.Speed;
            _weapon.MvtInterface.SetBaseSpeed(_weapon.Speed);

            _weapon.MvtInterface.Follow(player, 12, 20, true);

            player.OSInterface.AddEffect(0xB, (byte)FLAG_EFFECTS.Mball1);

            player.SendClientMessage($"Now carrying the {_Owner.Name}.");
        }

        /// <summary>
        /// Called when a player attempts to tow this weapon.
        /// </summary>
        /// <param name="player"></param>
        private void HandleTow(Player player)
        {
            if (PlayerCount > 0)
            {
                if (player != Creator || (_leader != null && _leader.GmLevel > 1))
                {
                    player.SendClientMessage("A player is already towing this weapon.");
                    return;
                }

                if (player == _leader)
                    return;

                RemoveTower(_leader);
            }

            Players.Add(new KeyValuePair<Player, byte>(player, 0));
            player.CurrentSiege = _weapon;
            SetLeader(player);

            player.Speed = 90;
            player.UpdateSpeed();

            _weapon.Speed = (ushort)(235 * 1.44f);
            _weapon.StsInterface.Speed = _weapon.Speed;
            _weapon.MvtInterface.SetBaseSpeed(_weapon.Speed);

            _weapon.MvtInterface.Follow(player, 12, 20, true);

            player.SendClientMessage($"Began towing the {_Owner.Name}.");
        }

        /// <summary>
        /// Attempts to change the state of a weapon from mobile to deployed.
        /// </summary>
        /// <param name="player"></param>
        private void TryDeploy(Player player)
        {
            if (PlayerCount > 0)
            {
                player.SendClientMessage("You cannot deploy a siege weapon while it is being towed.");
                return;
            }

            IsDeployed = true;

            _weapon.MvtInterface.StopMove();

            if (Type == SiegeType.RAM)
            {
                var zoneKeeps = _Owner.Region.Campaign.GetZoneKeeps((ushort)player.ZoneId);
                var dictKeepDistances = new Dictionary<BattleFrontKeep, float>();
                foreach (var battleFrontKeep in zoneKeeps)
                {
                    ulong curDist = battleFrontKeep.GetDistanceSquare(player.WorldPosition);
                    dictKeepDistances.Add(battleFrontKeep, curDist);
                }
                var keepToUse = dictKeepDistances.OrderByDescending(pair => pair.Value).Take(1).ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var f in keepToUse)
                {
                    f.Key.TryAlignRam(_Owner, player);
                    break;
                }
            }

            foreach (Player p in _Owner.PlayersInRange)
            {
                _Owner.SendRemove(p);
                _Owner.SendMeTo(p);
            }

            _nextUseTime = TCPManager.GetTimeStampMS() + 1000;
            _deathTime = TCPManager.GetTimeStampMS() + 300 * 1000;

            player.SendClientMessage($"Deployed the {_Owner.Name}.");
        }

        /// <summary>
        /// Called when a player attempts to use this weapon in its deployed state.
        /// </summary>
        /// <param name="player"></param>
        private void HandleCannon(Player player)
        {
            if (HasPlayer(player))
            {
                SendSiegeUseState(player, player == _leader);
                return;
            }

            if (PlayerCount >= _maxPlayers && (Type == SiegeType.RAM || player != Creator))
                return;

            lock (Players)
                if (Players.All(e => e.Key.Oid != player.Oid))
                {
                    Players.Add(new KeyValuePair<Player, byte>(player, 0));
                    player.CurrentSiege = _weapon;
                }

            if (Type == SiegeType.RAM && PlayerCount > 1)
                SendSiegeResponse(player, Type, SiegeControlType.Helper, 1, _weapon.Spawn.Proto.Name);
            else
                SendSiegeResponse(player, Type, SiegeControlType.Leader, 1, _weapon.Spawn.Proto.Name);

            if (_leader == null)
                SetLeader(player);

            // Remove other players on the siege if the owner attempts to take control of it
            if (Type != SiegeType.RAM && PlayerCount > 1 && player == Creator)
            {
                SetLeader(player);

                lock (Players)
                {
                    for (int i = 0; i < Players.Count; ++i)
                    {
                        if (Players[i].Key == Creator)
                            continue;

                        Player curPlayer = Players[i].Key;
                        //_players.RemoveAt(i);
                        //curPlayer.CurrentSiege = null;

                        if (curPlayer.IsInWorld())
                        {
                            SendSiegeResponse(curPlayer, Type, SiegeControlType.Release, 0);
                            SendSiegeResponse(curPlayer, Type, SiegeControlType.Leader, 1);
                        }

                        //if (Type == SiegeType.RAM)
                        //    RamPlayerLeft(curPlayer);

                        curPlayer.SendClientMessage("Displaced by weapon owner", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        curPlayer.SendClientMessage("The owner of this siege weapon has displaced you from it.");

                        //--i;
                    }
                }
            }

            SendSiegeUseState(player, player == _leader);
        }

        /// <summary>
        /// Changes the current operator of the siege weapon.
        /// </summary>
        /// <param name="player"></param>
        public void SetLeader(Player player)
        {
            _leader = player;
            if (_leader != null)
            {
                SendSiegeIdleTimer(_leader);
            }
        }

        /// <summary>
        /// Determines whether or not the provided player is already interacting with this siege weapon.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool HasPlayer(Player player)
        {
            lock (Players)
                return Players.Any(e => e.Key.Oid == player.Oid);
        }

        /// <summary>
        /// Causes the provided player to cease towing this siege weapon.
        /// </summary>
        /// <param name="player"></param>
        public void RemoveTower(Player player)
        {
            if (player == null)
            {
                Log.Error("Siege", "No input player to RemoveTower");
                return;
            }

            lock (Players)
            {
                for (byte i = 0; i < Players.Count; i++)
                {
                    if (Players[i].Key.Oid == player.Oid)
                    {
                        if (i == 0)
                            Players.RemoveAt(i);
                        player.CurrentSiege = null;
                    }
                }
            }

            player.SendClientMessage($"No longer towing the {_Owner.Name}.");
            player.StsInterface.Speed = 100;
            player.UpdateSpeed();
            _leader = null;

            _weapon.MvtInterface.StopMove();
        }

        /// <summary>
        /// Drops a currently carried weapon to the ground.
        /// </summary>
        /// <param name="player"></param>
        public void Drop(Player player)
        {
            lock (Players)
            {
                for (byte i = 0; i < Players.Count; i++)
                {
                    if (i == 0)
                        Players.RemoveAt(i);
                    player.CurrentSiege = null;
                }
            }

            player.SendClientMessage($"No longer carrying the {_Owner.Name}.");

            player.Speed = 100;
            player.UpdateSpeed();

            _leader = null;

            _weapon.MvtInterface.StopMove();

            _deathTime = TCPManager.GetTimeStampMS() + 300 * 1000;

            player.OSInterface.RemoveEffect(0xB);

            IsDeployed = true;

            _weapon.IsActive = true;
        }

        /// <summary>
        /// Removes a player from the siege weapon.
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(Player player)
        {
            if (!IsDeployed)
            { 
                if (_moveType == MoveType.Tow)
                    RemoveTower(player);
                else if (_moveType == MoveType.Pickup)
                    Drop(player);
                return;
            }

            bool newramleader = false;

            lock (Players)
            {
                for(byte i= 0;i < Players.Count ;i++)
                {
                    if (Players[i].Key.Oid == player.Oid)
                    {
                        if (i == 0)
                            newramleader = true;
                        Players.RemoveAt(i);
                        player.CurrentSiege = null;
                    }
                }
            }

            if (player.IsInWorld())
                SendSiegeResponse(player, Type, SiegeControlType.Release, 0);

            if (Type == SiegeType.RAM)
            {
                if (newramleader && Players.Count > 0)
                    SendSiegeResponse(Players.First().Key, SiegeType.RAM, SiegeControlType.Leader, 4);
            }

            if (_leader == player)
            {
                if (PlayerCount == 0)
                    SetLeader(null);
                else
                {
                    SetLeader(Players.First().Key);
                    SendSiegeUseState(_leader, true);
                    SendSiegeIdleTimer(_leader);
                }
            }
        }

        /// <summary>
        /// Removes all existing players from the siege weapon.
        /// </summary>
        public void RemoveAllPlayers()
        {
            lock (Players)
            {
                while (Players.Count > 0)
                {
                    Player curPlayer = Players[0].Key;
                    Players.RemoveAt(0);
                    curPlayer.CurrentSiege = null;

                    if (curPlayer.IsInWorld())
                        SendSiegeResponse(curPlayer, Type, SiegeControlType.Release, 0);
                }
            }

        }

        public bool IsAbandoned => _leader == null && !_weapon.ObjectWithinRadiusFeet(Creator, 250);

        #endregion

        /// <summary>
        /// Assigns the correct ability to the siege weapon and determines whether or not it is movable.
        /// </summary>
        /// <returns></returns>
        public override bool Load()
        {
            switch (Type)
            {
                case SiegeType.GTAOE:
                    switch (_weapon.Spawn.Proto.Model1)
                    {
                        case 1569: // Dwarf Organ Gun
                            _abilityId = 14379;
                            _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                            break;
                        case 1573: // Greenskin Rock Lobba
                            _abilityId = 14383;
                            _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                            break;
                        case 1553: // Hellblaster Cannon
                            _abilityId = 14387;
                            _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                            break;
                        case 1557: // Chaos Tri-Barrel Hellcannon
                            _abilityId = 14391;
                            _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                            break;
                        case 1561: // High Elf Repeater
                            _abilityId = 14395;
                            _moveType = _Owner is Siege ? MoveType.Pickup : MoveType.Static;
                            break;
                        case 1565: // Dark Elf Repeater
                            _abilityId = 14399;
                            _moveType = _Owner is Siege ? MoveType.Pickup : MoveType.Static;
                            break;
                    }
                    break;

                case SiegeType.OIL:
                    switch (_weapon.Spawn.Proto.Model1)
                    {
                        case 1567: // Dwarf Oil
                            _abilityId = 14440;
                            break;
                        case 1571: // Greenskin Oil
                            _abilityId = 14435;
                            break;
                        case 1551: // Empire Oil
                            _abilityId = 14445;
                            break;
                        case 1555: // Chaos Oil
                            _abilityId = 14450;
                            break;
                        case 1559: // High Elf Oil
                            _abilityId = 14455;
                            break;
                        case 1563: // Dark Elf Oil
                            _abilityId = 14460;
                            break;
                    }


                    /*Point2D displacement = Point2D.GetOffsetFromHeading(_Owner.Heading, 15);
                    Point3D displacement3D = new Point3D(_Owner.X + displacement.X, _Owner.Y + displacement.Y, _Owner.Z);
                    
                    WarZoneLib.Vector3 hitLocation = new WarZoneLib.Vector3();
                    RegionData.OcclusionQuery(_Owner.Zone.ZoneId, displacement3D.X, displacement3D.Y, displacement3D.Z, _Owner.Zone.ZoneId, displacement3D.X, displacement3D.Y, displacement3D.Z - 1200, ref hitLocation);
                    
                    _oilDest = new Point3D((int)hitLocation.X, (int)hitLocation.Y, (int)hitLocation.Z);
                    */

                    // new Point3D(displacement3D.X, displacement3D.Y, displacement3D.Z - (45 * 12));
                    //Log.Info("Siege weapon check", $"HitLocation: {_oilDest.X} {_oilDest.Y} {_oilDest.Z}");

                    break;

                case SiegeType.SNIPER:
                    switch (_weapon.Spawn.Proto.Model1)
                    {
                        case 1570: // Dwarf Cannon
                            _abilityId = 14378;
                            _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                            break;
                        case 1574: // Greenskin Supa Chucka
                            _abilityId = 14382;
                            _moveType = _Owner is Siege ? MoveType.Pickup : MoveType.Static;
                            break;
                        case 1554: // Empire Great Cannon
                            _abilityId = 14386;
                            _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                            break;
                        case 1558: // Chaos Hellfire Cannon
                        case 1636: // 60 size variant
                            _abilityId = 14390;
                            _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                            break;
                        case 1562: // High Elf Ballista
                            _abilityId = 14394;
                            _moveType = _Owner is Siege ? MoveType.Pickup : MoveType.Static;
                            break;
                        case 1566: // Dark Elf Ballista
                            _abilityId = 14398;
                            _moveType = _Owner is Siege ? MoveType.Pickup : MoveType.Static;
                            break;
                    }
                    break;

                case SiegeType.RAM:
                    _abilityId = 24684;   // Ram damage.
                    _moveType = _Owner is Siege ? MoveType.Tow : MoveType.Static;
                    break;

                default:
                    IsDeployed = false;
                    break;
            }
              
            return base.Load();
        }

        public void Fire(Player player, ushort targetID, ushort targetX, ushort targetY, ushort targetZ, ushort zoneId, ushort power)
        {
            if (!HasPlayer(player) || !_weapon.AbtInterface.CanCastCooldown(_abilityId))
                return;

            // RB   6/25/2016   Reset timer before death every time the siege is fired
            _deathTime = TCPManager.GetTimeStampMS() + 300 * 1000;


            switch (Type)
            { 
                case SiegeType.GTAOE:
                    Point3D targetPos = ZoneService.GetWorldPosition(_weapon.Zone.Info, targetX, targetY, targetZ);

                    if (!ArcHit(zoneId, new Point3D(targetX, targetY, targetZ), targetPos))
                    {
                        player.SendClientMessage("Can't hit that position from here", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        return;
                    }

                    Siege artillery = _weapon as Siege;
                    if (artillery != null && !artillery.CanFire(player))
                        return;

                    // 72675 is the orcapult - catapults player rather than a stone!
                    if (this._weapon.Entry == 72675)
                    {
                        var targetPosition = ZoneService.GetWorldPosition(_weapon.Zone.Info, targetX, targetY, targetZ);
                        float flightTimePuntee = 4;//(float)this._leader.GetDistanceSquare(new Point3D(targetPosition.X, targetPosition.Y, targetPosition.Z)) / speed / 1000f;
                        this._leader.Catapult(_weapon.Zone, new Point3D(targetPosition.X, targetPosition.Y, targetPosition.Z), (ushort)flightTimePuntee, (ushort)340);
                        this._leader.AbtInterface.Cancel(true);
                        SendSiegeResponse(_leader, Type, SiegeControlType.Leader, 1);
                    }
                    else
                    {
                        _weapon.AbtInterface.StartCastAtPos(player, _abilityId, ZoneService.GetWorldPosition(_weapon.Zone.Info, targetX, targetY, targetZ), _weapon.Zone.ZoneId, 0);
                    }

                    
                    break;
                case SiegeType.OIL:
                    _weapon.AbtInterface.StartCastAtPos(player, _abilityId, ZoneService.GetWorldPosition(_weapon.Zone.Info, targetX, targetY, targetZ), _weapon.Zone.ZoneId, 0);
                    break;
                case SiegeType.SNIPER:
                    Unit target = _weapon.Region.GetObject(targetID) as Unit;

                    if (target == null || !CombatInterface.CanAttack(player, target))
                        return;

                    Siege cannon = _weapon as Siege;
                    if (cannon != null && !cannon.CanFire(player))
                        return;

                    _weapon.CbtInterface.SetTarget(targetID, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                    _weapon.AbtInterface.StartCast(player, _abilityId, 0, 0, Math.Max((byte)1, (byte)(_weapon.Level * power * 0.01f)));
                    break;
                case SiegeType.RAM:
                    Unit ramTarget = null;
                    foreach (Object obj in _Owner.ObjectsInRange)
                    {
                        KeepDoor.KeepGameObject door = obj as KeepDoor.KeepGameObject;

                        if (door == null)
                            continue;

                        if (!CombatInterface.CanAttack(player, door))
                            continue;

                        if (!_Owner.IsObjectInFront(door, 90) || !door.IsWithinRadiusFeet(_Owner, 20))
                            continue;

                        ramTarget = door;
                        break;
                    }
                    //Unit target = _weapon.Region.GetObject(targetID) as Unit;

                    if (ramTarget != null)
                    {
                        _weapon.CbtInterface.SetTarget(ramTarget.Oid, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                        _weapon.AbtInterface.StartCast(player, _abilityId, 0, 0, Math.Max((byte) 1, (byte) (_weapon.Level*power*0.01f)));
                    }

                    else
                        foreach (var plrInfo in Players)
                            plrInfo.Key.SendClientMessage("No target", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    break;
            }

            SendSiegeCooldown();


            // Disable Firing?
            /*
            Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(0x1C);
            Out.WriteByte(2);
            Out.Fill(0, 6);
            player.SendPacket(Out);
            */

            SendSiegeUserUpdate();

            /*
            Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(0x1E);
            Out.WriteUInt16(0);
            Out.WriteByte((byte)_Owner.Name.Length);
            Out.WriteUInt16(0);
            Out.WriteCString(_Owner.Name);
            Out.WriteByte(0);
            player.SendPacket(Out);
            */

            SendSiegeIdleTimer(player);
        }

        /// <summary>
        /// Called when a player sends information about their contribution to the swing of a ram.
        /// </summary>
        /// <param name="plr">The player contributing to the swing.</param>
        /// <param name="targetID">The target of the ram.</param>
        /// <param name="power">The strength of their contribution.</param>
        public void RamSwing(Player plr, ushort targetID, byte power)
        {
            _strikeTarget = targetID;

            for (byte i = 0; i < Players.Count; i++)
            {
                if (Players[i].Key.Oid == plr.Oid)
                {
                    Players[i] = new KeyValuePair<Player, byte>(plr, power);
                    _strikePower += power;
                    #if (DEBUG)
                    Log.Info("SiegeInterface.RamSwing", plr.Name + " is adding " + power + " power to the ram swing. Power total is " + _strikePower);
                    #endif
                }
            }
        }

        public void Repair()
        {


        }

        private bool ArcHit(ushort zoneId, Point3D pinPos, Point3D worldPos)
        {
            if (_weapon.LOSHit(zoneId, pinPos))
                return true;

            #if DEBUG && ARTILLERY_ARC_DEBUG
            Log.Info("Artillery", "Direct LOS check failed, checking arc...");
            #endif

            var playnice = new World.Map.OcclusionInfo();
            int maxZDisplacement = Math.Min(_Owner.Z, pinPos.Z) + 100 * 12; // check 100 feet upwards

            Point3D weaponTopPoint = new Point3D();
            Point3D destPoint = new Point3D();
            int fromRegionX = weaponTopPoint.X + (_weapon.Zone.Info.OffX << 12);
            int fromRegionY = weaponTopPoint.Y + (_weapon.Zone.Info.OffY << 12);
            int toRegionX = pinPos.X + (_weapon.Zone.Info.OffX << 12);
            int toRegionY = pinPos.Y + (_weapon.Zone.Info.OffY << 12);

            Vector2 toTarget = new Vector2(worldPos.X - _weapon.WorldPosition.X, worldPos.Y - _weapon.WorldPosition.Y);
            toTarget.Normalize();

            weaponTopPoint.SetCoordsFrom(_weapon);
            destPoint.SetCoordsFrom(_weapon);

            #if DEBUG && ARTILLERY_ARC_DEBUG
            Log.Info("Artillery", "Weapon location: "+weaponTopPoint);
            #endif
            
            // Check LOS between weapon and close apex (if applicable)
            if (weaponTopPoint.Z < maxZDisplacement)
            {
                destPoint.Z = maxZDisplacement;

                // Check 50 feet in front of the weapon
                destPoint.X += (int)(toTarget.X*50*12);
                destPoint.Y += (int)(toTarget.Y*50*12);

#if DEBUG && ARTILLERY_ARC_DEBUG
                Log.Info("Artillery", "Checking weapon to destination point: " + destPoint);
#endif

               fromRegionX = weaponTopPoint.X + (_weapon.Zone.Info.OffX << 12);
               fromRegionY = weaponTopPoint.Y + (_weapon.Zone.Info.OffY << 12);
               toRegionX = destPoint.X + (_weapon.Zone.Info.OffX << 12);
               toRegionY = destPoint.Y + (_weapon.Zone.Info.OffY << 12);

                World.Map.Occlusion.SegmentIntersect(zoneId, zoneId, fromRegionX, fromRegionY, weaponTopPoint.Z + 120, toRegionX, toRegionY, destPoint.Z, true, true, 190, ref playnice);

                if (playnice.Result != OcclusionResult.NotOccluded)
                {
                    #if DEBUG && ARTILLERY_ARC_DEBUG
                    Log.Info("Artillery", "Arc check failed (obstruction between cannon and arc apex 1)");
                    foreach (var player in Players)
                        player.Key.ZoneTeleport((ushort)destPoint.X, (ushort)destPoint.Y, (ushort)destPoint.Z, 0);
                    #endif
                    return false;
                }

                weaponTopPoint.SetCoordsFrom(destPoint);
            }

            // Check LOS between target and far apex (if applicable)
            destPoint.SetCoordsFrom(pinPos);
            #if DEBUG && ARTILLERY_ARC_DEBUG
            Log.Info("Artillery", "Target location: " + pinPos);
            #endif

            if (pinPos.Z < maxZDisplacement)
            {
                destPoint.Z = maxZDisplacement;

                // Much more lenient on drop - 10ft
                destPoint.X -= (int)(toTarget.X * 10 * 12);
                destPoint.Y -= (int)(toTarget.Y * 10 * 12);
                //#if DEBUG && ARTILLERY_ARC_DEBUG
                //Log.Info("Artillery", "Checking target location to destination point: " + destPoint);
                //#endif

                //RegionData.OcclusionResult arc2Result = RegionData.OcclusionQuery(
                //   zoneId, pinPos.X, pinPos.Y, pinPos.Z + 120,
                //   zoneId, destPoint.X, destPoint.Y, destPoint.Z, ref playnice);
               fromRegionX = weaponTopPoint.X + (_weapon.Zone.Info.OffX << 12);
               fromRegionY = weaponTopPoint.Y + (_weapon.Zone.Info.OffY << 12);
               toRegionX = destPoint.X + (_weapon.Zone.Info.OffX << 12);
               toRegionY = destPoint.Y + (_weapon.Zone.Info.OffY << 12);

                World.Map.Occlusion.SegmentIntersect(zoneId, zoneId, fromRegionX, fromRegionY, weaponTopPoint.Z + 120, toRegionX, toRegionY, destPoint.Z, true, true, 190, ref playnice);


                if (playnice.Result != OcclusionResult.NotOccluded)
                {
                    #if DEBUG && ARTILLERY_ARC_DEBUG
                    Log.Info("Artillery", "Arc check failed (obstruction between target point and arc apex 2)");
                    foreach (var player in Players)
                        player.Key.ZoneTeleport((ushort)destPoint.X, (ushort)destPoint.Y, (ushort)destPoint.Z, 0);
                    #endif
                    return false;
                }

                pinPos.SetCoordsFrom(destPoint);
            }

#if DEBUG && ARTILLERY_ARC_DEBUG
            Log.Info("Artillery", "Checking apex 1 "+weaponTopPoint+" to apex 2 " + pinPos);
#endif

            fromRegionX = weaponTopPoint.X + (_weapon.Zone.Info.OffX << 12);
            fromRegionY = weaponTopPoint.Y + (_weapon.Zone.Info.OffY << 12);
            toRegionX = pinPos.X + (_weapon.Zone.Info.OffX << 12);
            toRegionY = pinPos.Y + (_weapon.Zone.Info.OffY << 12);

            // Check LOS between two apex points
            World.Map.Occlusion.SegmentIntersect(zoneId, zoneId, fromRegionX, fromRegionY, weaponTopPoint.Z + 120, toRegionX, toRegionY, pinPos.Z, true, true, 190, ref playnice);

            if (playnice.Result != OcclusionResult.NotOccluded)
            {
#if DEBUG && ARTILLERY_ARC_DEBUG
                Log.Info("Artillery", "Arc check failed (obstruction between arc apexes)");
                //foreach (var player in Players)
                //    player.Key.ZoneTeleport((ushort)weaponTopPoint.X, (ushort)weaponTopPoint.Y, (ushort)weaponTopPoint.Z, 0);
                foreach (var player in Players)
                    player.Key.ZoneTeleport((ushort)pinPos.X, (ushort)pinPos.Y, (ushort)pinPos.Z, 0);
#endif
                return false;
            }

            #if DEBUG && ARTILLERY_ARC_DEBUG
            Log.Info("Artillery", "Arc check passed");
            #endif
            return true;
        }

        #region Senders

        public void SendSiegeResponse(Player player, SiegeType type, SiegeControlType controlType, byte maxPlayers = 0, string siegeName = null)
        {
            if (type == SiegeType.RAM)
                maxPlayers = 4;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);

            Out.WriteByte(0x18); //siege 
            Out.WriteByte((byte)controlType); //control type LEADER, HELPER, RELEASE
            Out.WriteByte(0);

            if (controlType != SiegeControlType.Release)
            {
                Out.WriteByte(0x46); //some sort of ID
                Out.WritePascalString(siegeName);
                Out.WriteByte(1);
                Out.WriteByte(0x28);
                Out.WriteByte(0);
                Out.WriteByte(0);

                if (type == SiegeType.OIL)
                {
                    Out.WriteByte(0x18);
                    Out.WriteByte(0x6A);
                }
                else if (type == SiegeType.GTAOE)
                {
                    Out.WriteByte(0x18);
                    Out.WriteByte(0x6A);
                }
                else if (type == SiegeType.RAM)
                {
                    Out.WriteByte(0x5B);
                    Out.WriteByte(0x60);
                }
                else if (type == SiegeType.SNIPER)
                {
                    Out.WriteByte(0x18);
                    Out.WriteByte(0x6A);
                }

                Out.WriteUInt16(_abilityId);

                Out.WriteByte(0);
                Out.WriteByte(04);
                Out.WriteByte(0);
                Out.WriteByte(maxPlayers); //player count
                Out.WriteByte(0x1E);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0x10);
                Out.WriteByte((byte)type);//aim type
                Out.WriteByte(0);        //time? 
                Out.WriteByte(0);
                Out.WriteByte(0x27);
                Out.WriteByte(0x0);
            }

            player.SendPacket(Out);
        }
        public void SendSiegeMultiuser()
        {
            _strikePower = 0;

            _Owner.EvtInterface.AddEvent(SendSiegeResult, 3000, 1);

            PacketOut Out = new PacketOut((byte)Opcodes.F_START_SIEGE_MULTIUSER);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(1);
            Out.WriteByte(0x2c);
            Out.WriteByte(0x32);
            Out.WriteByte(0);

            foreach(KeyValuePair<Player,byte> plr in Players)
            {
                plr.Key.SendPacket(Out);
            }
           // SendSiegeResult();
        }

        private void SendSiegeUseState(Player player, bool enabled)
        {
            //enable fire button
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(_weapon.Oid);
            Out.WriteByte(0x1C);
            Out.WriteByte(enabled ? (byte)0x3 : (byte)0x5); // 0x1c03
            player.SendPacket(Out);
        }
        private void SendSiegeResult()
        {
            if (Players != null && Players.Count > 0)
            {
                _strikePower = (ushort)(_strikePower / 4);

                PacketOut Out = new PacketOut((byte)Opcodes.F_SIEGE_WEAPON_RESULTS);
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte((byte)_strikePower);  // owner power ??
                Out.WriteByte((byte)_strikePower);  // total power ??
                Out.WriteByte((byte)Players.Count);
                foreach (KeyValuePair<Player, byte> plr in Players)
                {
                    Out.WriteUInt16(plr.Key.Oid);
                    Out.WriteByte(plr.Value);  // power
                }
                foreach (KeyValuePair<Player, byte> plr in Players)
                {
                    plr.Key.SendPacket(Out);
                }

                Fire(Players.First().Key, _strikeTarget, 0, 0, 0, 0, _strikePower);
            }

        }
            
        /// <summary>
        /// Sends the cooldown after the siege weapon has fired.
        /// </summary>
        private void SendSiegeCooldown()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER);
            Out.WriteUInt16(_abilityId);
            Out.WriteUInt16(0x80); // Unk
            Out.WriteUInt32(Type == SiegeType.SNIPER ? (uint)2500 : (uint)4500); // 5s cooldown
            Out.Fill(0, 4);

            foreach (KeyValuePair<Player, byte> plr in Players)
            {
                plr.Key.SendPacket(Out);
            }

            Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER);
            Out.WriteHexStringBytes("000101800000000000000000");

            foreach (KeyValuePair<Player, byte> plr in Players)
            {
                plr.Key.SendPacket(Out);
            }

        }

        /// <summary>
        /// Updates the list of current users of the siege weapon on clients.
        /// </summary>
        private void SendSiegeUserUpdate()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(0x1d);
            Out.WriteByte((byte)Players.Count);  // players
            Out.WriteByte(_maxPlayers); // MAXIMUM USERS
            Out.WriteByte((byte)(Players.Count * 2));  // players
            Out.WriteByte(0);
            Out.WriteByte(0);
            foreach (KeyValuePair<Player, byte> plr in Players)
            {
                Out.WriteUInt16(plr.Key.Oid);
            }
            Out.WriteByte(0);
            Out.WriteByte(0);

            foreach (KeyValuePair<Player, byte> plr in Players)
            {
                plr.Key.SendPacket(Out);
            }
        }

        /// <summary>
        /// Updates the idle timer, which counts down to the siege weapon's destruction through lack of use.
        /// </summary>
        /// <param name="plr"></param>
        private void SendSiegeIdleTimer(Player plr)
        {
            PacketOut idleTimerPacket = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            idleTimerPacket.WriteUInt16(_Owner.Oid);
            idleTimerPacket.WriteByte((byte)StateOpcode.SiegeIdleTimer); // Update Siege Timer
            idleTimerPacket.WriteByte(60); // Siege Auto-Kick Timer
            idleTimerPacket.Fill(0, 6);
            plr.SendPacket(idleTimerPacket);
        }

        #endregion

        #region Cannon Targeting

        public  List<Unit> CurrentTargetList { get; } = new List<Unit>(); 

        /// <summary>
        /// Builds a list of targets which would be hit by a line attack performed by a siege cannon.
        /// </summary>
        /// <param name="instigator">The player firing the cannon.</param>
        /// <returns>The farthest target from the siege weapon which would be struck by the attack, for use as the target of the ability.</returns>
        public Unit BuildTargetList(Unit instigator)
        {
            CurrentTargetList.Clear();

            Unit initialTarget = _weapon.CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY);

            Unit bestTarget = initialTarget;
            int bestDist = _weapon.GetDistanceToObject(initialTarget);

            CurrentTargetList.Add(initialTarget);

            Vector3 unitDir = new Vector3(initialTarget.WorldPosition.X - _weapon.WorldPosition.X,
                                      initialTarget.WorldPosition.Y - _weapon.WorldPosition.Y,
                                      initialTarget.WorldPosition.Z - _weapon.WorldPosition.Z);

            unitDir.Normalize();

            Vector3 toTarget = new Vector3();
            Vector3 projection = new Vector3();

            foreach (Object obj in _weapon.ObjectsInRange)
            {
                Unit unit = obj as Unit;

                if (unit == null || unit == initialTarget || unit.Realm == instigator.Realm || !_weapon.IsObjectInFront(unit, 45) || !CombatInterface.CanAttack(instigator, unit))
                    continue;

                // Determine whether this unit is within 5ft on either side of the cannon's attack.

                // Unit vector in direction of cannon's view
                projection.X = unitDir.X;
                projection.Y = unitDir.Y;
                projection.Z = unitDir.Z;

                toTarget.X = unit.WorldPosition.X - _weapon.WorldPosition.X;
                toTarget.Y = unit.WorldPosition.Y - _weapon.WorldPosition.Y;
                toTarget.Z = unit.WorldPosition.Z - _weapon.WorldPosition.Z;

                // Vector projection ((a dot ^b) ^b)
                projection.Multiply(Vector3.DotProduct3D(toTarget, unitDir));

                // Vector rejection (a - (a dot ^b) ^b)
                toTarget.X -= projection.X;
                toTarget.Y -= projection.Y;
                toTarget.Z -= projection.Z;

                if (toTarget.MagnitudeSquare < 60 * 60 && _weapon.LOSHit(unit)) // 5ft either side
                {
                    CurrentTargetList.Add(unit);

                    // Select the target furthest away as the target of the ability, for the projectile effect to look best
                    if (!_weapon.IsWithinRadiusFeet(unit, bestDist))
                    {
                        bestTarget = unit;
                        bestDist = _weapon.GetDistanceToObject(bestTarget);
                    }
                }
            }

            return bestTarget;
        }

        #endregion
    }
}
