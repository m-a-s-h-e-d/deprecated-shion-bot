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
    // Remember to make your module reference the ShardedCommandContext
    public class PublicModule : ShionModuleBase
    {
        private readonly ILogger<PublicModule> logger;

        public PublicModule(ILogger<PublicModule> logger)
        {
            this.logger = logger;
        }

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