using System;
using System.Collections.Generic;
using System.Threading;
using SystemData;
using FrameWork;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects.Instances;
using WorldServer.World.Objects.Instances.TomboftheVultureLord;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public class Object : Point3D
    {
        public static int RANGE_UPDATE_INTERVAL = 300; // Check des Ranged toutes les 300Ms

        public List<BaseInterface> Interfaces = new List<BaseInterface>();

        public EventInterface EvtInterface;
        public ScriptsInterface ScrInterface;

        // This is an int for the purpose of the atomic Interlocked functions.
        // Its value should not exceed 65535.
        private int _pendingOid;

        public ushort Oid { get; private set; }

        public virtual string Name { get; set; }

        public Object()
        {
            if (EvtInterface == null && !IsPlayer())
                EvtInterface = AddInterface<EventInterface>();

            ScrInterface = AddInterface<ScriptsInterface>();

            IsActive = false;
        }

        #region Disposal

        public bool IsDisposed { get; protected set; }
        public bool PendingDisposal { get; set; }

        public virtual void Destroy()
        {
            if (IsInWorld())
                PendingDisposal = true;
            else
            {
                //Log.Info(Name, "Object disposed directly.");
                Dispose();
            }
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            for (int i = 0; i < Interfaces.Count; ++i)
                Interfaces[i].Stop();

            RemoveFromWorld();
        }

        #endregion

        public virtual void Update(long msTick)
        {
            if (PendingDisposal)
            {
                Dispose();
                return;
            }

            for (int i = 0; i < Interfaces.Count; ++i)
                Interfaces[i].Update(msTick);
        }

        #region Load/Save

        public bool Loaded;

        public void Load()
        {
            Loaded = true;
            OnLoad();
        }

        public virtual void OnLoad()
        {
            LoadInterfaces();
        }

        protected virtual void LoadInterfaces()
        {
            foreach (BaseInterface Interface in Interfaces)
                Interface.Load();
        }

        public virtual void Save()
        {
            foreach (BaseInterface Interface in Interfaces)
            {
                try
                {
                    Interface.Save();
                }
                catch (Exception e)
                {
                    Log.Error("Interface", e.ToString());
                }
            }
        }

        #endregion

        #region Interfaces

        public BaseInterface AddInterface(BaseInterface Interface)
        {
            lock (Interfaces)
            {
                Interfaces.Add(Interface);
            }

            Interface.SetOwner(this);
            return Interface;
        }

        public T AddInterface<T>() where T : BaseInterface
        {
            BaseInterface Interface = Activator.CreateInstance<T>();

            lock (Interfaces)
            {
                Interfaces.Add(Interface);
            }

            Interface.SetOwner(this);
            return (T)Interface;
        }

        public void PlayEffect(ushort effectID, Point3D position = null)
        {

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_EFFECT, 30);
            Out.WriteUInt16(effectID);
            Out.WriteUInt16(0);
            if (position != null)
            {
                Out.WriteUInt32((uint)position.X);
                Out.WriteUInt32((uint)position.Y);
                Out.WriteUInt32((uint)position.Z);
            }
            else
            {
                Out.WriteUInt32((uint)WorldPosition.X);
                Out.WriteUInt32((uint)WorldPosition.Y);
                Out.WriteUInt32((uint)WorldPosition.Z);
            }
            Out.WriteUInt16(100);
            Out.WriteUInt16(100);
            Out.WriteUInt16(100);
            Out.WriteUInt16(100);

            foreach (var p in GetPlayersInRange(400))
                p.SendPacket(Out);

            if (this is Player)
                ((Player)this).SendPacket(Out);
        }



        /// <summary>
        /// Sets object interaction state 
        /// </summary>
        /// <param name="state">1 = interactive, 15=disabled</param>
        public void UpdateInteractState(byte state)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 20);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(state);
            Out.Fill(0, 6);
            DispatchPacket(Out, false);
        }

        public void PlaySound(ushort soundID, Boolean sendarea = true)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_SOUND, 30);
            Out.WriteByte(0);
            Out.WriteUInt16(soundID);
            Out.Fill(10, 0);

            if (sendarea)
                foreach (var p in GetPlayersInRange(400))
                    p.SendPacket(Out);

            if (this is Player)
                ((Player)this).SendPacket(Out);
        }

        public BaseInterface RemoveInterface(BaseInterface Interface)
        {
            lock (Interfaces)
                Interfaces.Remove(Interface);

            return Interface;
        }

        public T GetInterface<T>() where T : BaseInterface
        {
            lock (Interfaces)
                foreach (BaseInterface Interface in Interfaces)
                    if (Interface is T)
                        return (T)Interface;

            return null;
        }

        #endregion

        /// <summary>
        /// Sets the object ID in a thread-safe manner.
        /// </summary>
        public void SetOid(int newOid)
        {
            Interlocked.Exchange(ref _pendingOid, newOid);

            Oid = (ushort)_pendingOid;
        }

        /// <summary>
        /// Sets the object ID to zero only if it has not been changed by another region.
        /// </summary>
        /// <param name="oldOid">The Oid previously set by the region which is calling this function.</param>
        public void ZeroOid(int oldOid)
        {
            Interlocked.CompareExchange(ref _pendingOid, 0, oldOid);

            Oid = (ushort)_pendingOid;
        }

        #region Sender

        public virtual void SendMeTo(Player plr)
        {

        }

        public virtual void SendRemove(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_REMOVE_PLAYER, 4);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            if (plr != null)
                plr.SendPacket(Out);
            else
                DispatchPacket(Out, false);

        }
        public virtual void SendInteract(Player player, InteractMenu menu)
        {
            ScrInterface.OnInteract(this, player, menu);
            WorldMgr.GeneralScripts.OnWorldPlayerEvent("INTERACT", player, this);
        }
        public virtual void SendInteractEnd(Player plr)
        {

        }
        public virtual void Say(string message, ChatLogFilters chatFilter = ChatLogFilters.CHATLOGFILTERS_SAY)
        {
            if (string.IsNullOrEmpty(message))
                return;

            foreach (Player Plr in PlayersInRange.ToArray())
                Plr.SendMessage(this, message, chatFilter);
        }

        public virtual void DispatchPacketUnreliable(PacketOut Out, bool sendToSelf, Unit sender)
        {
            if (PlayersInRange.Count > 100)
            {
                if (sender != this)
                {
                    Player plrSender = sender as Player;
                    plrSender?.SendPacket(Out);
                }

                return;
            }

            lock (PlayersInRange)
                foreach (Player player in PlayersInRange)
                    player.SendCopy(Out);
        }

        public virtual void DispatchPacket(PacketOut Out, bool sendToSelf, bool playerstate = false)
        {
            lock (PlayersInRange)
                foreach (Player player in PlayersInRange)
                    player.SendCopy(Out);
        }

        #endregion

        #region Detection

        public bool IsPlayer() { return this is Player; }
        public bool IsUnit() { return this is Unit; }
        public bool IsCreature() { return this is Creature; }
        public bool IsInstanceSpawn() { return this is InstanceSpawn; }
        public bool IsPet() { return this is Pet; }
        public bool IsGameObject() { return this is GameObject; }
        public bool IsChapter() { return this is ChapterObject; }

        public Creature ToCreature() { return IsCreature() ? (this as Creature) : null; }
        public Player ToPlayer() { return IsPlayer() ? (this as Player) : null; }
        public GameObject ToGameObject() { return IsGameObject() ? (this as GameObject) : null; }
        public Unit ToUnit() { return IsUnit() ? (this as Unit) : null; }
	
        public Unit GetUnit() { return this as Unit; }
        public Player GetPlayer() { return this as Player; }
        public Creature GetCreature() { return this as Creature; }
        public InstanceSpawn GetInstanceSpawn() { return this as InstanceSpawn; }
        public InstanceBossSpawn GetInstanceBossSpawn() { return this as InstanceBossSpawn; }
        public Pet GetPet() { return this as Pet; }
        public GameObject GetGameObject() { return this as GameObject; }

        #endregion

        #region Position

        public override string ToString()
        {
            return $"(OffX = {XOffset}, OffY = {YOffset}, Heading = {Heading}, Oid = {Oid}, Name= {Name}, Radius= {BaseRadius}, Active= {_isActive})" + base.ToString();
        }

        public ushort Heading;
        public ushort XOffset, YOffset;

        public float BaseRadius { get; set; } = 4.5f;

        public CellMgr _Cell;

        /// <summary>Current zone containing the object, may be null</summary>
        public ZoneMgr Zone { get; protected set; }

        /// <summary>Current zone id containing the object, may be null</summary>
        public ushort? ZoneId
        {
            get => Zone?.ZoneId;
            set => throw new NotImplementedException();
        }

        /// <summary>Current region containing the object, may be null</summary>
        public RegionMgr Region => Zone?.Region;
        /// <summary>True is zone is not null</summary>
        public bool IsInWorld() => Zone != null;

        public readonly Point3D WorldPosition = new Point3D();

        public virtual void SetZone(ZoneMgr newZone)
        {
            if (newZone == null)
                throw new NullReferenceException("NULL ZoneMgr was passed in SetZone.");
            Zone = newZone;
        }

        public void ClearZone()
        {
            Zone = null;
        }

        public void RemoveFromWorld()
        {
            if (!IsInWorld())
                return;

            Region.RemoveObject(this);
        }

        public void UpdateOffset()
        {
            if (!IsInWorld() || X == 0 || Y == 0)
                return;

            ushort offX = (ushort)Math.Truncate((decimal)(X / 4096 + Zone.Info.OffX));
            ushort offY = (ushort)Math.Truncate((decimal)(Y / 4096 + Zone.Info.OffY));

            if (offX != XOffset || offY != YOffset)
                SetOffset(offX, offY);
        }

        public void UpdateWorldPosition()
        {
            //int x = X > 32768 ? X - 32768 : X;
            //int y = Y > 32768 ? Y - 32768 : Y;

            //WorldPosition.X = (int)XZone + (x & 0x00000FFF);
            //WorldPosition.Y = (int)YZone + (y & 0x00000FFF);
            WorldPosition.X = (Zone.Info.OffX << 12) + X;
            WorldPosition.Y = (Zone.Info.OffY << 12) + Y;
            WorldPosition.Z = Z;
        }

        /// <summary>
        /// Sets the object's X and Y offsets for coordinate calculations.
        /// Return whether or not the player's zone changed as a result of this.
        /// </summary>
        /// <param name="offX"></param>
        /// <param name="offY"></param>
        /// <returns></returns>
        public ushort SetOffset(ushort offX, ushort offY, bool checkZone = true)
        {
            Player player = this as Player;
            if (player != null && player.MoveBlock)
                return 0;

            if (offX == 0 || offY == 0)
                return 0;

            XOffset = offX;
            YOffset = offY;

            if (checkZone && IsInWorld())
                return Region.CheckZone(this);
            return 0;
        }

        /// <summary>
		/// Returns the angle towards a target spot in degrees, clockwise
		/// </summary>
		/// <param name="tx">target x</param>
		/// <param name="ty">target y</param>
		/// <returns>the angle towards the spot</returns>
        public float GetAngle(IPoint2D point)
        {
            float headingDifference = (GetWorldHeading(point) & 0xFFF) - (Heading & 0xFFF);

            if (headingDifference < 0)
                headingDifference += 4096.0f;

            return (headingDifference * 360.0f / 4096.0f);
        }

        #region Distance and Heading Checks

        public int Get2DDistanceToWorldPoint(Point2D point)
        {
            double dx = WorldPosition.X - point.X;
            double dy = WorldPosition.Y - point.Y;
            double range = Math.Sqrt(dx * dx + dy * dy);
            range = range / UNITS_TO_FEET;
            return (int)(range);
        }

        public int Get2DDistanceToObject(Object obj, bool factorRadius = false)
        {
            if (obj == null || Region != obj.Region)
                return int.MaxValue;

            double dx = WorldPosition.X - obj.WorldPosition.X;
            double dy = WorldPosition.Y - obj.WorldPosition.Y;
            double range = Math.Sqrt(dx * dx + dy * dy);
            range = range / UNITS_TO_FEET;

            if (!factorRadius)
                return (int)range;

            return Math.Max(0, (int)(range - (BaseRadius + obj.BaseRadius)));
        }

        /// <summary>
        /// Returns the distance between this object's WorldPosition and the supplied world point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int GetDistanceToWorldPoint(Point3D point)
        {
            double dx = (WorldPosition.X - point.X);
            double dy = (WorldPosition.Y - point.Y);
            double dz = (Z - point.Z);
            double range = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            range = range / UNITS_TO_FEET;
            return (int)(range);
        }

        /// <summary>
        /// Returns the distance between the WorldPositions of two objects.
        /// If factorRadius is true, removes the collision radii of the two objects from the returned value.
        /// </summary>
        /// <param name="obj">The distant object.</param>
        /// <param name="factorRadius">Whether or not to remove the collision radii of the two objects from the final result (used by ability range checks)</param>
        /// <returns></returns>
        public int GetDistanceToObject(Object obj, bool factorRadius = false)
        {
            if (obj == null || Region != obj.Region)
                return int.MaxValue;

            double dx = WorldPosition.X - obj.WorldPosition.X;
            double dy = WorldPosition.Y - obj.WorldPosition.Y;
            double dz = Z - obj.Z;
            double range = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            range = range / UNITS_TO_FEET;

            if (!factorRadius)
                return (int)range;

            return Math.Max(0, (int)(range - (BaseRadius + obj.BaseRadius)));
        }

        public ulong GetDistanceSquare(Point3D target)
        {
            double dx = WorldPosition.X - target.X;
            double dy = WorldPosition.Y - target.Y;
            double dz = Z - target.Z;
            return (ulong)((dx * dx + dy * dy + dz * dz) / UNITS_TO_FEET);
        }

        public ulong GetDistanceSquare(Object obj)
        {
            if (obj == null || Region != obj.Region)
                return int.MaxValue;

            double dx = WorldPosition.X - obj.WorldPosition.X;
            double dy = WorldPosition.Y - obj.WorldPosition.Y;
            double dz = Z - obj.Z;
            return (ulong)((dx * dx + dy * dy + dz * dz) / UNITS_TO_FEET);
        }

        public virtual int GetAbilityRangeTo(Unit caster)
        {
            return GetDistanceToObject(caster, true);
        }

        public ushort GetWorldHeading(IPoint2D point)
        {
            float dx = point.X - WorldPosition.X;
            float dy = point.Y - WorldPosition.Y;

            double heading = Math.Atan2(-dx, dy) * RADIAN_TO_HEADING;

            if (heading < 0)
                heading += 4096;

            return (ushort)heading;
        }

        public bool IsInCastRange(Object obj, uint radiusFeet)
        {
            if (obj == null || Region != obj.Region)
                return false;

            if (IsMoving && obj.IsMoving && radiusFeet == 5)
                radiusFeet = 8;


            radiusFeet = (uint)(radiusFeet + BaseRadius + obj.BaseRadius);

            return WorldPosition.IsWithinRadiusFeet(obj.WorldPosition, (int)radiusFeet);
        }

        public bool ObjectWithinRadiusFeet(Object obj, int radius)
        {
            if (obj.WorldPosition == null)
            {
                //Log.Error(Name, "RadiusFeet check against " + obj.Name + " with NULL WorldPosition");
                return false;
            }

            radius *= UNITS_TO_FEET;

            if (radius > ushort.MaxValue)
                return GetDistance(obj) <= radius;

            double dx = WorldPosition.X - obj.WorldPosition.X;
            double dy = WorldPosition.Y - obj.WorldPosition.Y;
            double dz = WorldPosition.Z - obj.WorldPosition.Z;
            double distSquare = dx * dx + dy * dy + dz * dz;

            return distSquare <= radius * radius;
        }

        public bool PointWithinRadiusFeet(Point3D point, int radius)
        {

            if (WorldPosition == null)
                return false;

            if (point == null)
                return false;

            radius *= UNITS_TO_FEET;

            if (radius > ushort.MaxValue)
                return GetDistance(point) <= radius;

            double dx = WorldPosition.X - point.X;
            double dy = WorldPosition.Y - point.Y;
            double dz = WorldPosition.Z - point.Z;
            double distSquare = dx * dx + dy * dy + dz * dz;

            return distSquare <= radius * radius;
        }

        /// <summary>
        /// Determines whether a target object is in front of this one. Optionally factors in the distance between the objects. In front is defined as north +- viewangle/2.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="viewangle"></param>
        /// <param name="rangeCheck"></param>
        /// <returns></returns>
        public virtual bool IsObjectInFront(Object target, double viewangle, uint MaxRadius = 0)
        {
            if (target == null || target.Zone == null)
                return false;
            float angle = GetAngle(new Point2D(target.WorldPosition.X, target.WorldPosition.Y));
            if (angle >= 360 - viewangle / 2 || angle < viewangle / 2)
            {
                return MaxRadius == 0 || IsInCastRange(target, MaxRadius);
            }

            return false;
        }

        #endregion

        private bool _isMoving;
        protected DateTime? _knockbackTime;

        public DateTime? KnockbackTime
        {
            get
            {
                return _knockbackTime;
            }
        }

        public bool IsMoving
        {
            get { return _isMoving; }
            set
            {
                if (_isMoving && !value)
                    EvtInterface.Notify(EventName.OnStopMove, this, null);

                _isMoving = value;
                if (_isMoving)
                    EvtInterface.Notify(EventName.OnMove, this, null);
            }
        }

        public Point2D LastRangeCheck = new Point2D(0, 0);

        public virtual void InitPosition(ushort OffX, ushort OffY, ushort PinX, ushort PinY)
        {
            X = PinX;
            Y = PinY;
            XOffset = OffX;
            YOffset = OffY;
        }
        public virtual bool SetPosition(ushort pinX, ushort pinY, ushort pinZ, ushort heading, ushort zoneId, bool sendState = false)
        {
            //Player plr = this as Player;

            bool updated = false;
            bool doUpdate = false;

            if (zoneId != Zone.ZoneId)
            {
                ZoneMgr newZone = Region.GetZoneMgr(zoneId);

                if (newZone == null)
                    return false;

                //plr?.DebugMessage("Moving from "+Zone.Info.Name+" to "+newZone.Info.Name, ChatLogFilters.CHATLOGFILTERS_ZONE_AREA, true);

                Zone.RemoveObject(this);
                newZone.AddObject(this);

                doUpdate = true;
            }

            if (doUpdate || pinX != X || pinY != Y || pinZ != Z || heading != Heading)
            {
                X = pinX;
                Y = pinY;
                Z = pinZ;
                Heading = heading;

                //plr?.DebugMessage($"Moving to {X}, {Y}, {Z} in {Zone.Info.Name}", ChatLogFilters.CHATLOGFILTERS_ZONE_AREA, true);

                UpdateWorldPosition();
                UpdateOffset();

                if (!IsPlayer())
                {
                    IsMoving = true;
                    if (sendState)
                        GetUnit().StateDirty = true;
                }
                else
                {
                    ushort newOffsetX = (ushort)Math.Truncate((decimal)(X / 4096 + Zone.Info.OffX));
                    ushort newOffsetY = (ushort)Math.Truncate((decimal)(Y / 4096 + Zone.Info.OffY));

                    if (newOffsetX != XOffset || newOffsetY != YOffset)
                        return false;
                }

                updated = Region.UpdateRange(this);
            }
            else if (!IsPlayer())
                IsMoving = false;

            return updated;
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    if (IsInWorld())
                        Region.UpdateRange(this, true);
                }
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                // Azarael - disabling Active status did not clear ranged list
                if (value == _isActive)
                    return;

                _isActive = value;
                if (IsInWorld())
                {
                    if (_isActive)
                        Region.UpdateRange(this, true);
                    else
                        ClearRange();
                }
            }
        }

        #endregion

        #region Range

        public List<Object> ObjectsInRange = new List<Object>();
        public List<Player> PlayersInRange = new List<Player>();

        public bool InRegionChange;

        public virtual bool HasInRange(Object obj)
        {
            lock (ObjectsInRange)
                return ObjectsInRange.Contains(obj);
        }

        public List<Player> GetPlayersInRange(int distance, bool includeSelf = false)
        {
            List<Player> players = new List<Player>();
            lock (PlayersInRange)
            {
                foreach (var player in PlayersInRange)
                    if (ObjectWithinRadiusFeet(player, distance))
                        players.Add(player);
            }

            if (includeSelf && GetPlayer() != null)
                players.Add(GetPlayer());
            return players;
        }

        public List<T> GetInRange<T>(int distance) where T : Object
        {
            List<T> objList = new List<T>();
            lock (ObjectsInRange)
            {
                foreach (var obj in ObjectsInRange)
                    if (obj is T && ObjectWithinRadiusFeet(obj, distance))
                        objList.Add((T)obj);
            }

            return objList;
        }

        /// <summary>
        /// Called by the Region manager when another object comes into this object's range.
        /// </summary>
        public virtual void AddInRange(Object obj)
        {
            if (obj == null)
                return;

            lock (ObjectsInRange)
            {
                ObjectsInRange.Add(obj);
            }

            Player plr = obj as Player;

            if (plr != null)
            {
                lock (PlayersInRange)
                {
                    PlayersInRange.Add(plr);
                }
            }

            ScrInterface.OnEnterRange(this, obj);
        }

        /// <summary>
        /// Called by the Region manager when an object leaves this object's range, and by another object when it clears its ranged object lists.
        /// </summary>
        /// <param name="obj"></param>
        public virtual void RemoveInRange(Object obj)
        {
            if (obj == null)
                return;

            lock (ObjectsInRange)
            {
                if (!ObjectsInRange.Contains(obj))
                    return;

                ObjectsInRange.Remove(obj);
            }

            Player plr = obj as Player;

            if (plr != null)
            {
                lock (PlayersInRange)
                {
                    PlayersInRange.Remove(plr);
                }
            }

            Player thisPlayer = this as Player;

            if (thisPlayer != null)
                obj.SendRemove(thisPlayer);
        }

        /// <summary>
        /// Called by the player when loading a new region and by the Region manager when leaving a region.
        /// </summary>
        public virtual void ClearRange(bool fromNewRegion = false)
        {
            // When leaving a region, notify players within that this player left if the region is still open
            if (!fromNewRegion)
                SendRemove(null);

            List<Object> rangedObjects = new List<Object>();

            lock (ObjectsInRange)
                rangedObjects.AddRange(ObjectsInRange);

            // Remove this object from other objects' ranged lists
            foreach (Object rangedObject in rangedObjects)
                rangedObject.RemoveInRange(this);

            lock (PlayersInRange)
                PlayersInRange.Clear();

            lock (ObjectsInRange)
                ObjectsInRange.Clear();
        }

        public virtual void OnRangeUpdate()
        {

        }

        #endregion

        #region Interaction

        public long CountdownTimerEnd { get; set; }

        protected Player CapturingPlayer;
        protected object CaptureLock = new object();
        public ushort CaptureDuration;

        public virtual bool AllowInteract(Player interactor)
        {
            return GetDistanceToObject(interactor, true) <= 15;
        }

        // Should always be from same thread as this object
        public virtual void BeginInteraction(Player interactor)
        {
            if (CapturingPlayer != null)
            {
                if (!CapturingPlayer._Value.Online || GetDistanceTo(CapturingPlayer) > 50)
                    interactor.SendClientMessage($"Removed bugged capturer {CapturingPlayer.Name} from {Name}.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);

                else
                {
                    interactor.SendClientMessage(CapturingPlayer.Name + " is already interacting with this object.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }
            }

            CapturingPlayer = interactor;

            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)GameBuffs.Interaction);
            buffInfo.Duration = CaptureDuration;
            CapturingPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(CapturingPlayer, CapturingPlayer.Level, buffInfo, InteractionBuff.GetNew, LinkToCaptureBuff));

            if (interactor.IsMounted)
                interactor.Dismount();
        }

        public virtual void LinkToCaptureBuff(NewBuff b)
        {
            if (b != null)
            {
                InteractionBuff captureBuff = (InteractionBuff)b;
                captureBuff.SetObject(this);
            }
            else
                CapturingPlayer = null;
        }

        public virtual void NotifyInteractionBroken(NewBuff b)
        {
            if (CapturingPlayer == b.Target)
                CapturingPlayer = null;
        }

        public virtual void NotifyInteractionComplete(NewBuff b)
        {
            CapturingPlayer = null;
        }

        #endregion
    }
}
