using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    
    [Route("api/ItemSet")]
    public class ItemSetController : ApocApiController
    {
     
        [HttpGet]
        public IHttpActionResult GetAll(string name, int classId=0,  int numberRecords = 0)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {name} {classId} {numberRecords}");
            try
            {
                if ((numberRecords == 0) || (numberRecords > 20))
                    numberRecords = 20;

                var nameMatch = string.Empty;
                if (!string.IsNullOrEmpty(name))
                {
                    nameMatch = $" and Name like '%{name}%' ";
                }

                var classMatch = string.Empty;
                if (classId != 0)
                {
                    classMatch = $" and sets.ClassId = {classId} ";
                }


                var query =
                    $" SELECT sets.Entry, sets.Name, sets.ClassId, sets.ItemsString, sets.BonusString, sets.Comments, sets.ItemSetList, sets.ItemSetFullDescription " +
                    $" FROM war_world.item_sets sets" +
                    $" WHERE 1=1 " +
                    $" {nameMatch}" +
                    $" {classMatch}" +
                    $" order by sets.Name " +
                    $" limit {numberRecords}";

                    Logger.Debug(query);

                var itemSetList = DbConnection.Query<ItemSet>(query).ToList();

                return Ok(itemSetList);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }

        }

        public IHttpActionResult GetById(long id)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Item {id} ");
            try
            {
                var item = DbConnection.Query<ItemSet>(
                    $"SELECT sets.Entry, sets.Name, sets.ClassId, c.ClassName, sets.ItemSetFullDescription FROM war_world.item_sets sets, war_world.classes c " +
                    $"where c.ClassId = sets.ClassId " +
                    $" and sets.Entry = {id}; ").ToList();

                if (item == null)
                {
                    return NotFound();
                }
                return Ok(item);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }
    }
}
