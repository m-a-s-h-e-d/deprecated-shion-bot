using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Shion.Modules;
using Shion.Services;

namespace Shion
{
    /// <summary>
    /// The entry point of the bot.
    /// </summary>
    public class Shion
    {
        /// <summary>
        /// An asynchronous main method for initializing the bot.
        /// </summary>
        /// <param name="args">The <see cref="string"/> arguments to be passed.</param>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            // Log is available everywhere, useful for places where it isn't practical to use ILogger injection
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                // Attempt to start the client
                Log.Information("Starting host...");
                var host = CreateHostBuilder(args).Build();
                using (host)
                {
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                // Failed to start the client
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                // Close and flush the stream
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Configures and returns a new sharded discord bot builder object.
        /// </summary>
        /// <param name="args">The <see cref="string"/> arguments to be passed.</param>
        /// <returns>A configured <see cref="IHostBuilder"/> representing a discord sharded host builder.</returns>
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
                        MessageCacheSize = 200,

                        // Gateway intentions
                        GatewayIntents = GatewayIntents.All,
                    };

                    // Retrive BOT_TOKEN from environment variables
                    config.Token = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? string.Empty;
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
                    ConfigureServices.RegisterServices(services);
                })
                .UseConsoleLifetime();
    }
}