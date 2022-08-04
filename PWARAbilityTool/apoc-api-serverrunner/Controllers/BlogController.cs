using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dapper;

namespace PWARAbilityTool.Controllers
{
    public class BlogController : ApocApiController
    {
        [HttpPost]
        public IHttpActionResult AddOrUpdate(Blog blog)
        {
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} {blog.ToString()}");
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var item = DbConnection.Query<Blog>($"select BlogId, BlogTimestamp, BlogText, BlogUrl " +
                                                        $"from war_accounts.blogs where BlogId = {blog.BlogId}");
                    if (item.Count() == 0)
                    {
                        if (blog.BlogTimestamp.Year == 1)
                            blog.BlogTimestamp = DateTime.Now;

                        Logger.Debug($"adding blog");
                        var result = DbConnection.Execute($"insert into war_accounts.blogs (BlogTimestamp, BlogText, BlogTitle, BlogUrl) values (@BlogTimestamp, @BlogText, @BlogTitle, @BlogUrl);",
                            new { BlogText = blog.BlogText, BlogTitle = blog.BlogTitle, BlogUrl = blog.BlogUrl, BlogTimestamp = blog.BlogTimestamp });
                    }
                    else
                    {
                        Logger.Debug($"updating blog");
                        var result = DbConnection.Execute($"update war_accounts.blogs set BlogTimestamp=@BlogTimestamp, BlogText=@BlogText, BlogTitle=@BlogTitle, BlogUrl=@BlogUrl where BlogId=@BlogId;",
                            new { BlogText = blog.BlogText, BlogTitle = blog.BlogTitle, BlogUrl = blog.BlogUrl, BlogId = blog.BlogId, BlogTimestamp = blog.BlogTimestamp });
                    }

                    return Ok();
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
        public IHttpActionResult GetAll()
        {
            try
            {
                Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress}");
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    return Ok(DbConnection.Query<Blog>($"select BlogId, BlogTimestamp, BlogText, BlogUrl, BlogTitle " +
                                                         $"from war_accounts.blogs order by BlogId desc ").ToList());
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
            Logger.Debug($"+ {Request.GetOwinContext().Request.RemoteIpAddress} Blog Id {id}");
            try
            {
                var token = Request.Headers.GetValues("auth-token").FirstOrDefault();
                if (Startup.tokenManager.IsValidToken(token))
                {
                    var item = DbConnection.Query<Blog>($"select BlogId, BlogTimestamp, BlogText, BlogUrl, BlogTitle " +
                                                        $"from war_accounts.blogs where BlogId = {id}");
                    if (item == null)
                    {
                        return NotFound();
                    }
                    return Ok(item);
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
    }
}