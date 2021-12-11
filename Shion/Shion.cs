namespace Shion
{
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
    using Services;

    public class Shion
    {
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
                    };

                    // Retrive BOT_TOKEN from environment variables
                    config.Token = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? string.Empty;

                    // Use this to configure a custom format for Client/CommandService logging if needed. The default is below and should be suitable for Serilog usage
                    config.LogFormat = (message, exception) => $"{message.Source}: {message.Message}";
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