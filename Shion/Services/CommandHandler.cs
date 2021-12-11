namespace Shion.Services
{
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

    public class CommandHandler : DiscordShardedClientService
    {
        private readonly IServiceProvider provider;
        private readonly CommandService commandService;
        private readonly IConfiguration config;

        public CommandHandler(DiscordShardedClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config)
            : base(client, logger)
        {
            this.provider = provider;
            this.commandService = commandService;
            this.config = config;
        }

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
            await context.Channel.SendMessageAsync($"Error: {result.ToString()}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.Client.MessageReceived += this.MessageReceivedAsync;
            this.commandService.Log += this.LogAsync;
            this.commandService.CommandExecuted += this.CommandExecutedAsync;
            await this.commandService.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }

        private Task LogAsync(LogMessage log)
        {
            this.Logger.LogInformation(log.ToString());

            return Task.CompletedTask;
        }
    }
}
