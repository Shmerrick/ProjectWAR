using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.AI.Abilities
{
    public class Conditions
    {
        // Melee range for the boss - could use baseradius perhaps?
        public static int BOSS_MELEE_RANGE = 25;
        public static int NPC_MELEE_RANGE = 10;

        public Conditions(Unit owner, CombatInterface_Npc combat)
        {
            Owner = owner;
            Combat = combat;
        }

        public Unit Owner { get; }
        public CombatInterface_Npc Combat { get; }


        public bool PlayersWithinRange()
        {
            if (Owner != null)
            {
                var players = Owner.GetPlayersInRange(30, false);
                if (players == null)
                    return false;
                return true;
            }

            return false;
        }

        public bool TargetInMeleeRange()
        {
            if (Combat.HasTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY))
            {
                if (Owner.GetDistanceToObject(Owner.CbtInterface.GetCurrentTarget()) < BOSS_MELEE_RANGE
                ) // In melee range
                    return true;
                return false;
            }

            return false;
        }

        public bool PlayerInMeleeRange()
        {
            if (!Combat.HasTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY)) return false;
            if (Combat.CurrentTarget is Player)
            {
                return Owner.GetDistanceToObject(Owner.CbtInterface.GetCurrentTarget()) < NPC_MELEE_RANGE;
            }
            else
            {
                return false;
            }
        }

        public bool HasBlessing()
        {
            if (Combat.HasTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY))
            {
                if (Owner.GetDistanceToObject(Owner.CbtInterface.GetCurrentTarget()) < BOSS_MELEE_RANGE
                ) // In melee range
                {
                    var blessing = Combat.CurrentTarget.BuffInterface.HasBuffOfType((byte)BuffTypes.Blessing);
                    return blessing;
                }

                return false;
            }

            return false;
        }

        public bool CanKnockDownTarget()
        {
            if (!TargetInMeleeRange())
                return false;
            if (TargetIsUnstoppable())
                return false;

            return true;
        }

        public bool CanPuntTarget()
        {
            if (!TargetInMeleeRange())
                return false;
            if (TargetIsUnstoppable())
                return false;

            return true;
        }

        public bool TwentyPercentHealth()
        {
            if (Owner.PctHealth <= 20)
                return true;
            return false;
        }

        public bool FourtyNinePercentHealth()
        {
            if (Owner.PctHealth <= 49)
                return true;
            return false;
        }

        public bool SeventyFivePercentHealth()
        {
            if (Owner.PctHealth <= 74)
                return true;
            return false;
        }

        public bool NinetyNinePercentHealth()
        {
            if (Owner.PctHealth <= 99)
                return true;
            return false;
        }

        public bool TargetIsUnstoppable()
        {
            var buff = Combat.CurrentTarget.BuffInterface.GetBuff((ushort)GameBuffs.Unstoppable, Combat.CurrentTarget);
            return buff != null;
        }
    }
}