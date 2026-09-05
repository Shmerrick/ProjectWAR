using System;
using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using SystemData;
using WorldServer.Managers;
using WorldServer.NetWork;
using WorldServer.World.Objects;

namespace WorldServer.Services.World
{
    [Service]
    public class LotdService : ServiceBase
    {
        private enum LotdTrackerState : byte
        {
            Active = 0,
            Paused = 1
        }

        public const ushort LotdZoneId = 191;

        private const byte RetailTrackerCount = 1;
        private const byte RetailTrackerId = 1;
        private const uint RetailTrackerHeaderValue = 4;
        private const byte RetailDisplayType = (byte)RRQDisplayType.ERRQDISPLAY_TOMB_KINGS;
        private const byte RetailHeaderByte7 = 0;
        private const byte RetailHeaderByte10 = 0;
        private const byte RetailHeaderByte11 = 3;
        private const byte RetailRealmBlockByte1 = 0;
        private const byte RetailRealmBlockByte2 = 1;

        private static readonly ushort[] FortZones = { 104, 110, 204, 210 };

        private static LotdResourceTracker _tracker;
        private static int _lastBroadcastRemainingMinutes = -1;

        [LoadingFunction(true)]
        public static void LoadLotdResourceTracker()
        {
            Log.Debug("WorldMgr", "Loading Land of the Dead resource tracker...");

            try
            {
                _tracker = Database.SelectObject<LotdResourceTracker>($"TrackerId = {RetailTrackerId}");
                if (_tracker == null)
                {
                    _tracker = CreateDefaultTracker();
                    SaveTracker();
                }

                NormalizeTrackerState(false);

                Log.Success("LotdService", "Loaded Land of the Dead resource tracker");
            }
            catch (Exception ex)
            {
                _tracker = null;
                Exception root = ex.InnerException ?? ex;
                Log.Error("LotdService",
                    $"Failed to load Land of the Dead resource tracker. Apply Database/update_005_lotd_resource_tracker.sql and, if the table already exists, Database/update_006_lotd_resource_tracker_schema_fix.sql. {root.Message}");
            }
        }

        public static void Update()
        {
            if (_tracker == null || (LotdTrackerState)_tracker.State != LotdTrackerState.Paused)
                return;

            int remainingMinutes = GetRemainingOpenMinutes();
            if (remainingMinutes <= 0)
            {
                ResumeRace(true);
                return;
            }

            if (remainingMinutes == _lastBroadcastRemainingMinutes)
                return;

            _lastBroadcastRemainingMinutes = remainingMinutes;
            BroadcastTrackerUpdate();
        }

        public static void SendResourceTracker(Player player)
        {
            if (_tracker == null || player == null || player.Client == null)
                return;

            // The activation below names zone 191, and the client treats that as entering the
            // zone: it displays the "Necropolis of Zandri" title card over whatever zone the
            // player is really in. Both callers reached every player in the world -- one on
            // login, one broadcasting on every tracker change -- so the card appeared repeatedly
            // for players who had never been to Land of the Dead. Only send it to players who
            // are actually there.
            if (player.Zone == null || player.Zone.ZoneId != LotdZoneId)
                return;

            player.SendObjectiveTrackerActivation(LotdZoneId, 0);
            player.SendRvrTracker();
            player.SendPacket(BuildTrackerPacket(player));
        }

        public static bool ShouldExposeTaxi(Player player, Zone_Taxi taxi)
        {
            if (taxi == null)
                return false;

            if (taxi.ZoneID != LotdZoneId)
                return true;

            if (_tracker == null)
                return false;

            return CanRealmAccessLotd(player?.Realm ?? Realms.REALMS_REALM_NEUTRAL);
        }

        public static bool CanRealmAccessLotd(Realms realm)
        {
            if (_tracker == null)
                return false;

            // Access is open only during the Paused window when a realm has won the expedition.
            if ((LotdTrackerState)_tracker.State != LotdTrackerState.Paused)
                return false;

            // Owning realm has access; neutral means nobody won yet.
            if (_tracker.OwningRealm == (byte)Realms.REALMS_REALM_NEUTRAL)
                return false;

            return _tracker.OwningRealm == (byte)realm;
        }

