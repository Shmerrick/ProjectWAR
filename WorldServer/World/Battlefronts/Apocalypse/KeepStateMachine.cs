using NLog;
using System;
using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class KeepStateMachine
    {
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
            LordWounded

        }

        public enum Command
        {
            OnOpenBattleFront,
            OnOuterDownTimerEnd,
            OnInnerDownTimerEnd,
            OnSeizedTimerEnd,
            OnOuterDoorDown,
            OnInnerDoorDown,
            OnLordKilled,
            LockZone,
            OnLordKilledTimerEnd,
            OnDefenceTickTimerEnd,
            OnBackToSafeTimerEnd,
            OnLordWounded
        }

        public class Process
        {
            private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");

            class StateTransition
            {
                readonly ProcessState CurrentState;
                readonly Command Command;

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
                    StateTransition other = obj as StateTransition;
                    return other != null && CurrentState == other.CurrentState && Command == other.Command;
                }
            }

            Dictionary<StateTransition, ProcessState> transitions;
            public ProcessState CurrentState { get; private set; }

            public Process()
            {
                CurrentState = ProcessState.Initial;
                transitions = new Dictionary<StateTransition, ProcessState>
            {

                { new StateTransition(ProcessState.Initial, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Safe, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.OuterDown, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.InnerDown, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.LordKilled, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Seized, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Locked, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Initial, Command.OnOpenBattleFront), ProcessState.Safe },
                { new StateTransition(ProcessState.Safe, Command.OnOuterDoorDown), ProcessState.OuterDown },
                { new StateTransition(ProcessState.OuterDown, Command.OnInnerDoorDown), ProcessState.InnerDown },
                { new StateTransition(ProcessState.Safe, Command.OnLordWounded), ProcessState.LordWounded },
                { new StateTransition(ProcessState.OuterDown, Command.OnLordWounded), ProcessState.LordWounded },
                { new StateTransition(ProcessState.InnerDown, Command.OnLordWounded), ProcessState.LordWounded },
                { new StateTransition(ProcessState.LordWounded, Command.OnLordKilled), ProcessState.LordKilled },
                { new StateTransition(ProcessState.LordKilled, Command.OnLordKilledTimerEnd), ProcessState.Seized },
                { new StateTransition(ProcessState.Seized, Command.OnSeizedTimerEnd), ProcessState.Safe },
                { new StateTransition(ProcessState.OuterDown, Command.OnDefenceTickTimerEnd), ProcessState.DefenceTick },
                { new StateTransition(ProcessState.InnerDown, Command.OnDefenceTickTimerEnd), ProcessState.DefenceTick },
                { new StateTransition(ProcessState.DefenceTick, Command.OnBackToSafeTimerEnd), ProcessState.Safe }

            };
            }

            public ProcessState GetNext(Command command)
            {
                StateTransition transition = new StateTransition(CurrentState, command);
                ProcessState nextState;
                if (!transitions.TryGetValue(transition, out nextState))
                    throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
                return nextState;
            }

            public ProcessState MoveNext(Command command)
            {
                BattlefrontLogger.Debug($"{CurrentState}.{command}==>");
                CurrentState = GetNext(command);
                BattlefrontLogger.Debug($"{CurrentState}");
                return CurrentState;
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
