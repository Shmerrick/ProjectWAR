using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using WorldServer.World.BattleFronts.Objectives;
using static WorldServer.World.BattleFronts.BattleFrontConstants;

namespace WorldServer.World.BattleFronts
{
     /// <summary>
    /// Implementation of BattleFronts dedicated to t1 mechanics.
    /// </summary>
    /// <remarks>
    /// Mostly concerns victory points computing and transmission.
    /// </remarks>
    public class T1BattleFront : RoRBattleFront
    {
        
        #region Load
        public T1BattleFront(RegionMgr region, bool oRvRFront) : base(region, oRvRFront)
        {
            if (oRvRFront)
            {
                _EvtInterface.AddEvent(UpdateVictoryPoints, FLAG_SECURE_REWARD_INTERVAL, 0);
            }
        }
        #endregion

        #region Victory points
        private float _orderVictoryPoints;
        private float _destroVictoryPoints;

        /// <summary>
        /// Updates the victory points per realm and fires lock when necessary.
        /// </summary>
        private void UpdateVictoryPoints()
        {
            if (PairingLocked)
                return; // Nothing to do

            // Victory depends on objective securization in t1
            float orderVictoryPoints = _orderVictoryPoints;
            float destroVictoryPoints = _destroVictoryPoints;
            int flagCount = 0, destroFlagCount = 0, orderFlagCount = 0;

            foreach (ProximityFlag flag in Objectives)
            {
                float progression = flag.GrantSecureRewards();
                if (progression < 0f)
                    orderVictoryPoints -= progression;
                else if (progression > 0f)
                    destroVictoryPoints += progression;

                flagCount++;
                Realms secureRealm = flag.GetSecureRealm();
                if (secureRealm == Realms.REALMS_REALM_ORDER)
                    orderFlagCount++;
                else if (secureRealm == Realms.REALMS_REALM_DESTRUCTION)
                    destroFlagCount++;
            }

            // Victry points update
            _orderVictoryPoints = Math.Min(LOCK_VICTORY_POINTS, orderVictoryPoints);
            _destroVictoryPoints = Math.Min(LOCK_VICTORY_POINTS, destroVictoryPoints);

            if (_orderVictoryPoints >= LOCK_VICTORY_POINTS && orderFlagCount == flagCount)
                LockPairing(Realms.REALMS_REALM_ORDER, true);
            else if (_destroVictoryPoints >= LOCK_VICTORY_POINTS && destroFlagCount == flagCount)
                LockPairing(Realms.REALMS_REALM_DESTRUCTION, true);
        }

        /// <summary>
        /// Writes the current zone capture status (gauge in upper right corner of client UI).
        /// </summary>
        /// <param name="Out">Packet to write</param>
        public override void WriteCaptureStatus(PacketOut Out)
        {
            Out.WriteByte(0);
            byte orderPercent, destroPercent = 0;
            switch (LockingRealm)
            {
                case Realms.REALMS_REALM_ORDER:
                    orderPercent = 100;
                    destroPercent = 0;
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    orderPercent = 0;
                    destroPercent = 100;
                    break;
                default:
                    orderPercent = (byte)(_orderVictoryPoints / LOCK_VICTORY_POINTS * 50f);
                    destroPercent = (byte)(_destroVictoryPoints / LOCK_VICTORY_POINTS * 50f);
                    break;
            }
            Out.WriteByte(orderPercent);
            Out.WriteByte(destroPercent);
        }
        #endregion

        #region Lock
        /// <summary>
        /// Updates specfic t1 values on lock.
        /// </summary>
        public override void LockPairing(Realms realm, bool announce, bool restoraStatus = false, bool noReward = false, bool draw = false)
        {
            if (PairingLocked)
                return; // No effect

            base.LockPairing(realm, announce);

            if (realm == Realms.REALMS_REALM_ORDER)
            {
                _destroVictoryPoints = 0f;
                _orderVictoryPoints = LOCK_VICTORY_POINTS;
            }
            else
            {
                _orderVictoryPoints = 0f;
                _destroVictoryPoints = LOCK_VICTORY_POINTS;
            }

            // T1 unlock process
            // 2 active parings
            // then 1 lock -> 1 active zone
            // the the last parings is locked -> unlock the two others
            IList<IBattleFront> BattleFronts = BattleFrontList.BattleFronts[Tier - 1];
            int lockedParingCount = 0;
            foreach (IBattleFront BattleFront in BattleFronts)
            {
                if (((RoRBattleFront)BattleFront).PairingLocked)
                    lockedParingCount++;
            }

            if (lockedParingCount == BattleFronts.Count)
                UnlockOtherBattleFronts();
            // Can't delay for some reason
            //     _EvtInterface.AddEvent(UnlockOtherBattleFronts, 10 * 1000, 1);
        }

        /// <summary>
        /// Unlocks other BattleFronts except the one that was just locked.
        /// </summary>
        private void UnlockOtherBattleFronts()
        {
            foreach (IBattleFront BattleFront in BattleFrontList.BattleFronts[Tier - 1])
            {
                if (BattleFront != this && ((RoRBattleFront)BattleFront).PairingLocked)
                    BattleFront.ResetPairing();
            }
        }

        /// <summary>
        /// Resets specfic t1 values.
        /// </summary>
        public override void ResetPairing()
        {
            base.ResetPairing();

            _orderVictoryPoints = 0f;
            _destroVictoryPoints = 0f;
        }
        #endregion

        #region Send
        protected override void InternalCampaignDiagnostic(Player player, bool bLocalZone)
        {
            player.SendClientMessage($"Victory points for order/destro : {_orderVictoryPoints} / {_destroVictoryPoints}");
        }
        #endregion
    }
}