using BulletSharp;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Threading;
using SystemData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.NetWork;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects.Instances;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    // This is the base class for EVERYTHING that exists in the game world.
    // Players, monsters, items on the ground, doors... they all start here.
    // It handles the most basic things that all objects share, like:
    //  - A unique ID (Oid)
    //  - A position in the world (X, Y, Z)
    //  - The ability to be seen by other objects (range checking)
    //  - A collection of "Interfaces" that add more specific functionality (like combat, quests, etc.)
    public class Object : Point3D
    {
        // How often, in milliseconds, the server checks for objects that have entered or left this object's range.
        public static int RANGE_UPDATE_INTERVAL = 300;

        // A list of all the "Interfaces" attached to this object.
        // Interfaces are like plugins that add specific behaviors (e.g., Health, Abilities, Quests).
        public List<BaseInterface> Interfaces = new List<BaseInterface>();

        // Specific, commonly used interfaces for quick access.
        public EventInterface EvtInterface; // Handles events like OnMove, OnDie, etc.
        public ScriptsInterface ScrInterface; // Handles custom scripts attached to this object.

        // This is a temporary holder for the Oid. Using 'Interlocked' functions on it
        // prevents race conditions when multiple threads try to assign an Oid at the same time.
        private int _pendingOid;

        // The object's Unique ID in the world. No two objects can have the same Oid.
        public ushort Oid { get; private set; }

        // The object's name. Can be a player's name, a monster's name, etc.
        public virtual string Name { get; set; }

        public Object()
        {
            // All objects except players get an EventInterface by default.
            // Players have their own special version of this.
            if (EvtInterface == null && !IsPlayer())
                EvtInterface = AddInterface<EventInterface>();

            ScrInterface = AddInterface<ScriptsInterface>();

            // The object is not active (visible/interactive) by default.
            IsActive = false;
        }

        #region Disposal

        // This section handles how objects are safely removed from the game world.

        // True if the object has been fully disposed of and removed.
        public bool IsDisposed { get; protected set; }
        // True if the object is scheduled to be disposed on the next server tick.
        public bool PendingDisposal { get; set; }

        // Call this to start the process of removing the object from the world.
        public virtual void Destroy()
        {
            // If the object is currently in the world, we mark it for disposal on the next update.
            // This is to prevent errors from trying to remove it mid-tick.
            if (IsInWorld())
                PendingDisposal = true;
            // If it's not in the world, we can dispose of it immediately.
            else
            {
                Dispose();
            }
        }

        // The actual disposal logic. Cleans up all resources used by the object.
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            // Tell all interfaces to stop and clean themselves up.
            for (int i = 0; i < Interfaces.Count; ++i)
                Interfaces[i].Stop();

            // Remove the object from the region and cell it's in.
            RemoveFromWorld();
        }

        #endregion Disposal

        // Called on every server tick for each object in the world.
        public virtual void Update(long msTick)
        {
            // If the object is waiting to be disposed, do it now and stop.
            if (PendingDisposal)
            {
                Dispose();
                return;
            }

            // Tell all interfaces to run their own update logic.
            for (int i = 0; i < Interfaces.Count; ++i)
                Interfaces[i].Update(msTick);
        }

        #region Load/Save

        // This section handles loading the object's data from the database.

        public bool Loaded;

        // Called when the object is fully loaded into the world.
        public void Load()
        {
            Loaded = true;
            OnLoad();
        }

        // Can be overridden by child classes to add custom logic after loading.
        public virtual void OnLoad()
        {
            LoadInterfaces();
        }

        // Tells all interfaces to load their data.
        protected virtual void LoadInterfaces()
        {
            foreach (BaseInterface Interface in Interfaces)
                Interface.Load();
        }

        // Tells all interfaces to save their data to the database.
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

        #endregion Load/Save

        #region Interfaces

        // This section manages adding, removing, and finding interfaces on the object.

        // Adds a pre-created interface to this object.
        public BaseInterface AddInterface(BaseInterface Interface)
        {
            lock (Interfaces)
            {
                Interfaces.Add(Interface);
            }

            Interface.SetOwner(this);
            return Interface;
        }

        // Creates a new interface of a specific type and adds it to this object.
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

        // Plays a visual effect at the object's location (or a specified location).
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

            // Send the effect packet to all nearby players.
            foreach (var p in GetPlayersInRange(400))
                p.SendPacket(Out);

            if (this is Player)
                ((Player)this).SendPacket(Out);
        }

        /// <summary>
        /// Sets object interaction state (e.g., making a creature kneel or a door open).
        /// </summary>
        /// <param name="state">1 = interactive, 15=disabled</param>
        public void UpdateInteractState(CreatureStateOpcode state)
        {
            DispatchPacket(Packets.UpdateCreatureState(Oid, state), false);
        }

        // Plays a sound at the object's location.
        public void PlaySound(ushort soundID, bool sendarea = true)
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

        // Removes an interface from the object.
        public BaseInterface RemoveInterface(BaseInterface Interface)
        {
            lock (Interfaces)
                Interfaces.Remove(Interface);

            return Interface;
        }

        // Finds and returns a specific type of interface on this object.
        public T GetInterface<T>() where T : BaseInterface
        {
            lock (Interfaces)
                foreach (BaseInterface Interface in Interfaces)
                    if (Interface is T)
                        return (T)Interface;

            return null;
        }

        #endregion Interfaces

        /// <summary>
        /// Sets the object ID in a thread-safe manner. This is important because multiple
        /// region threads could try to assign an ID to the same object at the same time.
        /// </summary>
        public void SetOid(int newOid)
        {
            Interlocked.Exchange(ref _pendingOid, newOid);

            Oid = (ushort)_pendingOid;
        }

        /// <summary>
        /// Sets the object ID to zero only if it has not been changed by another region.
        /// This is used to safely de-assign an Oid when an object moves between regions.
        /// </summary>
        /// <param name="oldOid">The Oid previously set by the region which is calling this function.</param>
        public void ZeroOid(int oldOid)
        {
            Interlocked.CompareExchange(ref _pendingOid, 0, oldOid);

            Oid = (ushort)_pendingOid;
        }

        #region Sender

        // This section handles sending information about this object to players.

        // Sends the "create" packet for this object to a specific player.
        // This makes the object appear for that player.
        // Each object type (Player, Creature, etc.) has its own version of this.
        public virtual void SendMeTo(Player plr)
        {
        }

        // Sends the "destroy" packet for this object to players.
        // This makes the object disappear for them.
        public virtual void SendRemove(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_REMOVE_PLAYER, 4);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            if (plr != null)
                plr.SendPacket(Out); // Send to a specific player.
            else
                DispatchPacket(Out, false); // Send to all players in range.
        }

        // Called when a player right-clicks on this object.
        public virtual void SendInteract(Player player, InteractMenu menu)
        {
            // Triggers any scripts associated with interaction.
            ScrInterface.OnInteract(this, player, menu);
            WorldMgr.GeneralScripts.OnWorldPlayerEvent("INTERACT", player, this);
        }

        // Called when a player stops interacting with this object.
        public virtual void SendInteractEnd(Player plr)
        {
        }

        // Makes the object "say" something in chat.
        public virtual void Say(string message, ChatLogFilters chatFilter = ChatLogFilters.CHATLOGFILTERS_SAY)
        {
            if (string.IsNullOrEmpty(message))
                return;

            // Send the message to all nearby players.
            foreach (Player Plr in PlayersInRange.ToArray())
                Plr.SendMessage(this, message, chatFilter);
        }

        // Sends a packet to all players in range, but unreliably (UDP). Good for frequent, non-essential updates like movement.
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

        // Sends a packet to all players in range, reliably (TCP). Good for essential updates.
        public virtual void DispatchPacket(PacketOut Out, bool sendToSelf, bool playerstate = false)
        {
            lock (PlayersInRange)
                foreach (Player player in PlayersInRange)
                    player.SendCopy(Out);
        }

        #endregion Sender

        #region Detection

        // This section contains helper methods to quickly check what type of object this is.
        // This is more efficient and readable than using "is" checks everywhere.

        public bool IsPlayer()
        {
            return this is Player;
        }

        public bool IsUnit()
        {
            return this is Unit;
        }

        public bool IsCreature()
        {
            return this is Creature;
        }

        public bool IsInstanceSpawn()
        {
            return this is InstanceSpawn;
        }

        public bool IsPet()
        {
            return this is Pet;
        }

        public bool IsGameObject()
        {
            return this is GameObject;
        }

        public bool IsChapter()
        {
            return this is ChapterObject;
        }

        // These "To" methods are for safely casting the object to a more specific type.
        // If the cast is invalid, they return null instead of throwing an error.
        public Creature ToCreature()
        {
            return IsCreature() ? (this as Creature) : null;
        }

        public Player ToPlayer()
        {
            return IsPlayer() ? (this as Player) : null;
        }

        public GameObject ToGameObject()
        {
            return IsGameObject() ? (this as GameObject) : null;
        }

        public Unit ToUnit()
        {
            return IsUnit() ? (this as Unit) : null;
        }

        // These "Get" methods are similar to the "To" methods.
        // They provide a direct cast, which is slightly faster but assumes you already know the type.
        public Unit GetUnit()
        {
            return this as Unit;
        }

        public Player GetPlayer()
        {
            return this as Player;
        }

        public Creature GetCreature()
        {
            return this as Creature;
        }

        public InstanceSpawn GetInstanceSpawn()
        {
            return this as InstanceSpawn;
        }

        public InstanceBossSpawn GetInstanceBossSpawn()
        {
            return this as InstanceBossSpawn;
        }

        public Pet GetPet()
        {
            return this as Pet;
        }

        public GameObject GetGameObject()
        {
            return this as GameObject;
        }

        #endregion Detection

        #region Position

        // This section handles everything related to the object's position, zone, and orientation.

        public override string ToString()
        {
            return $"(OffX = {XOffset}, OffY = {YOffset}, Heading = {Heading}, Oid = {Oid}, Name= {Name}, Radius= {BaseRadius}, Active= {_isActive})" + base.ToString();
        }

        // The direction the object is facing, from 0 to 4095.
        public ushort Heading;
        // The zone cell coordinates. The world is divided into a grid of cells.
        public ushort XOffset, YOffset;

        // The object's collision radius in feet. Used for range checks and physics.
        public float BaseRadius { get; set; } = 4.5f;

        // A reference to the cell manager this object is currently in.
        public CellMgr _Cell;

        /// <summary>Current zone containing the object, may be null</summary>
        public ZoneMgr Zone { get; protected set; }

        /// <summary>Current zone id containing the object, may be null</summary>
        public ushort? ZoneId
        {
            get => Zone?.ZoneId;
            set => throw new NotImplementedException(); // This is read-only.
        }

        /// <summary>Current region containing the object, may be null</summary>
        public RegionMgr Region => Zone?.Region;

        /// <summary>True is zone is not null</summary>
        public bool IsInWorld() => Zone != null;

        // The object's absolute position in the world (not relative to the zone).
        public readonly Point3D WorldPosition = new Point3D();

        // Sets the object's current zone.
        public virtual void SetZone(ZoneMgr newZone)
        {
            if (newZone == null)
                throw new NullReferenceException("NULL ZoneMgr was passed in SetZone.");
            Zone = newZone;
        }

        // Removes the object's reference to its zone.
        public void ClearZone()
        {
            Zone = null;
        }

        // Removes the object from its region, which effectively removes it from the world.
        public void RemoveFromWorld()
        {
            if (!IsInWorld())
                return;

            Region.RemoveObject(this);
        }

        // Recalculates the object's cell offset based on its current position.
        public void UpdateOffset()
        {
            if (!IsInWorld() || X == 0 || Y == 0)
                return;

            ushort offX = (ushort)Math.Truncate((decimal)(X / 4096 + Zone.Info.OffX));
            ushort offY = (ushort)Math.Truncate((decimal)(Y / 4096 + Zone.Info.OffY));

            if (offX != XOffset || offY != YOffset)
                SetOffset(offX, offY);
        }

        // Recalculates the object's absolute world position from its zone-relative coordinates.
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
        /// Sets the object's cell offsets. If the new offset moves the object into a
        /// new zone, this method handles the zone change.
        /// </summary>
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
                return Region.CheckZone(this); // Tell the region to see if we've crossed a zone boundary.
            return 0;
        }

        /// <summary>
        /// Returns the angle towards a target point in degrees, clockwise.
        /// </summary>
        public float GetAngle(IPoint2D point)
        {
            float headingDifference = (GetWorldHeading(point) & 0xFFF) - (Heading & 0xFFF);

            if (headingDifference < 0)
                headingDifference += 4096.0f;

            return (headingDifference * 360.0f / 4096.0f);
        }

        #region Distance and Heading Checks

        // This sub-section contains many helper methods for calculating distances and headings.
        // These are heavily used for ability ranges, AI, and general game logic.

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

            // If factoring radius, subtract the collision radii of both objects to get the distance between their edges.
            return Math.Max(0, (int)(range - (BaseRadius + obj.BaseRadius)));
        }

        /// <summary>
        /// Returns the distance between this object's WorldPosition and the supplied world point.
        /// </summary>
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

        // Gets the squared distance. This is much faster for comparisons as it avoids a square root calculation.
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

        // A special distance check for abilities, which always factors in radius.
        public virtual int GetAbilityRangeTo(Unit caster)
        {
            return GetDistanceToObject(caster, true);
        }

        // Calculates the heading (0-4095) required to face a specific point.
        public ushort GetWorldHeading(IPoint2D point)
        {
            float dx = point.X - WorldPosition.X;
            float dy = point.Y - WorldPosition.Y;

            double heading = Math.Atan2(-dx, dy) * RADIAN_TO_HEADING;

            if (heading < 0)
                heading += 4096;

            return (ushort)heading;
        }

        // Checks if an object is within a given cast range, accounting for the radii of both objects.
        public bool IsInCastRange(Object obj, uint radiusFeet)
        {
            if (obj == null || Region != obj.Region)
                return false;

            // A small fudge factor for moving targets.
            if (IsMoving && obj.IsMoving && radiusFeet == 5)
                radiusFeet = 8;

            radiusFeet = (uint)(radiusFeet + BaseRadius + obj.BaseRadius);

            return WorldPosition.IsWithinRadiusFeet(obj.WorldPosition, (int)radiusFeet);
        }

        // A fast, squared-distance check.
        public bool ObjectWithinRadiusFeet(Object obj, int radius)
        {
            if (obj.WorldPosition == null)
            {
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
            if (WorldPosition == null || point == null)
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
        /// Determines whether a target object is in front of this one, within a given view angle.
        /// </summary>
        public virtual bool IsObjectInFront(Object target, double viewangle, uint MaxRadius = 0)
        {
            if (target == null || target.Zone == null)
                return false;
            float angle = GetAngle(new Point2D(target.WorldPosition.X, target.WorldPosition.Y));
            // Check if the angle to the target is within the "cone" of vision.
            if (angle >= 360 - viewangle / 2 || angle < viewangle / 2)
            {
                return MaxRadius == 0 || IsInCastRange(target, MaxRadius);
            }

            return false;
        }

        #endregion Distance and Heading Checks

        private bool _isMoving;
        protected DateTime? _knockbackTime; // Used to track when the object was last knocked back.

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

        // The last position where a full range check was performed.
        public Point2D LastRangeCheck = new Point2D(0, 0);

        // Sets the initial position of the object when it's first spawned.
        public virtual void InitPosition(ushort OffX, ushort OffY, ushort PinX, ushort PinY)
        {
            X = PinX;
            Y = PinY;
            XOffset = OffX;
            YOffset = OffY;
        }

        // Updates the object's position. This is a core function for movement.
        public virtual bool SetPosition(ushort pinX, ushort pinY, ushort pinZ, ushort heading, ushort zoneId, bool sendState = false)
        {
            bool updated = false;
            bool doUpdate = false;

            // Check if the object has moved to a new zone.
            if (zoneId != Zone.ZoneId)
            {
                ZoneMgr newZone = Region.GetZoneMgr(zoneId);

                if (newZone == null)
                    return false;

                // Handle the zone transition.
                Zone.RemoveObject(this);
                newZone.AddObject(this);

                doUpdate = true;
            }

            // If the position or heading has changed, update everything.
            if (doUpdate || pinX != X || pinY != Y || pinZ != Z || heading != Heading)
            {
                X = pinX;
                Y = pinY;
                Z = pinZ;
                Heading = heading;

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

                // Tell the region to update who can see this object.
                updated = Region.UpdateRange(this);
            }
            else if (!IsPlayer())
                IsMoving = false;

            return updated;
        }

        private bool _isVisible = true;

        // Controls whether the object is visible to players (e.g., for stealth).
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
                        Region.UpdateRange(this, true); // Force a range update to show/hide the object.
                }
            }
        }

        private bool _isActive;

        // Controls whether the object is "active" (can be interacted with, part of physics, etc.).
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
                        Region.UpdateRange(this, true); // Activate, so update range.
                    else
                        ClearRange(); // Deactivate, so clear range lists.
                }
            }
        }

        #endregion Position

        #region Range

        // This section handles which other objects are "in range" of this object.
        // This is the core of visibility and interaction in the game.

        // A list of all objects close enough to be seen or interacted with.
        public List<Object> ObjectsInRange = new List<Object>();
        // A filtered list containing only the players from ObjectsInRange, for quick access.
        public List<Player> PlayersInRange = new List<Player>();

        public bool InRegionChange;

        // Checks if a specific object is in our range list.
        public virtual bool HasInRange(Object obj)
        {
            lock (ObjectsInRange)
                return ObjectsInRange.Contains(obj);
        }

        // Gets a list of all players within a specific distance (in feet).
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

        // A generic version of GetPlayersInRange that can get any type of object.
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

            // Trigger any "OnEnterRange" scripts.
            ScrInterface.OnEnterRange(this, obj);
        }

        /// <summary>
        /// Called by the Region manager when an object leaves this object's range, and by another object when it clears its ranged object lists.
        /// </summary>
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

            // If we are a player, tell the other object to send us its "destroy" packet.
            if (thisPlayer != null)
                obj.SendRemove(thisPlayer);
        }

        /// <summary>
        /// Called by the player when loading a new region and by the Region manager when leaving a region.
        /// It completely clears all range lists.
        /// </summary>
        public virtual void ClearRange(bool fromNewRegion = false)
        {
            // When leaving a region, notify players within that this player left if the region is still open
            if (!fromNewRegion)
                SendRemove(null);

            List<Object> rangedObjects = new List<Object>();

            lock (ObjectsInRange)
                rangedObjects.AddRange(ObjectsInRange);

            // Tell every object in our range to remove us from their range.
            foreach (Object rangedObject in rangedObjects)
                rangedObject.RemoveInRange(this);

            lock (PlayersInRange)
                PlayersInRange.Clear();

            lock (ObjectsInRange)
                ObjectsInRange.Clear();
        }

        // Called by the region manager after range has been updated.
        public virtual void OnRangeUpdate()
        {
        }

        #endregion Range

        #region Interaction

        // This section handles direct interaction with objects, like capturing a flag or talking to an NPC.

        public long CountdownTimerEnd { get; set; }
        public RigidBody PhysicsRigidBody { get; internal set; }
        public object PQCreature { get; set; } // A reference if this is a Public Quest creature.

        protected Player CapturingPlayer; // The player currently interacting with/capturing this object.
        protected object CaptureLock = new object();
        public ushort CaptureDuration; // How long it takes to capture this object.

        // Checks if a player is close enough to interact with this object.
        public virtual bool AllowInteract(Player interactor)
        {
            return GetDistanceToObject(interactor, true) <= 15;
        }

        // Called when a player starts interacting (e.g., starts capturing a flag).
        // Should always be from same thread as this object
        public virtual void BeginInteraction(Player interactor)
        {
            // Ensure no one else is already capturing it.
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

            // Apply the "Interaction" buff, which shows a casting bar to the player.
            BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)GameBuffs.Interaction);
            buffInfo.Duration = CaptureDuration;
            CapturingPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(CapturingPlayer, CapturingPlayer.Level, buffInfo, InteractionBuff.GetNew, LinkToCaptureBuff));

            if (interactor.IsMounted)
                interactor.Dismount();
        }

        // Links the capture buff back to this object, so we know when it's broken or complete.
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

        // Called by the buff if the interaction is broken (e.g., player moves or takes damage).
        public virtual void NotifyInteractionBroken(NewBuff b)
        {
            if (CapturingPlayer == b.Target)
                CapturingPlayer = null;
        }

        // Called by the buff when the interaction completes successfully.
        public virtual void NotifyInteractionComplete(NewBuff b)
        {
            CapturingPlayer = null;
        }

        #endregion Interaction
    }
}