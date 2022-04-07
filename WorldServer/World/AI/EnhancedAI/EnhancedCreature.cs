using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using NLog;
using WorldServer.World.AI.PathFinding.AStar;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.AI.EnhancedAI
{
    public class EnhancedCreature : Creature
    {
        private const int COMBAT_TOO_FAR_DISTANCE = 100;
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public PathFinder PathFinderEngine;
        public byte[,] ZoneMap;
        
        public EnhancedCreature(IUnitAdaptor unitAdaptor)
        {
            UnitAdaptor = unitAdaptor;
            TargetOptions = new ConcurrentDictionary<Unit, int>();
            PatrolPath = new List<PathFinderNode>();
            PatrolStateMachine = new NonCombatCreatureStateMachine(this);
            CombatStateMachine = new CombatCreatureStateMachine(this);
            DebugMode = false;
        }

        public new Zone_Info Zone { get; set; }

        public IUnitAdaptor UnitAdaptor { get; set; }
        public bool InCombat { get; set; }

        public List<PathFinderNode> PatrolPath { get; set; }
        public Point StartPatrolPoint { get; set; }

        public Point EndPatrolPoint { get; set; }

        // The current patrol point (this acts as the "Reset" point for the creature) in the Patrol Path.
        public Point CurrentPatrolPoint { get; set; }

        // The position of the creature now (might be varied by getting into combat, or moving around an obstacle)
        public Point CurrentPosition { get; set; }

        // The position of the target now 
        public Point TargetPosition { get; set; }

        // Path the creature needs to take to approach the target
        public List<PathFinderNode> CombatPath { get; set; }

        public int PatrolPathIndex { get; set; }

        // How far along the combat path we are
        public int CombatPathIndex { get; set; }

        public Unit CombatTarget { get; set; }
        public byte SightRange { get; set; }

        public TargetSelectionMethod TargetSelectionMethod { get; set; }

        public ConcurrentDictionary<Unit, int> TargetOptions { get; set; }
        public NonCombatCreatureStateMachine PatrolStateMachine { get; }
        public CombatCreatureStateMachine CombatStateMachine { get; }
        public bool DebugMode { get; set; }


        public void Initialise()
        {
            PathFinderEngine = new PathFinder(ZoneMap,
                new PathFinderOptions {PunishChangeDirection = true, Diagonals = true});

            if (!PatrolStateMachine.fsm.IsRunning)
                PatrolStateMachine.fsm.Initialize(NonCombatCreatureStateMachine.ProcessState.Initial);
            if (!PatrolStateMachine.fsm.IsRunning)
                PatrolStateMachine.fsm.Start();
            
            PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.OnLoading);
        }


        /// <summary>
        ///     Given a start and end point, determine the path given the known map.
        /// </summary>
        public void DeterminePath(Point start, Point end)
        {
            var path = PathFinderEngine.FindPath(start, end);
            PatrolPath = path;
        }

        /// <summary>
        ///     Now in process state of Combat. Start the Combat statemachine.
        /// </summary>
        public void EnterCombatState()
        {
            InCombat = true;

            if (!CombatStateMachine.fsm.IsRunning)
                CombatStateMachine.fsm.Initialize(CombatCreatureStateMachine.ProcessState.PreparingForCombat);
            if (!CombatStateMachine.fsm.IsRunning)
                CombatStateMachine.fsm.Start();

            CombatStateMachine.fsm.Fire(CombatCreatureStateMachine.Command.OnOutofRange);
        }

        /// <summary>
        ///     Now in process state of Combat. Start the Combat statemachine.
        /// </summary>
        public void LeaveCombatState()
        {
            InCombat = false;
            CombatStateMachine.fsm.Stop();

            PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.OnLeavingCombat);
        }

        public void SetLookForTargets()
        {
            SearchForTargets(SightRange);
            SelectTarget();
        }

        public void SearchForTargets(byte sightRange)
        {
            var unitsInRange = UnitAdaptor.GetTargetsInRange(sightRange);

            foreach (var unit in unitsInRange)
            {
                if (!UnitAdaptor.IsObjectInFront(unit, 60f, sightRange))
                    continue;
                if (!UnitAdaptor.CanAttack(this, unit))
                    continue;
                if (!UnitAdaptor.IsEnemy(this, unit))
                    continue;
                var range = UnitAdaptor.Get2DDistanceToObject(this, unit);

                TargetOptions.TryAdd(unit, range);
            }
        }

        public void SelectTarget()
        {
            if (TargetOptions.Count == 0)
                PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.OnNoTargetSpotted);
            else
                switch (TargetSelectionMethod)
                {
                    case TargetSelectionMethod.Closest:
                        var closestOption = TargetOptions.OrderByDescending(x => x.Value).FirstOrDefault();
                        CombatTarget = closestOption.Key;
                        PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.OnTargetSpotted,
                            CombatTarget);

                        break;

                    case TargetSelectionMethod.First:
                    {
                        CombatTarget = TargetOptions.First().Key;
                        PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.OnTargetSpotted,
                            CombatTarget);

                        break;
                    }

                    case TargetSelectionMethod.Random:
                        var rand = StaticRandom.Instance.Next(TargetOptions.Count);
                        var randomOption = TargetOptions.Keys.ToList()[rand];
                        CombatTarget = randomOption;
                        PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.OnTargetSpotted,
                            CombatTarget);

                        break;
                    case TargetSelectionMethod.None:
                        CombatTarget = null;
                        PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.OnNoTargetSpotted);
                        break;
                }
        }


        public void DetermineCombatPath()
        {
            if (DebugMode)
            {
                if (WorldPosition == null)
                    _logger.Debug($"WorldPosition is null");
                if (CombatTarget == null)
                    _logger.Debug($"CombatTarget is null");
                if (PathFinderEngine == null)
                    _logger.Debug($"PathFinderEngine is null");

            }


            // Where is the creature now.
            CurrentPosition = new Point(WorldPosition.X, WorldPosition.Y);
            TargetPosition = new Point(CombatTarget.X, CombatTarget.Y);

            var path = PathFinderEngine.FindPath(CurrentPosition, TargetPosition);
            CombatPath = path;
        }

        public void MoveAlongCombatPath()
        {
            // When movement completes, raise an event so we can start the next stage
            MvtInterface.OnMovementComplete += MoveStageComplete;

            if (CombatPath.Count > 0)
            {
                CombatPathIndex = 0;
                var node = CombatPath[CombatPathIndex];

                // HACK : CombatTarget.Z <-- need to make a proper mapping from the zonemap to the worldpositions.
                MvtInterface.Move(new Point3D(node.X, node.Y, CombatTarget.Z), Zone);

                CombatPathIndex++;
            }
        }

        /// <summary>
        ///     Called when a movement stage is completed by the creature.
        /// </summary>
        public void MoveStageComplete()
        {
            if (InCombat)
                CombatStateMachine.fsm.Fire(CombatCreatureStateMachine.Command.MoveStageComplete);
            else
            {
                PatrolStateMachine.fsm.Fire(NonCombatCreatureStateMachine.Command.MoveStageComplete);    
            }
           

            //var node = CombatPath[CombatPathIndex];

            //// HACK : CombatTarget.Z <-- need to make a proper mapping from the zonemap to the worldpositions.
            //MvtInterface.Move(new Point3D(node.X, node.Y, CombatTarget.Z));

            //CombatPathIndex++;
        }

        public void Fight()
        {
            if (TargetIsDead())
            {
                CombatStateMachine.fsm.Fire(CombatCreatureStateMachine.Command.OnTargetDeath);

                return;
            }

            if (CombatTargetTooFar())
            {
                CombatStateMachine.fsm.Fire(CombatCreatureStateMachine.Command.OnTargetLost);

                return;
            }

            if (NotWithinRangeOfCombatTarget())
            {
                CombatStateMachine.fsm.Fire(CombatCreatureStateMachine.Command.OnOutofRange);

                return;
            }

            UnitAdaptor.CbtInterface(this).TryAttack();
        }

        /// <summary>
        ///     The creature has died
        /// </summary>
        /// <param name="killer"></param>
        protected override void SetDeath(Unit killer)
        {
            base.SetDeath(killer);
            CombatStateMachine.fsm.Fire(CombatCreatureStateMachine.Command.OnDeath);
        }

        /// <summary>
        ///     If the unit has strayed more than a certain distance from their "home" spawn point, leave combat.
        /// </summary>
        /// <returns></returns>
        public bool CombatTargetTooFar()
        {
            return UnitAdaptor.Get2DDistanceToPoint(CurrentPatrolPoint, this) > COMBAT_TOO_FAR_DISTANCE;
        }

        public bool WithinRangeOfCombatTarget()
        {
            return UnitAdaptor.IsInCastRange(this, CombatTarget, (uint) (0.5f + BaseRadius));
        }

        public bool NotWithinRangeOfCombatTarget()
        {
            return !UnitAdaptor.IsInCastRange(this, CombatTarget, (uint) (0.5f + BaseRadius));
        }

        public void SetInRangeOfCombatTarget()
        {
            CombatStateMachine.fsm.Fire(CombatCreatureStateMachine.Command.OnBeginCombat);
        }

        public bool TargetIsDead()
        {
            if (CombatTarget != null)
            {
                if (CombatTarget.IsDead) return true;
                if (CombatTarget.PendingDisposal) return true;
                if (CombatTarget.IsDisposed) return true;
            }

            return false;
        }

        /// <summary>
        /// Return to the last patrol point. When complete, should trigger a call to MoveStageComplete - which will allow the SM
        /// to look around, select targets or move into the patrol.
        /// </summary>
        public void ReturnToLastPatrolPoint()
        {
            MvtInterface.OnMovementComplete += MoveStageComplete;
            MvtInterface.Move(new Point3D(CurrentPatrolPoint.X, CurrentPatrolPoint.Y, CombatTarget.Z), Zone);
        }

        private void LastPatrolPointReached()
        {
            throw new NotImplementedException();
        }

        public void SetPatrolling()
        {
            MvtInterface.OnMovementComplete += MoveStageComplete;

            if (PatrolPath.Count > 0)
            {
                PatrolPathIndex = 0;
                var node = PatrolPath[PatrolPathIndex];

                // HACK : Z <-- need to make a proper mapping from the zonemap to the worldpositions.
                MvtInterface.Move(new Point3D(node.X, node.Y, Z), Zone);

                PatrolPathIndex++;
            }
        }

        
    }
}