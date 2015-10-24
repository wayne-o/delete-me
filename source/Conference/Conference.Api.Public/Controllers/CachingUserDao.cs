using System;
using System.Configuration;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Conference.Api.Public.Models;
using Newtonsoft.Json;
using System.Web.Http.Tracing;
using System.Web.Http;

namespace Conference.Api.Public.Controllers
{
    public class CachingUserDao
    {
        private readonly ObjectCache _cache;
        ITraceWriter _tracer;

        public CachingUserDao(ObjectCache cache)
        {
            this._cache = cache;
            _tracer = GlobalConfiguration.Configuration.Services.GetTraceWriter();
        }

        public async Task<User> GetUser(string token)
        {
            try
            {
                var key = "User_" + token;

                var user = this._cache.Get(key) as User;
                if (user != null) return user;

                var client = new HttpClient { BaseAddress = new Uri(ConfigurationManager.AppSettings["SonatribeApi"]) };
                client.DefaultRequestHeaders.TryAddWithoutValidation("x-access-token", token);

                _tracer.Info(null, "CachingUserDao", "fetching with code {0}", token);

                var responseString = await client.GetStringAsync("users/me");
                user = JsonConvert.DeserializeObject<User>(responseString);

                return user;
            }
            catch (Exception exc)
            {
                _tracer.Error(null,"CachingUserDao", "exception: ", exc);
                throw;
            }
        }
    }
}