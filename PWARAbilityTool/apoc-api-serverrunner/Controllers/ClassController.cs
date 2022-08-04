using apoc_api.common.Reference;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{

    [Route("api/@Career")]
    public class ClassController : ApocApiController
    {
        public IconService IconManager { get; set; }
        public List<Career> CareerList { get; set; }

        public ClassController(IconService iconservice)
        {
            IconManager = iconservice;
            CareerList = LoadCareerLines();
        }

        public List<Career> LoadCareerLines()
        {
            var returnList = new List<Career>();

            returnList.Add(new Career { ClassId = 0, ClassName = "", ClassIcon = IconManager.GetIcon(20204, false).TextByteArray });
            returnList.Add(new Career { ClassId = 1, ClassName = "Ironbreaker", ClassIcon = IconManager.GetIcon(20189, false).TextByteArray });
            returnList.Add(new Career { ClassId = 2, ClassName = "Slayer", ClassIcon = IconManager.GetIcon(20188, false).TextByteArray });
            returnList.Add(new Career { ClassId = 3, ClassName = "Runepriest", ClassIcon = IconManager.GetIcon(20193, false).TextByteArray });
            returnList.Add(new Career { ClassId = 4, ClassName = "Engineer", ClassIcon = IconManager.GetIcon(20187, false).TextByteArray });
            returnList.Add(new Career { ClassId = 5, ClassName = "Black Orc", ClassIcon = IconManager.GetIcon(20182, false).TextByteArray });
            returnList.Add(new Career { ClassId = 6, ClassName = "Choppa", ClassIcon = IconManager.GetIcon(20184, false).TextByteArray });
            returnList.Add(new Career { ClassId = 7, ClassName = "Shaman", ClassIcon = IconManager.GetIcon(20195, false).TextByteArray });
            returnList.Add(new Career { ClassId = 8, ClassName = "Squig Herder", ClassIcon = IconManager.GetIcon(20197, false).TextByteArray });
            returnList.Add(new Career { ClassId = 9, ClassName = "Witch Hunter", ClassIcon = IconManager.GetIcon(20202, false).TextByteArray });
            returnList.Add(new Career { ClassId = 10, ClassName = "Knight", ClassIcon = IconManager.GetIcon(20190, false).TextByteArray });
            returnList.Add(new Career { ClassId = 11, ClassName = "Bright Wizard", ClassIcon = IconManager.GetIcon(20183, false).TextByteArray });
            returnList.Add(new Career { ClassId = 12, ClassName = "Warrior Priest", ClassIcon = IconManager.GetIcon(20199, false).TextByteArray });
            returnList.Add(new Career { ClassId = 13, ClassName = "Chosen", ClassIcon = IconManager.GetIcon(20185, false).TextByteArray });
            returnList.Add(new Career { ClassId = 14, ClassName = "Marauder", ClassIcon = IconManager.GetIcon(20192, false).TextByteArray });
            returnList.Add(new Career { ClassId = 15, ClassName = "Zealot", ClassIcon = IconManager.GetIcon(20203, false).TextByteArray });
            returnList.Add(new Career { ClassId = 16, ClassName = "Magus", ClassIcon = IconManager.GetIcon(20191, false).TextByteArray });
            returnList.Add(new Career { ClassId = 17, ClassName = "Swordmaster", ClassIcon = IconManager.GetIcon(20198, false).TextByteArray });
            returnList.Add(new Career { ClassId = 18, ClassName = "Shadow Warrior", ClassIcon = IconManager.GetIcon(20194, false).TextByteArray });
            returnList.Add(new Career { ClassId = 19, ClassName = "White Lion", ClassIcon = IconManager.GetIcon(20200, false).TextByteArray });
            returnList.Add(new Career { ClassId = 20, ClassName = "Archmage", ClassIcon = IconManager.GetIcon(20180, false).TextByteArray });
            returnList.Add(new Career { ClassId = 21, ClassName = "Blackguard", ClassIcon = IconManager.GetIcon(20181, false).TextByteArray });
            returnList.Add(new Career { ClassId = 22, ClassName = "Witch Elf", ClassIcon = IconManager.GetIcon(20201, false).TextByteArray });
            returnList.Add(new Career { ClassId = 23, ClassName = "Disciple of Khaine", ClassIcon = IconManager.GetIcon(20186, false).TextByteArray });
            returnList.Add(new Career { ClassId = 24, ClassName = "Sorcerer", ClassIcon = IconManager.GetIcon(20196, false).TextByteArray });

            return returnList;
        }

        public IHttpActionResult GetAll()
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");
            try
            {
                //var classList = DbConnection.Query<Career>( $"SELECT ClassId, ClassName from war_world.Classes ").ToList();
                return Ok(CareerList);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }

        }


    }


}
