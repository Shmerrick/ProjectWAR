using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using NLog;
using WorldServer.Services.World;
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
            fsm = new PassiveStateMachine<ProcessState, Command>();

            fsm.TransitionCompleted += RecordTransition;

            ///* Initial State */
            //fsm.In(ProcessState.Initial)
            //    .On(Command.OnStartPatrol)
            //    .Goto(ProcessState.Patrol)
            //    .Execute(() => brain.PerformPatrolling());
            //fsm.In(ProcessState.Patrol)
            //    .On(Command.OnDetectEnemy)
            //    .Goto(ProcessState.Closing)
            //    .Execute(() => brain.SelectTarget());
            //fsm.In(ProcessState.Closing)
            //    .On(Command.OnAttack)
            //    .Goto(ProcessState.Combat)
            //    .Execute(() => brain.PerformCombat());
            //fsm.In(ProcessState.Combat)
            //    .On(Command.OnCheckHealth)
            //    .Goto(ProcessState.HealthCheck)
            //    .Execute(() => brain.PerformHealthCheck());

        }

        private void RecordTransition(object sender, EventArgs e)
        {
            var stateInformation = (Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<SM.ProcessState, SM.Command>)e;
            _logger.Debug($"Changing mdps AI state, " +
                          $"State : {stateInformation.StateId}=>{stateInformation.NewStateId} due to {stateInformation.EventId}");
        }
    }
}
