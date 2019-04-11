using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DemoGrains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansWithWebApp.TypedOptions;
using Serilog;

namespace OrleansWithWebApp
{
    public class GeneicHostBuilderHelper
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("hostsettings.json", optional: true)
                        .AddEnvironmentVariables(prefix: "ORLEANS_HOST_")
                        .AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions();

                    var orleansSettings = context.Configuration.GetSection("Orleans");

                    services.Configure<SiloConfigOption>(orleansSettings.GetSection("SiloConfig"));
                    services.Configure<OrleansProviderOption>(orleansSettings.GetSection("Provider"));
                    services.Configure<OrleansDashboardOption>(orleansSettings.GetSection("Dashboard"));
                })
                .UseOrleans((context, siloBuilder) =>
                {
                    var orleansSettings = context.Configuration.GetSection("Orleans");

                    var (siloConfig, orleansProvider, orleansDashboard) = GetServerConfig(orleansSettings);

                    if (orleansDashboard.Enable)
                    {
                        Log.Information("Enable Orleans Dashboard (https://github.com/OrleansContrib/OrleansDashboard) on this host.");
                        siloBuilder.UseDashboard(options =>
                        {
                            options.Port = orleansDashboard.Port;
                        });
                    }

                    siloBuilder.Configure<SiloMessagingOptions>(options =>
                    {
                        options.ResponseTimeout = TimeSpan.FromMinutes(siloConfig.ResponseTimeoutMinutes);
                        options.ResponseTimeoutWithDebugger = TimeSpan.FromMinutes(siloConfig.ResponseTimeoutMinutes + 60);
                    }).Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = siloConfig.ClusterId;
                        options.ServiceId = siloConfig.ServiceId;
                    }).ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences());


                    if (IpAddressNotSpecified(siloConfig.AdvertisedIp))
                    {
                        siloBuilder.ConfigureEndpoints(siloConfig.SiloPort, siloConfig.GatewayPort,
                            listenOnAnyHostAddress: siloConfig.ListenOnAnyHostAddress);
                    }
                    else
                    {
                        var advertisedIp = IPAddress.Parse(siloConfig.AdvertisedIp.Trim());
                        siloBuilder.ConfigureEndpoints(advertisedIp, siloConfig.SiloPort, siloConfig.GatewayPort,
                            siloConfig.ListenOnAnyHostAddress);
                    }

                    switch (orleansProvider.DefaultProvider)
                    {
                        case "MongoDB":
                            var mongoDbConfig = orleansProvider.MongoDB;
                            siloBuilder.UseMongoDBClustering(options =>
                            {
                                var cluster = mongoDbConfig.Cluster;

                                options.ConnectionString = cluster.DbConn;
                                options.DatabaseName = cluster.DbName;

                                if (!string.IsNullOrEmpty(cluster.CollectionPrefix))
                                {
                                    options.CollectionPrefix = cluster.CollectionPrefix;
                                }
                            })
                            .AddMongoDBGrainStorageAsDefault(optionsBuilder =>
                            {
                                var storage = mongoDbConfig.Storage;
                                optionsBuilder.Configure(options =>
                                {
                                    options.ConnectionString = storage.DbConn;
                                    options.DatabaseName = storage.DbName;

                                    if (!string.IsNullOrEmpty(storage.CollectionPrefix))
                                    {
                                        options.CollectionPrefix = storage.CollectionPrefix;
                                    }
                                });
                            })
                            .UseMongoDBReminders(options =>
                            {
                                var reminder = mongoDbConfig.Reminder;

                                options.ConnectionString = reminder.DbConn;
                                options.DatabaseName = reminder.DbName;

                                if (!string.IsNullOrEmpty(reminder.CollectionPrefix))
                                {
                                    options.CollectionPrefix = reminder.CollectionPrefix;
                                }
                            });
                            break;

                        default:
                            siloBuilder.UseLocalhostClustering().UseInMemoryReminderService();
                            break;
                    }
                })
                .ConfigureLogging(logging => logging.AddSerilog(dispose: true))
                .UseConsoleLifetime()
                .UseSerilog();


        #region Util Methods

        private static (SiloConfigOption, OrleansProviderOption, OrleansDashboardOption) GetServerConfig(IConfigurationSection config)
        {
            var siloConfigOption = new SiloConfigOption();
            config.GetSection("SiloConfig").Bind(siloConfigOption);

            var orleansProviderOption = new OrleansProviderOption();
            config.GetSection("Provider").Bind(orleansProviderOption);

            var orleansDashboardOption = new OrleansDashboardOption();
            config.GetSection("Dashboard").Bind(orleansDashboardOption);

            return (siloConfigOption, orleansProviderOption, orleansDashboardOption);
        }

        private static bool IpAddressNotSpecified(string ipString)
        {
            if (ipString == null) { return true; }

            return string.IsNullOrEmpty(ipString.Trim()) || "*".Equals(ipString.Trim());
        }

        #endregion

    }
}
