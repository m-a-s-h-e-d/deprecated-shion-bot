using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shion.Core.Common.BotOptions;
using Victoria;

namespace Shion.Services
{
    /// <summary>
    /// The class responsible for handling the commands and various events.
    /// </summary>
    public class CommandHandler : DiscordShardedClientService
    {
        private readonly IServiceProvider provider;
        private readonly CommandService commandService;
        private readonly IConfiguration config;
        private readonly LavaNode lavaNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordShardedClient"/> that should be injected.</param>
        /// <param name="logger">The <see cref="ILogger"/> that should be injected.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> that should be injected.</param>
        /// <param name="commandService">The <see cref="CommandService"/> that should be injected.</param>
        /// <param name="config">The <see cref="IConfiguration"/> that should be injected.</param>
        public CommandHandler(DiscordShardedClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config, LavaNode lavaNode)
            : base(client, logger)
        {
            this.provider = provider;
            this.commandService = commandService;
            this.config = config;
            this.lavaNode = lavaNode;
        }

        /// <summary>
        /// Handles messages received from the client.
        /// </summary>
        /// <param name="incomingMessage">The <see cref="SocketMessage"/> to be passed.</param>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        public async Task MessageReceivedAsync(SocketMessage incomingMessage)
        {
            // Ignore system messages, or messages from other bots
            if (incomingMessage is not SocketUserMessage message)
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            // This value holds the offset where the prefix ends
            var argPos = 0;

            // Stores the server prefix, or default
            var prefix = ".";

            // Check if message received has a valid prefix
            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(this.Client.CurrentUser, ref argPos))
            {
                return;
            }

            // A new kind of command context, ShardedCommandContext can be utilized with the commands framework
            var context = new ShardedCommandContext(this.Client, message);
            await this.commandService.ExecuteAsync(context, argPos, this.provider);
        }

        /// <summary>
        /// Handles commands executed and provides error logs and user error responses.
        /// </summary>
        /// <param name="command">The <see cref="CommandInfo"/> to be passed.</param>
        /// <param name="context">The <see cref="ICommandContext"/> to be passed.</param>
        /// <param name="result">The <see cref="IResult"/> to be passed.</param>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation.</returns>
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            this.Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);

            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
            {
                return;
            }

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
            {
                return;
            }

            // the command failed, let's notify the user that something happened.
            this.Logger.LogError("Failed to execute {command}: {error}", command.Value.Name, result.ToString());
            var errorEmbed = CreateErrorEmbedBuilder(command, result);
            await context.Channel.SendMessageAsync(embed: errorEmbed.Build());
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.Client.MessageReceived += this.MessageReceivedAsync;
            this.Client.ShardReady += this.OnReadyAsync;
            this.commandService.CommandExecuted += this.CommandExecutedAsync;
            await this.commandService.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }

        /// <summary>
        /// Configures settings based on the passed <see cref="IResult"/> and returns a new <see cref="EmbedBuilder"/>.
        /// </summary>
        /// <param name="command">The <see cref="CommandInfo"/> to be passed.</param>
        /// <param name="result">The <see cref="IResult"/> to be passed.</param>
        /// <returns>A configured <see cref="EmbedBuilder"/> representing a discord embed builder.</returns>
        private static EmbedBuilder CreateErrorEmbedBuilder(Optional<CommandInfo> command, IResult result) =>
            new EmbedBuilder()
                .WithColor(ShionOptions.EmbedColor)
                .WithTitle("Failed to execute command")
                .WithDescription($"The command `{command.Value.Name}` could not be completed.\n**Error**: `{result.ErrorReason}`")
                .WithCurrentTimestamp();

        private async Task OnReadyAsync(DiscordSocketClient client)
        {
            if (!this.lavaNode.IsConnected)
            {
                await this.lavaNode.ConnectAsync();
            }
        }
    }
}
