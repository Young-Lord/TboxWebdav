using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TboxWebdav.Server.Modules.Tbox
{
    public class TboxUserTokenProvider
    {
        private string userToken;
        private readonly IHttpContextAccessor _contextAccessor;

        public TboxUserTokenProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            userToken = _contextAccessor.HttpContext.Items.TryGetValue("UserToken", out var token) ? token.ToString() : string.Empty;
        }

        public void SetUserToken(string usertoken)
        {
            userToken = usertoken;
        }

        public string GetUserToken()
        {
            return userToken;
        }
    }
}
