using PWARAbilityTool.apoc_api_serverrunner.Services;
using PWARAbilityTool.Dtos;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace PWARAbilityTool.Controllers
{
    public class AbilityController : ApocApiController
    {
        #region HttpGet

        [HttpGet]
        [Route("api/Ability/all")]
        public IHttpActionResult GetAll()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all Abilities...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllAbilites(out query);
                    List<AbilitySingle> abilityList = DbConnection.Query<AbilitySingle>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(abilityList);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/Entry/{Entry}")]
        public IHttpActionResult GetByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all Abilities with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityByEntry(Entry, out query);
                    List<AbilitySingle> abilities = DbConnection.Query<AbilitySingle>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(abilities);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/AbilityCommand/Entry/{Entry}")]
        public IHttpActionResult GetAbsCommandByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all AbilityCommands with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityCommandQueryByEntry(Entry, out query);
                    List<AbilityCommand> ability = DbConnection.Query<AbilityCommand>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(ability);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/AbilityDmgHeal/Entry/{Entry}")]
        public IHttpActionResult GetAbsDmgHealByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all AbilityDamageHeals with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityDmgHealQueryByEntry(Entry, out query);
                    List<AbilityDamageHeals> ability = DbConnection.Query<AbilityDamageHeals>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(ability);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/AbilityKnockBack/Entry/{Entry}")]
        public IHttpActionResult getAbsKnockBackByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all AbilityKnockBackInfo with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityKnockBackInfoQueryByEntry(Entry, out query);
                    List<AbilityKnockBackInfo> abilities = DbConnection.Query<AbilityKnockBackInfo>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(abilities);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/AbilityMod/Entry/{Entry}")]
        public IHttpActionResult getAbsModByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all AbilityModifiers with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityModifiersQueryByEntry(Entry, out query);
                    List<AbilityModifiers> ability = DbConnection.Query<AbilityModifiers>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(ability);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/AbilityModChecks/Entry/{Entry}")]
        public IHttpActionResult getAbsModChecksByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all AbilityModifierChecks with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityModifierChecksQueryByEntry(Entry, out query);
                    List<AbilityModifierChecks> ability = DbConnection.Query<AbilityModifierChecks>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(ability);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/BuffInfo/Entry/{Entry}")]
        public IHttpActionResult getBuffInfoByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all AbilityBuffInfos with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityBuffInfosQueryByEntry(Entry, out query);
                    List<AbilityBuffInfos> ability = DbConnection.Query<AbilityBuffInfos>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(ability);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/BuffCommand/Entry/{Entry}")]
        public IHttpActionResult getBuffCommandByEntry(int Entry)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all AbilityBuffCommands with Entry {Entry} ...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityBuffCommandsQueryByEntry(Entry, out query);
                    List<AbilityBuffCommands> ability = DbConnection.Query<AbilityBuffCommands>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(ability);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/player")]
        public IHttpActionResult getBySpeclinePlayer()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all Player-Abilities...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityBySpeclinePlayer(out query);
                    List<AbilitySingle> abilities = DbConnection.Query<AbilitySingle>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(abilities);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/npc")]
        public IHttpActionResult getBySpeclineNPC()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all NPC-Abilities...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAbilityBySpeclineNPC(out query);
                    List<AbilitySingle> abilities = DbConnection.Query<AbilitySingle>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(abilities);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/speclines")]
        public IHttpActionResult getAllSpeclines()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all speclines...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllSpeclines(out query);
                    List<string> speclines = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(speclines);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/careerLines")]
        public IHttpActionResult getAllCareerLines()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all careerLines...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllCareerLines(out query);
                    List<string> speclines = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(speclines);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/abilityTypes")]
        public IHttpActionResult getAllAbilityTypes()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all careerLines...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllAbilityTypes(out query);
                    List<string> abilityTypes = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(abilityTypes);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/masteryTrees")]
        public IHttpActionResult getAllAbilityMasteryTrees()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all masteryTrees...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllMasteryTrees(out query);
                    List<string> masteryTrees = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(masteryTrees);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/targets")]
        public IHttpActionResult getAllTargets()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all targets...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllTargets(out query, "ability_commands");
                    List<string> targets = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(targets);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/effectSource")]
        public IHttpActionResult getAllEffectSource()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all effectSource...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllEffectSources(out query, "ability_commands");
                    List<string> effectSource = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(effectSource);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/cmdNames")]
        public IHttpActionResult getAllAbsCommandNames()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all commandNames...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllCommandNames(out query, "ability_commands");
                    List<string> commandNames = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(commandNames);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/failCodes")]
        public IHttpActionResult getAllFailCodes()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all failCodes...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllFailCodes(out query);
                    List<string> failCodes = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(failCodes);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/commandNames")]
        public IHttpActionResult getAllCommandNames()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all commandNames...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllCommandNames(out query, "ability_modifier_checks");
                    List<string> commandNames = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(commandNames);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/modifierCommandNames")]
        public IHttpActionResult getAllModifierCommandNames()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all modifierCommandNames...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllModifierCommandNames(out query);
                    List<string> modifierCommandNames = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(modifierCommandNames);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/buffCommandNames")]
        public IHttpActionResult getAllBuffCommandNames()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all buffCommandNames...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllCommandNames(out query, "buff_commands");
                    List<string> buffCommandNames = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(buffCommandNames);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/buffTargets")]
        public IHttpActionResult getAllBuffTargets()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all buffTargets...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllTargets(out query, "buff_commands");
                    List<string> targets = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(targets);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/buffEffectSource")]
        public IHttpActionResult getAllBuffEffectSource()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all buffEffectSource...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllEffectSources(out query, "buff_commands");
                    List<string> effectSource = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(effectSource);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/buffInfoBuffClassString")]
        public IHttpActionResult getAllBuffInfoBuffClassString()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all buffInfoBuffClassString...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllBuffInfoClassStrings(out query);
                    List<string> effectSource = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(effectSource);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("api/Ability/buffInfoTypeString")]
        public IHttpActionResult getAllBuffInfoTypeString()
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Getting all buffInfoTypeString...");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.getSelectAllBuffInfoTypeString(out query);
                    List<string> effectSource = DbConnection.Query<string>(query).ToList();
                    Logger.Info($"query: {query}");
                    return Ok(effectSource);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion HttpGet

        #region HttpPost

        #region update Single Ability

        [HttpPost]
        [Route("api/Ability/update/abilitySingle")]
        public IHttpActionResult update(AbilitySingle abilitySingle)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating AbilitySingle... {abilitySingle.Name}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilitySingle(abilitySingle, abilitySingle.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated ability + " + abilitySingle.Name);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update Single Ability

        #region update AbilityCommand

        [HttpPost]
        [Route("api/Ability/update/abilityCommand")]
        public IHttpActionResult updateAbilityCommand(AbilityCommand abilityCommand)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating AbilityCommand... {abilityCommand.AbilityName}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilityCommand(abilityCommand, abilityCommand.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated abilityCommand + " + abilityCommand.AbilityName);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update AbilityCommand

        #region update AbilityKnockBackInfo

        [HttpPost]
        [Route("api/Ability/update/abilityKnockBackInfo")]
        public IHttpActionResult updateAbilityKnockBackInfo(AbilityKnockBackInfo abilityKnockBackInfo)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating abilityKnockBackInfo... {abilityKnockBackInfo.Entry}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilityKnockBackInfo(abilityKnockBackInfo, abilityKnockBackInfo.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated abilityKnockBackInfo + " + abilityKnockBackInfo.Entry);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update AbilityKnockBackInfo

        #region update AbilityModifierChecks

        [HttpPost]
        [Route("api/Ability/update/abilityModifierChecks")]
        public IHttpActionResult updateAbilityModifierChecks(AbilityModifierChecks abilityModifierChecks)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating abilityModifierChecks... {abilityModifierChecks.Entry}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilityModifierChecks(abilityModifierChecks, abilityModifierChecks.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated abilityModifierChecks + " + abilityModifierChecks.Entry);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update AbilityModifierChecks

        #region update AbilityModifiers

        [HttpPost]
        [Route("api/Ability/update/abilityModifiers")]
        public IHttpActionResult updateAbilityModifiers(AbilityModifiers abilityModifiers)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating abilityModifiers... {abilityModifiers.Entry}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilityModifiers(abilityModifiers, abilityModifiers.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated abilityModifiers + " + abilityModifiers.Entry);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update AbilityModifiers

        #region update AbilityDamageHeals

        [HttpPost]
        [Route("api/Ability/update/abilityDamageHeals")]
        public IHttpActionResult updateAbilityDamageHeals(AbilityDamageHeals abilityDamageHeals)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating abilityDamageHeals... {abilityDamageHeals.Entry}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilityDamageHeals(abilityDamageHeals, abilityDamageHeals.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated abilityDamageHeals + " + abilityDamageHeals.Entry);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update AbilityDamageHeals

        #region update AbilityBuffInfos

        [HttpPost]
        [Route("api/Ability/update/abilityBuffInfos")]
        public IHttpActionResult updateAbilityBuffInfos(AbilityBuffInfos abilityBuffInfos)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating abilityBuffInfos... {abilityBuffInfos.Entry}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilityBuffInfos(abilityBuffInfos, abilityBuffInfos.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated abilityBuffInfos + " + abilityBuffInfos.Entry);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update AbilityBuffInfos

        #region update AbilityBuffCommands

        [HttpPost]
        [Route("api/Ability/update/abilityBuffCommands")]
        public IHttpActionResult updateAbilityBuffCommands(AbilityBuffCommands abilityBuffCommands)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Updating abilityBuffCommands... {abilityBuffCommands.Entry}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildUpdateAbilityBuffCommand(abilityBuffCommands, abilityBuffCommands.Entry, out query);
                    DbConnection.Execute(query);
                    Logger.Info($"query: {query}");
                    return Ok("updated abilityBuffCommands + " + abilityBuffCommands.Entry);
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion update AbilityBuffCommands

        #region insert abilitySingle

        [HttpPost]
        [Route("api/Ability/insert/abilitySingle")]
        public IHttpActionResult insert(AbilitySingle abilitySingle)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting Ability... {abilitySingle.Name}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilitySingleQuery(abilitySingle, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");
                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilitySingle

        #region insert abilityCommand

        [HttpPost]
        [Route("api/Ability/insert/abilityCommand")]
        public IHttpActionResult insert(AbilityCommand abilityCommand)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting AbilityCommand... {abilityCommand.AbilityName}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilityCommand(abilityCommand, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");

                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilityCommand

        #region insert abilityDmgHeals

        [HttpPost]
        [Route("api/Ability/insert/abilityDmgHeals")]
        public IHttpActionResult insert(AbilityDamageHeals abilityDmgHeals)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting abilityDmgHeals... {abilityDmgHeals.Name}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilityDamageHeals(abilityDmgHeals, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");

                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilityDmgHeals

        #region insert abilityKnockBack

        [HttpPost]
        [Route("api/Ability/insert/abilityKnockBack")]
        public IHttpActionResult insert(AbilityKnockBackInfo abilityKnockBack)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting abilityKnockBack... {abilityKnockBack.Id}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilityKnockBackInfo(abilityKnockBack, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");

                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilityKnockBack

        #region insert abilityModifiers

        [HttpPost]
        [Route("api/Ability/insert/abilityModifiers")]
        public IHttpActionResult insert(AbilityModifiers abilityModifiers)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting abilityModifiers... {abilityModifiers.ability_modifiers_ID}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilityModifiers(abilityModifiers, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");

                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilityModifiers

        #region insert abilityModifierChecks

        [HttpPost]
        [Route("api/Ability/insert/abilityModifierChecks")]
        public IHttpActionResult insert(AbilityModifierChecks abilityModifierChecks)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting abilityModifierChecks... {abilityModifierChecks.ability_modifier_checks_ID}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilityModifierChecks(abilityModifierChecks, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");

                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilityModifierChecks

        #region insert abilityBuffInfos

        [HttpPost]
        [Route("api/Ability/insert/abilityBuffInfos")]
        public IHttpActionResult insert(AbilityBuffInfos abilityBuffInfos)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting abilityBuffInfos... {abilityBuffInfos.Name}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilityBuffInfo(abilityBuffInfos, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");

                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilityBuffInfos

        #region insert abilityBuffCommands

        [HttpPost]
        [Route("api/Ability/insert/abilityBuffCommands")]
        public IHttpActionResult insert(AbilityBuffCommands abilityBuffCommands)
        {
            try
            {
                Logger.Info($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Inserting abilityBuffCommands... {abilityBuffCommands.Name}");
                string token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    string query = string.Empty;
                    AbilityService.buildInsertAbilityBuffCommand(abilityBuffCommands, out query, out object queryDataBinding);
                    DbConnection.Execute(query, queryDataBinding);
                    Logger.Info($"query: {query}");

                    return Ok("Insert was successfull.");
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }

        #endregion insert abilityBuffCommands

        #endregion HttpPost
    }
}