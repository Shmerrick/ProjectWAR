using System.Linq;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;

//1174
namespace WorldServer.World.AI
{
    public class HealerBrain : RangedBrain
    {
        public Unit FriendlyTarget { get; set; }
        public int runeofShieldingCooldown { get; set; }

        // Cooldown between special attacks 
        public static int NEXT_ATTACK_COOLDOWN = 4000;


        public HealerBrain(Unit myOwner)
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
                var friendlyPlayers = _unit.GetInRange<Unit>(120).Where(x => x.Realm == _unit.Realm&& x.PctHealth< 100).OrderBy(o=>o.PctHealth).ToList();
                if (friendlyPlayers.Count() > 0)
                {
                    _logger.Debug($"{_unit} changing friendly target to  {(friendlyPlayers[0]).Name}");
                    SpeakYourMind($"{_unit.Name} friendly target=> {(friendlyPlayers[0]).Name}");
                    FriendlyTarget = friendlyPlayers[0];
                }
            }

            if (_unit.AbtInterface.CanCastCooldown(0) &&
                TCPManager.GetTimeStampMS() > NextTryCastTime)
            {

                Combat.SetTarget(FriendlyTarget.Oid, TargetTypes.TARGETTYPES_TARGET_ALLY);

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

                NextTryCastTime = TCPManager.GetTimeStampMS() + NEXT_ATTACK_COOLDOWN;
            }


        }
    }
}
