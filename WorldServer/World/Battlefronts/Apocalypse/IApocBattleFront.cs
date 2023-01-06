using Common;
using FrameWork;
using GameData;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    /// <summary>
    /// RegionManagers as seen by external parts of the server.
    /// </summary>
    /// <remarks>
    /// Both lecacy and RoR battlefonts implement this interface.
    /// </remarks>
    public interface IApocBattleFront
    {
        /// <summary>
        /// Main Campaign update method, invoked by region manager, short perdiod.
        /// </summary>
        /// <param name="start">Timestamp of region manager update start time</param>
        void Update(long start);

        /// <summary>
        /// Notifies the given player has entered the lake,
        /// removing it from the Campaign's active players list and setting the rvr buff(s).
        /// </summary>
        /// <param name="plr">Player to add, not null</param>
        void NotifyEnteredLake(Player plr);

        /// <summary>
        /// Notifies the given player has left the lake,
        /// removing it from the Campaign's active players lift and removing the rvr buff(s).
        /// </summary>
        /// <param name="plr">Player to remove, not null</param>
        void NotifyLeftLake(Player plr);

        /// <summary>
        /// Locks a pairing, preventing any interaction with objectives within.
        /// </summary>
        /// <param name="realm">Realm that locked the Campaign</param>
        void LockPairing(Realm realm);

        /// <summary>
        /// Returns the pairing to its open state, allowing interaction with objectives.
        /// </summary>
        void ResetPairing();

        /// <summary>
        /// Gets the active zone name for player chat display purpose.
        /// </summary>
        string ActiveZoneName { get; }

        #region Send

        /// <summary>
        /// Sends information to a player about the objectives within a Campaign upon their entry.
        /// </summary>
        /// <param name="plr">Player to send list to</param>
        void SendObjectives(Player plr);

        /// <summary>
        /// Writes current capture status of the Campaign in output.
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
        void WriteBattleFrontStatus(PacketOut Out);

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

        #endregion Send
    }
}