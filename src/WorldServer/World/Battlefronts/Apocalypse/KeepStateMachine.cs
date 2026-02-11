using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using NLog;
using System;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Keeps;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class SM
    {
        public enum Command
        {
            OnOpenBattleFront,
            OnGuildClaimTimerEnd,
            OnOuterDoorDown,
            OnInnerDoorDown,
            OnLordKilled,
            OnLockZone,
            OnLordKilledTimerEnd,
            OnBackToSafeTimerEnd,
            OnLordWounded,
            OnGuildClaimInteracted
        }

        public enum ProcessState
        {
            Initial,
            Safe,
            OuterDown,
            InnerDown,
            LordKilled,
            GuildClaim,
            Locked,
            DefenceTick,
            LordWounded
        }

        public BattleFrontKeep Keep { get; set; }
        public PassiveStateMachine<ProcessState, Command> fsm { get; set; }

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public SM(BattleFrontKeep keep)
        {
            Keep = keep;
            var builder = new StateMachineDefinitionBuilder<ProcessState, Command>();

            /* Initial State */
            builder.In(ProcessState.Initial)
                .On(Command.OnOpenBattleFront)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.SetKeepSafe());
            /* Any call to Lock Zone will execute Lock */
            builder.In(ProcessState.Initial)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.Safe)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.OuterDown)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.InnerDown)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.LordKilled)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.GuildClaim)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.Locked)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.DefenceTick)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());
            builder.In(ProcessState.LordWounded)
                .On(Command.OnLockZone).Goto(ProcessState.Locked).Execute(() => Keep.SetKeepLocked());

            builder.In(ProcessState.Safe)
                .On(Command.OnOuterDoorDown)
                .Goto(ProcessState.OuterDown)
                .Execute<uint>((uint doorId) => Keep.SetOuterDoorDown(doorId));
            builder.In(ProcessState.Safe)
                .On(Command.OnInnerDoorDown)
                .Goto(ProcessState.InnerDown)
                .Execute<uint>((uint doorId) => Keep.SetInnerDoorDown(doorId));
            builder.In(ProcessState.OuterDown)
                .On(Command.OnInnerDoorDown)
                .Goto(ProcessState.InnerDown)
                .Execute<uint>((uint doorId) => Keep.SetInnerDoorDown(doorId));
            /* Lord Wounded events */
            builder.In(ProcessState.Safe)
                .On(Command.OnLordWounded).Goto(ProcessState.LordWounded).Execute(() => Keep.SetLordWounded());
            builder.In(ProcessState.InnerDown)
                .On(Command.OnInnerDoorDown).Goto(ProcessState.LordWounded).Execute(() => Keep.SetLordWounded());

            /* GM only events - lord kill */
            builder.In(ProcessState.InnerDown)
                .On(Command.OnLordKilled).Goto(ProcessState.LordKilled).Execute(() => Keep.SetLordKilled());
            builder.In(ProcessState.OuterDown)
                .On(Command.OnLordKilled).Goto(ProcessState.LordKilled).Execute(() => Keep.SetLordKilled());
            builder.In(ProcessState.LordWounded)
                .On(Command.OnLordKilled).Goto(ProcessState.LordKilled).Execute(() => Keep.SetLordKilled());
            // Allow for keep to be safe (doors repaired) and lord still to be killed.
            builder.In(ProcessState.Safe)
                .On(Command.OnLordKilled).Goto(ProcessState.LordKilled).Execute(() => Keep.SetLordKilled());
            builder.In(ProcessState.LordKilled)
                .On(Command.OnLordKilledTimerEnd).Goto(ProcessState.GuildClaim).Execute(() => Keep.SetKeepSeized());

            builder.In(ProcessState.GuildClaim) // Guild claim interacted, go to safe.
                .On(Command.OnGuildClaimInteracted)
                .Goto(ProcessState.Safe)
                .Execute<uint>((uint guildId) => Keep.SetGuildClaimed(guildId));
            //.Execute(() => Keep.SetKeepSafe());
            builder.In(ProcessState.GuildClaim)
                .On(Command.OnGuildClaimTimerEnd)
                .If(Keep.IsFortress)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.ForceLockZone())
                .Execute(() => Keep.SetKeepSafe());
            builder.In(ProcessState.GuildClaim)
                .On(Command.OnGuildClaimTimerEnd)
                .If(Keep.IsNotFortress)
                .Goto(ProcessState.Safe)
                .Execute(() => Keep.GenerateKeepTakeRewards())
                .Execute(() => Keep.SetKeepSafe());

            //fsm.In(ProcessState.OuterDown)
            //    .On(Command.OnDoorRepaired)
            //    .If(Keep.AllDoorsRepaired)
            //    .Goto(ProcessState.Safe)
            //    //.Execute<uint>((uint doorId) => Keep.SetDoorRepaired(doorId))
            //    .Execute(() => Keep.SetKeepSafe());
            //fsm.In(ProcessState.InnerDown)                             // Inner down, but all repaired (implies outer repaired) -> safe
            //    .On(Command.OnDoorRepaired)
            //    .If(Keep.AllDoorsRepaired)
            //    .Goto(ProcessState.Safe)
            //    .Execute(() => Keep.SetKeepSafe());
            //fsm.In(ProcessState.InnerDown)                             // Inner down, but not all repaired -> outer still
            //    .On(Command.OnDoorRepaired)
            //    .Goto(ProcessState.Safe)
            //    .Execute<uint>((uint doorId) => Keep.SetDoorRepaired(doorId));
            //fsm.In(ProcessState.LordWounded)
            //    .On(Command.OnDoorRepaired)
            //    .If(Keep.AllDoorsRepaired)
            //    .Goto(ProcessState.Safe)
            //    //.Execute<uint>((uint doorId) => Keep.SetDoorRepaired(doorId))
            //    .Execute(() => Keep.SetKeepSafe());

            builder.In(ProcessState.Locked)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Safe).Execute(() => Keep.SetKeepSafe());

            builder.WithInitialState(ProcessState.Initial);

            fsm = builder
                .Build()
                .CreatePassiveStateMachine();
            fsm.TransitionCompleted += RecordTransition;
        }

        private void RecordTransition(object sender, EventArgs e)
        {
            foreach (var plr in Keep.PlayersInRange)
            {
                Keep.SendKeepInfo(plr);
            }

            var stateInformation = (Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<SM.ProcessState, SM.Command>)e;
            // Save the state transition.
            _logger.Debug($"Saving keep state KeepId : {Keep.Info.KeepId} {Keep.Info.Name}, " +
                          $"State : {stateInformation.StateId}=>{stateInformation.NewStateId} due to {stateInformation.EventId}");
            RVRProgressionService.SaveBattleFrontKeepState(Keep.Info.KeepId, stateInformation.NewStateId);
        }
    }
}