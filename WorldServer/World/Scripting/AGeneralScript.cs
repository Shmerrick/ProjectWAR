using WorldServer.NetWork.Handler;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting
{
    public abstract class AGeneralScript
    {
        public string ScriptName;

        public virtual void OnInitObject(Object Obj)
        {

        }

        public virtual void OnRemoveObject(Object Obj)
        {

        }

        public virtual void OnWorldUpdate(long Tick)
        {

        }

        public virtual void OnObjectLoad(Object Obj)
        {

        }

        public virtual void OnInteract(Object Obj, Player Target, InteractMenu Menu)
        {

        }

        public virtual void OnEnterWorld(Object Obj)
        {

        }

        public virtual void OnRemoveFromWorld(Object Obj)
        {

        }

        public virtual void OnEnterRange(Object Obj, Object DistObj)
        {

        }

        public virtual void OnLeaveRange(Object Obj, Object DistObj)
        {

        }

        public virtual void OnReceiveDamages(Object Obj, Object Attacker)
        {

        }

        public virtual void OnDealDamages(Object Obj, Object Target)
        {

        }

        public virtual void OnDie(Object Obj)
        {

        }

        public virtual void OnRevive(Object Obj)
        {

        }

        public virtual void OnCastAbility(AbilityInfo Ab)
        {

        }

        #region World

        public virtual bool OnPlayerCommand(Player Plr, string Text)
        {
            return true; // True if the text can be draw
        }

        public virtual void OnWorldPlayerEvent(string EventName, Player Plr, object Data)
        {

        }

        public virtual void OnWorldCreatureEvent(string EventName, Creature Creature, object Data)
        {

        }

        public virtual void OnWorldGameObjectEvent(string EventName, Objects.GameObject Obj, object Data)
        {

        }

        public virtual void OnWorldZoneEvent(string EventName, ZoneMgr Zone, object Data)
        {

        }

        #endregion
    }
}
