using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.AI;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Object = System.Object;

namespace WorldServer.World.AI.Abilities
{
    public class Executions
    {
        public Unit Owner { get; }
        public CombatInterface_Npc Combat { get; }
        public BossBrain Brain { get; set; }

        public Executions(Unit owner, CombatInterface_Npc combat, BossBrain bossBrain)
        {
            Owner = owner;
            Combat = combat;
            Brain = bossBrain;
        }
    

        //public void IncrementPhase()
        //{
        //    // Phases must be ints in ascending order.
        //    var currentPhase = CurrentPhase;
        //    if (Phases.Count == currentPhase)
        //        return;
        //    CurrentPhase = currentPhase + 1;

        //    Brain.SpeakYourMind($" using Increment Phase vs {currentPhase}=>{CurrentPhase}");
        //}

        public void ShatterBlessing()
        {
            if (Combat.CurrentTarget != null)
            {
                Brain.SpeakYourMind($" using Shatter Confidence vs {(Combat.CurrentTarget as Player).Name}");
                Brain.SimpleCast(Owner, Combat.CurrentTarget, "Shatter Confidence", 8023);
            }
        }

        public void PrecisionStrike()
        {
            if (Combat.CurrentTarget != null)
            {
                Brain.SpeakYourMind($" using PrecisionStrike vs {(Combat.CurrentTarget as Player).Name}");
                Brain.SimpleCast(Owner, Combat.CurrentTarget, "PrecisionStrike", 8005);
            }
        }

        public void SeepingWound()
        {
            if (Combat.CurrentTarget != null)
            {
                Brain.SpeakYourMind($" using Seeping Wound vs {(Combat.CurrentTarget as Player).Name}");
                Brain.SimpleCast(Owner, Combat.CurrentTarget, "Seeping Wound", 8346);
            }
        }



        public void KnockDownTarget()
        {
            Brain.SpeakYourMind($" using Downfall vs {(Combat.CurrentTarget as Player).Name}");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "Downfall", 8346);
        }

        public void PuntTarget()
        {
            if (Combat.CurrentTarget != null)
            {
                Brain.SpeakYourMind($" using Repel vs {(Combat.CurrentTarget as Player).Name}");
                Combat.CurrentTarget.ApplyKnockback(Owner, AbilityMgr.GetKnockbackInfo(8329, 0));
            }
        }

        public void Corruption()
        {
            if (Combat.CurrentTarget != null)
            {
                Brain.SpeakYourMind($" using Corruption vs {(Combat.CurrentTarget as Player).Name}");
                Brain.SimpleCast(Owner, Combat.CurrentTarget, "Corruption", 8400);
            }
        }


        public void Stagger()
        {
            if (Combat.CurrentTarget != null)
            {
                Brain.SpeakYourMind($" using Quake vs {(Combat.CurrentTarget as Player).Name}");
                Brain.SimpleCast(Owner, Combat.CurrentTarget, "Quake", 8349);
            }
        }

        public void BestialFlurry()
        {
            if (Combat.CurrentTarget != null)
            {
                Brain.SpeakYourMind($" using BestialFlurry vs {(Combat.CurrentTarget as Player).Name}");
                Brain.SimpleCast(Owner, Combat.CurrentTarget, "BestialFlurry", 5347);
            }
        }

