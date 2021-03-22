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
            ushort ID = dbObj.ID;
            string A00 = dbObj.A00;
            string Values = dbObj.Values;
            string Multipliers = dbObj.Multipliers;
            ushort ActivationDelay = dbObj.ActivationDelay;
            ushort Duration = dbObj.Duration;
            ushort Flags = dbObj.Flags;
            ushort IconAlwaysVisible = dbObj.IconAlwaysVisible;
            ushort Operation = dbObj.Operation;
            ushort Interval = dbObj.Interval;
            ushort Radius = dbObj.Radius;
            ushort ConeAngle = dbObj.ConeAngle;
            ushort FlightSpeed = dbObj.FlightSpeed;
            ushort A15 = dbObj.A15;
            ushort MaxTargets = dbObj.MaxTargets;
            ushort Description = dbObj.Description;
        }

        public AbilityCalculations(Ability dbObj)
        {
            ushort ID = dbObj.ID;
            string Name = dbObj.Name;
            string Description = dbObj.Description;
            ushort Castime = dbObj.Castime;
            ushort Cooldown = dbObj.Cooldown;
            /*
             *
            ushort dbObj.TacticType;
            ushort dbObj.AbilityType;
            ushort dbObj.A20;
            ushort dbObj.A24;
            ushort dbObj.CareerLine;
            ushort dbObj.A32;
            ushort dbObj.Flags;
            ushort dbObj.EffectID;
            ushort dbObj.A44;
            ushort dbObj.Range;
            ushort dbObj.Angle;
            ushort dbObj.MoraleCost;
            ushort dbObj.ChannelInterval;
            ushort dbObj.A54;
            ushort dbObj.ScaleStatMult;
            ushort dbObj.NumTacticSlots;
            ushort dbObj.AP;
            ushort dbObj.A61;
            ushort dbObj.A62;
            ushort dbObj.A63;
            ushort dbObj.AbilityImprovementThreshold;
            ushort dbObj.Specialization;
            ushort dbObj.StanceOrder;
            ushort dbObj.A68;
            ushort dbObj.MinLevel;
            ushort dbObj.A70;
            ushort dbObj.A71;
            ushort dbObj.A132;
            ushort dbObj.A136;
            ushort dbObj.A140;
            ushort dbObj.A142C;
            ushort dbObj.Disabled;
            ushort dbObj.CanClickOff;
            ushort dbObj.ScaleStat;
            ushort dbObj.CantCrit;
            ushort dbObj.MoraleLevel;
            ushort dbObj.AttackType;
            ushort dbObj.CounterAmount;
            ushort dbObj.RangeMin;
            ushort dbObj.EnemyTargetIgnoreLOS;
            ushort dbObj.FriendlyTargetIgnoreLOS;
            ushort dbObj.RangeMax;
            ushort dbObj.UniqueGroup;
            ushort dbObj.Faction;
            ushort dbObj.UsableWithBuff;
            ushort dbObj.AnimationDelay;
            ushort dbObj.BinIndex;
            ushort dbObj.SpellDamageType
            ushort dbObj.Toggle
            ushort dbObj.ToggleGroup
            ushort dbObj.PatcherFileID
            ushort dbObj.CreateBinData
           */
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