        /// <summary>
        /// Human-readable tracker state, for GM diagnostics. The expedition flight is hidden whenever the
        /// tracker is not Paused with an owning realm, and that is indistinguishable in-game from a broken
        /// flight master — so being able to read the state directly matters.
        /// </summary>
        public static string GetStatusSummary()
        {
            if (_tracker == null)
                return "Land of the Dead tracker is not loaded; the expedition flight stays hidden. Apply Database/update_005_lotd_resource_tracker.sql.";

            string window = (LotdTrackerState)_tracker.State == LotdTrackerState.Paused
                ? GetRemainingOpenMinutes() + " min remaining"
                : "closed";

            return "LOTD " + (LotdTrackerState)_tracker.State
                 + " | owner " + (Realms)_tracker.OwningRealm
                 + " | Order " + _tracker.OrderResourcePoints + "/" + _tracker.Threshold
                 + " | Destruction " + _tracker.DestructionResourcePoints + "/" + _tracker.Threshold
                 + " | +" + _tracker.PointsPerBattlefrontLock + " per T4 battlefront lock"
                 + " | access window " + _tracker.UnlockDurationMinutes + " min (" + window + ")";
        }

        /// <summary>
        /// Opens the expedition for a realm immediately, as winning the race would. Testing aid: reaching this
        /// state legitimately needs enough T4 battlefront locks to cross the threshold, which is impractical to
        /// stage by hand. Returns false if the realm is not Order or Destruction.
        /// </summary>
        public static bool ForceUnlock(Realms realm)
        {
            if (_tracker == null)
                return false;

            if (realm != Realms.REALMS_REALM_ORDER && realm != Realms.REALMS_REALM_DESTRUCTION)
                return false;

            SetPausedState(realm, true);
            return true;
        }

        /// <summary>Returns the tracker to the accumulating race and clears ownership. Testing aid.</summary>
        public static bool ForceReset()
        {
            if (_tracker == null)
                return false;

            ResumeRace(true);
            return true;
        }

        /// <summary>
        /// Awards resource points to a realm exactly as a battlefront lock does, threshold check included, so
        /// the real unlock path can be exercised rather than bypassed. Testing aid.
        /// </summary>
        public static bool ForceAwardPoints(Realms realm, int points)
        {
            if (_tracker == null || points <= 0)
                return false;

            if (realm == Realms.REALMS_REALM_ORDER)
                _tracker.OrderResourcePoints = Math.Min(_tracker.Threshold, _tracker.OrderResourcePoints + points);
            else if (realm == Realms.REALMS_REALM_DESTRUCTION)
                _tracker.DestructionResourcePoints = Math.Min(_tracker.Threshold, _tracker.DestructionResourcePoints + points);
            else
                return false;

            _tracker.LastScoringRealm = (byte)realm;

            if (GetRealmScore(realm) >= _tracker.Threshold)
            {
                SetPausedState(realm, true);
                return true;
            }

            SaveTracker();
            BroadcastTrackerUpdate();
            return true;
        }

        public static void TryAwardBattlefrontLock(RVRProgression battlefront, Realms lockingRealm)
        {
            if (_tracker == null || battlefront == null)
                return;

            if ((LotdTrackerState)_tracker.State != LotdTrackerState.Active)
                return;

            if (lockingRealm != Realms.REALMS_REALM_ORDER && lockingRealm != Realms.REALMS_REALM_DESTRUCTION)
                return;

            if (battlefront.Tier != 4 || !IsEligibleBattlefrontZone((ushort)battlefront.ZoneId))
                return;

            if (lockingRealm == Realms.REALMS_REALM_ORDER)
                _tracker.OrderResourcePoints = Math.Min(_tracker.Threshold, _tracker.OrderResourcePoints + _tracker.PointsPerBattlefrontLock);
            else
                _tracker.DestructionResourcePoints = Math.Min(_tracker.Threshold, _tracker.DestructionResourcePoints + _tracker.PointsPerBattlefrontLock);

            _tracker.LastScoringRealm = (byte)lockingRealm;

            if (GetRealmScore(lockingRealm) >= _tracker.Threshold)
            {
                SetPausedState(lockingRealm, true);
                return;
            }

            SaveTracker();
            BroadcastTrackerUpdate();
        }

        private static LotdResourceTracker CreateDefaultTracker()
        {
            return new LotdResourceTracker
            {
                TrackerId = RetailTrackerId,
                State = (byte)LotdTrackerState.Active,
                OwningRealm = (byte)Realms.REALMS_REALM_NEUTRAL,
                OrderResourcePoints = 0,
                DestructionResourcePoints = 0,
                Threshold = 500,
                PointsPerBattlefrontLock = 100,
                UnlockDurationMinutes = 30,
                UnlockEndsOnUtc = null,
                LastScoringRealm = (byte)Realms.REALMS_REALM_NEUTRAL,
                LastUpdatedOnUtc = DateTime.UtcNow
            };
        }

