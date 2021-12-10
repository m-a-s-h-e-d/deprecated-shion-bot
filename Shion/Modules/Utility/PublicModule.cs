﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shion.Modules.Utility
{
    // Remember to make your module reference the ShardedCommandContext
    public class PublicModule : ModuleBase<ShardedCommandContext>
    {
        private readonly ILogger<PublicModule> _logger;

        public PublicModule(ILogger<PublicModule> logger)
        {
            _logger = logger;
        }

        [Command("ping")]
        [Alias("pong", "hello")]
        public async Task PingAsync()
        {
            _logger.LogInformation($"User {Context.User.Username} used the ping command!");
            await ReplyAsync("pong!");
        }

        [Command("info")]
        public async Task InfoAsync()
        {
            var msg = $"Hi {Context.User}! There are currently {Context.Client.Shards.Count} shards!\n" +
                      $"This guild is being served by shard number {Context.Client.GetShardFor(Context.Guild).ShardId}";
            await ReplyAsync(msg);
        }
    }
}