/* Londo
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarGameServer.Game.Ability;
using WarGameServer.Protocol;
using WarServer;
using WarServer.Game.Ability;
using WarServer.Game.Ability.Ext.Components;
using WarServer.Game.Entities;
using WarServer.Services.Combat;
using WarServer.Services.Event;
using WarShared;
using WarShared.Data;


namespace WarGameServer.Game
{
    public class AbilityEngine
    {
        private CombatService Svc;

        public AbilityEngine(CombatService svc)
        {
            Svc = svc;
        }

        public static async Task<AbilityResult> RequirmentCheck(CastToken token)
        {
            var ccList = await token.Caster.Buffs.GetComponentsByType<CC>();
            foreach (var cc in ccList)
            {
                var result = ((CC)cc.Component).CanCast(token.Ability);
                if (result != AbilityResult.OK)
                    return result;
            }

            if (token.AP > 0 && token.Caster.AP < token.AP)
                return AbilityResult.NOT_ENOUGH_ACTION_POINTS;

            if (token.Ability.MoraleCost > 0 && token.Caster.Morale < token.Ability.MoraleCost)
                return AbilityResult.NOT_ENOUGH_MORALE;

            if (token.Ability.TargetType == TargetType.ENEMY && token.EnemyTarget == null)
                return AbilityResult.INVALID_TARGET;

            if (token.Ability.TargetType == TargetType.GTAE && token.GTAELocation == null)
                return AbilityResult.INVALID_TARGET;

            if (token.Ability.RangeMax > 0 && token.Ability.TargetType == TargetType.ENEMY && token.Caster.Distance(token.EnemyTarget) > token.Ability.RangeMax + 120)
                return AbilityResult.TARGET_IS_OUT_OF_RANGE;

            if (token.Ability.RangeMax > 0 && token.Ability.TargetType == TargetType.FRIENDLY_TARGET && token.Caster.Distance(token.FriendlyTarget) > token.Ability.RangeMax + 120)
                return AbilityResult.TARGET_IS_OUT_OF_RANGE;

            if (!token.EnemyVisible && token.Ability.TargetType == TargetType.ENEMY && !token.Ability.EnemyTargetIgnoreLOS)
                return AbilityResult.NOT_VISIBLE;

            if (!token.FriendlyVisible && token.Ability.TargetType == TargetType.FRIENDLY_TARGET && !token.Ability.FriendlyTargetIgnoreLOS)
                return AbilityResult.NOT_VISIBLE;

            if (token.Ability.RangeMax > 0 && token.Ability.TargetType == TargetType.ENEMY && Util.FOVCheck((double)(token.Ability.RangeMax + 120), 180,
                (float)token.Caster.Heading,
                token.Caster.X, token.Caster.Y,
                (float)token.EnemyTarget.X, (float)token.EnemyTarget.Y))
                return AbilityResult.TARGET_OUTSIDE_CONE_OF_FIRE;

            return AbilityResult.FAILED;
        }

        public async Task Cast(CastToken token)
        {
            Log.Info($"Casting ability {token.Ability} by {token.Caster}", ConsoleColor.DarkGray);

            Entity target = null;
            bool existing = false;

            if (token.Target == null)
                target = token.SelectTarget();
            else
                target = token.Target;

            //get existing buffs this ability applies
            token.CastBuff = (await target.Buffs.GetBuffsByAbility(token.Ability, token.Caster.ID)).FirstOrDefault();

            //raise use ability event
            await RaiseComponentEvent(token.Caster, token, AbilityEvent.OnUseAbility);

            //ability hasnt been applied, create new buff
            if (token.CastBuff == null)
            {
                token.CastBuff = await Buff.Create(Svc, token);
            }
            else
            {
                //ability already been applied, reset buff timer
                existing = true;
                await token.CastBuff.Reset(Svc);
            }

            //loop over components, if fire requirments are met, activate
            for (byte i = 0; i < 10; i++)
            {
                if (!token.Ability.Components.ContainsKey(i))
                    continue;

                if (existing && !token.CastBuff.Components.ContainsKey(i))
                    continue;

                if (token.Ability.Components[i].Disabled)
                    continue;

                //ask component on whom it should be applied
                token.CastBuff.Components[i].Target = token.CastBuff.Components[i].Component.SelectTarget(token.CastBuff.Components[i]);

                //ask component if fire requirments are met
                token.CastBuff.Components[i].CastRequirmentsMet = await CheckComponentRequirments(token.CastBuff.Components[i], token, AbilitySourceType.CAST);

                //if component should be activation on use
                if (token.Ability.Components[i].Trigger == AbilityTrigger.OnApply && token.Ability.Components[i].Component.Operation != ComponentOperationType.EVENT_LISTENER)
                {
                    if (token.CastBuff.Components[i].CastRequirmentsMet)
                    {
                        //if can fire, activate component logic
                        await ApplyComponent(token.CastBuff.Components[i], token, true, AbilitySourceType.APPLY);
                    }
                }
            }
        }

        public Task<bool> CheckComponentRequirments(ComponentData data, CastToken triggeredBy, params AbilitySourceType[] type)
        {
            if (!data.Component.Expressions.Keys.Any(e => type.Contains(e)))
                return Task.FromResult(true);

            return EvaluateMultipleExp(data, triggeredBy,
                data.Component.Expressions
                .Where(e => type.Contains(e.Key))
                .SelectMany(e => e.Value).ToList());
        }

        public async Task ApplyComponent(ComponentData data, CastToken triggeredBy, bool propogate = true, params AbilitySourceType[] types)
        {
            if (data.Target == null)
                data.Target = data.Component.SelectTarget(data);


            bool canApply = await CheckComponentRequirments(data, triggeredBy, types);

            if (!canApply)
            {
                if (data.Component is CC)
                {
                    await data.Target.ApplyEffect(data, AbilityResult.IMMUNE, null, false);
                }
                return;
            }

            //calculate spell vlaues
            bool result = await data.Component.Initialize(data, triggeredBy);

            if (!result)
                return;


            if (data.Component.FlightSpeed > 0 && data.Target != null)
            {
                //calculate travel time
                float travelTime = ((float)(data.Caster.Location.Distance(data.Target.Location) / (float)data.Component.FlightSpeed) * 1000);

                //notify players of projectile travel time
                await data.Caster.Controller.Broadcast(data.Caster, F_USE_ABILITY.Create(data.Caster.ObjectID, data.Target?.ObjectID ?? 0, (ushort)data.Ability.ID, (ushort)data.Ability.EffectID,
                    AbilityActionInfo.SET_PROJECTILE_IMPACT_TIME, AbilityResult.OK, (ushort)travelTime, (byte)data.Token.Sequence));

                if (travelTime > 0)
                    await Svc.Timers.AddTimerAsync(data.Caster, data.Caster, (int)(travelTime + 200), (e) => data.Component.Apply(data, triggeredBy));
                else
                    await data.Component.Apply(data, triggeredBy);
            }
            else
                await data.Component.Apply(data, triggeredBy);
        }

        public async Task RemoveBuff(Entity target, long abilityID, long casterID, bool cleansed = false)
        {
            Validate.IsTrue(Svc.Abilities.ContainsKey(abilityID), nameof(abilityID));

            var buffs = await target.Buffs.GetBuffsByAbility(Svc.Abilities[abilityID], casterID);
            await RemoveBuffs(buffs);
        }

        public async Task RemoveBuffs(List<Buff> buffs)
        {
            foreach (var buff in buffs)
                await RemoveBuff(buff);

        }

        public async Task RemoveBuffs(Entity target)
        {
            await RemoveBuffs(await target.Buffs.GetBuffs());
        }

        public async Task RemoveBuff(Buff buff, bool cleansed = false)
        {
            await buff.PurgeTimers(Svc.Timers);
            buff.Deleted = true;

            if (await buff.Target.Buffs.RemoveBuff(buff))
            {
                foreach (var active in buff.Components.Values)
                {
                    if (active.Initialized)
                        await active.Component.OnDeactivated(active);
                }

                Log.Info($"Removing buff:{buff}", CombatService.REMOVE_BUFF);

                foreach (var sub in buff.Token.SubTokens)
                {
                    if (sub.CastBuff != buff)
                        await RemoveBuff(sub.CastBuff);
                }

                await Svc.PrintBuffs(buff.Target);

                if (cleansed)
                    await Svc.GetService<EventService>().Raise(EventType.BUFF_CLEANSED, buff);
                await Svc.GetService<EventService>().Raise(EventType.BUFF_PURGED, buff);

                if (buff.Token.CastBuff == buff && buff.Ability.ChannelInterval > 0)
                {
                    var currentToken = await buff.Target.GetCastToken(nameof(RemoveBuff));

                    if (currentToken != null && currentToken.Ability == buff.Ability && currentToken.Caster == buff.Caster)
                    {
                        currentToken.CastBuff = null;
                        await Svc.Interrupt(currentToken, buff.Target, AbilityResult.OK);
                    }
                }

                if (buff.Token.StaticSurrogate != null)
                {
                    await buff.Token.StaticSurrogate.Controller.RemoveObject(buff.Token.StaticSurrogate);
                    buff.Token.StaticSurrogate = null;
                }
                await buff.Target.RemoveComponentEffects(buff.Token);
            }
        }

        public async Task RaiseComponentEvent(Entity target, CastToken triggeredBy, AbilityEvent eventType)
        {
            var components = await target.Buffs.GetActiveComponents<EVENT_LISTENER>();
            foreach (var c in components)
            {
                var el = (EVENT_LISTENER)c.Component;
                if (el.Type == eventType)
                {
                    await RaiseNextComponent(c, triggeredBy, AbilityTrigger.OnEventTriggered, 1);
                }
            }
        }

        public Task UpdateWoundsWatches(Entity target, ComponentData changedBy)
        {
            return UpdateWatches(target, changedBy, AbilityOperation.WoundsPercent);
        }

        public async Task UpdateWatches(Entity target, ComponentData changedBy, params AbilityOperation[] ops)
        {
            var comps = await target.Buffs.GetComponents();

            foreach (var comp in comps)
            {
                if (comp.Component.GetExpressionByOperation(AbilitySourceType.WATCH, ops) != null)
                {
                    bool canActivate = await CheckComponentRequirments(comp, changedBy.Token, AbilitySourceType.WATCH);
                    if (canActivate)
                    {
                        await ApplyComponent(comp, changedBy.Token, false, AbilitySourceType.WATCH);
                    }
                }
            }
        }

        public async Task<bool> EvaluateMultipleExp(ComponentData data, CastToken triggeredBy, List<AbilityExpressionData> expList)
        {
            if (expList.Count == 0)
                return true;
            var expReader = new ExpressionReader(expList);
            var exp = expReader.GetNext();

            bool result = false;

            while (exp != null)
            {
                if (!Svc.ExpressionHandlers.ContainsKey(exp.Operation))
                {
                    Log.Info($"EXPRESSION Handler missing {exp.Operation}", ConsoleColor.Yellow);
                    return false;
                }
                result = Svc.ExpressionHandlers[exp.Operation].Evaluate(Svc, exp, await Svc.ExpressionHandlers[exp.Operation].Execute(Svc, data, exp, triggeredBy));

                if (!result && exp.LogicOperator == AbilityLogicOperator.And)
                {
                    exp = expReader.GetNextOR();
                    if (exp == null) //short circuit if no next OR exists
                        return false;
                    continue;
                }
                else if (result && exp.LogicOperator == AbilityLogicOperator.Or)
                    return true;

                exp = expReader.GetNext();
            }
            return result;
        }

        public async Task<float> GetCastTimeSum(CastToken token)
        {
            if (token.Ability.ChannelInterval > 0)
                return token.Ability.GetCastime();

            float percentSum = 0;
            float incSum = (await token.Caster.Buffs.StatSums(token, BonusType.BUILD_TIME))[BonusType.BUILD_TIME].Total;

            foreach (var data in await token.Caster.Buffs.GetComponentsByType<CASTIME_CHANGE>())
            {
                var comp = (CASTIME_CHANGE)data.Component;

                bool active = await CheckComponentRequirments(data, token, AbilitySourceType.WATCH);

                if (active)
                {
                    if (comp.ValueType == ComponentValueType.ReducePct)
                    {
                        percentSum += comp.Value;
                    }
                    if (comp.ValueType == ComponentValueType.ReduceToPct)
                    {
                        percentSum += -(100 - comp.Value);
                    }
                    else
                    {
                        incSum += data.Value;
                    }
                }
            }

            float result = token.Ability.GetCastime() + incSum;
            return result + (result * (percentSum / 100f));
        }

        public async Task<float> GetAPCostSum(CastToken token)
        {
            float percentSum = 0;
            float incSum = (await token.Caster.Buffs.StatSums(token, BonusType.AP_COST))[BonusType.AP_COST].Total;

            foreach (var data in await token.Caster.Buffs.GetComponentsByType<AP_CHANGE>())
            {
                var comp = (AP_CHANGE)data.Component;

                bool active = await CheckComponentRequirments(data, token, AbilitySourceType.WATCH);
                if (active)
                {
                    if (comp.ValueType == ComponentValueType.ReducePct)
                    {
                        percentSum += comp.Value;
                    }
                    if (comp.ValueType == ComponentValueType.ReduceToPct)
                    {
                        percentSum += -(100 - comp.Value);
                    }
                    else
                    {
                        incSum += data.Value;
                    }
                }
            }

            float result = token.Ability.GetAPCost() + incSum;
            return result + (result * (percentSum / 100f));
        }

        public async Task OnComponentTimerRemoved(TimerTick timer, int tick, int totalTicks, ComponentData data)
        {
            if (data.Buff.GetMaxRemainTimer() == 0 && !data.Buff.Ability.IsPermaBuff)
                await RemoveBuff(data.Buff);
        }

        public async Task RaiseNextComponent(ComponentData data, CastToken triggeredBy, AbilityTrigger triggerType, int tick)
        {
            for (int i = data.Index + 1; i < 10; i++)
            {
                if (data.Buff.Components.ContainsKey(i))
                {
                    if (data.Ability.Components[i].Trigger == triggerType)
                    {
                        data.Buff.Components[i].Target = data.Target;
                        data.Buff.Components[i].TriggeredByComponent = data;
                        await ApplyComponent(data.Buff.Components[i], triggeredBy, true, AbilitySourceType.APPLY, AbilitySourceType.CAST);
                        break;
                    }
                }
            }
        }

        public async Task OnComponentHandleEvent(ComponentData data, CastToken triggeredBy)
        {
            bool canCast = await CheckComponentRequirments(data, triggeredBy, AbilitySourceType.EVENT_REQ);

            if (canCast)
            {
                for (int i = data.Index + 1; i < 10; i++)
                {
                    if (data.Buff.Components.ContainsKey(i) && data.Ability.Components.ContainsKey(i) && data.Ability.Components[i].Trigger == AbilityTrigger.OnEventTriggered)
                        await ApplyComponent(data.Buff.Components[i], triggeredBy);
                }
            }

            await Task.CompletedTask;
        }



    }
}
*/