        public void Whirlwind()
        {
            Brain.SpeakYourMind(" using Whirlwind");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "Whirlwind", 5568);
        }

        public void EnfeeblingShout()
        {
            Brain.SpeakYourMind(" using Enfeebling Shout");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "Enfeebling Shout", 5575);
        }

        public void Cleave()
        {
            Brain.SpeakYourMind(" using Cleave");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "Cleave", 13626);
        }

        public void Stomp()
        {
            Brain.SpeakYourMind(" using Stomp");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "Stomp", 4811);
        }

        public void EnragedBlow()
        {
            Brain.SpeakYourMind(" using EnragedBlow");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "EnragedBlow", 8315);
        }


        public void FlingSpines()
        {
            var newTarget = Brain.SetRandomTarget();
            if (newTarget != null)
            {
                Brain.SpeakYourMind($" using FlingSpines {newTarget.Name}");
                Combat.SetTarget(newTarget, TargetTypes.TARGETTYPES_TARGET_ENEMY);
                Brain.SimpleCast(Owner, Combat.CurrentTarget, "FlingSpines", 13089);
            }
        }


        public void Terror()
        {
            Brain.SpeakYourMind(" using Terror");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "Terror", 5968);
        }


        public void PlagueAura()
        {
            Brain.SpeakYourMind(" using PlagueAura");
            Brain.SimpleCast(Owner, Combat.CurrentTarget, "PlagueAura", 13660);
        }

        /// <summary>
        /// Aslong as the Banner of Bloodlust i s up,Borzhar will charge a t a new target
        /// and should s tay locked on the target for a medium duration before charging
        /// at a new target. Players can destroy the banner which will prevent him f rom
        /// using this charge anymore.
        /// </summary>
        public void DeployBannerOfBloodlust()
        {
            Brain.SpeakYourMind(" using DeployBannerOfBloodlust");

            GameObject_proto proto = GameObjectService.GetGameObjectProto(3100412);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldX = Owner.WorldPosition.X + StaticRandom.Instance.Next(50),
                WorldY = Owner.WorldPosition.Y + StaticRandom.Instance.Next(50),
                WorldZ = Owner.WorldPosition.Z,
                WorldO = Owner.Heading,
                ZoneId = Owner.Zone.ZoneId
            };

            spawn.BuildFromProto(proto);
            proto.IsAttackable = 1;

            var go = Owner.Region.CreateGameObject(spawn);
            go.EvtInterface.AddEventNotify(EventName.OnDie, RemoveGOs);

        }

        private bool RemoveGOs(Object obj, object args)
        {
            GameObject go = obj as GameObject;
            go.EvtInterface.AddEvent(go.Destroy, 2 * 1000, 1);
            return false;
        }


        /// <summary>
        /// Aslong as the Banner of the Bloodherdisup, Bloodherd Gors willrally to
        /// Borzhar’s side. To stopthe reinforcement, players must destroy the Banner of
        /// the Bloodherd.
        /// </summary>
        public void BannerOfTheBloodHerd()
        {
            // If the Banner exists within 150 feet, allow spawn adds
            var creatures = Owner.GetInRange<GameObject>(150);
            foreach (var creature in creatures)
            {
                if (creature.Entry == 3100412)
                {
                    SpawnAdds();
                    break;
                }
            }
        }

        public void SpawnAdds()
        {
            if (Owner is Boss)
            {
                var adds = (Owner as Boss).AddDictionary;

                foreach (var entry in adds)
                {
                    Spawn(entry);
                }

                // Force zones to update
                Owner.Region.Update();
            }
        }

        private void Spawn(BossSpawn entry)
        {
            ushort facing = 2093;

            var X = Owner.WorldPosition.X;
            var Y = Owner.WorldPosition.Y;
            var Z = Owner.WorldPosition.Z;


            var spawn = new Creature_spawn { Guid = (uint)CreatureService.GenerateCreatureSpawnGUID() };
            var proto = CreatureService.GetCreatureProto(entry.ProtoId);
            if (proto == null)
                return;
            spawn.BuildFromProto(proto);

            spawn.WorldO = facing;
            spawn.WorldX = X + StaticRandom.Instance.Next(500);
            spawn.WorldY = Y + StaticRandom.Instance.Next(500);
            spawn.WorldZ = Z;
            spawn.ZoneId = (ushort)Owner.ZoneId;


            var creature = Owner.Region.CreateCreature(spawn);
            creature.EvtInterface.AddEventNotify(EventName.OnDie, RemoveNPC);
            entry.Creature = creature;
            (Owner as Boss).SpawnDictionary.Add(entry);

            if (entry.Type == BrainType.AggressiveBrain)
                creature.AiInterface.SetBrain(new AggressiveBrain(creature));
            if (entry.Type == BrainType.HealerBrain)
                creature.AiInterface.SetBrain(new HealerBrain(creature));
            if (entry.Type == BrainType.PassiveBrain)
                creature.AiInterface.SetBrain(new PassiveBrain(creature));

        }

        private bool RemoveNPC(Object obj, object args)
        {
            Creature c = obj as Creature;
            if (c != null) c.EvtInterface.AddEvent(c.Destroy, 20000, 1);

            return false;
        }


    }
}
