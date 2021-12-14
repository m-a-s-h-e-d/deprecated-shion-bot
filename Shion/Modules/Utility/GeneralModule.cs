using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shion.Core.Common.BotOptions;
using Shion.Core.Extensions;
using Shion.Core.Structures;

namespace Shion.Modules.Utility
{
    /// <summary>
    /// The general module containing common commands like ping.
    /// </summary>
    public class GeneralModule : ShionModuleBase
    {
        private readonly ILogger<GeneralModule> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralModule"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> representing a stream for logging to be injected.</param>
        public GeneralModule(ILogger<GeneralModule> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// A command that shows the bot's latency to Discord.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        [Command("ping")]
        [Alias("pong", "ms")]
        public async Task PingAsync()
        {
            await CreateEmbedBuilder(new EmbedInfo(
                ShionOptions.EmbedColor,
                null,
                "Pong!",
                $"Responded in {this.Context.Client.Latency}ms",
                null,
                null))
                .BuildAndReplyEmbed(this.Context.Message);
        }

        /// <summary>
        /// A command that shows which shard is connected to the current server as well as the shard count.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        [Command("shard")]
        [Alias("shard-info")]
        public async Task ShardInfoAsync()
        {
            await CreateEmbedBuilder(new EmbedInfo(
                ShionOptions.EmbedColor,
                null,
                "Bot Shard Information",
                $"There are currently {this.Context.Client.Shards.Count} shards!\nThis guild is being served by shard number {this.Context.Client.GetShardFor(this.Context.Guild).ShardId}.",
                null,
                null))
                .BuildAndReplyEmbed(this.Context.Message);
        }
    }
}