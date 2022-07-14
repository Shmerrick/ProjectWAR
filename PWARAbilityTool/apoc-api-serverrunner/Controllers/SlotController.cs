using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PWARAbilityTool.Controllers
{
    public class SlotController : ApocApiController
    {
        // GET: api/Slot
        public List<KeyValuePair<int, string>> GetAll()
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");
            try
            {
                var result = new List<KeyValuePair<int, string>>();

                var slotIds = Enum.GetValues(typeof(EquipmentSlotEnum)).Cast<int>();
                var slotNames = Enum.GetNames(typeof(EquipmentSlotEnum)).ToList();

                int i = 0;
                foreach (var slotId in slotIds)
                {
                    result.Add(new KeyValuePair<int, string>(slotId, slotNames[i]));
                    i++;
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return new List<KeyValuePair<int, string>>();
            }

        }


      
    }
}
