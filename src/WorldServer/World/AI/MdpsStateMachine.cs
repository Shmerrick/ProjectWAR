using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using NLog;
using System;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.World.AI
{
    public class MdpsStateMachine
    {
        public enum Command
        {
            OnStartPatrol,
            OnDetectEnemy,
            OnAttack,
            OnCheckHealth,
            OnFlee
        }

        public enum ProcessState
        {
            Initial,
            Patrol,
            Closing,
            Combat,
            HealthCheck
        }

        public MarauderBrain Brain { get; set; }
        public PassiveStateMachine<ProcessState, Command> fsm { get; set; }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MdpsStateMachine(MarauderBrain brain)
        {
            Brain = brain;
            var builder = new StateMachineDefinitionBuilder<ProcessState, Command>();

            ///* Initial State */
            builder.In(ProcessState.Initial)
                .On(Command.OnStartPatrol)
                .Goto(ProcessState.Patrol)
                .Execute(() => brain.PerformPatrolling());
            builder.In(ProcessState.Patrol)
                .On(Command.OnDetectEnemy)
                .Goto(ProcessState.Closing)
                .Execute(() => brain.SelectTarget());
            builder.In(ProcessState.Closing)
                .On(Command.OnAttack)
                .Goto(ProcessState.Combat)
                .Execute(() => brain.PerformCombat());
            builder.In(ProcessState.Combat)
                .On(Command.OnCheckHealth)
                .Goto(ProcessState.HealthCheck)
                .Execute(() => brain.PerformHealthCheck());

            builder.WithInitialState(ProcessState.Initial);

            fsm = builder
                .Build()
                .CreatePassiveStateMachine();

            fsm.TransitionCompleted += RecordTransition;
        }

        private void RecordTransition(object sender, EventArgs e)
        {
            var stateInformation = (Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<ProcessState, Command>)e;
            _logger.Debug($"Changing mdps AI state, " +
                          $"State : {stateInformation.StateId}=>{stateInformation.NewStateId} due to {stateInformation.EventId}");
        }
    }
}