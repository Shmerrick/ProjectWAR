using System;
using Appccelerate.StateMachine;
using FrameWork;
using GameData;
using WorldServer.World.AI;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer
{
    public class MdpsBrain : ABrain
    {
        public PassiveStateMachine<MdpsStateMachine.ProcessState, MdpsStateMachine.Command> fsm { get; set; }

        public MdpsBrain(Unit myOwner)
            : base(myOwner)
        {
            fsm = new PassiveStateMachine<MdpsStateMachine.ProcessState, MdpsStateMachine.Command>("MDPSStateMachine");
        }

        public override void Think()
        {

            if (!fsm.IsRunning)
            {
                fsm.Start();
                fsm.Initialize(MdpsStateMachine.ProcessState.Initial);
            }
                
            base.Think();

            // Has waypoints
            if (_unit.AiInterface.Waypoints.Count > 0)
            {
                fsm.Fire(MdpsStateMachine.Command.OnStartPatrol);
            }

            // Only bother to seek targets if we're actually being observed by a player
            if (Combat.CurrentTarget == null && _unit.PlayersInRange.Count > 0)
            {
                if (_pet != null && (_pet.IsHeeling || ((CombatInterface_Pet)_pet.CbtInterface).IgnoreDamageEvents))
                    return;

                Unit target = _unit.AiInterface.GetAttackableUnit();
                if (target != null)
                    _unit.AiInterface.ProcessCombatStart(target);
            }

            if (Combat.IsFighting)
            {
                var percentHealth = _unit.Health / _unit.MaxHealth;

                if (percentHealth < 0.2f)
                {
                    // Low health
                }

                
            }

        }





        public void PerformPatrolling()
        {
            //_unit.AiInterface.ProcessNpcWaypoints();
        }

        public void SelectTarget()
        {
            throw new NotImplementedException();
        }

        public void PerformCombat()
        {
            throw new NotImplementedException();
        }

        public void PerformHealthCheck()
        {
            throw new NotImplementedException();
        }
    }
}
