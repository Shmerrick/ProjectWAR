using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    
    [Route("api/emailqueue")]
    public class EmailQueueController : ApocApiController
    {
      

        [HttpGet]
        public IHttpActionResult GetAll()
        {
            try
            {
                Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    return Ok(DbConnection.Query<EmailQueue>($"select * from war_accounts.email_queue  ").ToList());
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

        
        public IHttpActionResult GetById(long id)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} EmailId {id}");
            try
            {
                var item = DbConnection.Query<Character>($"select *  from war_accounts.email_queue where account_id = {id} ");
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

        public IHttpActionResult AddEmailQueue(EmailQueue emailQueue)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");
            try
            {
                var result = DbConnection.Execute($" insert into war_accounts.email_queue (EmailSubject, EmailBody, EmailSentFlag, EmailSentTimestamp, EmailCreateTimestamp, AccountId, EmailToAddress) values " +
                                                          $" (@emailSubject, @emailBody, @emailSentFlag, null, @emailCreateTimestamp, @accountId, @emailToAddress)", new
                {
                    emailSubject = emailQueue.EmailSubject,
                    emailBody = emailQueue.EmailBody,
                    emailSentFlag = 0,
                    emailCreateTimestamp = DateTime.Now,
                    accountId = emailQueue.AccountId,
                    emailToAddress = emailQueue.EmailToAddress

                });
              
                return Ok();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return InternalServerError(e);
            }
        }
    }
}