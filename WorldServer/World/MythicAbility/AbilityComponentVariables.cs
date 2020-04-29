using Common.Database.World.MythicAbility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.MythicAbility
{    public class AbilityComponentVariables
    {
        public ushort ID;
        public string A00;
        public string Values;
        public ushort Multipliers;
        public ushort ActivationDelay;
        public ushort Duration;
        public ushort Flags;
        public ushort IconAlwaysVisible;
        public ushort Operation;
        public ushort Interval;
        public ushort Radius;
        public ushort ConeAngle;
        public ushort FlightSpeed;
        public ushort A15;
        public ushort MaxTargets;
        public ushort Description;

        public AbilityComponentVariables()
        { }

    }
}
