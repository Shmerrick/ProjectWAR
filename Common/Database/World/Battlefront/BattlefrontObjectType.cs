
namespace Common.Database.World.BattleFront
{
    /// <summary>
    /// Enumeration of BattleFront object types.
    /// </summary>
    public enum BattleFrontObjectType : ushort
    {
        /// <summary>Point marking the entrance of warcamps.</summary>
        /// <remarks>Necessary for warcamp farm debuffs and objective reward scalers</remarks>
        WARCAMP_ENTRANCE = 0,
        /// <summary>Portal in lakes allowing to port to warcamp.</summary>
        WARCAMP_PORTAL = 1,
        /// <summary>Portal in warcamps allowing players to port to battlefield objectives.</summary>
        OBJECTIVE_PORTAL = 2,
    }
}
