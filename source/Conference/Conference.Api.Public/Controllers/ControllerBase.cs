using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Tracing;
using Conference.Api.Public.Models;

namespace Conference.Api.Public.Controllers
{
    public class ControllerBase : ApiController
    {
        private readonly ITraceWriter _tracer;
        private readonly CachingUserDao _userCache;
        protected User _user;

        public ControllerBase()
        {
            _userCache = new CachingUserDao(new MemoryCache("user_cache"));
            _tracer = GlobalConfiguration.Configuration.Services.GetTraceWriter();
        }

        public override async Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext,
            CancellationToken cancellationToken)
        {
            var token = ((string[])(controllerContext.Request.Headers.GetValues("x-access-token")))[0];

            _user = await _userCache.GetUser(token);

            return await base.ExecuteAsync(controllerContext, cancellationToken);
        }
    }
}