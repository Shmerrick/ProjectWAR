using System;

namespace WorldServer.World.MythicAbility
{
    public class AbilityComponentVariables
    {
        public ushort ID;
        public string A00;
        public string Values;
        public string Multipliers;
        public ushort ActivationDelay;
        public ushort Duration;
        public ushort Flags;
        public ushort IconAlwaysVisible;
        public ushort Operation;
        public ushort Interval;
        public ushort Radius;
        public ushort ConeAngle;
        public ushort FlightSpeed;
        public ushort A15;
        public ushort MaxTargets;
        public ushort Description;

        public AbilityComponentVariables() // Trying to replicate client damage formula
        {
            WorldServer.World.Abilities.Components.AbilityInfo _AbilityInfo = new WorldServer.World.Abilities.Components.AbilityInfo();
            int Level = _AbilityInfo.Level;

            //float CalculatedValue = ((((Level - 1.0f) * (1 / 6) * Values) + Values) * /*MultipliersOne*/) / 100;

            //Console.WriteLine(CalculatedValue);
            Console.ReadKey();
        }

        /*
        public ItemInfoStats(string stat, IEnumerable<ItemBonus> itemBonusList)
        {
            var temp = stat.Split(':');
            Type = Convert.ToInt32(temp[0]);
            Value = Convert.ToInt32(temp[1]);
            Description = itemBonusList.Single(x => x.Entry == Convert.ToInt32(Type)).BonusName;
        }
        */
    }

    /*
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