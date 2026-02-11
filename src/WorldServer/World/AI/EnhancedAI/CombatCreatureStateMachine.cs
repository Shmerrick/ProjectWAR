using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Machine.Events;
using NLog;
using System;
using System.Collections.Generic;

namespace WorldServer.World.AI.EnhancedAI
{
    public class CombatCreatureStateMachine
    {


        public enum Command
        {
            OnDeath,
            OnTargetDeath,
            OnOutofRange,
            OnBeginCombat,
            OnTargetLost,
            MoveStageComplete
        }

        public enum ProcessState
        {
            PreparingForCombat,
            EnteringRange,
            InRange,
            Dead,
            OutOfCombat,
            Fight
        }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public List<TransitionCompletedEventArgs<ProcessState, Command>> StateDebugList;

        public CombatCreatureStateMachine(EnhancedCreature creature)
        {
            Creature = creature;
            var builder = new StateMachineDefinitionBuilder<ProcessState, Command>();
            StateDebugList = new List<TransitionCompletedEventArgs<ProcessState, Command>>();

            ///* Initial State */
            builder.In(ProcessState.PreparingForCombat)
                .On(Command.OnOutofRange)
                .Goto(ProcessState.EnteringRange)
                .Execute(creature.DetermineCombatPath)
                .Execute(creature.MoveAlongCombatPath); // triggers fsm fire InRange.
            builder.In(ProcessState.EnteringRange)
                .On(Command.MoveStageComplete)
                .If(creature.WithinRangeOfCombatTarget)
                    .Goto(ProcessState.InRange)
                    .Execute(creature.SetInRangeOfCombatTarget) // triggers fsm fire OnBeginCombat .
                .If(creature.NotWithinRangeOfCombatTarget)
                    .Goto(ProcessState.EnteringRange)
                    .Execute(creature.MoveAlongCombatPath); // triggers fsm fire InRange.
            builder.In(ProcessState.InRange)
                .On(Command.OnBeginCombat)
                .Goto(ProcessState.Fight)
                .Execute(creature.Fight);
            builder.In(ProcessState.Fight)
                .On(Command.OnOutofRange)
                .Execute(creature.DetermineCombatPath)
                .Execute(creature.MoveAlongCombatPath); // triggers fsm fire InRange.
            builder.In(ProcessState.Fight)
                .On(Command.OnDeath)
                    .Goto(ProcessState.OutOfCombat)
                    .Execute(creature.LeaveCombatState);
            builder.In(ProcessState.Fight)
                .On(Command.OnTargetLost)
                .Goto(ProcessState.OutOfCombat)
                .Execute(creature.LeaveCombatState);
            builder.In(ProcessState.Fight)
                .On(Command.OnTargetDeath)
                .Goto(ProcessState.OutOfCombat)
                .Execute(creature.LeaveCombatState);

            builder.WithInitialState(ProcessState.PreparingForCombat);

            fsm = builder
                .Build()
                .CreatePassiveStateMachine();

            fsm.TransitionCompleted += RecordTransition;
        }

        public EnhancedCreature Creature { get; set; }
        public PassiveStateMachine<ProcessState, Command> fsm { get; set; }

        private void RecordTransition(object sender, EventArgs e)
        {
            var stateInformation = (TransitionCompletedEventArgs<ProcessState, Command>)e;

            if (Creature.DebugMode)
            {
                StateDebugList.Add(stateInformation);

                // Save the state transition.
                _logger.Debug(
                    $"State : {stateInformation.StateId}=>{stateInformation.NewStateId} due to {stateInformation.EventId}");
            }
        }
    }
}