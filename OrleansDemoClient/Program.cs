using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Configuration;
using OrleansDemoClient.TypedOptions;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using SharedOrleansInterface;

namespace OrleansDemoClient
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .WriteTo.Trace()
                .WriteTo.Debug();

            Log.Logger = logConfig.CreateLogger();

            try
            {
                Log.Information("Press Enter to begin connecting to server");
                Console.ReadLine();

                using (var client = CreateClientBuilder(args).Build())
                {
                    await client.Connect(CreateRetryFilter());
                    Log.Information("Client successfully connect to silo host");

                    // Use the connected client to call a grain, writing the result to the terminal.
                    var grain = client.GetGrain<IHello>(0);
                    var response = await grain.SayHello("Good morning, my friend!");

                    Log.Information($"response = {response}");

                    Console.WriteLine("Press any key to stop Client.");
                    Console.ReadKey();
                    await client.Close();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Client side error");
                Console.ReadKey();
                return 1;
            }
        }

        #region Orleans Client Builder

        private static IClientBuilder CreateClientBuilder(string[] args)
        {
            var (clusterInfo, providerInfo) = GetConfigSettings(args);

            var clientBuilder = new ClientBuilder()
                .Configure<ClientMessagingOptions>(options =>
                {
                    options.ResponseTimeout = TimeSpan.FromSeconds(20);
                    options.ResponseTimeoutWithDebugger = TimeSpan.FromMinutes(60);
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterInfo.ClusterId;
                    options.ServiceId = clusterInfo.ServiceId;
                })
                .ConfigureLogging(logging => logging.AddSerilog(dispose: true));

            if (providerInfo.DefaultProvider == "MongoDB")
            {
                clientBuilder.UseMongoDBClustering(options =>
                {
                    var mongoDbSettings = providerInfo.MongoDB.Cluster;

                    options.ConnectionString = mongoDbSettings.DbConn;
                    options.DatabaseName = mongoDbSettings.DbName;

                    if (!string.IsNullOrEmpty(mongoDbSettings.CollectionPrefix))
                    {
                        options.CollectionPrefix = mongoDbSettings.CollectionPrefix;
                    }
                });
            }
            else
            {
                clientBuilder.UseLocalhostClustering();
            }

            return clientBuilder;
        }

        private static (ClusterInfoOption, OrleansProviderOption) GetConfigSettings(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables(prefix: "ORLEANS_CLIENT_APP_")
                .AddCommandLine(args);

            var config = builder.Build().GetSection("Orleans");

            var clusterInfo = new ClusterInfoOption();
            config.GetSection("Cluster").Bind(clusterInfo);

            var providerInfo = new OrleansProviderOption();
            config.GetSection("Provider").Bind(providerInfo);

            return (clusterInfo, providerInfo);
        }

        #endregion

        private static Func<Exception, Task<bool>> CreateRetryFilter(int maxAttempts = 5)
        {
            var attempt = 0;
            return RetryFilter;

            async Task<bool> RetryFilter(Exception exception)
            {
                attempt++;
                Console.WriteLine($"Cluster client attempt {attempt} of {maxAttempts} failed to connect to cluster.  Exception: {exception}");
                if (attempt > maxAttempts)
                {
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(4));
                return true;
            }
        }
    }
}
