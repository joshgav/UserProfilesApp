using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace users_auth_app
{
    public class Server {

        // implements Microsoft.AspNetCore.Http.RequestDelegate
        public static async Task RequestDelegate(HttpContext context) {
            await context.Response.WriteAsync("Hello!");
        }

    }
}
