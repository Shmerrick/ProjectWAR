using Appccelerate.StateMachine;
using NLog;
using System;
using WorldServer.Services.World;
using WorldServer.World.BattleFronts.Keeps;

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
            OnSafeTimerEnd,
            OnLockZone,
            OnOpenBattleFront
        }


        public BO Objective { get; set; }
        public PassiveStateMachine<Apocalypse.CampaignObjectiveStateMachine.ProcessState, Apocalypse.CampaignObjectiveStateMachine.Command> fsm { get; set; }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public CampaignObjectiveStateMachine(BO objective)
        {
            Objective = objective;
            fsm = new PassiveStateMachine<ProcessState, Command>();

            fsm.TransitionCompleted += RecordTransition;

            /* Initial State */
            fsm.In(ProcessState.Neutral)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());
            /* Any call to Lock Zone will execute Lock */
            fsm.In(ProcessState.Neutral)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            fsm.In(ProcessState.Capturing)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            fsm.In(ProcessState.Captured)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            fsm.In(ProcessState.Locked)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            fsm.In(ProcessState.Guarded)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Objective.SetObjectiveLocked());
            fsm.In(ProcessState.Locked)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());


            fsm.In(ProcessState.Neutral)
                .On(Command.OnPlayerInteractionComplete).Goto(ProcessState.Capturing).Execute(() => Objective.SetObjectiveCapturing());
            fsm.In(ProcessState.Neutral)
                .On(Command.OnPlayerInteractionBroken).Goto(ProcessState.Neutral).Execute(() => Objective.SetObjectiveSafe());

            fsm.In(ProcessState.Capturing)
                .On(Command.OnCaptureTimerEnd).Goto(ProcessState.Captured).Execute(() => Objective.SetObjectiveCaptured());
            fsm.In(ProcessState.Captured)
                .On(Command.OnSafeTimerEnd).Goto(ProcessState.Guarded).Execute(() => Objective.SetObjectiveGuarded());

        }

        private void RecordTransition(object sender, EventArgs e)
        {
            _logger.Debug($"{e.ToString()}");

            var s = (Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<ProcessState, Command>)e;
            // Save the state transition.
            //_logger.Debug($"Saving campaign objective state {Objective.Id},{s.StateId}");
            //RVRProgressionService.SaveBattleFrontKeepState(Objective.Id, s.StateId);

        }
    }
}
