using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OrleanWithWebApp
{
    public class GeneraicHostStart
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("hostsettings.json", optional: true)
                        .AddEnvironmentVariables(prefix: "ORLEANS_HOST_")
                        .AddCommandLine(args);
                });

    }
}
