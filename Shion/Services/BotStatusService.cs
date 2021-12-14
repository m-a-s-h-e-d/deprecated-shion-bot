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

namespace Shion.Services
{
    /// <summary>
    /// A class that represents a <see cref="DiscordShardedClientService"/> to handle a bot's status.
    /// </summary>
    public class BotStatusService : DiscordShardedClientService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotStatusService"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordShardedClient"/> that should be injected.</param>
        /// <param name="logger">The <see cref="ILogger"/> that should be injected.</param>
        public BotStatusService(DiscordShardedClient client, ILogger<BotStatusService> logger)
            : base(client, logger)
        {
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.Client.WaitForReadyAsync(stoppingToken);
            this.Logger.LogInformation("Client is ready!");

            await this.Client.SetActivityAsync(new Game("Migrating to Discord.Net-Labs"));
        }
    }
}
