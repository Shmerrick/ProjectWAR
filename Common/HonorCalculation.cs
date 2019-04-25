using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class HonorCalculation 
    {
        private Dictionary<int, int> honorLevelReference = new Dictionary<int, int>();

        public HonorCalculation()
        {
            // This list MUST be descending in sequence for this to work ;)
            honorLevelReference.Add(4, 8000);
            honorLevelReference.Add(3, 4000);
            honorLevelReference.Add(2, 2000);
            honorLevelReference.Add(1, 1000);
            
        }

        public int GetHonorLevel(int honorPoints)
        {

            foreach (var honorLevel in honorLevelReference)
            {
                if (honorLevel.Value < honorPoints)
                    return honorLevel.Key;
            }
            return 0;
        }
    }
}
