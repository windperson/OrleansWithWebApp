using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrleansWithWebApp.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace OrleansWithWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.Management.ManagementGrain", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.SiloControl", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With(new TraceIdEnricher())
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.Trace()
                .WriteTo.Debug();

            Log.Logger = logConfig
                .Enrich.FromLogContext().CreateLogger();

            var webHost = CreateWebHostBuilder(args).Build();
            var genericHost = GeneicHostBuilderHelper.CreateHostBuilder(args).Build();

            Task.WaitAll(webHost.RunAsync(), genericHost.RunAsync());
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().UseSerilog();
    }
}
