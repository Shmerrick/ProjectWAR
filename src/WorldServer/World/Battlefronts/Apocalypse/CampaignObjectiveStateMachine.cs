using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using NLog;
using System;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class CampaignObjectiveStateMachine
    {
        public enum ProcessState
        {
            Neutral,
            Capturing,
            Captured,
            Locked,
            Guarded
        }

        public enum Command
        {
            OnPlayerInteractionComplete,
            OnPlayerInteractionBroken,
            OnCaptureTimerEnd,
            OnGuardedTimerEnd,
            OnLockZone,
            OnOpenBattleFront
        }

        public BattlefieldObjective Objective { get; set; }
        public PassiveStateMachine<ProcessState, Command> Fsm { get; set; }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public CampaignObjectiveStateMachine(BattlefieldObjective objective)
        {
            Objective = objective;
            var builder = new StateMachineDefinitionBuilder<ProcessState, Command>();

            /* Initial State */
            builder.In(ProcessState.Neutral)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());
            builder.In(ProcessState.Guarded)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());
            builder.In(ProcessState.Locked)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());
            builder.In(ProcessState.Captured)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());
            builder.In(ProcessState.Capturing)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());

            /* Any call to Lock Zone will execute Lock */
            builder.In(ProcessState.Neutral)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            builder.In(ProcessState.Capturing)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            builder.In(ProcessState.Captured)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            builder.In(ProcessState.Locked)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            builder.In(ProcessState.Guarded)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());

            builder.In(ProcessState.Neutral)
                .On(Command.OnPlayerInteractionComplete).Goto(ProcessState.Capturing).Execute(() => Objective.SetObjectiveCapturing());
            builder.In(ProcessState.Neutral)
                .On(Command.OnPlayerInteractionBroken).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());

            builder.In(ProcessState.Capturing)
                .On(Command.OnPlayerInteractionComplete).Goto(ProcessState.Capturing).Execute(() => Objective.SetObjectiveCapturing());
            builder.In(ProcessState.Capturing)
                .On(Command.OnPlayerInteractionBroken).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());

            builder.In(ProcessState.Capturing)  //if BO was already captured by the current realm, go to guarded.
                .On(Command.OnCaptureTimerEnd).Goto(ProcessState.Captured).Execute(() => Objective.SetObjectiveCaptured());
            builder.In(ProcessState.Captured)
                .On(Command.OnGuardedTimerEnd).Goto(ProcessState.Guarded).Execute(() => Objective.SetObjectiveGuarded());

            builder.In(ProcessState.Guarded)
                .On(Command.OnPlayerInteractionComplete).Goto(ProcessState.Capturing).Execute(() => Objective.SetObjectiveCapturing());
            builder.In(ProcessState.Guarded)
                .On(Command.OnPlayerInteractionBroken).Goto(ProcessState.Guarded).Execute(() => Objective.SetObjectiveGuarded());

            builder.WithInitialState(ProcessState.Neutral);

            Fsm = builder
                .Build()
                .CreatePassiveStateMachine();

            Fsm.TransitionCompleted += RecordTransition;

        }

        private void RecordTransition(object sender, EventArgs e)
        {
            var s = (Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<ProcessState, Command>)e;
            _logger.Debug($"{Objective.Name} : {e.ToString()} {s.StateId}=>{s.NewStateId}");

            // Save the state transition.
            //_logger.Debug($"Saving campaign objective state {Objective.Id},{s.StateId}");
            //RVRProgressionService.SaveBattleFrontKeepState(Objective.Id, s.StateId);
        }
    }
}