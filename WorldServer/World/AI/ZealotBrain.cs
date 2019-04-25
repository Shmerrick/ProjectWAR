using System.Linq;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Objects;

namespace WorldServer.World.AI
{
    public class ZealotBrain : RangedBrain
    {
        public Unit FriendlyTarget { get; set; }
        public int runeofShieldingCooldown { get; set; }


        public ZealotBrain(Unit myOwner)
            : base(myOwner)
        {
            runeofShieldingCooldown = 0;
        }

        public override void Think(long tick)
        {
            base.Think(tick);

            if (_unit.IsDead)
                return;

            if ((FriendlyTarget == null))

            base.Think(tick);

            // Only bother to seek targets if we're actually being observed by a player
            if (Combat.CurrentTarget == null && _unit.PlayersInRange.Count > 0)
            {
                var friendlyPlayers = _unit.GetInRange<Unit>(120).Where(x => x.Realm == _unit.Realm && x.PctHealth < 100).OrderBy(o => o.PctHealth).ToList();
                if (friendlyPlayers.Count() > 0)
                {
                    _logger.Debug($"{_unit} changing friendly target to  {(friendlyPlayers[0]).Name}");
                    FriendlyTarget = friendlyPlayers[0];
                }
            }

            if (_unit.AbtInterface.CanCastCooldown(0) &&
                TCPManager.GetTimeStampMS() > NextTryCastTime)
            {
                var percentHealth = (_unit.Health * 100) / _unit.MaxHealth;
                var target = Combat.GetCurrentTarget();

                if (percentHealth < 20f)
                {
                    // 695 is healing pot model - bit of hack
                    // This needs to be timed if we dont have a proper inventory to work with.
                    var items = CreatureService.GetCreatureItems((_unit as Creature).Entry)
                        .Where(x => x.ModelId == 695);
                    // Low health -- potion of healing
                    if (items.Count() > 0)
                    {
                        // 7872 - Potion of Healing ability
                        if (!potionUsed)
                        {
                            SimpleCast(_unit, _unit, "Potion of Healing", 7872);
                            potionUsed = true;
                        }
                    }
                    else
                    {
                        if (nextDetauntAvailable < FrameWork.TCPManager.GetTimeStamp())
                        {
                            SimpleCast(_unit, target, "Chaotic Blur (detaunt)", 8621);
                            nextDetauntAvailable =
                                FrameWork.TCPManager.GetTimeStamp() + 30; // available in another 30 seconds
                        }
                    }

                    return;
                }



                var rand = StaticRandom.Instance.Next(10);
                switch (rand)
                {
                    case 0:
                        {
                            // 8566 - Elixir Of Dark Blessings
                            SimpleCast(_unit, FriendlyTarget, "Elixir Of Dark Blessings", 8566);
                            break;
                        }
                    case 1:
                        {
                            // 8557 - Leaping Alteration
                            SimpleCast(_unit, FriendlyTarget, "Leaping Alteration", 8557);
                            break;
                        }
                    case 2:
                        {
                            // 8564 - Veil of Chaos
                            if (runeofShieldingCooldown < FrameWork.TCPManager.GetTimeStamp())
                            {
                                SimpleCast(_unit, FriendlyTarget, "Veil of Chaos", 8564);
                                runeofShieldingCooldown = FrameWork.TCPManager.GetTimeStamp() + 20; // available in another 20 seconds
                            }

                            break;
                        }
                    case 3:
                        {
                            // 8569 - Flash of Chaos
                            SimpleCast(_unit, FriendlyTarget, "Flash of Chaos", 8569);
                            break;
                        }
                    case 4:
                    case 5:
                    case 6:
                        {
                            var friendlyPlayers = _unit.GetInRange<Unit>(120).Where(x => x.Realm == _unit.Realm && x.PctHealth < 100).OrderBy(o => o.PctHealth).ToList();
                            if (friendlyPlayers.Count() > 0)
                            {
                                _logger.Debug($"{_unit} changing friendly target to  {(friendlyPlayers[0]).Name}");
                                FriendlyTarget = friendlyPlayers[0];
                            }

                            break;
                        }
                }
            }


        }
    }
}
