
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using FrameWork;
using GameData;
using NLog;
using System.Linq;
using System.Threading;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;


//test with .spawnmobinstance 2000681 -- https://github.com/Eraclys/BehaviourTree
namespace WorldServer.World.AI.BT
{
    public class ChosenBrain : ABrain
    {
        private static new readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public IBehaviour<Creature> BehaviourTree { get; set; }
        public PartyDirective Directive { get; set; }



        public ChosenBrain(Unit myOwner)
            : base(myOwner)
        {

        }



        public override void Think(long tick)
        {
            if (_unit.IsDead)
                return;


            base.Think(tick);

            if (BehaviourTree == null)
            {
                BehaviourTree = (IBehaviour<Creature>)MakeTree();
            }

            BehaviourTree.Tick((Creature)_unit);


            // Guard nearest friend
            //var friendlyPlayers = arg.GetPlayersInRange(30, false).Where(x => x.Realm == arg.Realm).ToList();
            //if (friendlyPlayers.Count() > 0)
            //{
            //    lock (friendlyPlayers)
            //    {
            //        var randomFriend = StaticRandom.Instance.Next(friendlyPlayers.Count());
            //        LinkedBuffInteraction lbi = new LinkedBuffInteraction((ushort)8325, arg, friendlyPlayers[randomFriend],
            //            BuffEffectInvoker.CreateGuardBuff);
            //        lbi.Initialize();
            //    }
            //}

            

        }

     
        public IBehaviour<Creature> ProcessDirectiveBehaviour()
        {
            return FluentBuilder.Create<Creature>()
                .Sequence("directive-processing")
                .Condition("directive-exists", HasDirective)
                .Do("processDirective", t =>
                {
                    SpeakYourMind($"Processing Directive..");
                    return BehaviourStatus.Succeeded;
                })
                .End()
                .Build();
        }

        public IBehaviour<Creature> HighLevelActionBehaviour()
        {
            return FluentBuilder.Create<Creature>()
                .Selector("high-level-actions")
                    .Do("setTarget", SetNewTarget)
                    .Do("drink-potion", DrinkPotion)
                    .Random("crowd-control", 0.2)
                        .Do("knock-down", KnockDownTarget)
                    .End()
                    .Random("crowd-control", 0.1)
                        .Do("punt", PuntTarget)
                    .End()
                    .Condition("can-attack", CanAttack)
                        .Subtree(AttackBehaviour())
                .End()
                .Build();
        }

        

        public IBehaviour<Creature> AttackBehaviour()
        {
            return FluentBuilder.Create<Creature>()
                .Selector("attack-behaviour")
                //.Wait("GCD", 1400)


                .Random("change-to-random", .05)
                    .Do("change-to-random", ChangeToRandomTarget)
                .End()
                .Random("seeping-wound", .05)
                    .Do("seeping-wound", SeepingWound)
                .End()
                .Random("touch-of-palsy", .10)
                    .Do("touch-of-palsy", TouchOfPalsy)
                .End()
                .Random("ravage", 25.00)
                    .Do("ravage", Ravage)
                .End()
                .Random("Taunt", 0.05)
                    .Do("Taunt", Taunt)
                .End()

                .End()
                .Build();
        }

        public IBehaviour<Creature> MakeTree()
        {
            return FluentBuilder.Create<Creature>()
                .PrioritySelector("root")
                .Subtree(ProcessDirectiveBehaviour())
                .Subtree(HighLevelActionBehaviour())
                .End()
                .Build();

        }

        private BehaviourStatus SetNewTarget(Creature arg)
        {
            if (!ClosePlayers(arg))
                return BehaviourStatus.Failed;

            if (!HasNoTarget(arg))   // has a target
                return BehaviourStatus.Failed;

            var currentTarget = arg.CbtInterface.GetCurrentTarget();

            // We have no target, set one
            if (currentTarget == null)
            {
                SelectNewTarget(arg);
                return BehaviourStatus.Succeeded;
            }

            // If target is dead, set one.
            if (currentTarget.IsDead)
            {
                SelectNewTarget(arg);
                return BehaviourStatus.Succeeded;
            }
            return BehaviourStatus.Failed;
        }

        public void SelectNewTarget(Creature arg)
        {
            Unit target = arg.AiInterface.GetAttackableUnit();
            Combat.SetTarget(target, TargetTypes.TARGETTYPES_TARGET_ENEMY);
            SpeakYourMind($"New target selected : {target?.Name}. Buffing!");


            //if (arg.AbtInterface.NPCAbilities == null)
            //    return;

            //foreach (NPCAbility ability in arg.AbtInterface.NPCAbilities)
            //{
            //    // If ability is set to Active = 0 it will not be played
            //    if (ability.Active == 0 || ability.ActivateOnCombatStart == 0) continue;

            //    arg.AbtInterface.StartCast(arg, ability.Entry, 1);
            //    Thread.Sleep(1500);
            //}

            arg.AiInterface.ProcessCombatStart(target);
                

        }

        private bool IsInCombat(Creature arg)
        {
            if (arg.CbtInterface.IsInCombat)
                SpeakYourMind($" Is in combat");
            else
            {
                SpeakYourMind($"is NOT in combat");
            }
            return arg.CbtInterface.IsInCombat;
        }


