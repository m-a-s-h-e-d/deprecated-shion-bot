using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Shion.Services
{
    public class CommandHandler : DiscordShardedClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;

        public CommandHandler(DiscordShardedClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config) : base(client, logger)
        {
            _provider = provider;
            _commandService = commandService;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.MessageReceived += MessageReceivedAsync;
            _commandService.Log += LogAsync;
            _commandService.CommandExecuted += CommandExecutedAsync;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        public async Task MessageReceivedAsync(SocketMessage incomingMessage)
        {
            // Ignore system messages, or messages from other bots
            if (incomingMessage is not SocketUserMessage message)
                return;
            if (message.Source != MessageSource.User)
                return;

            // This value holds the offset where the prefix ends
            var argPos = 0;

            // Stores the server prefix, or default
            // var prefix = GetServerPrefix ?? ".";
            var prefix = ".";
            
            // Check if message received has a valid prefix
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                return;

            // A new kind of command context, ShardedCommandContext can be utilized with the commands framework
            var context = new ShardedCommandContext(Client, message);
            await _commandService.ExecuteAsync(context, argPos, _provider);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);

            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            Logger.LogError("Failed to execute {command}: {error}", command.Value.Name, result.ToString());
            await context.Channel.SendMessageAsync($"Error: {result.ToString()}");
        }

        private Task LogAsync(LogMessage log)
        {
            Logger.LogInformation(log.ToString());

            return Task.CompletedTask;
        }
    }
}
