using System.Collections.Generic;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities
{
    public class RunicBlessingsHandler : Object
    {
        private Player _slayerChoppa;
        private long _rezTime;

        public RunicBlessingsHandler(Player player)
        {
            _slayerChoppa = player;

            _rezTime = TCPManager.GetTimeStampMS() + 2000;
        }

        public override void Update(long msTick)
        {
            if (msTick <= _rezTime)
                return;

            _slayerChoppa.RezUnit(_slayerChoppa.Realm == Realms.REALMS_REALM_ORDER ? (ushort)1489 : (ushort)1795, 25, true);

            AbilityDamageInfo damageThisPass = AbilityMgr.GetExtraDamageFor(_slayerChoppa.Realm == Realms.REALMS_REALM_ORDER ? (ushort)1489 : (ushort)1795, 0, 0);

            List<Object> objects;

            lock (_slayerChoppa.PlayersInRange)
                objects = new List<Object>(_slayerChoppa.ObjectsInRange);

            int count = 0;

            foreach (Object obj in objects)
            {
                Unit unit = obj as Unit;
                if (unit == null || unit == _slayerChoppa)
                    continue;

                if (unit.ObjectWithinRadiusFeet(_slayerChoppa, 40) && CombatInterface.CanAttack(_slayerChoppa, unit) && _slayerChoppa.LOSHit(unit))
                    CombatManager.InflictDamage(damageThisPass.Clone(), _slayerChoppa.AbtInterface.GetMasteryLevelFor(3), _slayerChoppa, unit);

                ++count;

                if (count == 9)
                    break;
            }

            Dispose();
        }
    }
}