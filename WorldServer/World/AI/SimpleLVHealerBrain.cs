using System.Collections.Generic;
using System.Linq;

namespace WorldServer.World.AI
{
    public class SimpleLVHealerBrain : ABrain
    {
        #region Constructors

        public SimpleLVHealerBrain(Unit unit) : base(unit)
        {
            _unit.AbtInterface.NPCAbilities = new List<NPCAbility>()
            {
                new NPCAbility(13682, 60, 0, true, "<character name>, be restored by our chaos gods!", 0, 100, 1, 1, 1, 0, 0, 0, 0)
            };
        }

        #endregion Constructors

        #region Members

        #endregion Members

        #region Methods
        
        public override Unit GetNextTarget()
        {
            return base.GetNextTarget();
        }
        
        public override void Think()
        {
            base.Think();
        }

        public override void TryUseAbilities()
        {
            base.TryUseAbilities();

            //Unit boss = (Unit)_unit.ObjectsInRange.Where(x => x is InstanceBossSpawn).FirstOrDefault();

            //foreach (var ability in _unit.AbtInterface.NPCAbilities)
            //{
            //    _unit.CbtInterface.SetTarget(boss.Oid, GameData.TargetTypes.TARGETTYPES_TARGET_ALLY);
            //    AbilityMgr.GetAbilityInfo(ability.Entry).Target = boss;

            //    // This list of parameters is passed to the function that delays the cast by 1000 ms
            //    var prms = new List<object>() { _unit, ability.Entry, ability.RandomTarget };
            //    if (ability.Text != "") _unit.Say(ability.Text.Replace("<character name>", _unit.CbtInterface.GetCurrentTarget().Name));
            //    _unit.EvtInterface.AddEvent(StartDelayedCast, 1000, 1, prms);
            //    ability.AbilityUsed = 1;
            //}
        }

        #endregion Methods
    }
}
