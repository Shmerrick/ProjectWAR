using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace CharacterUtility
{
    public class SetBonus
    {
     
        public int BonusId { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public int MinLevel { get; set; }
        public int NumberPieces { get; set; }

        private static Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// bonus can be in the form A:B,C,D (or A:B)
        /// </summary>
        /// <param name="bonus"></param>
        /// <param name="itemBonusList"></param>
        /// <returns></returns>
        public SetBonus CalculateSetBonus(string bonus, IEnumerable<ItemBonus> itemBonusList)
        {
            var setBonus = new SetBonus();

            if (String.IsNullOrEmpty(bonus))
                return null;
            
            var bonusEntry = bonus.Split(':');

            // If the form is A:B,C,D
            if (bonusEntry[1].Contains(','))
            {

                var bonusTypeId = bonusEntry[1].Split(',')[0];
                var bonusValue = bonusEntry[1].Split(',')[1];

                setBonus.MinLevel = Convert.ToInt32(bonusEntry[0]);

                var bonusObject = itemBonusList.SingleOrDefault(x => x.Entry == Convert.ToInt32(bonusTypeId));

                if (bonusObject != null)
                {
                    setBonus.BonusId = itemBonusList.Single(x => x.Entry == Convert.ToInt32(bonusTypeId)).Entry;
                    setBonus.Name = itemBonusList.Single(x => x.Entry == Convert.ToInt32(bonusTypeId)).BonusName;
                }
                else
                {
                    Logger.Warn($"Could not locate item {bonusTypeId} in bonus list ");
                }
                setBonus.Value = Convert.ToInt32(bonusValue);
            }
            else
            {

                var bonusObject = itemBonusList.Single(x => x.Entry == Convert.ToInt32(bonusEntry[0]));
                // Form is A:B
                if (bonusObject != null)
                {
                    setBonus.MinLevel = 0;
                    setBonus.BonusId = bonusObject.Entry;
                    setBonus.Name = bonusObject.BonusName;
                }
                else
                {
                    Logger.Warn($"Could not locate item {Convert.ToInt32(bonusEntry[0])} in bonus list ");
                }
                setBonus.Value = Convert.ToInt32(bonusEntry[1]);
            }

            return setBonus;
        }

        public override string ToString()
        {
            return $"BonusId : {BonusId}, Name : {Name}, Value : {Value} MinLevel : {MinLevel}";
        }

        public JObject ToJson()
        {
            return new JObject(
                new JProperty("bonus-id", BonusId),
                new JProperty("name", Name),
                new JProperty("value", Value),
                new JProperty("min-level", MinLevel)
                );
        }
    }
}
