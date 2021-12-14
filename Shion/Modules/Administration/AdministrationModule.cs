using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shion.Core.Common.BotOptions;
using Shion.Core.Extensions;
using Shion.Core.Preconditions;
using Shion.Core.Structures;
using Shion.Modules;
using Shion.Modules.Utility;

namespace Shion.Modules.Administration
{
    /// <summary>
    /// The administration module containing bot owner level commands commands like shutdown.
    /// </summary>
    public class AdministrationModule : ShionModuleBase
    {
        // You can inject the host. This is useful if you want to shutdown the host via a command, but be careful with it.
        private readonly IHost host;
        private readonly ILogger<AdministrationModule> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationModule"/> class.
        /// </summary>
        /// <param name="host">An <see cref="IHost"/> to be injected.</param>
        /// <param name="logger">An <see cref="ILogger"/> representing a stream for logging to be injected.</param>
        public AdministrationModule(IHost host, ILogger<AdministrationModule> logger)
        {
            this.host = host;
            this.logger = logger;
        }

        /// <summary>
        /// A command to delete a specified number of messages. Requires Manage Messages permission.
        /// </summary>
        /// <param name="amount">An <see cref="int"/> to be passed.</param>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amount)
        {
            if (amount <= 0)
            {
                throw new Exception("You must pass a value greater than 0.");
            }

            var messages = await this.Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            var channel = (SocketTextChannel)this.Context.Channel;
            await channel.DeleteMessagesAsync(messages);

            var message = await CreateEmbedBuilder(new EmbedInfo(
                ShionOptions.EmbedColor,
                null,
                "Purge Command",
                $"{messages.Count()} messages were deleted.",
                null,
                null))
                .BuildAndSendEmbed(this.Context.Channel);
            await Task.Delay(2500);
            await message.DeleteAsync();
        }

        /// <summary>
        /// A command to turn off the bot. Can only be used by the configured Bot Owner.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        [Command("shutdown")]
        [RequireBotOwner]
        public Task Stop()
        {
            _ = this.host.StopAsync();
            return Task.CompletedTask;
        }

        /// <summary>
        /// A command to test Serilog logging. Can only be used by the configured Bot Owner.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        [Command("log")]
        [RequireBotOwner]
        public Task TestLogs()
        {
            this.logger.LogTrace("This is a trace log");
            this.logger.LogDebug("This is a debug log");
            this.logger.LogInformation("This is an information log");
            this.logger.LogWarning("This is a warning log");
            this.logger.LogError(new InvalidOperationException("Invalid Operation"), "This is a error log with exception");
            this.logger.LogCritical(new InvalidOperationException("Invalid Operation"), "This is a critical load with exception");

            this.logger.Log(GetLogLevel(LogSeverity.Error), "Error logged from a Discord LogSeverity.Error");
            this.logger.Log(GetLogLevel(LogSeverity.Info), "Information logged from Discord LogSeverity.Info");

            return Task.CompletedTask;
        }

        private static LogLevel GetLogLevel(LogSeverity severity)
            => (LogLevel)Math.Abs((int)severity - 5);
    }
}
