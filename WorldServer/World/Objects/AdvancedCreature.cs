using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using Common;
using FrameWork;
using NLog;
using WorldServer.World.AI;
using WorldServer.World.AI.BT;

namespace WorldServer.World.Objects
{
    /// <summary>
    /// WIP - thinking about a cut down, but smarter mob
    /// </summary>
    public class AdvancedCreature : Creature, IClock
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public new Creature_spawn Spawn;
        public uint ProtoEntryId { get; set; }
        protected new byte Scale { get; set; }
        public new ushort Ranged { get; set; }
        public new ushort Model1 { get; set; }
        public new ushort Model2 { get; set; }

        public List<Waypoint> Waypoints { get; set; }

        public AdvancedCreature(Creature_spawn spawn)
        {
            if (spawn == null)
                throw new ArgumentNullException("NULL spawn passed to AdvancedCreature.");
            Spawn = spawn;
            Name = spawn.Proto.Name;
            Ranged = spawn.Proto.Ranged;
            Model1 = spawn.Proto.Model1;
            Model2 = spawn.Proto.Model2;
            if (spawn.Proto.Invulnerable == 1)
                IsInvulnerable = true;

            Scale = (byte)StaticRandom.Instance.Next(Spawn.Proto.MinScale, Spawn.Proto.MaxScale);

            if (spawn.Proto.BaseRadiusUnits > 0)
                BaseRadius = spawn.Proto.BaseRadiusUnits * (Scale / 50f) / UNITS_TO_FEET;
            else
                BaseRadius *= (Scale / 50f);

            
        }
        public new long GetTimeStampInMilliseconds()
        {
            return DateTime.Now.Millisecond;
        }


        public override void OnLoad()
        {
            _logger.Trace($"Calling AdvCreature.OnLoad");
            base.OnLoad();
        }

    }
}
