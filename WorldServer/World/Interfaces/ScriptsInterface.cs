using System.Collections.Generic;
using FrameWork;
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

        #endregion

        #region Local

        public void OnWorldUpdate(long Tick)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldUpdate(Tick);
        }

        public void OnObjectLoad(Object Obj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnObjectLoad(Obj);
        }

        public void OnInteract(Object Obj, Player Target, InteractMenu Menu)
        {
            //Log.Info("ScriptsInterface", "OnInteract Scripts : " + Scripts.Count);

            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnInteract(Obj, Target, Menu);
        }

        public virtual void OnEnterWorld(Object Obj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnEnterWorld(Obj);
        }

        public void OnRemoveFromWorld(Object Obj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnRemoveFromWorld(Obj);
        }

        public void OnEnterRange(Object Obj, Object DistObj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnEnterRange(Obj, DistObj);
        }

        public void OnLeaveRange(Object Obj, Object DistObj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnLeaveRange(Obj, DistObj);
        }

        public void OnReceiveDamages(Object Obj, Object Attacker)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnReceiveDamages(Obj, Attacker);
        }

        public void OnDealDamages(Object Obj, Object Target)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnDealDamages(Obj, Target);
        }

        public void OnDie(Object Obj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnDie(Obj);
        }

        public void OnRevive(Object Obj)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnRevive(Obj);
        }

        public void OnCastAbility(AbilityInfo Ab)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnCastAbility(Ab);
        }

        #endregion

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

        public void OnWorldGameObjectEvent(string EventName, GameObject Obj, object Data)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldGameObjectEvent(EventName, Obj, Data);
        }

        public void OnWorldZoneEvent(string EventName, ZoneMgr Zone, object Data)
        {
            for (int i = 0; i < Scripts.Count; ++i)
                Scripts[i].OnWorldZoneEvent(EventName, Zone, Data);
        }

        #endregion
    }
}
