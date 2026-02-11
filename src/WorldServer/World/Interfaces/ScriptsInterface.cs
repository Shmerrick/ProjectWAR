using FrameWork;
using System.Collections.Generic;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Scripting;

namespace WorldServer.World.Interfaces
{
    public class ScriptsInterface : BaseInterface
    {
        #region General

        public List<AGeneralScript> Scripts = new List<AGeneralScript>();

        public void AddScript(string Name)
        {
            if (string.IsNullOrEmpty(Name))
                return;

            if (HasScript(Name))
                return;

            AGeneralScript Script = WorldMgr.GetScript(_Owner, Name);
            if (Script != null)
            {
                AddScript(Script);
            }
            else
                Log.Debug("ScriptsInterface", "Invalid Script :" + Name);
        }

        public void AddScript(AGeneralScript Script)
        {
            Scripts.Add(Script);
            Script.OnInitObject(_Owner);
        }

        public bool HasScript(string Name)
        {
            foreach (AGeneralScript Script in Scripts)
                if (Script.ScriptName == Name)
                    return true;

            return false;
        }

        public void RemoveScript(string Name)
        {
            if (string.IsNullOrEmpty(Name))
                return;

            Scripts.RemoveAll(Script =>
                {
                    if (Script.ScriptName == Name)
                    {
                        Script.OnRemoveObject(_Owner);
                        return true;
                    }

                    return false;
                });
        }

        public override bool Load()
        {
            AGeneralScript Script = WorldMgr.GetScript(_Owner, "");
            if (Script != null)
                AddScript(Script);

            OnObjectLoad(_Owner);
            return base.Load();
        }

        public void ClearScripts()
        {
            foreach (AGeneralScript Script in Scripts)
                Script.OnRemoveObject(_Owner);

            Scripts.Clear();
        }

        #endregion General

        #region Local

        public void OnWorldUpdate(long Tick)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldUpdate(Tick);
        }

        public void OnObjectLoad(Object Creature)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnObjectLoad(Creature);
        }

        public void OnInteract(Object Creature, Player Target, InteractMenu Menu)
        {
            //Log.Info("ScriptsInterface", "OnInteract Scripts : " + Scripts.Count);

            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnInteract(Creature, Target, Menu);
        }

        public virtual void OnEnterWorld(Object Creature)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnEnterWorld(Creature);
        }

        public void OnRemoveFromWorld(Object Creature)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnRemoveFromWorld(Creature);
        }

        public void OnEnterRange(Object Creature, Object DistObj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnEnterRange(Creature, DistObj);
        }

        public void OnLeaveRange(Object Creature, Object DistanceCreature)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnLeaveRange(Creature, DistanceCreature);
        }

        public void OnReceiveDamages(Object Creature, Object Attacker)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnReceiveDamages(Creature, Attacker);
        }

        public void OnDealDamages(Object Creature, Object Target)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnDealDamages(Creature, Target);
        }

        public void OnDie(Object Creature)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnDie(Creature);
        }

        public void OnRevive(Object Creature)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnRevive(Creature);
        }

        public void OnCastAbility(AbilityInfo Ability)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnCastAbility(Ability);
        }

        #endregion Local

        #region World

        public bool OnPlayerCommand(Player Plr, string Text)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                if (!Scripts[i].OnPlayerCommand(Plr, Text))
                    return false;

            return true;
        }

        public void OnWorldPlayerEvent(string EventName, Player Plr, object Data)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldPlayerEvent(EventName, Plr, Data);
        }

        public void OnWorldCreatureEvent(string EventName, Creature Creature, object Data)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldCreatureEvent(EventName, Creature, Data);
        }

        public void OnWorldGameObjectEvent(string EventName, GameObject GameObject, object Data)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldGameObjectEvent(EventName, GameObject, Data);
        }

        public void OnWorldZoneEvent(string EventName, ZoneMgr Zone, object Data)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldZoneEvent(EventName, Zone, Data);
        }

        #endregion World
    }
}