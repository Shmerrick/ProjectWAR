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
        /// <summary> Flag without the bright halo </summary>
        Abandoned = 2,
        /// <summary>A realm in contesting the objective to the other</summary>
        Contested = 4,
        /// <summary>Locked (because of Campaign state)</summary>
        Locked = 8,
        /// <summary>Secured (or securing), have bright halo</summary>
        Secure = 16,

        ZoneLocked = 9
    };
}
