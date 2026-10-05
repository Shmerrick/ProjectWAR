using System.Globalization;
using Common;
using NLog;
using WorldServer.World.Objects;

namespace WorldServer.Managers
{
    /// <summary>
    /// One line per item or gold movement between players, written to logs/ITEMS_{date}.log.
    ///
    /// Duplicated items and gold only matter once they change hands, so this records the paths they
    /// change hands through -- trade, mail, guild vault, auction and the Black Market -- and nothing
    /// else (loot, vendors and crafting would multiply the volume roughly twenty-fold for little
    /// tracing value). It cannot prevent a duplicate; it lets one be traced afterwards.
    ///
    /// Format: <c>{timestamp}|{channel}|{action}|from={id}:{name}|to={id}:{name}|item={entry}x{count}|money={copper}|ref={id}</c>,
    /// with absent fields omitted. Lines are written at Info through NLog's asynchronous queue, which
    /// discards rather than blocks when full, so the log is near-complete rather than guaranteed.
    /// Movements are player actions, never per-tick work; the message is only built when enabled.
    /// </summary>
    public static class ItemAudit
    {
        private static readonly Logger _logger = LogManager.GetLogger("ItemAudit");

        public static void Item(string channel, string action, uint fromId, string fromName, uint toId, string toName, uint entry, ushort count, ulong reference = 0)
        {
            if (!_logger.IsInfoEnabled)
                return;
            _logger.Info(Format(channel, action, fromId, fromName, toId, toName, entry, count, 0, reference));
        }

        public static void Money(string channel, string action, uint fromId, string fromName, uint toId, string toName, uint money, ulong reference = 0)
        {
            if (!_logger.IsInfoEnabled || money == 0)
                return;
            _logger.Info(Format(channel, action, fromId, fromName, toId, toName, 0, 0, money, reference));
        }

        public static void Item(string channel, string action, Player from, Player to, uint entry, ushort count, ulong reference = 0)
        {
            Item(channel, action, from?.CharacterId ?? 0, from?.Name, to?.CharacterId ?? 0, to?.Name, entry, count, reference);
        }

        public static void Money(string channel, string action, Player from, Player to, uint money, ulong reference = 0)
        {
            Money(channel, action, from?.CharacterId ?? 0, from?.Name, to?.CharacterId ?? 0, to?.Name, money, reference);
        }

        private static string Format(string channel, string action, uint fromId, string fromName, uint toId, string toName,
            uint entry, ushort count, uint money, ulong reference)
        {
            var line = new System.Text.StringBuilder(160);
            line.Append(channel).Append('|').Append(action);
            if (fromId != 0 || fromName != null)
                line.Append("|from=").Append(fromId.ToString(CultureInfo.InvariantCulture)).Append(':').Append(fromName);
            if (toId != 0 || toName != null)
                line.Append("|to=").Append(toId.ToString(CultureInfo.InvariantCulture)).Append(':').Append(toName);
            if (entry != 0)
                line.Append("|item=").Append(entry.ToString(CultureInfo.InvariantCulture)).Append('x').Append(count.ToString(CultureInfo.InvariantCulture));
            if (money != 0)
                line.Append("|money=").Append(money.ToString(CultureInfo.InvariantCulture));
            if (reference != 0)
                line.Append("|ref=").Append(reference.ToString(CultureInfo.InvariantCulture));
            return line.ToString();
        }
    }
}
