using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Scenarios
{
    public class DominationScenarioEC : DominationScenario
    {

        private int _potralShift = 0;
        private Dictionary<Player, DateTime> _fallGuard = new Dictionary<Player, DateTime>();

        private List<Tuple<uint,uint,ushort,ushort>> _portals = new List<Tuple<uint, uint, ushort, ushort>>()
        {
            new Tuple<uint, uint, ushort, ushort>(540362, 1444127, 25567, 3026),
            new Tuple<uint, uint, ushort, ushort>(542883, 1444145, 25567, 980),
            new Tuple<uint, uint, ushort, ushort>(541668, 1445711, 25858, 2050),
        };


        private List<Point3D> _landingSpots = new List<Point3D>()
        {
            new Point3D(15974,51938,25567),
            new Point3D(15957,52103,25567),
            new Point3D(15951,52297,25567),
            new Point3D(15945,52538,25567),
            new Point3D(16239,52568,25656),
            new Point3D(16439,52544,25783),
            new Point3D(16616,52056,25857),
            new Point3D(17151,51885,25857),
            new Point3D(17828,51985,25857),
            new Point3D(18217,52097,25857),
            new Point3D(17817,52385,25857),
            new Point3D(17375,52285,25858),
            new Point3D(16910,52721,25857),
            new Point3D(17422,52744,25858),
            new Point3D(17870,52744,25857),
            new Point3D(18299,52609,25806),
            new Point3D(18776,52585,25567),
            new Point3D(18799,52456,25567),
            new Point3D(18823,52091,25567),
            new Point3D(18835,51891,25567),
            new Point3D(15986,53044,25857),
            new Point3D(18699,53003,25857),
            new Point3D(18017,52880,25857),
            new Point3D(17057,52597,25858),
            new Point3D(17104,51820,25857),
            new Point3D(17087,52126,25857),
            new Point3D(17575,52303,25857),
            new Point3D(17593,52568,25858),
            new Point3D(17287,52609,25858),
            new Point3D(17063,52550,25858),
            new Point3D(17340,52085,25858),
            new Point3D(17511,52279,25857),
            new Point3D(15904,52350,25567),
            new Point3D(15945,52568,25567)
        };

        public DominationScenarioEC(Scenario_Info info, int tier)
            : base(info, tier)
        {
           
        }

        public override void OnStart()
        {
            base.OnStart();
            foreach (var obj in Region.Objects.Where(e => e != null).ToList())
            {
                Log.Info("Scenario", "Object " + obj.ToString() + " WorldPos " + obj.WorldPosition.ToString()); ;
            }
            Random r = new Random();
            EvtInterface.AddEvent(WindsOfChange, r.Next(15000, 40000), 1);
            EvtInterface.AddEvent(ChaosMonitor, 200, 1);
        }

        public override void OnClose()
        {
            EvtInterface.RemoveEvent(WindsOfChange);
            EvtInterface.RemoveEvent(ChaosMonitor);
            base.OnClose();
        }

        public override void GeneratePoints()
        {
            var players = Region.WorldQuery<Player>(new Point3D((int)_portals[2].Item1, (int)_portals[2].Item2, (int)_portals[2].Item3), 400);

            var orderCount = players.Count(e => e.Realm == Realms.REALMS_REALM_ORDER && !e.IsDead);
            var destroCount = players.Count(e => e.Realm == Realms.REALMS_REALM_DESTRUCTION && !e.IsDead);

            if (orderCount > destroCount)
                GivePoints(1, (uint)orderCount * 2);
            else if (destroCount > orderCount)
                GivePoints(2, (uint)destroCount * 2);
        }

        private void ChaosMonitor()
        {
            lock (_fallGuard)
            {
                foreach (var plr in _fallGuard.Keys.ToList())
                {
                    if(DateTime.Now.Subtract(_fallGuard[plr]).TotalSeconds > 8)
                        _fallGuard.Remove(plr);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    if (plr.Z < 24100 && !plr.IsDead && plr.PuntedBy != null && StaticRandom.Instance.NextDouble() > .70)
                    {
                        SwapFallPlaces(plr, plr.PuntedBy);
                        plr.PuntedBy = null;
                    }
                }
            }

          EvtInterface.AddEvent(ChaosMonitor, 200, 1);
        }

        private void WindsOfChange()
        {
            Random r = new Random();
            _potralShift = r.Next(0, 3);

            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {
                    plr.SendLocalizeString("The Winds of Change have shifted!", 0x0211, Localized_text.CHAT_TAG_DEFAULT);
                    plr.SendLocalizeString("The Winds of Change have shifted!", 0x0117, Localized_text.CHAT_TAG_DEFAULT);
                }
            }

            EvtInterface.AddEvent(WindsOfChange, r.Next(15000, 40000), 1);
        }

        private void SwapFallPlaces(Player fallingPlayer, Unit punter)
        {
          
            var landingSpot = _landingSpots[StaticRandom.Instance.Next(_landingSpots.Count)];
            landingSpot = new Point3D(landingSpot.X + (fallingPlayer.Zone.Info.OffX << 12), landingSpot.Y + (fallingPlayer.Zone.Info.OffY << 12), landingSpot.Z );
            float speed = 180f;

            float flightTimePunter = (float)fallingPlayer.GetDistanceSquare(punter) / speed / 1000f;
            float flightTimePuntee = (float)fallingPlayer.GetDistanceSquare(landingSpot) / speed / 1000f;

            if (StaticRandom.Instance.NextDouble() > .50) // 50/50 chance to suck in punter
            {
                if (punter is Player)
                    ((Player)punter).Catapult(fallingPlayer.Zone, fallingPlayer.WorldPosition, (ushort)flightTimePunter, (ushort)1800);
            }

            if (StaticRandom.Instance.NextDouble() > .50) // 50/50 chance to spit out puntee
            {
                fallingPlayer.Catapult(punter.Zone, landingSpot, (ushort)flightTimePuntee, (ushort)3400);
                AddFallGuard(fallingPlayer);
            }
        }

        private void AddFallGuard(Player player)
        {
            lock (_fallGuard)
            {
                _fallGuard[player] = DateTime.Now;
            }
        }

        private bool IsFallGuarded(Player player)
        {
            bool guarded = false;
            lock (_fallGuard)
            {
                if(_fallGuard.ContainsKey(player))
                    guarded = true;
            }
            return guarded;
        }
        protected override void UpdateScenario()
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (Player plr in Players[i])
                {

                    if (plr.Z < 24100 && !plr.IsDead && !IsFallGuarded(plr))
                    {
                        plr.CalculateFallDamage(true);
                    }
                }
            }

            base.UpdateScenario();
        }

        public override void Interact(GameObject obj, Player player, InteractMenu menu)
        {
            if (HasStarted)
            {
                int shift = (((int)player.Realm) + _potralShift) % 3;
                player.Teleport(172, _portals[shift].Item1, _portals[shift].Item2, _portals[shift].Item3, _portals[shift].Item4);
            }
        }
    }
}
