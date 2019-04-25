namespace WorldServer.World.Objects
{
    public class NPCAbility
    {
        public ushort Entry;
        public byte MinRange;
        public ushort Range;
        public ushort Cooldown;
        public bool AutoUse;
        public string Text;
        public uint TimeStart;
        public byte ActivateAtHealthPercent;
        public byte DisableAtHealthPercent;
        public byte AbilityCycle;
        public byte AbilityUsed;
        public byte Active;
        public byte ActivateOnCombatStart;
        public byte RandomTarget;
        public byte TargetFocus;
        public long CooldownEnd;

        public NPCAbility(ushort entry, ushort range, ushort cooldown, bool autoUse, string text, uint timestart = 0, byte percent = 0, byte abilitycycle = 1, byte active = 1, byte activateoncombatstart = 0, byte randomtarget = 0, byte targetFocus = 0, byte disablepercent = 0, byte minrange = 0)
        {
            Entry = entry;
            MinRange = minrange;
            Range = range;
            Cooldown = cooldown;
            AutoUse = autoUse;
            Text = text;
            TimeStart = timestart;
            ActivateAtHealthPercent = percent;
            DisableAtHealthPercent = disablepercent;
            AbilityCycle = abilitycycle;
            AbilityUsed = 0;
            Active = active;
            ActivateOnCombatStart = activateoncombatstart;
            RandomTarget = randomtarget;
            TargetFocus = targetFocus;
        }
    }
}