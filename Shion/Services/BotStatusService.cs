namespace Shion.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Addons.Hosting;
    using Discord.Addons.Hosting.Util;
    using Discord.WebSocket;
    using Microsoft.Extensions.Logging;

    public class BotStatusService : DiscordShardedClientService
    {
        public BotStatusService(DiscordShardedClient client, ILogger<BotStatusService> logger)
            : base(client, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.Client.WaitForReadyAsync(stoppingToken);
            this.Logger.LogInformation("Client is ready!");

            await this.Client.SetActivityAsync(new Game("Migrating to Discord.Net-Labs"));
        }
    }
}
