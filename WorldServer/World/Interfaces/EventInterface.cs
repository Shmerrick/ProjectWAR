using System;
using System.Collections.Generic;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Interfaces
{
    public enum EventName
    {
        OnRemoveFromWorld,  // Pet Dismissal (buff should handle?)

        OnMove, 
        OnStopMove,  // Unused
        // ON_CHANGE_OFFSET,
        // ON_CHANGE_ZONE,

        OnEnterCombat,  // Unused
        OnDealDamage,
        OnReceiveDamage,
        OnDealHeal,
        OnReceiveHeal,
        OnTargetDie,
        OnLeaveCombat,

        OnDie,
        OnKill,

        OnAddXP, // Args = XP
        OnAddRenown, // Args = Renown

        OnAcceptQuest, // Unused - Sender = Player, Obj = Character_Quest
        OnDeclineQuest,// Unused -  Sender = Player, Obj = Character_Quest
        OnDoneQuest,  // Unused
        OnAbortQuest, // Unused - Sender = Player, Obj = Character_Quest

        Playing,
        Leave,
        OnLevelUp,

        ArriveAtTarget,  // Unused
        OnWalkTo,  // Unused
        OnWalk,  // Unused

        OnStartCasting,  // Unused

        OnLeaveGroup  // Unused
        ,
        OnOuterDoorDestroyed
    };

    public delegate void EventDelegate();
    public delegate void EventDelegateEx(object userData);

    public class EventInfo
    {
        public readonly int Interval;
        public int Count;
        public readonly int BaseCount;
        public readonly EventDelegate Del;
        public readonly EventDelegateEx DelEx;
        private long _nextExecute;
        public bool ToDelete;
        public readonly object UserData;

        public EventInfo(EventDelegate del,int interval, int count)
        {
            Del = del;
            Interval = interval;
            Count = count;
            BaseCount = count;
            if (interval == 0 )
                ToDelete = true;

            //Log.Success("AddEvent", "Del =" + Del.Method.Name + ",Name" + Del.Target.ToString());
        }

        public EventInfo(EventDelegateEx del, int interval, int count, object userData)
        {
            DelEx = del;
            Interval = interval;
            Count = count;
            BaseCount = count;
            UserData = userData;
            if (interval == 0)
                ToDelete = true;

            //Log.Success("AddEvent", "Del =" + Del.Method.Name + ",Name" + Del.Target.ToString());
        }

        /// <summary>
        /// Checks if associated delegate have to be invoked
        /// and updates this object's state.
        /// </summary>
        /// <param name="tick">Current timestamp</param>
        /// <returns>True if this object has to be removed from events list.</returns>
        public bool Update(long tick)
        {
            if (ToDelete)
                return true;

            if (_nextExecute == 0)
                _nextExecute = tick + Interval;

            if (_nextExecute <= tick)
            {
                if(BaseCount > 0)
                    --Count;

                Del?.Invoke();

                if(UserData != null)
                    DelEx?.Invoke(UserData);

                _nextExecute = tick + Interval;
            }

            if (Count <= 0 && BaseCount != 0)
                ToDelete = true;

            return ToDelete;
        }
    }

    public class EventInterface : BaseInterface
    {
        //#error EventInterfaces are never removed
        public static Dictionary<uint, EventInterface> EventInterfaces = new Dictionary<uint, EventInterface>();

        public static EventInterface GetEventInterface(uint characterId)
        {
            lock (EventInterfaces)
            {
                if (!EventInterfaces.ContainsKey(characterId))
                    EventInterfaces.Add(characterId, new EventInterface());

                return EventInterfaces[characterId];
            }
        }

        /// <summary>
        /// <para>A delegate representing a generic event.</para>
        /// <para>Return true in order to delete the event after it's been fired.</para>
        /// </summary>
        public delegate bool EventNotify(Object obj, object args);

        public override void Stop()
        {
            _running = false;

            if (_hasLock)
                throw new InvalidOperationException("Stop() was called from an event in the EventInterface");

            lock (_eventList)
                _eventList.Clear();

            lock (_notify)
                _notify.Clear();

            lock (_forceNotify)
                _forceNotify.Clear();

            base.Stop();
        }

        public void Start()
        {
            _running = true;
        }

        #region Events

        private bool _running = true;
        private readonly List<EventInfo> _eventList = new List<EventInfo>();

        private bool _hasLock;

        /// <summary>
        /// Updates the interface state, firing registered events that are ready.
        /// </summary>
        /// <param name="tick">Current timestamp</param>
        public override void Update(long tick)
        {
            if (!_running)
            {
                if (_eventList.Count > 0)
                {
                    lock (_eventList)
                        _eventList.Clear();
                }

                return;
            }

            if (_eventList.Count == 0)
                return;

            lock (_eventList)
            {
                _hasLock = true;
                for (int i = _eventList.Count - 1; i >= 0; --i)
                {
                    if (!_running || _eventList[i].Update(tick))
                        _eventList.RemoveAt(i);
                }
                _hasLock = false;
            }
        }

        /// <summary>
        /// Registers a new delayed event.
        /// </summary>
        /// <param name="del">Delegate to invoke when event is fired</param>
        /// <param name="interval">LastUpdatedTime before first execution and between each execution</param>
        /// <param name="count">Number of expected executions, if 0 executions has no limit</param>
        /// <param name="userData">Custom user data</param>
        public void AddEvent(EventDelegate del, int interval, int count)
        {
            lock (_eventList)
                _eventList.Add(new EventInfo(del, interval, count));
        }

        /// <summary>
        /// Registers a new delayed event expecting user data.
        /// </summary>
        /// <param name="del">Delegate to invoke when event is fired</param>
        /// <param name="interval">LastUpdatedTime before first execution and between each execution</param>
        /// <param name="count">Number of expected executions, if 0 executions has no limit</param>
        /// <param name="userData">Custom user data</param>
        public void AddEvent(EventDelegateEx del, int interval, int count, object userData)
        {
            lock (_eventList)
                _eventList.Add(new EventInfo(del, interval, count, userData));
        }

        public void RemoveEvent(EventDelegate del)
        {
            lock (_eventList)
                for (int i = _eventList.Count - 1; i >= 0; --i)
                {
                    if (_eventList[i].Del == del)
                        _eventList[i].ToDelete = true;
                }
        }

        public void RemoveEvent(EventDelegateEx del)
        {
            lock (_eventList)
                for (int i = _eventList.Count - 1; i >= 0; --i)
                {
                    if (_eventList[i].DelEx == del)
                        _eventList[i].ToDelete = true;
                }
        }

        public bool HasEvent(EventDelegate del)
        {
            lock (_eventList)
            {
                for (int i = 0; i < _eventList.Count; ++i)
                {
                    if (_eventList[i].Del == del)
                        return true;
                }
            }
            return false;
        }

        public bool HasEvent(EventDelegateEx del)
        {
            lock (_eventList)
            {
                for (int i = 0; i < _eventList.Count; ++i)
                {
                    if (_eventList[i].DelEx == del)
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region Notify

        private readonly Dictionary<int, List<EventNotify>> _notify = new Dictionary<int, List<EventNotify>>();
        private readonly Dictionary<int, List<EventNotify>> _forceNotify = new Dictionary<int, List<EventNotify>>();

        public void Notify(EventName name, Object sender, object args)
        {
            List<EventNotify> eventNotifies;
            int i, count;

            lock (_notify)
            {
                if (_notify.TryGetValue((int) name, out eventNotifies))
                {
                    for (i = 0; i < eventNotifies.Count; ++i)
                    {
                        EventNotify target = eventNotifies[i];
                        if (target.Invoke(sender, args))
                        {
                            if (ReferenceEquals(eventNotifies[i], target))
                                eventNotifies.RemoveAt(i);
                            else
                                eventNotifies.Remove(target);
                            --i;
                        }
                    }
                }
            }

            lock (_forceNotify)
            {
                if (_forceNotify.TryGetValue((int) name, out eventNotifies))
                {
                    for (i = 0; i < eventNotifies.Count; ++i)
                    {
                        EventNotify target = eventNotifies[i];
                        if (target.Invoke(sender, args))
                        {
                            if (ReferenceEquals(eventNotifies[i], target))
                                eventNotifies.RemoveAt(i);
                            else
                                eventNotifies.Remove(target);
                            --i;
                        }
                    }
                }
            }
        }

        public void AddEventNotify(EventName name, EventNotify Event)
        {
            AddEventNotify(name, Event, false);
        }

        public void AddEventNotify(EventName name, EventNotify Event, bool force)
        {
            List<EventNotify> eventNotifies;

            if (!force)
            {
                lock (_notify)
                {
                    if (!_notify.TryGetValue((int)name, out eventNotifies))
                    {
                        eventNotifies = new List<EventNotify>();
                        _notify.Add((int)name, eventNotifies);
                    }

                    if (!eventNotifies.Contains(Event))
                        eventNotifies.Add(Event);
                }
            }
            else
            {
                lock (_forceNotify)
                {
                    if (!_forceNotify.TryGetValue((int)name, out eventNotifies))
                    {
                        eventNotifies = new List<EventNotify>();
                        _forceNotify.Add((int)name, eventNotifies);
                    }

                    if (!eventNotifies.Contains(Event))
                        eventNotifies.Add(Event);
                }
            }
        }

        public void RemoveEventNotify(EventName name, EventNotify Event)
        {
            List<EventNotify> l;
            lock (_notify)
            {
                if (_notify.TryGetValue((int)name, out l))
                    l.Remove(Event);
            }

            lock (_forceNotify)
            {
                if (_forceNotify.TryGetValue((int)name, out l))
                    l.Remove(Event);
            }
        }

        #endregion


    }
}
