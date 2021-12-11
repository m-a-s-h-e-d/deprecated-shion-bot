namespace Shion.Modules.Utility
{
    using System;
    using System.Threading.Tasks;
    using Core.Common.BotOptions;
    using Core.Extensions;
    using Core.Structures;
    using Discord;
    using Discord.Commands;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    // Remember to make your module reference the ShardedCommandContext
    public class PublicModule : ShionModuleBase
    {
        public PublicModule(ILogger<PublicModule> logger)
            : base(logger)
        {
        }

        [Command("ping")]
        [Alias("pong", "ms")]
        public async Task PingAsync()
        {
            await this.CreateEmbedBuilder(new EmbedInfo(
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
            var msg = $"Hi {this.Context.User}! There are currently {(this.Context.Client.Shards.Count == 1 ? this.Context.Client.Shards.Count : 1)} shards!\n" +
                      $"This guild is being served by shard number {this.Context.Client.GetShardFor(this.Context.Guild).ShardId}";

            await this.CreateEmbedBuilder(new EmbedInfo(
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