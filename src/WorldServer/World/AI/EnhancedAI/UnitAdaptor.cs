using System.Collections.Generic;
using WorldServer.World.AI.PathFinding.AStar;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.AI.EnhancedAI
{
    public class UnitAdaptor : IUnitAdaptor
    {
        public Unit SourceUnit { get; set; }

        public List<Unit> GetTargetsInRange(int range)
        {
            return SourceUnit.GetInRange<Unit>(range);
        }

        public bool IsInCombat()
        {
            return SourceUnit.CbtInterface.IsInCombat;
        }

        public bool IsObjectInFront(Unit unit, float viewangle, byte sightRange)
        {
            return unit.IsObjectInFront(unit, viewangle, sightRange);
        }

        public bool CanAttack(EnhancedCreature enhancedCreature, Unit unit)
        {
            return CombatInterface.CanAttack(enhancedCreature, unit);
        }

        public bool IsEnemy(EnhancedCreature enhancedCreature, Unit unit)
        {
            return CombatInterface.IsEnemy(enhancedCreature, unit);
        }

        public int Get2DDistanceToObject(EnhancedCreature enhancedCreature, Unit unit)
        {
            return unit.Get2DDistanceToObject(enhancedCreature);
        }

        public int Get2DDistanceToPoint(Point point, Unit unit)
        {
            return unit.Get2DDistanceToWorldPoint(new Point2D(point.X, point.Y));
        }

        public CombatInterface_Npc CbtInterface(Unit unit)
        {
            return (unit.CbtInterface as CombatInterface_Npc);
        }

        public bool IsInCastRange(Unit unit,Unit combatTarget, uint baseRadius)
        {
            return unit.IsInCastRange(combatTarget, baseRadius);
        }
    }

    public interface IUnitAdaptor
    {
        List<Unit> GetTargetsInRange(int range);
        bool IsInCombat();
        bool IsObjectInFront(Unit unit, float viewangle, byte sightRange);
        bool CanAttack(EnhancedCreature enhancedCreature, Unit unit);
        bool IsEnemy(EnhancedCreature enhancedCreature, Unit unit);
        int Get2DDistanceToObject(EnhancedCreature enhancedCreature, Unit unit);
        int Get2DDistanceToPoint(Point point, Unit unit);
        CombatInterface_Npc CbtInterface(Unit unit);

        bool IsInCastRange(Unit unit, Unit combatTarget, uint baseRadius);
    }
}