using FrameWork;
using GameData;
using System.Collections.Generic;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Objects.PublicQuests;

namespace WorldServer.World.Battlefronts
{
    /// <summary>
    /// RegionManagers as seen by external parts of the server.
    /// </summary>
    /// <remarks>
    /// Both lecacy and RoR battlefonts implement this interface.
    /// </remarks>
    public interface IBattlefront
    {
        /// <summary>
        /// Main battlefront update method, invoked by region manager, short perdiod.
        /// </summary>
        /// <param name="start">Timestamp of region manager update start time</param>
        void Update(long start);

        /// <summary>
        /// Notifies the given player has entered the lake,
        /// removing it from the battlefront's active players list and setting the rvr buff(s).
        /// </summary>
        /// <param name="plr">Player to add, not null</param>
        void NotifyEnteredLake(Player plr);

        /// <summary>
        /// Notifies the given player has left the lake,
        /// removing it from the battlefront's active players lift and removing the rvr buff(s).
        /// </summary>
        /// <param name="plr">Player to remove, not null</param>
        void NotifyLeftLake(Player plr);

        /// <summary>
        /// Locks a pairing, preventing any interaction with objectives within.
        /// </summary>
        /// <param name="realm">Realm that locked the battlefront</param>
        /// <param name="announce">True to announce the lock to players</param>
        void LockPairing(Realms realm, bool announce, bool restoreStatus = false, bool noRewards = false, bool draw = false);

        /// <summary>
        /// Returns the pairing to its open state, allowing interaction with objectives.
        /// </summary>
        void ResetPairing();

        /// <summary>
        /// Enables the supplies in T4 zone...?
        /// </summary>
        // void EnableSupplies();

        /// <summary>
        /// Gets the active zone name for player chat display purpose.
        /// </summary>
        string ActiveZoneName { get; }

        void SupplyLineReset();

        #region Players
        /// <summary>
        /// Increases the value of the closest battlefield objective to the kill and determines reward scaling based on proximity to the objective. 
        /// </summary>
        /// <remarks>Has no effect in locked areas</remarks>
        float ModifyKill(Player killer, Player killed);

        /// <summary>
        /// Checks whether kills are prevented from granting rewards.
        /// </summary>
        bool PreventKillReward();

        /// <summary>
        /// <para>Adds contribution for a player. This is based on renown earned and comes from 4 sources at the moment:</para>
        /// <para>- Killing players.</para>
        /// <para>- Objective personal capture rewards.</para>
        /// <para>- Objective defense tick rewards.</para>
        /// <para>- Destroying siege weapons.</para>
        /// </summary>
        /// <param name="plr">Player to give contribution to</param>
        /// <param name="contribution">Contribution value, will be scaled to compute rewards</param>
        void AddContribution(Player plr, uint contribution);

        /// <summary>
        /// Gets a ream players contribution.
        /// </summary>
        /// <returns>Contribution infos indexed by character id</returns>
        Dictionary<uint, ContributionInfo> GetContributorsFromRealm(Realms realm);

        /// <summary>
        /// Scales battlefield objective rewards by the following factors:
        /// <para>- The internal AAO</para>
        /// <para>- The relative activity in this battlefront compared to others in its tier</para>
        /// <para>- The total number of people fighting</para>
        /// <para>- The capturing realm's population at this objective.</para>
        /// </summary>
        /// <param name="capturingRealm">Objective owner realm</param>
        /// <param name="playerCount">Number of attacking players in range of the objective</param>
        float GetObjectiveRewardScaler(Realms capturingRealm, int playerCount);

        /// <summary>
        /// Gets the artillery damage scale factor.
        /// </summary>
        /// <param name="realm">Owner realm of the weapon</param>
        /// <returns>Scale factor</returns>
        float GetArtilleryDamageScale(Realms realm);

        /// <summary>
        /// Gets a factor depending on the relation between the two realms.
        /// </summary>
        /// <param name="realm">Realm to apply value to</param>
        /// <remarks>Higher if enemy realm's population is lower.</remarks>
        /// <returns>Scalar value</returns>
        float GetLockPopulationScaler(Realms realm);

