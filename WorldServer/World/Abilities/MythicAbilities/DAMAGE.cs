/* Londo
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WarGameServer.Game;
using WarGameServer.Protocol;
using WarServer.Game.Ability.Stats;
using WarServer.Game.Entities;
using WarServer.Services.Combat;
using WarShared;
using WarShared.Data;


namespace WarServer.Game.Ability.Ext.Components
{
    public enum DamageFlag
    {
        NONE = 0,
        UNMITIGATABLE = 1
    }

    [ComponentInfo(WarShared.ComponentOperationType.DAMAGE)]
    public class DAMAGE : AbilityComponentBase
    {
        public int Increment { get { return Values[0]; } }
        public int MaxCounter { get { return Values[1]; } }
        public DamageFlag Type { get => (DamageFlag)Flags; }
        public override bool SendToClient => true;
        public DAMAGE(CombatService svc, AbilityComponentXComponent ac) : base(svc, ac)
        {
        }

        protected override async Task<bool> InitializeInternal(ComponentData data, CastToken triggeredBy)
        {
            if ((data.Target.Realm != data.Caster.Realm || data.Target.InCombat || data.Target is Monster) && data.Target != data.Caster)
            {
                await data.Caster.EnterCombat();
                await data.Target.EnterCombat();
            }
            return true;
        }

        public long DamageValue { get { return (long)Values[0]; } }

        private Task<bool> TryDefend(CombatService svc, Entity target, CastToken token, Buff buff, int index, int tick, int totalTicks)
        {

            //token.Result = AbilityResult.DISRUPT;

            //await svc.RemoveBuff(buff);
            //await svc.RaiseDefended(token, target, index);
            //svc.Log($"  DAMAGE '{token.Ability.Name}' Target:{target.Name} Result:{token.Result}", ConsoleColor.Red);
            //await target.Controller.Broadcast(target, new F_CAST_PLAYER_EFFECT()
            //{
            //    abilityID = (ushort)token.Ability.ID,
            //    targetID = target?.ObjectID ?? 0,
            //    casterID = token.Caster?.ObjectID ?? 0,
            //    ComponentIndex = (byte)index,
            //    result = token.Result,
            //    showEffect = true 
            //});

            //return true;
            return Task.FromResult(false);
        }

        public override float CalculateBaseValue(ComponentData data)
        {
            if ((data.Component.ValueType == ComponentValueType.ReducePct 
                || data.Component.ValueType == ComponentValueType.ReduceToPct))
                return Value;

            float increasePerLevel = IncreasePerLevel;


            int intr = 1;

            if (Interval > 0)
                intr = Duration / Interval;

            float abilityLevel = data.AbilityLevel;

            if (data.Token.ItemSetLevel > 0)
            {
                increasePerLevel = 1.0f;
                abilityLevel = data.Token.ItemSetLevel;
            }

            if (data.Token.ItemLevel > 0)
            {
                increasePerLevel = 1.0f;
                abilityLevel = data.Token.ItemLevel;
            }

            float baseValue = Value;
            float targetLevel = abilityLevel;
            float mult = Multiplier;

            int interval = 1;

            if (Duration > 0 && Interval > 0)
                interval = Duration / Interval;

            float result = ((abilityLevel - 1) * increasePerLevel) * baseValue;
            int val = (int)Math.Round(result, MidpointRounding.AwayFromZero);
            val += (int)baseValue;
            val = val * (int)mult / 100;

            return val;
        }

        private Task<int> TryMitigate(ref float amount, CombatService svc, Entity target, CastToken token, Buff buff, int index, int tick, int totalTicks)
        {
            return Task.FromResult(0);
        }

        private Task<int> TryAbsorb(ref float amount, CombatService svc, Entity target, CastToken token, Buff buff, int index, int tick, int totalTicks)
        {
            return Task.FromResult(0);
        }

        public override Task<bool> IsImmuneInternal(IMMUNITY immunity, ComponentData data)
        {
            return Task.FromResult(immunity.Value == 1);
        }

        protected override async Task<bool> ApplyInternal(ComponentData data, CastToken triggeredBy, int tick, int totalTicks)
        {
            var target = data.Target;
            bool defended = false;
            float damage = 0;
            float mitigated = 0;
            var spell = data.Spell;
            bool send = true;
            float absorbed = 0;
            List<int> values = new List<int>();
            bool showEffect = !data.Token.Disposed;

            if (data.Ability.ChannelInterval > 0 && data.Token.AP > 0)
            {
                if (data.Caster.AP < data.Token.AP)
                {
                    await Svc.Interrupt(data.Token, data.Caster, AbilityResult.NOT_ENOUGH_ACTION_POINTS);
                    return false;
                }
                await data.Caster.AdjustAP(data, -data.Token.AP);
            }

            await data.Token.Caster.Debug($"DAMAGE Tick {tick}:{totalTicks}");

            //first tick of dot is skipped (unless overwritten). Check for defend on initial apply of damage
            if (((tick == 1 && data.Token.Ability.ChannelInterval == 0) && totalTicks > 1 && !ApplyFirstTick) || (tick == totalTicks && totalTicks > 1 && ApplyFirstTick))
            {
                if (await TryDefend(Svc, target, data.Token, data.Buff, 0, 0, 0))
                {
                    return false;
                }
                send = false;
            }
            else
            {
                if (Type == DamageFlag.UNMITIGATABLE)
                {
                    damage = spell.DamageMagical;
                }
                else
                {
                    if (totalTicks == 0)
                    {
                        if (await TryDefend(Svc, target, data.Token, data.Buff, 0, 0, 0))
                        {
                            return false;
                        }
                    }
                    if (data.Caster != data.Target)
                        spell.TargetStats = await target.Buffs.StatSumCombat(triggeredBy);
                    else
                        spell.TargetStats = spell.CasterStats;

                    damage = spell.GetResultMagicDamage();
                    data.Token.Result = AbilityResult.OK;
                    if (spell.Critical)
                        data.Token.Result = AbilityResult.CRITICAL;

                    absorbed = await TryAbsorb(ref damage, Svc, target, data.Token, data.Buff, 0, 0, 0);
                    mitigated = await TryMitigate(ref damage, Svc, target, data.Token, data.Buff, 0, 0, 0);
                }
            }

            AbilityResult result = data.Token.Result;

            if (send)
            {

                bool immune = await IsImmune(data, triggeredBy);

                if (!defended && !immune)
                {
                    values.Add((int)Math.Round(-damage, MidpointRounding.AwayFromZero));

                    if (damage != 0)
                        await target.AdjustHealth(data, -damage);

                    if (mitigated != 0)
                        values.Add((int)Math.Round(mitigated, MidpointRounding.AwayFromZero));

                    if (absorbed != 0)
                        values.Add((int)Math.Round(absorbed, MidpointRounding.AwayFromZero));
                }
                else if (immune)
                {
                    result = AbilityResult.IMMUNE;
                }
                Log.Info($"    APPLY DAMAGE {data.Ability.Name}:{damage} tick:{tick}/{totalTicks}", CombatService.DAMAGE);

                await data.Target.ApplyEffect(data, result, values);

                await Svc.EventSvc.Raise(EventType.ON_HIT, triggeredBy, data.Caster, data);
                await Svc.EventSvc.Raise(EventType.ON_BEING_HIT, triggeredBy, data.Target, data);


                return true;
            }
            return false;

        }
    }
}
*/