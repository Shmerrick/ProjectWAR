using FrameWork;
using System.Linq;
using WorldServer.Services.World;
using WorldServer.World.Objects;

//1174
namespace WorldServer.World.AI
{
    public class RunepriestBrain : RangedBrain
    {
        public Unit FriendlyTarget { get; set; }
        public int runeofShieldingCooldown { get; set; }

        public RunepriestBrain(Unit myOwner)
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
                            SimpleCast(_unit, target, "Rune of preservation (detaunt)", 1595);
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
                            //Rune of Regeneration
                            SimpleCast(_unit, FriendlyTarget, "Rune of Regeneration", 1590);
                            break;
                        }
                    case 1:
                        {
                            //Rune of Mending
                            SimpleCast(_unit, FriendlyTarget, "Rune of Mending", 1599);
                            break;
                        }
                    case 2:
                        {
                            //Rune of Shielding
                            if (runeofShieldingCooldown < FrameWork.TCPManager.GetTimeStamp())
                            {
                                SimpleCast(_unit, FriendlyTarget, "Rune of Shielding", 1593);
                                runeofShieldingCooldown = FrameWork.TCPManager.GetTimeStamp() + 20; // available in another 20 seconds
                            }

                            break;
                        }
                    case 3:
                        {
                            // 1587 - Grungni's Gift
                            SimpleCast(_unit, FriendlyTarget, "Grungni's Gift", 1587);
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