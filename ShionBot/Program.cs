﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Schema;
using Serilog;
using Serilog.Events;
using ShionBot.Services;

namespace ShionBot
{
    class Program
    {
        public static int Main(string[] args)
        {
            //Log is available everywhere, useful for places where it isn't practical to use ILogger injection
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            //CreateDefaultBuilder configures a lot of stuff for us automatically.
            //See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0#default-builder-settings
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200
                    };

                    config.Token = context.Configuration["Token"];

                    //Use this to configure a custom format for Client/CommandService logging if needed. The default is below and should be suitable for Serilog usage
                    config.LogFormat = (message, exception) => $"{message.Source}: {message.Message}";
                })
                .UseCommandService((context, config) =>
                {
                    config.LogLevel = LogSeverity.Verbose;
                    config.DefaultRunMode = RunMode.Async;
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<CommandHandler>();
                    services.AddHostedService<BotStatusService>();
                    services.AddDbContext<SchemaContext>(ServiceLifetime.Transient);
                    services.AddSingleton<Servers>();
                    services.AddSingleton<Users>();
                    services.AddSingleton<ServerUsers>();
                    services.AddSingleton<Balances>();
                    services.AddSingleton<Experiences>();
                });
    }
}