        private static void NormalizeTrackerState(bool broadcast)
        {
            if (_tracker == null)
                return;

            bool dirty = false;

            if (_tracker.Threshold <= 0)
            {
                _tracker.Threshold = 500;
                dirty = true;
            }

            if (_tracker.PointsPerBattlefrontLock <= 0)
            {
                _tracker.PointsPerBattlefrontLock = 100;
                dirty = true;
            }

            if (_tracker.UnlockDurationMinutes <= 0)
            {
                _tracker.UnlockDurationMinutes = 30;
                dirty = true;
            }

            if (_tracker.OrderResourcePoints < 0)
            {
                _tracker.OrderResourcePoints = 0;
                dirty = true;
            }

            if (_tracker.DestructionResourcePoints < 0)
            {
                _tracker.DestructionResourcePoints = 0;
                dirty = true;
            }

            if (_tracker.OrderResourcePoints > _tracker.Threshold)
            {
                _tracker.OrderResourcePoints = _tracker.Threshold;
                dirty = true;
            }

            if (_tracker.DestructionResourcePoints > _tracker.Threshold)
            {
                _tracker.DestructionResourcePoints = _tracker.Threshold;
                dirty = true;
            }

            if (!IsSupportedRealm(_tracker.OwningRealm))
            {
                _tracker.OwningRealm = (byte)Realms.REALMS_REALM_NEUTRAL;
                dirty = true;
            }

            if (!IsSupportedRealm(_tracker.LastScoringRealm))
            {
                _tracker.LastScoringRealm = (byte)Realms.REALMS_REALM_NEUTRAL;
                dirty = true;
            }

            if ((LotdTrackerState)_tracker.State == LotdTrackerState.Paused)
            {
                if (_tracker.OwningRealm == (byte)Realms.REALMS_REALM_NEUTRAL || !_tracker.UnlockEndsOnUtc.HasValue)
                {
                    ResetTrackerForRace();
                    dirty = true;
                }
                else if (_tracker.UnlockEndsOnUtc.Value <= DateTime.UtcNow)
                {
                    ResumeRace(broadcast);
                    return;
                }
            }
            else
            {
                if (_tracker.UnlockEndsOnUtc.HasValue)
                {
                    _tracker.UnlockEndsOnUtc = null;
                    dirty = true;
                }

                if (_tracker.OwningRealm != (byte)Realms.REALMS_REALM_NEUTRAL)
                {
                    _tracker.OwningRealm = (byte)Realms.REALMS_REALM_NEUTRAL;
                    dirty = true;
                }

                if (_tracker.OrderResourcePoints >= _tracker.Threshold)
                {
                    SetPausedState(Realms.REALMS_REALM_ORDER, broadcast);
                    return;
                }

                if (_tracker.DestructionResourcePoints >= _tracker.Threshold)
                {
                    SetPausedState(Realms.REALMS_REALM_DESTRUCTION, broadcast);
                    return;
                }
            }

            if (!dirty)
                return;

            SaveTracker();
        }

        private static void SetPausedState(Realms owningRealm, bool broadcast)
        {
            _tracker.State = (byte)LotdTrackerState.Paused;
            _tracker.OwningRealm = (byte)owningRealm;
            _tracker.UnlockEndsOnUtc = DateTime.UtcNow.AddMinutes(_tracker.UnlockDurationMinutes);
            _lastBroadcastRemainingMinutes = GetRemainingOpenMinutes();

            SaveTracker();

            if (!broadcast)
                return;

            BroadcastUnlockMessages(owningRealm);
            BroadcastTrackerUpdate();
        }

        private static void ResumeRace(bool broadcast)
        {
            ResetTrackerForRace();
            _lastBroadcastRemainingMinutes = -1;
            SaveTracker();

            if (!broadcast)
                return;

            BroadcastResumeMessage();
            BroadcastTrackerUpdate();
        }

        private static void ResetTrackerForRace()
        {
            _tracker.State = (byte)LotdTrackerState.Active;
            _tracker.OwningRealm = (byte)Realms.REALMS_REALM_NEUTRAL;
            _tracker.OrderResourcePoints = 0;
            _tracker.DestructionResourcePoints = 0;
            _tracker.UnlockEndsOnUtc = null;
            _tracker.LastScoringRealm = (byte)Realms.REALMS_REALM_NEUTRAL;
        }

        private static void SaveTracker()
        {
            if (_tracker == null)
                return;

            _tracker.LastUpdatedOnUtc = DateTime.UtcNow;
            _tracker.Dirty = true;
            _tracker.IsValid = true;
            Database.SaveObject(_tracker);
            Database.ForceSave();
            _tracker.Dirty = false;
        }

        private static void BroadcastTrackerUpdate()
        {
            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player == null || player.IsDisposed || !player.IsInWorld())
                        continue;

