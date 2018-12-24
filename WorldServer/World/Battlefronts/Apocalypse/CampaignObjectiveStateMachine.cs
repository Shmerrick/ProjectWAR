using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            PlayerInteractTimerEnd,
            CaptureTimerEnd,
            SafeTimerEnd,
            LockZone
        }

        public class Process
        {
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
                    return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
                }
            }

            Dictionary<StateTransition, ProcessState> transitions;
            public ProcessState CurrentState { get; private set; }

            public Process()
            {
                CurrentState = ProcessState.Neutral;
                transitions = new Dictionary<StateTransition, ProcessState>
            {
                { new StateTransition(ProcessState.Neutral, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Capturing, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Captured, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Locked, Command.LockZone), ProcessState.Locked },
                { new StateTransition(ProcessState.Guarded, Command.LockZone), ProcessState.Locked },

                { new StateTransition(ProcessState.Neutral, Command.PlayerInteractTimerEnd), ProcessState.Capturing },
                { new StateTransition(ProcessState.Capturing, Command.CaptureTimerEnd), ProcessState.Captured },
                { new StateTransition(ProcessState.Captured, Command.SafeTimerEnd), ProcessState.Guarded },
                { new StateTransition(ProcessState.Guarded, Command.PlayerInteractTimerEnd), ProcessState.Capturing }
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
                CurrentState = GetNext(command);
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