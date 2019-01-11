using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using GameData;
using NLog;

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

            _logger.Debug($"{this.KeepTimerName} created, value={Value}, length={Length}");
        }
      
        public int Start()
        {
            Value = TCPManager.GetTimeStamp() + Length;
            _logger.Debug($"{this.KeepTimerName} started, value={Value}");

            return Value;
        }

        public bool IsExpired()
        {
            if (Value > 0)
            {
                if (TCPManager.GetTimeStamp() > Length)
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