        public bool HasDirective(Creature unit)
        {
            if (Directive == null)
            {
                SpeakYourMind($"has no Directive");
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool ClosePlayers(Creature unit)
        {
            SpeakYourMind($"{unit.PlayersInRange.Count} close players detected");
            return unit.PlayersInRange.Count > 0;
        }

        public bool HasNoTarget(Creature unit)
        {
            if (unit.CbtInterface.GetCurrentTarget() == null)
                SpeakYourMind($" has no target");
            else
            {
                SpeakYourMind($" has a target of {unit.CbtInterface.GetCurrentTarget().Name}");
            }
            return unit.CbtInterface.GetCurrentTarget() == null;
        }
        public bool HasTarget(Creature unit)
        {
            return !HasNoTarget(unit);
        }


        public bool HasLowHealth(Creature arg)
        {
            var percentHealth = (arg.Health * 100) / arg.MaxHealth;
            SpeakYourMind($" health at {percentHealth}%");
            return (percentHealth < 20f);
        }

        public BehaviourStatus DrinkPotion(Creature arg)
        {
            if (HasLowHealth(arg))
            {
                // 695 is healing pot model - bit of hack
                // This needs to be timed if we dont have a proper inventory to work with.
                var items = CreatureService.GetCreatureItems((arg as Creature).Entry).Where(x => x.ModelId == 695);
                // Low health -- potion of healing
                if (items.Count() > 0)
                {
                    // 7872 - Potion of Healing ability
                    SpeakYourMind($" using Potion of Healing");
                    return arg.AbtInterface.StartCast(arg, 7872, 1)
                        ? BehaviourStatus.Succeeded
                        : BehaviourStatus.Failed;
                }
                else
                {
                    return BehaviourStatus.Failed;
                }
            }
            else
            {
                return BehaviourStatus.Failed;
            }
        }

        public bool TargetInMeleeRange(Creature arg)
        {
            var result = arg.GetDistanceToObject(arg.CbtInterface.GetCurrentTarget()) < 5;
            SpeakYourMind($" in melee range = {result}");
            return result;
        }

        public bool TargetIsUnstoppable(Creature arg)
        {
            var buff = arg.CbtInterface.GetCurrentTarget().BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, arg.CbtInterface.GetCurrentTarget());
            if (buff != null)
                SpeakYourMind($" {arg.CbtInterface.GetCurrentTarget().Name} is unstoppable!");
            return buff != null;
        }

        public BehaviourStatus KnockDownTarget(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            if (TargetIsUnstoppable(arg))
                return BehaviourStatus.Failed;
            if (!CanAttack(arg))
                return BehaviourStatus.Failed;

            SpeakYourMind($" using Downfall vs {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
            return arg.AbtInterface.StartCast(arg, 8346, 1) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        public BehaviourStatus PuntTarget(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            if (TargetIsUnstoppable(arg))
                return BehaviourStatus.Failed;


            SpeakYourMind($" using Repel vs {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
            arg.CbtInterface.GetCurrentTarget().ApplyKnockback(arg, AbilityMgr.GetKnockbackInfo(8329, 0));
            return BehaviourStatus.Succeeded;
        }

        public bool CanAttack(Creature arg)
        {
            var result = arg.AbtInterface.CanCastCooldown(0) && TCPManager.GetTimeStampMS() > NextTryCastTime;
            if (result)
                SpeakYourMind($" {arg.CbtInterface.GetCurrentTarget()} can be attacked");
            return result;
        }

        public BehaviourStatus SeepingWound(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            // 8320 - Seeping Wound
            SpeakYourMind($" using Seeping Wound vs {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
            return arg.AbtInterface.StartCast(arg, 8320, 1) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        public BehaviourStatus TouchOfPalsy(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            // 8338 - Touch of Palsy
            SpeakYourMind($" using Touch of Palsy vs {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
            return arg.AbtInterface.StartCast(arg, 8338, 1) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        public BehaviourStatus ChangeToRandomTarget(Creature arg)
        {
            // Switch targets
            SpeakYourMind($" using Changing Targets {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
            var randomTarget = SetRandomTarget();
            SpeakYourMind($" => {(randomTarget as Player).Name}");
            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus Ravage(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            // 8323 - Ravage
            SpeakYourMind($" using Ravage vs {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
            return arg.AbtInterface.StartCast(arg, 8323, 1) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        public BehaviourStatus Taunt(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            var tauntTarget = SetRandomTarget();
            // Taunt
            SpeakYourMind($" using Taunt vs {(tauntTarget as Player).Name}");
            return arg.AbtInterface.StartCast(arg, 8322, 1) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }

        public BehaviourStatus ChampionsChallenge(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            // 608 - Champion's Challenge
            SpeakYourMind($" using Champion's Challenge vs {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
            return arg.AbtInterface.StartCast(arg, 608, 1) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
        }


        public BehaviourStatus SeverBlessing(Creature arg)
        {
            if (!TargetInMeleeRange(arg))
                return BehaviourStatus.Failed;
            var blessing = arg.CbtInterface.GetCurrentTarget().BuffInterface.HasBuffOfType((byte)BuffTypes.Blessing);
            if (blessing)
            {
                // 8339 - Sever blessing
                SpeakYourMind($" using Sever Blessing vs {(arg.CbtInterface.GetCurrentTarget() as Player).Name}");
                return arg.AbtInterface.StartCast(arg, 8339, 1) ? BehaviourStatus.Succeeded : BehaviourStatus.Failed;
            }

            return BehaviourStatus.Failed;
        }
        
        

    }



    public class PartyDirective
    {
    }
}