                    SendResourceTracker(player);
                }
            }
        }

        private static void BroadcastUnlockMessages(Realms owningRealm)
        {
            string[] messageArgs = { GetRealmName(owningRealm) };
            ChatLogFilters filter = owningRealm == Realms.REALMS_REALM_ORDER
                ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE
                : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player == null || player.IsDisposed || !player.IsInWorld())
                        continue;

                    player.SendLocalizeString(messageArgs, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.TEXT_TOMB_KINGS_DUNGEON_ACCESS_LINE1);
                    player.SendLocalizeString(messageArgs, filter, Localized_text.TEXT_TOMB_KINGS_DUNGEON_ACCESS_LINE1);
                    player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.TEXT_TOMB_KINGS_DUNGEON_ACCESS_LINE2);
                    player.SendLocalizeString(filter, Localized_text.TEXT_TOMB_KINGS_DUNGEON_ACCESS_LINE2);
                }
            }
        }

        private static void BroadcastResumeMessage()
        {
            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player == null || player.IsDisposed || !player.IsInWorld())
                        continue;

                    player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.TEXT_TOMB_KINGS_RRQ_UNPAUSED);
                }
            }
        }

        private static PacketOut BuildTrackerPacket(Player player)
        {
            PacketOut packet = new PacketOut((byte)WorldServer.NetWork.Opcodes.F_RRQ, 44);

            if (_tracker == null)
            {
                packet.WriteByte(RetailTrackerCount);
                packet.WriteByte(RetailTrackerId);
                packet.WriteUInt32(0);
                packet.Fill(0, 38);
                return packet;
            }

            packet.WriteByte(RetailTrackerCount);
            packet.WriteByte(RetailTrackerId);
            packet.WriteUInt32(RetailTrackerHeaderValue);
            packet.WriteByte(RetailDisplayType);
            packet.WriteByte(RetailHeaderByte7);
            packet.WriteByte(BuildHeaderTimerValue());
            packet.WriteByte(BuildHeaderRealmValue(player));
            packet.WriteByte(RetailHeaderByte10);
            packet.WriteByte(RetailHeaderByte11);
            packet.Fill(0, 7);

            WriteRealmProgress(packet, (byte)Realms.REALMS_REALM_ORDER, _tracker.OrderResourcePoints);
            WriteRealmProgress(packet, (byte)Realms.REALMS_REALM_DESTRUCTION, _tracker.DestructionResourcePoints);

            return packet;
        }

        private static byte BuildHeaderTimerValue()
        {
            if ((LotdTrackerState)_tracker.State != LotdTrackerState.Paused)
                return 0;

            return (byte)GetRemainingOpenMinutes();
        }

        private static byte BuildHeaderRealmValue(Player player)
        {
            // When LotD is locked (Paused state) a specific realm won access — show them as the owner.
            if ((LotdTrackerState)_tracker.State == LotdTrackerState.Paused &&
                _tracker.OwningRealm != (byte)Realms.REALMS_REALM_NEUTRAL)
            {
                return _tracker.OwningRealm;
            }

            // When LotD is still being contested (Active state), nobody owns it yet.
            // Show whichever realm is currently leading so both factions see the same value.
            // Never return the *viewing player's* realm — that made every player think they owned LotD.
            if (_tracker.LastScoringRealm != (byte)Realms.REALMS_REALM_NEUTRAL)
                return _tracker.LastScoringRealm;

            return (byte)Realms.REALMS_REALM_NEUTRAL;
        }

        private static void WriteRealmProgress(PacketOut packet, byte realm, int score)
        {
            packet.WriteByte(realm);
            packet.WriteByte(RetailRealmBlockByte1);
            packet.WriteByte(RetailRealmBlockByte2);
            packet.WriteUInt32((uint)_tracker.Threshold);
            packet.WriteUInt32((uint)Math.Max(0, score));
        }

        private static int GetRemainingOpenMinutes()
        {
            if (_tracker == null || !_tracker.UnlockEndsOnUtc.HasValue)
                return 0;

            double remaining = (_tracker.UnlockEndsOnUtc.Value - DateTime.UtcNow).TotalMinutes;
            if (remaining <= 0)
                return 0;

            return Math.Min(byte.MaxValue, (int)Math.Ceiling(remaining));
        }

        private static int GetRealmScore(Realms realm)
        {
            return realm == Realms.REALMS_REALM_ORDER
                ? _tracker.OrderResourcePoints
                : _tracker.DestructionResourcePoints;
        }

        private static bool IsEligibleBattlefrontZone(ushort zoneId)
        {
            if (zoneId == 0 || zoneId == LotdZoneId)
                return false;

            foreach (ushort fortZone in FortZones)
            {
                if (zoneId == fortZone)
                    return false;
            }

            return true;
        }

        private static bool IsSupportedRealm(byte realm)
        {
            return realm == (byte)Realms.REALMS_REALM_NEUTRAL ||
                   realm == (byte)Realms.REALMS_REALM_ORDER ||
                   realm == (byte)Realms.REALMS_REALM_DESTRUCTION;
        }

        private static string GetRealmName(Realms realm)
        {
            return realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction";
        }
    }
}
