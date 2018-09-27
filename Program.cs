using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration;

namespace ProfilesApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var tempConfig = config.Build();
                    if (tempConfig["USE_KEYVAULT"] == "1") {
                        if (Utilities.IsAzure()) {
                            config.AddAzureKeyVault(
                                tempConfig["Azure:KeyVaultAccessor:Endpoint"]);
                        } else { // ! Utilities.IsAzure()
                            config.AddAzureKeyVault(
                                tempConfig["Azure:KeyVaultAccessor:Endpoint"],
                                tempConfig["Azure:KeyVaultAccessor:ClientId"],
                                tempConfig["Azure:KeyVaultAccessor:ClientSecret"]);
                        }
                    }
                })
                .UseStartup<Startup>();
    }
}
