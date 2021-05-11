using System;
using Common;


namespace WorldServer.World.Abilities.MythicAbilities
{
    public class AbilityCalculations
    {
        //Ability ability = new Ability();
        //AbilityComponent abilityComponent = new AbilityComponent();
        //AbilityComponentXComponent abilityComponentX = new AbilityComponentXComponent();
        //AbilityExpression abilityExpression = new AbilityExpression();
        //AbilityKnockbackInfo abilityKnockbackInfo = new AbilityKnockbackInfo();
        //AbilityLine abilityLine = new AbilityLine();
        //AbilityLineName abilityLineName = new AbilityLineName();

        public AbilityCalculations()
        { }

        public AbilityCalculations(AbilityComponent dbObj)
        {

        }

        public AbilityCalculations(Ability dbObj)
        {

        }

        // Shmerrick
        //WorldServer.World.Abilities.Components.AbilityInfo _AbilityInfo = new WorldServer.World.Abilities.Components.AbilityInfo();
        //int Level = _AbilityInfo.Level;
        //float CalculatedValue = ((((Level - 1.0f) * (1 / 6) * Values) + Values) * /*MultipliersOne*/) / 100;
        //Console.WriteLine(CalculatedValue);

        /* Londo
        float baseValue = Value;
        float targetLevel = abilityLevel;
        float mult = Multiplier;

        float result = ((abilityLevel - 1) * increasePerLevel) * baseValue;
        int val = (int)Math.Round(result, MidpointRounding.AwayFromZero);
        val += (int) baseValue;
        val = val* (int) mult / 100;

        return val;
        */
    }
}