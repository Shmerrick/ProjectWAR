using System;
using System.Collections.Generic;
using GameData;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts
{
    /// <summary>
    /// Object bound to a Campaign responsible of tracking players to compute AAO.
    /// </summary>
    internal class AAOTracker
    {

        private readonly List<NewBuff> _orderAAOBuffs = new List<NewBuff>();
        private readonly List<NewBuff> _destroAAOBuffs = new List<NewBuff>();

        /// <summary>AAO multiplier, -20 if order has 400 aao, +20 if destro has 400 aao</summary>
        private int _againstAllOddsMult;
        
        /// <summary>
        /// Recalculates aao multiplier and updates buffs.
        /// </summary>
        /// <param name="players">Set of players in lake, accessible through main update thread</param>
        /// <param name="orderCount">Positive order count</param>
        /// <param name="destroCount">Positive destro count</param>
        internal void RecalculateAAO(IList<Player> players, int orderCount, int destroCount)
        {
            int newMult;

            float factor;
            if (orderCount == 0 || destroCount == 0)
                factor = 0f; // No need to set aao if missing a realm
            else  if (orderCount < destroCount)
                factor = ((float)destroCount / (float)orderCount) - 1f;
            else
                factor = ((float)orderCount / (float)destroCount) - 1f;

            // Inferior rounding (20%, 40%, 60% etc.)
            newMult = (int)(factor * 5f);

            // Less order than destro AAO (negative)
            if (orderCount < destroCount)
                newMult = -newMult;

            if (newMult < -20)
                newMult = -20; // order has 400% aao
            else if (newMult > 20)
                newMult = 20; // destro has 400% aao

            if (newMult == _againstAllOddsMult)
                return; // No change

            Realms newRealm = GetAaoRealm(newMult);
            Realms previousRealm = GetAaoRealm(_againstAllOddsMult);

            if (newRealm != previousRealm)
            {
                InitAAOBuffs(newRealm, players); // Switching from Order, or no AAO, to Destro, or reverse
                ClearAAOBuffs(previousRealm); // Clear previous realm aao
            }
            else
            {
                UpdateAAOBuffs(newRealm, newMult); // The realm already has AAO. Update it.
            }


            _againstAllOddsMult = newMult;
        }

        /// <summary>
        /// Initializes new aao buffs for the given realm.
        /// </summary>
        /// <param name="realm">Realm that got aao, can be neutral</param>
        private void InitAAOBuffs(Realms realm, IList<Player> players)
        {
            if (realm == Realms.REALMS_REALM_NEUTRAL)
                return;
            
            foreach (Player plr in players)
                if (plr.Realm == realm)
                    plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, 40, AbilityMgr.GetBuffInfo((ushort)GameBuffs.AgainstAllOdds), AaoAssigned));
        }

        /// <summary>
        /// Invoked when a player enters the lake.
        /// </summary>
        /// <param name="plr">Player firing this event</param>
        internal void NotifyEnteredLake(Player plr)
        {
            if (plr.Realm == GetAaoRealm(_againstAllOddsMult))
                plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.AgainstAllOdds), AaoAssigned));
        }

        /// <summary>
        /// Invoked when a player left the lake.
        /// </summary>
        /// <param name="plr">Player firing this event</param>
        internal void NotifyLeftLake(Player plr)
        {
            List<NewBuff> aaoBuffs = GetAaoBuffs(plr.Realm);
            lock (aaoBuffs)
            {
                for (int i = 0; i < aaoBuffs.Count; ++i)
                {
                    if (aaoBuffs[i].Caster == plr)
                    {
                        aaoBuffs[i].BuffHasExpired = true;
                        aaoBuffs.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Invoked by buff interface to keep track of aao buff.
        /// </summary>
        /// <param name="aaoBuff">Buff that was created</param>
        private void AaoAssigned(NewBuff aaoBuff)
        {
            if (aaoBuff == null)
                return;

            Realms aaoRealm = GetAaoRealm(_againstAllOddsMult);
            short aaoValue = (short)(Math.Abs(_againstAllOddsMult) * 20);
            float aaoBonus = Math.Abs(_againstAllOddsMult) * 0.2f;

            if (aaoRealm == aaoBuff.Caster.Realm)
            {
                lock (_orderAAOBuffs)
                    _orderAAOBuffs.Add(aaoBuff);

                aaoBuff.AddBuffParameter(1, 1);
                aaoBuff.AddBuffParameter(2, aaoValue);
                aaoBuff.AddBuffParameter(3, aaoValue);
                aaoBuff.AddBuffParameter(4, aaoValue);
                ((Player)aaoBuff.Caster).AAOBonus = aaoBonus;
            }
            else
                aaoBuff.BuffHasExpired = true;
        }

        /// <summary>
        /// Updates aao buffs for the given realm.
        /// </summary>
        /// <param name="realm">Realm that have aao, can be neutral</param>
        /// <param name="newMult">Signed multiplier</param>
        private void UpdateAAOBuffs(Realms realm, float newMult)
        {
            List<NewBuff> buffs = GetAaoBuffs(realm);
            if (buffs == null)
                return;

            short aaoValue = (short)(Math.Abs(newMult) * 20);
            float aaoBonus = Math.Abs(newMult) * 0.2f;
            lock (buffs)
            {
                foreach (NewBuff aaoBuff in buffs)
                {
                    aaoBuff.DeleteBuffParameter(1);
                    aaoBuff.DeleteBuffParameter(2);
                    aaoBuff.DeleteBuffParameter(3);
                    aaoBuff.DeleteBuffParameter(4);
                    aaoBuff.AddBuffParameter(1, 1);
                    aaoBuff.AddBuffParameter(2, aaoValue);
                    aaoBuff.AddBuffParameter(3, aaoValue);
                    aaoBuff.AddBuffParameter(4, aaoValue);
                    ((Player)aaoBuff.Caster).AAOBonus = aaoBonus;
                    aaoBuff.SoftRefresh();
                }
            }
        }

        /// <summary>
        /// Clears aao buffs for the given realm.
        /// </summary>
        /// <param name="realm">Realm that lost aao, can be neutral</param>
        /// <param name="newMult">Signed multiplicator</param>
        private void ClearAAOBuffs(Realms realm)
        {
            List<NewBuff> buffs = GetAaoBuffs(realm);
            if (buffs == null)
                return;

            lock (buffs)
            {
                foreach (NewBuff buff in buffs)
                {
                    buff.BuffHasExpired = true;
                    ((Player)buff.Caster).AAOBonus = 0;
                }
                buffs.Clear();
            }
        }

        /// <summary>
        /// Utility method returning the aao buffs for the given realm.
        /// </summary>
        /// <param name="realm">Realm to update aao of, can be neutral</param>
        /// <returns>Buff list, null if neutral</returns>
        private List<NewBuff> GetAaoBuffs(Realms realm)
        {
            switch (realm)
            {
                case Realms.REALMS_REALM_ORDER:
                    return _orderAAOBuffs;
                case Realms.REALMS_REALM_DESTRUCTION:
                    return _orderAAOBuffs;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Utility method returning the realm that have aao depending on given multiplier.
        /// </summary>
        /// <param name="newMult">Signed multiplier, positive for destro aao, negative for order aao</param>
        /// <returns>Realm of players to buff, can be neutral</returns>
        private Realms GetAaoRealm(float mult)
        {
            if (mult == 0)
                return Realms.REALMS_REALM_NEUTRAL;
            if (mult > 0) // destro
                return Realms.REALMS_REALM_DESTRUCTION;
            else // order
                return Realms.REALMS_REALM_ORDER;
        }

    }
}
