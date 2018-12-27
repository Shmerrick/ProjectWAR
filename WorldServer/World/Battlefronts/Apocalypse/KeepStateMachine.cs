using Appccelerate.StateMachine;
using NLog;
using System.Collections.Generic;
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

        public SM(BattleFrontKeep keep)
        {
            Keep = keep;
            fsm = new PassiveStateMachine<ProcessState, Command>();
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
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnInnerDoorDown).Goto(ProcessState.LordWounded).Execute(() => Keep.SetLordWounded());
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
                .On(Command.OnLordKilledTimerEnd).Goto(ProcessState.Seized).Execute(() => Keep.SetSeized());
            fsm.In(ProcessState.Seized)
                .On(Command.OnSeizedTimerEnd).Goto(ProcessState.Safe).Execute(() => Keep.SetKeepSafe());
            /* Defence tick events */
            fsm.In(ProcessState.OuterDown)
                .On(Command.OnDefenceTickTimerEnd).Goto(ProcessState.DefenceTick).Execute(() => Keep.SetDefenceTick());
            fsm.In(ProcessState.InnerDown)
                .On(Command.OnDefenceTickTimerEnd).Goto(ProcessState.DefenceTick).Execute(() => Keep.SetDefenceTick());
            fsm.In(ProcessState.OuterDown)
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

            fsm.In(ProcessState.Locked)
                .On(Command.OnOpenBattleFront).Goto(ProcessState.Safe).Execute(() => Keep.SetKeepSafe());





            //OnOuterDownTimerEnd -> door.spawn, check both doors, if ok - exec safe
            //{new StateTransition(ProcessState.InnerDoorRepaired, Command.AllDoorsRepaired), ProcessState.Safe},
            //{new StateTransition(ProcessState.OuterDoorRepaired, Command.AllDoorsRepaired), ProcessState.Safe},
            //{new StateTransition(ProcessState.OuterDoorRepaired, Command.OnInnerDownTimerEnd), ProcessState.InnerDoorRepaired},
            //{new StateTransition(ProcessState.InnerDoorRepaired, Command.OnOuterDownTimerEnd), ProcessState.OuterDoorRepaired},




        }






    }

    public class KeepStateMachine
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

        public class Process
        {
            private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");

            private readonly Dictionary<StateTransition, ProcessState> transitions;

            public Process()
            {
                CurrentState = ProcessState.Initial;

                transitions = new Dictionary<StateTransition, ProcessState>
                {
                    {new StateTransition(ProcessState.Initial, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.Safe, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.OuterDown, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.InnerDown, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.LordKilled, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.Seized, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.Locked, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.DefenceTick, Command.OnLockZone), ProcessState.Locked},
                    {new StateTransition(ProcessState.LordWounded, Command.OnLockZone), ProcessState.Locked},

                    {new StateTransition(ProcessState.Initial, Command.OnOpenBattleFront), ProcessState.Safe},
                    {new StateTransition(ProcessState.Safe, Command.OnOuterDoorDown), ProcessState.OuterDown},
                    {new StateTransition(ProcessState.Safe, Command.OnInnerDoorDown), ProcessState.InnerDown},
                    {new StateTransition(ProcessState.OuterDown, Command.OnInnerDoorDown), ProcessState.InnerDown},
                    {new StateTransition(ProcessState.Safe, Command.OnLordWounded), ProcessState.LordWounded},
                    {new StateTransition(ProcessState.OuterDown, Command.OnLordWounded), ProcessState.LordWounded},
                    {new StateTransition(ProcessState.InnerDown, Command.OnLordWounded), ProcessState.LordWounded},
                    // unlikely - but possible for a gm
                    {new StateTransition(ProcessState.InnerDown, Command.OnLordKilled), ProcessState.LordKilled},
                    // unlikely - but possible for a gm
                    {new StateTransition(ProcessState.OuterDown, Command.OnLordKilled), ProcessState.LordKilled},
                    {new StateTransition(ProcessState.LordWounded, Command.OnLordKilled), ProcessState.LordKilled},
                    {new StateTransition(ProcessState.LordKilled, Command.OnLordKilledTimerEnd), ProcessState.Seized},
                    {new StateTransition(ProcessState.Seized, Command.OnSeizedTimerEnd), ProcessState.Safe},
                    {new StateTransition(ProcessState.OuterDown, Command.OnDefenceTickTimerEnd), ProcessState.DefenceTick},
                    {new StateTransition(ProcessState.InnerDown, Command.OnDefenceTickTimerEnd), ProcessState.DefenceTick},
                    {new StateTransition(ProcessState.DefenceTick, Command.OnBackToSafeTimerEnd), ProcessState.Safe},

                    {new StateTransition(ProcessState.OuterDown, Command.OnOuterDownTimerEnd), ProcessState.Safe},
                    {new StateTransition(ProcessState.InnerDown, Command.OnOuterDownTimerEnd), ProcessState.Safe},
                    {new StateTransition(ProcessState.LordWounded, Command.OnOuterDownTimerEnd), ProcessState.Safe},
                    {new StateTransition(ProcessState.InnerDown, Command.OnInnerDownTimerEnd), ProcessState.Safe},
                    {new StateTransition(ProcessState.LordWounded, Command.OnInnerDownTimerEnd), ProcessState.Safe},
                    //OnOuterDownTimerEnd -> door.spawn, check both doors, if ok - exec safe
                    //{new StateTransition(ProcessState.InnerDoorRepaired, Command.AllDoorsRepaired), ProcessState.Safe},
                    //{new StateTransition(ProcessState.OuterDoorRepaired, Command.AllDoorsRepaired), ProcessState.Safe},
                    //{new StateTransition(ProcessState.OuterDoorRepaired, Command.OnInnerDownTimerEnd), ProcessState.InnerDoorRepaired},
                    //{new StateTransition(ProcessState.InnerDoorRepaired, Command.OnOuterDownTimerEnd), ProcessState.OuterDoorRepaired},

                    {new StateTransition(ProcessState.Locked, Command.OnOpenBattleFront), ProcessState.Safe}
                };
            }


            public ProcessState CurrentState { get; private set; }

            public ProcessState GetNext(Command command)
            {
                var transition = new StateTransition(CurrentState, command);
                ProcessState nextState;
                if (!transitions.TryGetValue(transition, out nextState))
                {
                    // throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
                    BattlefrontLogger.Warn("Invalid transition: " + CurrentState + " -> " + command);
                    return CurrentState;
                }
                return nextState;
            }

            public ProcessState MoveNext(Command command)
            {
                BattlefrontLogger.Debug($"{CurrentState}.{command}==>");
                CurrentState = GetNext(command);
                BattlefrontLogger.Debug($"{CurrentState}");
                return CurrentState;
            }

            private class StateTransition
            {
                private readonly Command Command;
                private readonly ProcessState CurrentState;

                public StateTransition(ProcessState currentState, Command command)
                {
                    CurrentState = currentState;
                    Command = command;
                }

                public override int GetHashCode()
                {
                    return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    var other = obj as StateTransition;
                    return other != null && CurrentState == other.CurrentState && Command == other.Command;
                }
            }
        }
    }

}


/*
 * public class Program
   {
   static void Main(string[] args)
   {
   Process p = new Process();
   Console.WriteLine("Current State = " + p.CurrentState);
   Console.WriteLine("Command.Begin: Current State = " + p.MoveNext(Command.Begin));
   Console.WriteLine("Command.Pause: Current State = " + p.MoveNext(Command.Pause));
   Console.WriteLine("Command.End: Current State = " + p.MoveNext(Command.End));
   Console.WriteLine("Command.Exit: Current State = " + p.MoveNext(Command.Exit));
   Console.ReadLine();
   }
   }

    */
