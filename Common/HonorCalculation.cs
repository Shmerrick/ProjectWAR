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
            honorLevelReference.Add(10, 6000);
            honorLevelReference.Add(9, 4000);
            honorLevelReference.Add(8, 2000);
            honorLevelReference.Add(7, 1400);
            honorLevelReference.Add(6, 1100);
            honorLevelReference.Add(5, 800);
            honorLevelReference.Add(4, 600);
            honorLevelReference.Add(3, 400);
            honorLevelReference.Add(2, 200);
            honorLevelReference.Add(1, 100);
            
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
