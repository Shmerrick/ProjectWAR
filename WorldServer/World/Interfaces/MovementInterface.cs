using System;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class MovementInterface : BaseInterface
    {
        [Flags]
        private enum EObjectState
        {
            Stationary = 0x0,
            Moving = 0x1,
            LookingAt = 0x2,
            EffectStates = 0x10,
            NoGrav = 0x20,
            Recall = 0x40
        }

        public enum EMoveState
        {
            None,
            Turn,
            Move,
            Follow,
            Recall,
            TacticalWithdrawl
        }

        public EMoveState MoveState { get; private set; }
        private EMoveState _lastState;

        private Unit _unit;

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _unit = (Unit)owner;
        }

        public override void Update(long tick)
        {
            if (_unit is Player)
                return;

            /*
            if (_lastState != MoveState)
            {
                _unit.Say("Updating: State: " + Enum.GetName(typeof (EMoveState), MoveState));
                _lastState = MoveState;
            }
            */
            
            switch (MoveState)
            {
                case EMoveState.Move:
                    UpdateMove(tick);
                    break;
                case EMoveState.Follow:
                    UpdateFollow(tick);
                    if (_moveDuration > 0)
                        UpdateMove(tick);
                    break;
                case EMoveState.Recall:
                    UpdateRecall(tick);
                    break;
                case EMoveState.TacticalWithdrawl:
                    UpdateWithdrawl(tick);
                    if (_moveDuration > 0)
                        UpdateMove(tick);
                    break;
            }
        }

        private void UpdateWithdrawl(long tick)
        {
            if (IsMoving)
            {
                if (_unit.GetDistanceTo(ThreateningTarget) > _minWithdrawlTolerance)
                {
                    StopWithdrawlMove();
                    return;
                }
            }

            if (MovementSpeed == 0 || _unit != null && _unit.IsDisabled || _unit != null && _unit.IsStaggered)
            {
                StopWithdrawlMove();
                return;
            }

            if (tick > _nextWithdrawlUpdate)
            {
                // already out of range
                int distTo = _unit.GetDistanceTo(ThreateningTarget);

                _withdrawlCatchupFactor = _pathThrough && distTo < _minWithdrawlTolerance + 30 ? 30 : 0;

                if (distTo > _maxWithdrawlTolerance)
                {
                    _nextWithdrawlUpdate = tick + WithdrawlReacquisitionInterval;
                    // Gets the heading of the threatening unit (should end facing the ThreateningTarget)
                    _unit.Heading = _unit.WorldPosition.GetHeading(ThreateningTarget.WorldPosition);
                    return;
                }

                _nextWithdrawlUpdate = tick + WITHDRAWL_PATH_UPDATE_INTERVAL;

                _destWithdrawlPos.SetCoordsFrom(ThreateningTarget.WorldPosition);

                _toTarget.X = _unit.WorldPosition.X - _destWithdrawlPos.X;
                _toTarget.Y = _unit.WorldPosition.Y - _destWithdrawlPos.Y;

                _toTarget.Normalize();

                if (!_pathThrough || !ThreateningTarget.IsMoving)
                {
                    _destWithdrawlPos.X += (int)(_toTarget.X * _minWithdrawlTolerance * 12);
                    _destWithdrawlPos.Y += (int)(_toTarget.Y * _minWithdrawlTolerance * 12);
                }

                else
                {
                    _destWithdrawlPos.X -= (int)(_toTarget.X * _minWithdrawlTolerance * 12);
                    _destWithdrawlPos.Y -= (int)(_toTarget.Y * _minWithdrawlTolerance * 12);
                }

                Move(_destWithdrawlPos);
            }
        }

        #region Speed Change

        public ushort BaseSpeed { get; private set; } = 100;
        public ushort MovementSpeed { get; private set; } = 100;
        private int _followCatchupFactor;
        private int _withdrawlCatchupFactor;

        private int _totalSpeed
        {
            get
            {
                if (MoveState == EMoveState.Follow)
                    return MovementSpeed + _followCatchupFactor;
                if (MoveState == EMoveState.TacticalWithdrawl)
                    return MovementSpeed + _withdrawlCatchupFactor;

                return MovementSpeed;
            }
        }

        private float _speedScaler = 1f;

        public void SetBaseSpeed(ushort newSpeed)
        {
            if (newSpeed == BaseSpeed)
                return;

            BaseSpeed = newSpeed;

            RecalculateMovement();
        }

        public void ScaleSpeed(float newScaler)
        {
            if (newScaler == _speedScaler)
                return;

            _speedScaler = newScaler;

            RecalculateMovement();
        }

        private void RecalculateMovement()
        {
            MovementSpeed = (ushort)(BaseSpeed * _speedScaler);

            switch (MoveState)
            {
                case EMoveState.Move:
                    Move(_destWorldPos);
                    break;
                case EMoveState.Follow:
                    _nextFollowUpdate = 0;
                    Follow(FollowTarget, _minFollowTolerance, _maxFollowTolerance);
                    break;
                case EMoveState.TacticalWithdrawl:
                    _nextWithdrawlUpdate = 0;
                    TacticalWithdrawl(ThreateningTarget, _minWithdrawlTolerance, _maxWithdrawlTolerance);
                    break;
            }
        }

        #endregion

        #region Facing

        public void TurnTo(Point3D destWorldPos)
        {
            ushort newHeading = _unit.WorldPosition.GetHeading(destWorldPos);

            if (newHeading != _unit.Heading)
            {
                _unit.Heading = newHeading;
                if (!IsMoving)
                    UpdateMovementState(null);
            }
        }


        public void TurnAwayFrom(Point3D destWorldPos)
        {
            ushort newHeading = (ushort) (_unit.WorldPosition.GetHeading(destWorldPos)+2048);

            if (newHeading != _unit.Heading)
            {
                _unit.Heading = newHeading;
                if (!IsMoving)
                    UpdateMovementState(null);
            }
        }

        #endregion

        #region Move

        private readonly Point3D _destWorldPos = new Point3D(0, 0, 0);
        private readonly Point3D _startWorldPos = new Point3D(0, 0, 0);
        private ushort _destZoneId;

        const int MOVE_TOLERANCE_UNITS = 2;
        private long _moveStartTime;
        private float _moveDuration;

        public bool Stationary { get; set; }

        public bool IsMoving => _moveDuration > 0;

        private float GetMoveFactor(long deltaMs)
        {
            return Math.Min(1f, deltaMs/_moveDuration);
        }

        /// <summary>
        /// Moves the creature to given coordinates.
        /// </summary>
        /// <param name="worldX">Target x coordinate</param>
        /// <param name="worldY">Target y coordinate</param>
        /// <param name="worldZ">Target z coordinate</param>
        /// <remarks>Convenience method for scripts</remarks>
        public void Move(int worldX, int worldY, int worldZ)
        {
            Move(new Point3D(worldX, worldY, worldZ));
        }

        public void Move(Point3D destWorldPos)
        {
            Vector2 destVect = new Vector2(_unit.WorldPosition.X - destWorldPos.X, _unit.WorldPosition.Y - destWorldPos.Y);

            float distance = destVect.Magnitude;

            if (distance <= MOVE_TOLERANCE_UNITS)
            {
                if (MoveState == EMoveState.Move)
                    MoveState = EMoveState.None;
                return;
            }

            _startWorldPos.SetCoordsFrom(_unit.WorldPosition);
            _destWorldPos.SetCoordsFrom(destWorldPos);

            Zone_Info destZone = _unit.Region.GetZone((ushort)(destWorldPos.X >> 12), (ushort)(destWorldPos.Y >> 12));

            if (destZone == null)
                return;

            _destZoneId = destZone.ZoneId;

            _unit.Heading = _unit.WorldPosition.GetHeading(_destWorldPos);

            _unit.AiInterface.Debugger?.SendClientMessage("MOVE: Start: " + _startWorldPos + " End: " + _destWorldPos +" New Heading: "+ _unit.Heading+" Speed: "+_totalSpeed);

            if (MovementSpeed > 0)
            {
                _moveStartTime = TCPManager.GetTimeStampMS();
                _moveDuration = (distance/ _totalSpeed) *1000;
            }

            if (MoveState == EMoveState.None)
                MoveState = EMoveState.Move;

            UpdateMovementState(null);
        }

        private void UpdateMove(long tick)
        {
            if (MovementSpeed == 0 || _unit != null && _unit.IsDisabled || _unit != null && _unit.IsStaggered)
                return;

            long deltaMs = tick - _moveStartTime;

            uint newWorldPosX = (uint)Point2D.Lerp(_startWorldPos.X, _destWorldPos.X, GetMoveFactor(deltaMs));
            uint newWorldPosY = (uint)Point2D.Lerp(_startWorldPos.Y, _destWorldPos.Y, GetMoveFactor(deltaMs));

            Zone_Info destZone = _unit.Region.GetZone((ushort)(newWorldPosX >> 12), (ushort)(newWorldPosY >> 12));

            if (destZone == null)
                return;

            ushort pinX = _unit.Zone.CalculPin(newWorldPosX, true);
            ushort pinY = _unit.Zone.CalculPin(newWorldPosY, false);
            ushort pinZ = (ushort)Point2D.Lerp(_startWorldPos.Z, _destWorldPos.Z, GetMoveFactor(deltaMs));

            _unit.SetPosition(pinX, pinY, pinZ, _unit.Heading, destZone.ZoneId);

            
            if (tick > _moveStartTime + _moveDuration)
            {
                if (MoveState == EMoveState.Move)
                    MoveState = EMoveState.None;
                _moveDuration = 0;
            }
        }

        public void StopMove()
        {
            _followCatchupFactor = 0;
            MoveState = EMoveState.None;
            _moveDuration = 0;
            FollowTarget = null;
            UpdateMovementState(null);
        }

        public void StopFollowMove()
        {
            _followCatchupFactor = 0;
            _moveDuration = 0;
            UpdateMovementState(null);
        }

        public void StopWithdrawlMove()
        {
            _followCatchupFactor = 0;
            MoveState = EMoveState.None;
            _moveDuration = 0;
            FollowTarget = null;
            UpdateMovementState(null);
        }

        #endregion

        #region Follow

        public Unit ThreateningTarget { get; private set; }
        private long _nextWithdrawlUpdate;
        private int _minWithdrawlTolerance, _maxWithdrawlTolerance;

        public Unit FollowTarget { get; private set; }
        private long _nextFollowUpdate;
        private int _minFollowTolerance, _maxFollowTolerance;
        /// <summary>
        /// Indicates that the unit should path to a spot on the far side of the unit (towed siege)
        /// </summary>
        private bool _pathThrough;
        const int FOLLOW_PATH_UPDATE_INTERVAL = 500;
        const int WITHDRAWL_PATH_UPDATE_INTERVAL = 500;
        public int FollowReacquisitionInterval = 500;
        public int WithdrawlReacquisitionInterval = 500;

        public void Follow(Unit followTarget, int minTolerance, int maxTolerance, bool pathThrough = false, bool ForceMove = false)
        {
            if (MovementSpeed == 0 && !ForceMove || _unit != null && _unit.IsDisabled && !ForceMove || _unit != null && _unit.IsStaggered && !ForceMove)
            {
                StopFollowMove();
                return;
            }

            _pathThrough = pathThrough;

            if (followTarget == FollowTarget && !ForceMove)
            {
                TurnTo(followTarget.WorldPosition);
                return;
            }

            FollowTarget = followTarget;

            _minFollowTolerance = minTolerance;
            _maxFollowTolerance = maxTolerance;
            _nextFollowUpdate = 0;

            MoveState = EMoveState.Follow;

            if (_unit.WorldPosition.IsWithinRadiusFeet(FollowTarget.WorldPosition, minTolerance))
                TurnTo(FollowTarget.WorldPosition);
        }


        public void TacticalWithdrawl(Unit threateningTarget, int minTolerance, int maxTolerance, bool pathThrough = false, bool ForceMove = false)
        {
            if (MovementSpeed == 0 || _unit != null && _unit.IsDisabled || _unit != null && _unit.IsStaggered)
            {
                return;
            }

            if (ThreateningTarget == threateningTarget && !ForceMove)
            {
                TurnTo(ThreateningTarget.WorldPosition);
                return;
            }

            ThreateningTarget = threateningTarget;

            _minWithdrawlTolerance = minTolerance;
            _maxWithdrawlTolerance = maxTolerance;
            _nextWithdrawlUpdate = 0;

            MoveState = EMoveState.TacticalWithdrawl;

            if (_unit.WorldPosition.IsWithinRadiusFeet(ThreateningTarget.WorldPosition, minTolerance))
                TurnTo(ThreateningTarget.WorldPosition);
        }

        private readonly Point3D _destFollowPos = new Point3D();
        private readonly Point3D _destWithdrawlPos = new Point3D();
        private readonly Vector2 _toTarget = new Vector2();

        private void UpdateFollow(long tick)
        {

            if (IsMoving)
            { 
                if (_unit.GetDistanceTo(FollowTarget) < _minFollowTolerance)
                {
                    StopFollowMove();
                    return;
                }
            }

            if (MovementSpeed == 0 || _unit != null && _unit.IsDisabled || _unit != null && _unit.IsStaggered)
            {
                StopFollowMove();
                return;
            }

            if (tick > _nextFollowUpdate)
            {
                // already within range
                int distTo = _unit.GetDistanceTo(FollowTarget);

                _followCatchupFactor = _pathThrough && distTo > _minFollowTolerance + 20 ? 20 : 0;

                if (distTo < _maxFollowTolerance)
                {
                    _nextFollowUpdate = tick + FollowReacquisitionInterval;
                    _unit.Heading = _unit.WorldPosition.GetHeading(FollowTarget.WorldPosition);
                    return;
                }

                _nextFollowUpdate = tick + FOLLOW_PATH_UPDATE_INTERVAL;

                _destFollowPos.SetCoordsFrom(FollowTarget.WorldPosition);

                _toTarget.X = _unit.WorldPosition.X - _destFollowPos.X;
                _toTarget.Y = _unit.WorldPosition.Y - _destFollowPos.Y;

                _toTarget.Normalize();

                if (!_pathThrough || !FollowTarget.IsMoving)
                {
                    _destFollowPos.X += (int) (_toTarget.X * _minFollowTolerance*12);
                    _destFollowPos.Y += (int) (_toTarget.Y * _minFollowTolerance*12);
                }

                else
                {
                    _destFollowPos.X -= (int)(_toTarget.X * _minFollowTolerance * 12);
                    _destFollowPos.Y -= (int)(_toTarget.Y * _minFollowTolerance * 12);
                }

                Move(_destFollowPos);
            }
        }

        #endregion

        #region Recall

        private long _lastRecallMove;
        private long _nextRecallSend;

        public void Recall(Unit owner)
        {
            if (MovementSpeed == 0)
                return;

            _followCatchupFactor = 0;

            FollowTarget = owner;

            MoveState = EMoveState.Recall;
            _lastRecallMove = TCPManager.GetTimeStampMS();
            _nextRecallSend = TCPManager.GetTimeStampMS() + 10000;
            UpdateMovementState(null);

            _unit.AiInterface.Debugger?.SendClientMessage("[MR]: Recalling.");
        }

        // Recall state just moves the target repeatedly towards its owner without sending any state updates
        // TODO: Move towards offset position (back-rear)
        private void UpdateRecall(long tick)
        {
            long deltaMs = tick - _lastRecallMove;
            _lastRecallMove = tick;

            // Stationary pets should not perform any moves
            if (MovementSpeed == 0)
                return;

            if (_nextRecallSend < tick)
            {
                _nextRecallSend = tick + 7500;
                UpdateMovementState(null);
            }

            Vector2 destVect = new Vector2(_unit.WorldPosition.X - FollowTarget.WorldPosition.X, _unit.WorldPosition.Y - FollowTarget.WorldPosition.Y);

            float distance = destVect.Magnitude;

            // Within tolerance for recall move
            if (distance < 60)
            {
                Pet pet = _unit as Pet;
                if (pet != null && pet.IsHeeling)
                {
                    pet.IsHeeling = false;
                    pet.AiInterface.Debugger?.SendClientMessage("[MR]: Successfully recalled. No longer ignoring enemies.");
                }
                return;
            }

            // Total time required to move fully towards the target
            _moveDuration = (distance / _totalSpeed) * 1000;

            // Lerp alpha is capped to 1, so the move will never overshoot the target
            uint newWorldPosX = (uint)Point2D.Lerp(_unit.WorldPosition.X, FollowTarget.WorldPosition.X, GetMoveFactor(deltaMs));
            uint newWorldPosY = (uint)Point2D.Lerp(_unit.WorldPosition.Y, FollowTarget.WorldPosition.Y, GetMoveFactor(deltaMs));

            Zone_Info destZone = _unit.Region.GetZone((ushort)(newWorldPosX >> 12), (ushort)(newWorldPosY >> 12));

            if (destZone == null)
                return;

            ushort pinX = _unit.Zone.CalculPin(newWorldPosX, true);
            ushort pinY = _unit.Zone.CalculPin(newWorldPosY, false);
            ushort pinZ = (ushort)Point2D.Lerp(_unit.WorldPosition.Z, FollowTarget.WorldPosition.Z, GetMoveFactor(deltaMs));

            _unit.SetPosition(pinX, pinY, pinZ, _unit.Heading, destZone.ZoneId);
        }

        #endregion

        private bool _forcePositionUpdate;

        public void Teleport(Point3D destWorldPos)
        {
            Zone_Info destZone = _unit.Region.GetZone((ushort)(destWorldPos.X >> 12), (ushort)(destWorldPos.Y >> 12));

            if (destZone == null)
                return;

            ushort pinX = _unit.Zone.CalculPin((uint)destWorldPos.X, true);
            ushort pinY = _unit.Zone.CalculPin((uint)destWorldPos.Y, false);
            ushort pinZ = (ushort)destWorldPos.Z;

            _destWorldPos.SetCoordsFrom(destWorldPos);

            _forcePositionUpdate = true;

            _unit.SetPosition(pinX, pinY, pinZ, _unit.Heading, destZone.ZoneId);

            UpdateMovementState(null);

            _forcePositionUpdate = false;
        }

        public void UpdateMovementState(Player player)
        {
            if (_unit.Zone == null)
            {
                Log.Error("UpdateMovementState", $"{_unit.Name} with no Zone - pendingDisposal:{_unit.PendingDisposal} isDisposed:{_unit.IsDisposed}");
                return;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_STATE, 28);
            WriteMovementState(Out);

            if (player == null)
                _unit.DispatchPacket(Out, false);
            else
                player.SendPacket(Out);

        }

        public void WriteMovementState(PacketOut Out)
        {
            if (_unit.Zone == null)
            {
                Log.Error("WriteMovementState", $"{_unit.Name} with no Zone - pendingDisposal:{_unit.PendingDisposal} isDisposed:{_unit.IsDisposed}");
                return;
            }
            Out.WriteUInt16(_unit.Oid);
            Out.WriteUInt16((ushort)_unit.X);
            Out.WriteUInt16((ushort)_unit.Y);
            Out.WriteUInt16((ushort)_unit.Z);
            Out.WriteByte(_unit.PctHealth);

            byte flags = 0;
            if ((IsMoving && MovementSpeed > 0) || _forcePositionUpdate)
                flags |= (byte)EObjectState.Moving;

            if (MoveState == EMoveState.Recall)
                flags |= (byte)EObjectState.Recall;
            else if (FollowTarget != null)
                flags |= (byte)EObjectState.LookingAt;

            //TODO buggy movement in zones > 255 ?
            if (_unit.Zone.ZoneId > 255)
            {
                flags = (byte)Utils.setBit(flags, 2, true);
            }
            
            // flying  
            if(_unit.IsCreature())
            if ((Utils.getBit(_unit.Faction,5) || (Utils.getBit(_unit.GetCreature().Spawn.Proto.Faction, 5) &&  Utils.HasFlag(flags, (int)EObjectState.Moving))) && _unit.Z - ClientFileMgr.GetHeight(_unit.Zone.ZoneId, _unit.X, _unit.Y) > 50)
                flags = (byte)Utils.setBit(flags, 5, true);
                
            Out.WriteByte(flags);
            Out.WriteByte((byte)_unit.Zone.ZoneId);
           

            if (_unit is BuffHostObject)
            {
                Out.WriteByte(4); // Unk1
                Out.WriteUInt32(3); // Unk2
            }

            else
            {
                Out.WriteByte(_forcePositionUpdate? (byte)6 : (byte)0);
                Out.WriteUInt32(0);
            }

            if (Utils.HasFlag(flags, (int)EObjectState.Moving))
            {
                Out.WriteUInt16R((ushort)_totalSpeed);
                Out.WriteByte(0); // DestUnk
                Out.WriteUInt16R(_unit.Zone.CalculPin((uint)_destWorldPos.X, true));
                Out.WriteUInt16R(_unit.Zone.CalculPin((uint)_destWorldPos.Y, false));
                Out.WriteUInt16R((ushort)_destWorldPos.Z);
                Out.WriteByte((byte)_destZoneId);
            }
            else
                Out.WriteUInt16R(_unit.Heading);

            if (Utils.HasFlag(flags, (int)EObjectState.LookingAt))
                Out.WriteUInt16R(FollowTarget.Oid);

        }
    }
}
