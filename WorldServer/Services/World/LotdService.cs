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

            // Two different packets were previously sent together and then suppressed together.
            //
            // The zone activation names zone 191, and the client treats that as entering the
            // zone: it shows the "Necropolis of Zandri" title card over whatever zone the player
            // is really in. That one genuinely must be limited to players who are there.
            //
            // The F_RRQ tracker packet must not be. It is the only source of the client's
            // RRQ table (EASystem_RRQ GetRRQData), and the world map, the HUD tracker and the
            // flight master all render straight from that table: EA_Window_WorldMap.ShouldShowRRQ
            // gates on the map view, never on the player's zone, and EA_Window_RRQTracker is a
            // HUD element. Suppressing it outside zone 191 meant the Land of the Dead bars could
            // only ever appear to a player already standing in Land of the Dead.
            //
            // Official captures settle it: F_RRQ (0x74) is sent 13 times during a Chaos Wastes
            // RvR session, 48 times during an Inevitable City siege and 29 times in Caledor,
            // the first within the login burst each time.
            // SendRvrTracker re-activates the tracker for whatever zone and area the player is
            // standing in, so it belongs with the zone activation, not with every broadcast.
            if (player.Zone != null && player.Zone.ZoneId == LotdZoneId)
            {
                player.SendObjectiveTrackerActivation(LotdZoneId, 0);
                player.SendRvrTracker();
            }

            player.SendPacket(BuildTrackerPacket(player));
        }

        /// <summary>
        /// Whether this destination may actually be flown to right now. The Land of the Dead
        /// entry is always *listed*; this only decides the availability byte the client reads as
        /// flightData.zoneAvailable.
        ///
        /// The client is built around the destination being present and disabled rather than
        /// missing: EA_InteractionFlightMasterWindow.ZoneNumbersLookup hard-codes zone 191 for
        /// both realms, ShowDefaultFrame disables every button and re-enables only what the
        /// server lists, and OnMouseOverFlightMapPoint has a dedicated zone-191 branch that
        /// prints TOOLTIP_TRAVEL_WINDOW_LAND_OF_DEAD_REQUIREMENTS -- "Locked for one or more
        /// reasons: ... Your realm currently does not have an active expedition to the Land of
        /// the Dead." Dropping the row from the list left the Tomb Kings map blank with no
        /// explanation instead.
        /// </summary>
        public static bool IsTaxiAvailable(Player player, Zone_Taxi taxi)
        {
            if (taxi == null)
                return false;

            if (taxi.ZoneID != LotdZoneId)
                return true;

            return CanRealmAccessLotd(player?.Realm ?? Realms.REALMS_REALM_NEUTRAL);
        }

        /// <summary>
        /// Whether a realm currently holds the expedition, and so may fly to Land of the Dead.
        ///
        /// Ownership is *not* limited to the pause window. The Paused state freezes the resource
        /// race for <see cref="LotdResourceTracker.UnlockDurationMinutes"/> after a win; the
        /// winning realm keeps access after it expires, until the other realm wins.
        ///
        /// Official capture PvE_Landofdead_SHAMY40RR95 shows this directly: a Destruction player
        /// quests in zone 191 for the whole session while the tracker header carries timer 0 and
        /// realm 2, i.e. no active pause but Destruction still holding. At ordinal 21916 Order
        /// crosses the threshold and the header flips to timer 30 / realm 1 in one packet.
        /// Requiring Paused here limited access to 30 minutes per win, which no capture supports.
        /// </summary>
        public static bool CanRealmAccessLotd(Realms realm)
        {
            if (_tracker == null)
                return false;

            // Neutral means nobody has won the expedition yet.
            if (_tracker.OwningRealm == (byte)Realms.REALMS_REALM_NEUTRAL)
                return false;

            return _tracker.OwningRealm == (byte)realm;
        }

        /// <summary>
        /// Human-readable tracker state, for GM diagnostics. Only the owning realm can fly to Land of
        /// the Dead, and a realm holds it until the other realm wins, so "who owns it" is the single
        /// question behind a flight master that appears to be refusing travel.
        /// </summary>
        public static string GetStatusSummary()
        {
            if (_tracker == null)
                return "Land of the Dead tracker is not loaded, so neither realm can reach it. Apply Database/update_005_lotd_resource_tracker.sql.";

            string race = (LotdTrackerState)_tracker.State == LotdTrackerState.Paused
                ? "race paused " + GetRemainingOpenMinutes() + "/" + _tracker.UnlockDurationMinutes + " min"
                : "race running";

            string access = _tracker.OwningRealm == (byte)Realms.REALMS_REALM_NEUTRAL
                ? "nobody -- neither realm can travel until one wins (.lotd unlock <realm> to stage it)"
                : ((Realms)_tracker.OwningRealm) + " (holds it until the other realm wins)";

            return "LOTD " + race
                 + " | expedition held by " + access
                 + " | Order " + _tracker.OrderResourcePoints + "/" + _tracker.Threshold
                 + " | Destruction " + _tracker.DestructionResourcePoints + "/" + _tracker.Threshold
                 + " | +" + _tracker.PointsPerBattlefrontLock + " per T4 battlefront lock";
        }

        /// <summary>
        /// Reports the Land of the Dead record exactly as the flight-master packet will carry it, for the
        /// player asking.
        ///
        /// The client discards a destination whose pairing is outside 1..3 and the expansion-map range,
        /// and it disables any zone the server does not list, so a flight master that silently refuses
        /// Land of the Dead has three possible causes that look identical in game: the taxi row is not in
        /// the list at all, its pairing is wrong, or the availability byte is 0. This prints which.
        /// </summary>
        public static string GetTaxiDiagnostic(Player player)
        {
            if (player == null)
                return "No player.";

            System.Collections.Generic.List<Zone_Taxi> destinations = WorldMgr.GetTaxis(player);

            for (int i = 0; i < destinations.Count; ++i)
            {
                Zone_Taxi taxi = destinations[i];
                if (taxi.ZoneID != LotdZoneId)
                    continue;

                return "Flight list: " + destinations.Count + " destinations; Land of the Dead is #" + (i + 1)
                     + " with pairing " + (taxi.Info?.Pairing.ToString() ?? "?")
                     + " (client keeps 1-3 and 100+; 4 is discarded), price " + (taxi.Info?.Price.ToString() ?? "?")
                     + ", available byte " + (IsTaxiAvailable(player, taxi) ? "1" : "0") + ".";
            }

            return "Flight list: " + destinations.Count + " destinations, and Land of the Dead is NOT among them, "
                 + "so the client cannot enable it.";
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

            // Grant the expedition in its settled state -- holder set, race running -- rather than in
            // the 30-minute post-win pause.
            //
            // The pause is what the client calls "a massive war is currently underway in your realm's
            // expedition camp; airships cannot safely land there at the moment, check back in a few
            // minutes" (TOOLTIP_TRAVEL_WINDOW_LAND_OF_DEAD_REQUIREMENTS). Staging a win with
            // SetPausedState therefore handed the tester the one state in which travel is expected to
            // be refused, which is the opposite of what the command is for. The captures show the
            // settled state is also the common one: the Inevitable City and Land of the Dead sessions
            // both run at timer 0 with a realm still holding the expedition.
            _tracker.State = (byte)LotdTrackerState.Active;
            _tracker.OwningRealm = (byte)realm;
            _tracker.UnlockEndsOnUtc = null;

            if (realm == Realms.REALMS_REALM_ORDER)
                _tracker.OrderResourcePoints = 0;
            else
                _tracker.DestructionResourcePoints = 0;

            _lastBroadcastRemainingMinutes = -1;

            SaveTracker();
            BroadcastUnlockMessages(realm);
            BroadcastTrackerUpdate();
            return true;
        }

        /// <summary>Returns the tracker to the accumulating race and clears ownership. Testing aid.</summary>
        public static bool ForceReset()
        {
            if (_tracker == null)
                return false;

            ResetTrackerForRace();
            _lastBroadcastRemainingMinutes = -1;
            SaveTracker();
            BroadcastResumeMessage();
            BroadcastTrackerUpdate();
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

                // Ownership deliberately survives here. An Active tracker with an owning realm is
                // the normal steady state -- the race running again after a win, with the winner
                // still holding the expedition -- and clearing it on every boot would have
                // revoked flight access at each restart.

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

            // Capture PvE_Landofdead_SHAMY40RR95 #21808 -> #21916: Order goes 448/500 -> 0/500 in
            // the packet that awards it the expedition, while Destruction stays untouched on
            // 256/500. Only the winner's progress is spent.
            if (owningRealm == Realms.REALMS_REALM_ORDER)
                _tracker.OrderResourcePoints = 0;
            else if (owningRealm == Realms.REALMS_REALM_DESTRUCTION)
                _tracker.DestructionResourcePoints = 0;

            _lastBroadcastRemainingMinutes = GetRemainingOpenMinutes();

            SaveTracker();

            if (!broadcast)
                return;

            BroadcastUnlockMessages(owningRealm);
            BroadcastTrackerUpdate();
        }

        /// <summary>
        /// Ends the post-win pause and lets the resource race accumulate again.
        ///
        /// This does not clear ownership or either realm's score. Capture
        /// 2013-09-25 Inevitable City shows the tracker running with timer 0, realm 2 still shown
        /// as the holder, and Order's total climbing 26 -> 30 -> 42 across the session; the
        /// Chaos Wastes capture shows Order frozen on 431 for the whole 29 -> 17 minute pause.
        /// So the pause freezes scoring, and the holder persists through it and beyond.
        /// </summary>
        private static void ResumeRace(bool broadcast)
        {
            _tracker.State = (byte)LotdTrackerState.Active;
            _tracker.UnlockEndsOnUtc = null;
            _lastBroadcastRemainingMinutes = -1;
            SaveTracker();

            if (!broadcast)
                return;

            BroadcastResumeMessage();
            BroadcastTrackerUpdate();
        }

        /// <summary>
        /// Returns the tracker to a clean, unowned race. Only for an explicit GM reset and for
        /// repairing a row that is Paused with no owner or no expiry -- never on normal unpause.
        /// </summary>
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

        /// <summary>
        /// Header byte 9, which the client reads as rrqData.realmWithAccess and uses to show the
        /// holder's emblem beside the bars.
        ///
        /// This is the realm holding the expedition, and it persists while the race runs again
        /// after a win -- captures show realm 2 held steady across a whole Inevitable City siege
        /// session at timer 0. It is never the viewing player's realm and never the current
        /// leader; before anyone has won it is neutral.
        /// </summary>
        private static byte BuildHeaderRealmValue(Player player)
        {
            return _tracker.OwningRealm;
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
