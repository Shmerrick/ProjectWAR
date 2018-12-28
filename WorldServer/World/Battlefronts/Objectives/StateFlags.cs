using System;

namespace WorldServer.World.BattleFronts.Objectives
{
    /// <summary>
    /// Bit flags describing an objective state.
    /// </summary>
    [Flags]
    public enum StateFlags
    {
        /// <summary>The flag is not secured by any realm (neutral)</summary>
        Unsecure = 0,
        Hidden = 1,
        /// <summary> Flag without the bright halo</summary>
        Secure = 2,
        /// <summary>Neutral Icon on Fire</summary>
        Contested = 4,
        /// <summary>Locked (because of Campaign state)</summary>
        Abandoned = 8,
        /// <summary>Bright halo</summary>
        Locked = 16,

        ZoneLocked = 9
    };
}
