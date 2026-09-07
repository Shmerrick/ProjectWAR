using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Objects;
using WorldServer.World.AI;
using WorldServer.World.Interfaces;

// Compiled server behavior checks. No database or live gameplay claims.
internal static class FourSystemsChecks
{
    private static int Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        try { Run(); return 0; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        CheckAbilityIsolation();
        CheckHealthCycles();
        CheckPetOwnership();
        CheckDelayedGuards();
        CheckNpcScheduling();
        Console.WriteLine("PASS: NPC instance isolation, independent health gates, vanity source ownership and repeated cleanup.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class CastProbeBrain : InstanceBossBrain
    {
        public bool Accept;
        public int Attempts;
        public CastProbeBrain(Unit owner) : base(owner) { }
        public void Queue(NPCAbility ability) { QueueNpcCast(ability); }
        public void Cancel() { CancelPendingNpcCast(); }
        protected override bool TryStartNpcCast(ushort ability) { Attempts++; return Accept; }
    }

    private static void CheckNpcScheduling()
    {
        var caster = new Creature { MaxHealth = 100, Health = 20 };
        var target = new Creature { MaxHealth = 100, Health = 100 };
        typeof(CombatInterface).GetProperty("IsInCombat").SetValue(caster.CbtInterface, true, null);
        typeof(CombatInterface_Npc).GetProperty("CurrentTarget").SetValue(caster.CbtInterface, target, null);
        var brain = new CastProbeBrain(caster);
        var ability = new NPCAbility(123, 100, 15, true, "", 0, 50, 0);
        var field = typeof(ABrain).GetField("_pendingNpcCast", BindingFlags.Instance | BindingFlags.NonPublic);
        var execute = typeof(ABrain).GetMethod("ExecutePendingNpcCast", BindingFlags.Instance | BindingFlags.NonPublic);
        brain.Queue(ability);
        object first = field.GetValue(brain);
        brain.Queue(ability);
        Assert(ReferenceEquals(first, field.GetValue(brain)), "Duplicate cast was queued");
        execute.Invoke(brain, new[] { first });
        Assert(brain.Attempts == 1 && ability.AbilityUsed == 0 && ability.CooldownEnd == 0, "Rejected cast consumed one-shot or cooldown");
        brain.Queue(ability);
        object cancelled = field.GetValue(brain);
        brain.Cancel();
        brain.Queue(ability);
        object current = field.GetValue(brain);
        execute.Invoke(brain, new[] { cancelled });
        Assert(brain.Attempts == 1 && ReferenceEquals(current, field.GetValue(brain)), "Old callback crossed combat reset");
        brain.Accept = true;
        execute.Invoke(brain, new[] { current });
        Assert(brain.Attempts == 2 && ability.AbilityUsed == 1 && ability.CooldownEnd > 0, "Accepted cast did not consume one-shot/cooldown");
        execute.Invoke(brain, new[] { current });
        Assert(brain.Attempts == 2, "Callback executed twice");
        ability.AbilityUsed = 0;
        ability.CooldownEnd = 0;
        brain.Queue(ability);
        caster.Health = 90;
        execute.Invoke(brain, new[] { field.GetValue(brain) });
        Assert(brain.Attempts == 2 && ability.AbilityUsed == 0, "Healed caster executed obsolete health phase");
    }

    private static void CheckAbilityIsolation()
    {
        var definition = new NPCAbility(123, 90, 15, true, "test", 3, 50, 1, 1, 1, 1, 2, 10, 5);
        definition.AbilityUsed = 1;
        definition.CooldownEnd = 123456;
        AbilityMgr.CreatureAbilities[uint.MaxValue] = new List<NPCAbility> { definition };
        try
        {
            var first = AbilityMgr.GetCreatureAbilities(uint.MaxValue);
            var second = AbilityMgr.GetCreatureAbilities(uint.MaxValue);
            Assert(!ReferenceEquals(first, second) && !ReferenceEquals(first[0], second[0]), "Creatures share mutable ability state");
            foreach (FieldInfo field in typeof(NPCAbility).GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (field.Name != "AbilityUsed" && field.Name != "CooldownEnd")
                    Assert(Equals(field.GetValue(definition), field.GetValue(first[0])), "Definition field lost: " + field.Name);
            Assert(first[0].AbilityUsed == 0 && first[0].CooldownEnd == 0, "Cached runtime state leaked into spawn");
            first[0].AbilityUsed = 1;
            first[0].CooldownEnd = 900;
            Assert(second[0].AbilityUsed == 0 && second[0].CooldownEnd == 0, "Another creature inherited cast state");
            first.Clear();
            Assert(second.Count == 1 && AbilityMgr.GetCreatureAbilities(uint.MaxValue).Count == 1, "Disposal cleared another creature or cache");
        }
        finally { AbilityMgr.CreatureAbilities.Remove(uint.MaxValue); }
    }

    private static void CheckHealthCycles()
    {
        var high = new NPCAbility(1, 0, 0, true, "", 0, 75);
        var low = new NPCAbility(2, 0, 0, true, "", 0, 25);
        foreach (var order in new[] { new[] { high, low }, new[] { low, high } })
            foreach (var ability in order)
                Assert(ability.IsHealthCycleActive(50, 100) == (ability == high), "Health gate depends on preceding ability");
        Assert(!low.IsHealthCycleActive(25, 100) && low.IsHealthCycleActive(24, 100), "Threshold boundary changed");
        low.AbilityCycle = 0;
        Assert(!low.IsHealthCycleActive(1, 100), "One-shot admitted as recurring cast");
        high.ActivateAtHealthPercent = 100;
        Assert(high.IsHealthCycleActive(uint.MaxValue - 1, uint.MaxValue), "Large health multiplication overflowed");
    }

    private static Pet FixturePet(Player owner, NewBuff source)
    {
        var pet = (Pet)FormatterServices.GetUninitializedObject(typeof(Pet));
        typeof(Pet).GetField("<Owner>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pet, owner);
        typeof(Pet).GetField("_vanitySource", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pet, source);
        pet.IsVanity = true;
        return pet;
    }

    private static void CheckPetOwnership()
    {
        var owner = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var other = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var oldBuff = new NewBuff();
        var newBuff = new NewBuff();
        var otherBuff = new NewBuff();
        var oldPet = FixturePet(owner, oldBuff);
        var replacement = FixturePet(owner, newBuff);
        var otherPet = FixturePet(other, otherBuff);
        owner.Companion = replacement;
        other.Companion = otherPet;
        replacement.RemoveVanityPet(oldBuff);
        Assert(!replacement.PendingDisposal && owner.Companion == replacement, "Old callback removed replacement");
        oldPet.RemoveVanityPet();
        oldPet.RemoveVanityPet();
        Assert(oldBuff.BuffHasExpired && owner.Companion == replacement && !newBuff.BuffHasExpired, "Old cleanup changed replacement source");
        replacement.RemoveVanityPet(newBuff);
        Assert(replacement.PendingDisposal && owner.Companion == null && newBuff.BuffHasExpired, "Source removal did not clear pet");
        Assert(other.Companion == otherPet && !otherBuff.BuffHasExpired && !otherPet.PendingDisposal, "Another owner's pet changed");
        otherPet.Destroy();
        Assert(other.Companion == null && otherBuff.BuffHasExpired, "Destroy left summon source active");
        otherPet.OnLoad();
        Assert(otherPet.PendingDisposal && other.Companion == null, "Queued load revived dismissed pet");
        var expired = new NewBuff { BuffHasExpired = true };
        var expiredPet = FixturePet(other, expired);
        other.Companion = expiredPet;
        expiredPet.OnLoad();
        Assert(expiredPet.PendingDisposal && other.Companion == null, "Expired summon loaded after queued add");
        foreach (byte state in new byte[] { 4, 5 })
        {
            var source = new NewBuff();
            typeof(NewBuff).GetProperty("Target").SetValue(source, owner, null);
            typeof(NewBuff).GetProperty("BuffState").SetValue(source, state, null);
            var pet = FixturePet(owner, source);
            owner.Companion = pet;
            var command = typeof(BuffEffectInvoker).GetMethod("SummonVanityPet", BindingFlags.Static | BindingFlags.NonPublic);
            Assert((bool)command.Invoke(null, new object[] { source, null, owner }), "Summon end callback rejected");
            Assert(owner.Companion == null && pet.PendingDisposal, "Expiry/removal left companion alive");
        }
    }

    private static void CheckDelayedGuards()
    {
        var brain = (ABrain)FormatterServices.GetUninitializedObject(typeof(InstanceBossBrain));
        brain.StartDelayedCast(null);
        brain.StartDelayedCast(new List<object>());
        brain.StartDelayedCast(new List<object> { null, "invalid" });
        brain.StartDelayedCast(new List<object> { null, (ushort)1 });
        var owner = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
        var pet = FixturePet(owner, new NewBuff());
        typeof(ABrain).GetField("_unit", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(brain, pet);
        pet.Destroy();
        brain.StartDelayedCast(new List<object> { pet, (ushort)1 });
        brain.DelayedChase();
        Assert(pet.PendingDisposal, "Delayed callback revived disposed caster");
    }
}
