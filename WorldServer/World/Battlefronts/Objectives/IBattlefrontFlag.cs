using GameData;

namespace WorldServer.World.Battlefronts.Objectives
{
    /// <summary>
    /// Battlefront flags as seen by external parts of the server.
    /// </summary>
    /// <remarks>
    /// Both lecacy and RoR flags implement this interface.
    /// </remarks>
    public interface IBattlefrontFlag
    {

        /// <summary>
        /// Flag's name.
        /// </summary>
        string ObjectiveName { get; }

        /// <summary>
        /// Gets the current flag state (captured, contested, etc.)
        /// </summary>
        ObjectiveFlags FlagState { get; }

        /// <summary>
        /// Gets the current flag state (captured, contested, etc.)
        /// </summary>
        Realms OwningRealm { get; }

        // TODO rename "handle kill" because it does not check anything.
        /// <summary>
        /// Handle a player kill, incrementing internal kill count.
        /// </summary>
        /// <param name="player">Player that was killed</param>
        /// <returns>unused</returns>
        bool CheckKillValid(Player player);

        /// <summary>
        /// Grants rewards upon a zone lock, depending on acumulated kills.
        /// </summary>
        void GrantKeepCaptureRewards();

        /// <summary>
        /// Stores kill-based delayed rewards for the quadrant represented by this battlefield objective.
        /// </summary>
        /// <param name="killer">Player who killed</param>
        /// <param name="killer">Player who was killed</param>
        /// <param name="xpShare">Total amount of xp gained (scaled?)</param>
        /// <param name="xpShare">Total amount of renown gained (scaled?)</param>
        void AddDelayedRewardsFrom(Player killer, Player killed, uint xpShare, uint renownShare);
        
    }
}