        /// <summary>
        /// A scaler for the reward of objectives captured in this battlefront, based on its activity relative to other fronts of the same tier.
        /// </summary>
        float RelativeActivityFactor { get; }

        /// <summary>
        /// Gets the ration factor that should be applied to given unit.
        /// </summary>
        /// <param name="unit">To applie factor to, not null (always Player?)</param>
        /// <returns>Factor less or equal 1f</returns>
        float GetRationFactor(Unit unit);
        #endregion

        #region Send
        /// <summary>
        /// Sends information to a player about the objectives within a battlefront upon their entry.
        /// </summary>
        /// <param name="plr">Player to send list to</param>
        void SendObjectives(Player plr);

        /// <summary>
        /// Writes current capture status of the battlefront in output.
        /// </summary>
        /// <param name="Out">TCP output</param>
        void WriteCaptureStatus(PacketOut Out);

        /// <summary>
        /// Writes current front victory points.
        /// </summary>
        /// <param name="realm">Recipent player's realm</param>
        /// <param name="Out">TCP output</param>
        void WriteVictoryPoints(Realms realm, PacketOut Out);

        /// <summary>
        /// Writes battle front advancement status (t4 only, otherwise throws a notimplemented exception).
        /// </summary>
        /// <param name="Out">TCP output</param>
        void WriteBattlefrontStatus(PacketOut Out);

        /// <summary>
        /// Broadcasts a message to all valid players in the lake.
        /// </summary>
        /// <param name="message">Message text to send</param>
        void Broadcast(string message);

        /// <summary>
        /// Broadcasts a message to all valid players in the lake.
        /// </summary>
        /// <param name="message">Message text to send</param>
        /// <param name="realm">Realm filter</param>
        void Broadcast(string message, Realms realm);

        /// <summary>
        /// Sends campain diagnostic information to player (gm only).
        /// </summary>
        /// <param name="player">GM to send data to</param>
        /// <param name="bLocalZone">True to display player's local zone, false for tier zones</param>
        void CampaignDiagnostic(Player player, bool bLocalZone);
        #endregion

        #region Objectives
        /// <summary>
        /// Checks whether the given realm can reclaim opposite keep.
        /// </summary>
        /// <param name="realm">Realm to check</param>
        /// <returns>True if can reclaim</returns>
        bool CanReclaimKeep(Realms realm);

        /// <summary>
        /// Checks whether the given realm's keep can sustain it's current rank.
        /// </summary>
        /// <param name="realm">Realm to check</param>
        /// <param name="resourceValueMax">Max resource value of the keep to check (in its current rank)</param>
        /// <returns>True if can sustain</returns>
        /// <remarks>
        /// May move this method to Keep class, kept it here for compatibility break risks.
        /// </remarks>
        bool CanSustainRank(Realms realm, int resourceValueMax);

        /// <summary>
        /// List of existing battlefield objectives within this battlefront.
        /// </summary>
        IEnumerable<IBattlefrontFlag> Objectives { get; }

        /// <summary>
        /// List of existing keeps in battlefront.
        /// </summary>
        /// <remarks>
        /// Must not be updated outside battlefront implementations.
        /// </remarks>
        List<Keep> Keeps { get; }

        /// <summary>
        /// Utility method returning the closest keep of the given point.
        /// </summary>
        /// <param name="destPos">Point to search flag from</param>
        /// <returns>Keep or null if battlefront has noo keep</returns>
        Keep GetClosestKeep(Point3D destPos);

        /// <summary>
        /// Utility method returning the closest flag of the given point.
        /// </summary>
        /// <param name="destPos">Point to search flag from</param>
        /// <param name="inPlay">True to require the returned flag being in an active zone</param>
        /// <returns>Flag (or null if battlefront has no flag)</returns>
        IBattlefrontFlag GetClosestFlag(Point3D destPos, bool inPlay = false);
        #endregion
        
        /// <summary>For legacy purpose.</summary>
        bool NoSupplies { get; }

        /// <summary>For legacy purpose.</summary>
        float GetControlHighFor(IBattlefrontFlag currentFlag, Realms realm);
    }
}
