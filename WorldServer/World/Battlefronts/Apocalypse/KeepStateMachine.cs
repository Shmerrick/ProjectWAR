using Appccelerate.StateMachine;
using NLog;
using System;
using WorldServer.Services.World;
using WorldServer.World.BattleFronts.Keeps;

namespace WorldServer.World.Battlefronts.Apocalypse
{

    public class SM
    {

        public enum Command
        {
            OnOpenBattleFront,
            OnOuterDownTimerEnd,
            OnInnerDownTimerEnd,
            OnSeizedTimerEnd,
            OnOuterDoorDown,
            OnInnerDoorDown,
            OnLordKilled,
            OnLockZone,
            OnLordKilledTimerEnd,
            OnDefenceTickTimerEnd,
            OnBackToSafeTimerEnd,
            OnLordWounded,
            AllDoorsRepaired
        }

        public enum ProcessState
        {
            Initial,
            Safe,
            OuterDown,
            InnerDown,
            LordKilled,
            Seized,
            Locked,
            DefenceTick,
            LordWounded,
            OuterDoorRepaired,
            InnerDoorRepaired

        }


        public BattleFrontKeep Keep { get; set; }
        public PassiveStateMachine<ProcessState, Command> fsm { get; set; }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public SM(BattleFrontKeep keep)
        {
            Keep = keep;
            fsm = new PassiveStateMachine<ProcessState, Command>();

            fsm.TransitionCompleted += RecordTransition;

            /* Initial State */
            fsm.In(ProcessState.Initial)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Safe).Execute(() => Keep.SetKeepSafe());
            /* Any call to Lock Zone will execute Lock */
            fsm.In(ProcessState.Initial)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.Safe)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.InnerDown)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.LordKilled)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.Seized)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.Locked)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.DefenceTick)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.LordWounded)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            fsm.In(ProcessState.Safe)
                .On(Command.OnOuterDoorDown).Goto(ProcessState.OuterDown).Execute(() => Keep.SetOuterDoorDown());
            fsm.In(ProcessState.Safe)
                .On(Command.OnInnerDoorDown).Goto(ProcessState.InnerDown).Execute(() => Keep.SetInnerDoorDown());
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnInnerDoorDown).Goto(ProcessState.InnerDown).Execute(() => Keep.SetInnerDoorDown());
            /* Lord Wounded events */
            fsm.In(ProcessState.Safe)
                .On(Command.OnLordWounded).Goto(ProcessState.LordWounded).Execute(() => Keep.SetLordWounded());
            fsm.In(ProcessState.InnerDown)
                .On(Command.OnInnerDoorDown).Goto(ProcessState.LordWounded).Execute(() => Keep.SetLordWounded());
            /* GM only events - lord kill */
            fsm.In(ProcessState.InnerDown)
                .On(Command.OnLordKilled).Goto(ProcessState.LordKilled).Execute(() => Keep.SetLordKilled());
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnLordKilled).Goto(ProcessState.LordKilled).Execute(() => Keep.SetLordKilled());

            fsm.In(ProcessState.LordWounded)
                .On(Command.OnLordKilled).Goto(ProcessState.LordKilled).Execute(() => Keep.SetLordKilled());
            fsm.In(ProcessState.LordKilled)
                .On(Command.OnLordKilledTimerEnd).Goto(ProcessState.Seized).Execute(() => Keep.SetKeepSeized());
            fsm.In(ProcessState.Seized)
                .On(Command.OnSeizedTimerEnd).Goto(ProcessState.Safe).Execute(() => Keep.SetKeepSafe());
            /* Defence tick events */
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnDefenceTickTimerEnd).Goto(ProcessState.DefenceTick).Execute(() => Keep.SetDefenceTick());
            fsm.In(ProcessState.InnerDown)
                .On(Command.OnDefenceTickTimerEnd).Goto(ProcessState.DefenceTick).Execute(() => Keep.SetDefenceTick());
            fsm.In(ProcessState.DefenceTick)
                .On(Command.OnBackToSafeTimerEnd).Goto(ProcessState.Safe).Execute(() => Keep.SetKeepSafe());
            /* OnOuterDown events */
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnOuterDownTimerEnd)
                .If(Keep.BothDoorsRepaired)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.SetOuterDoorRepaired())
                .Execute(() => Keep.SetKeepSafe());
            fsm.In(ProcessState.InnerDown)
                .On(Command.OnOuterDownTimerEnd)
                .If(Keep.BothDoorsRepaired)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.SetOuterDoorRepaired())
                .Execute(() => Keep.SetKeepSafe());
            fsm.In(ProcessState.LordWounded)
                .On(Command.OnOuterDownTimerEnd)
                .If(Keep.BothDoorsRepaired)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.SetOuterDoorRepaired())
                .Execute(() => Keep.SetKeepSafe());

            /* OnInnerDownTimerEnd events */
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnInnerDownTimerEnd)
                .If(Keep.BothDoorsRepaired)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.SetInnerDoorRepaired())
                .Execute(() => Keep.SetKeepSafe());
            fsm.In(ProcessState.InnerDown)
                .On(Command.OnInnerDownTimerEnd)
                .If(Keep.BothDoorsRepaired)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.SetInnerDoorRepaired())
                .Execute(() => Keep.SetKeepSafe());
            fsm.In(ProcessState.LordWounded)
                .On(Command.OnInnerDownTimerEnd)
                .If(Keep.BothDoorsRepaired)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.SetInnerDoorRepaired())
                .Execute(() => Keep.SetKeepSafe());

            fsm.In(ProcessState.Locked)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Safe).Execute(() => Keep.SetKeepSafe());



        }

        private void RecordTransition(object sender, EventArgs e)
        {
            _logger.Debug($"{e.ToString()}");

            var s = (Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<WorldServer.World.Battlefronts.Apocalypse.SM.ProcessState, WorldServer.World.Battlefronts.Apocalypse.SM.Command>)e;
            // Save the state transition.
            _logger.Debug($"Saving keep state {Keep.Info.KeepId},{s.StateId}");
            RVRProgressionService.SaveBattleFrontKeepState(Keep.Info.KeepId, s.StateId);

        }
    }
}

