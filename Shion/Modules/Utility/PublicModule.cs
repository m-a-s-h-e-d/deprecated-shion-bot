namespace Shion.Modules.Utility
{
    using System;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Core.Common.BotOptions;
    using Core.Extensions;
    using Core.Structures;

    // Remember to make your module reference the ShardedCommandContext
    public class PublicModule : ShionModuleBase
    {
        public PublicModule(ILogger<PublicModule> logger) : base(logger)
        {
        }

        [Command("ping")]
        [Alias("pong", "ms")]
        public async Task PingAsync()
        {
            await CreateEmbedBuilder(new EmbedInfo(
                ShionOptions.EmbedColor,
                null,
                "Pong!",
                $"Responded in {Context.Client.Latency}ms",
                null,
                null
            )).BuildAndReplyEmbed(Context.Message);
        }

        [Command("shard")]
        [Alias("shard-info")]
        public async Task ShardInfoAsync()
        {
            var msg = $"Hi {Context.User}! There are currently {(Context.Client.Shards.Count == 1 ? Context.Client.Shards.Count : 1)} shards!\n" +
                      $"This guild is being served by shard number {Context.Client.GetShardFor(Context.Guild).ShardId}";

            await CreateEmbedBuilder(new EmbedInfo(
                ShionOptions.EmbedColor,
                null,
                "Bot Shard Information",
                $"There are currently {Context.Client.Shards.Count} shards!\n" +
                $"This guild is being served by shard number {Context.Client.GetShardFor(Context.Guild).ShardId}.",
                null,
                null
            )).BuildAndReplyEmbed(Context.Message);
        }
    }
}