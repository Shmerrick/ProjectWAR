using FrameWork;
using NLog;
using System;

namespace WorldServer.World.Battlefronts.Keeps
{
    public class KeepTimer
    {
        public string KeepTimerName { get; set; }
        public int Value { get; set; }
        public int Length { get; set; }
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public KeepTimer(string keepTimerName, int value, int length)
        {
            KeepTimerName = keepTimerName;
            Value = value;
            Length = length;

            _logger.Trace($"{this.KeepTimerName} created, value={Value}, length={Length}");
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return new DateTime(1970, 1, 1).AddSeconds(unixTime);
        }

        public int Start()
        {
            Value = TCPManager.GetTimeStamp() + Length;
            _logger.Trace($"{this.KeepTimerName} started, value={Value} ({FromUnixTime(Value).ToString("R")})");

            return Value;
        }

        public bool IsExpired()
        {
            if (Value > 0)
            {
                if (TCPManager.GetTimeStamp() > Value)
                {
                    _logger.Debug($"{this.KeepTimerName} expired, value={Value}, resetting.");
                    Reset();
                    return true;
                }
                else
                    return false;
            }

            return false;
        }

        public void Reset()
        {
            _logger.Debug($"{this.KeepTimerName} reset");
            Value = 0;
        }
    }
}