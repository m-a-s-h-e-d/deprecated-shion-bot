using System;
using System.Threading;
using System.Threading.Tasks;
using Shion.Services;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Shion
{
    // This is a minimal example of using Discord.Net's Sharded Client
    // The provided DiscordShardedClient class simplifies having multiple
    // DiscordSocketClient instances (or shards) to serve a large number of guilds.
    public class Shion
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
                // Attempt to start the client
                Log.Information("Starting host...");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                // Failed to start the client
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                // Close and flush the stream
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureDiscordShardedHost((context, config) =>
                {
                    // Set up client socket configs
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        // Total number of shards, generally 1 shard per 1,500 - 2,000 guilds the bot is in.
                        TotalShards = 2,
                        // Log context, higher = more information from logs.
                        LogLevel = LogSeverity.Verbose,
                        // Cache users in guild for faster lookup at the cost of more space used.
                        AlwaysDownloadUsers = true,
                        // Number of messages being cached at a time.
                        MessageCacheSize = 200
                    };

                    // Retrive BOT_TOKEN from environment variables
                    config.Token = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "";
                })
                .UseCommandService((context, config) =>
                {
                    // Sets all commands to run asynchronously
                    config.DefaultRunMode = RunMode.Async;
                    // Set case sensitivity for command parameters
                    config.CaseSensitiveCommands = false;
                })
                .ConfigureServices((context, services) =>
                {
                    // Add additional services here.
                    services.AddHostedService<CommandHandler>();
                    services.AddHostedService<BotStatusService>();
                    // services.AddHostedService<LongRunningService>();
                    // services.AddHostedService<SchedulingService>(); //TODO This should be a BackgroundService, does not need access to Discord Client (most likely)
                });
    }
}