using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Machine.Events;
using NLog;
using System;

namespace WorldServer.World.AI.EnhancedAI
{
    public class NonCombatCreatureStateMachine
    {
        public enum Command
        {
            OnTargetSpotted,
            OnNoTargetSpotted,
            OnMoving,
            OnReachNode,
            OnEndPath,
            OnLoading,
            OnLeavingCombat,
            MoveStageComplete
        }

        public enum ProcessState
        {
            Initial,
            Patrolling,
            Looking,
            RecalculatePath,
            Combat,
            Loaded
        }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public NonCombatCreatureStateMachine(EnhancedCreature creature)
        {
            Creature = creature;
            var builder = new StateMachineDefinitionBuilder<ProcessState, Command>();


            ///* Initial State */
            builder.In(ProcessState.Initial)
                .On(Command.OnLoading)
                .Goto(ProcessState.Patrolling)
                .Execute(() => creature.DeterminePath(creature.StartPatrolPoint, creature.EndPatrolPoint))
                .Execute(creature.SetPatrolling);

            // Moving through patrol, move stage complete, look for targets

            builder.In(ProcessState.Patrolling)
                .On(Command.MoveStageComplete)
                .Goto(ProcessState.Looking)
                .Execute(creature.SetLookForTargets);

            // Found a target - go to combat

            builder.In(ProcessState.Looking)
                .On(Command.OnTargetSpotted)
                .Goto(ProcessState.Combat)
                .Execute(creature.EnterCombatState);

            // No target found, continue patrol

            builder.In(ProcessState.Looking)
                .On(Command.OnNoTargetSpotted)
                .Goto(ProcessState.Patrolling)
                .Execute(creature.SetPatrolling);

            // In Combat, asking to leave Combat

            builder.In(ProcessState.Combat)
                .On(Command.OnLeavingCombat)
                .Goto(ProcessState.Patrolling)
                .Execute(creature.ReturnToLastPatrolPoint);

            // Reached the end of the path, calculate the return journey, and begin it

            builder.In(ProcessState.Patrolling)
                .On(Command.OnEndPath)
                .Goto(ProcessState.Looking)
                .Execute(() => creature.DeterminePath(creature.EndPatrolPoint, creature.StartPatrolPoint))
                .Execute(creature.SetPatrolling);

            builder.WithInitialState(ProcessState.Initial);

            fsm = builder
                .Build()
                .CreatePassiveStateMachine();

            fsm.TransitionCompleted += RecordTransition;

        }


        public EnhancedCreature Creature { get; set; }
        public PassiveStateMachine<ProcessState, Command> fsm { get; set; }

        private void RecordTransition(object sender, EventArgs e)
        {
            //foreach (var plr in Keep.PlayersInRange)
            //{
            //    Keep.SendKeepInfo(plr);
            //}

            var stateInformation = (TransitionCompletedEventArgs<ProcessState, Command>)e;
            // Save the state transition.
            _logger.Debug(
                $"State : {stateInformation.StateId}=>{stateInformation.NewStateId} due to {stateInformation.EventId}");
        }
    }
}