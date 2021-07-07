using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ShionBot
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config) : base(client, logger)
        {
            _provider = provider;
            _commandService = commandService;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.MessageReceived += HandleMessage;
            _commandService.CommandExecuted += CommandExecutedAsync;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task HandleMessage(SocketMessage incomingMessage)
        {
            if (incomingMessage is not SocketUserMessage message) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            if (!message.HasStringPrefix(_config["Prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;
            if (message.ToString().Trim().Equals(".")) return;

            var context = new SocketCommandContext(Client, message);

            try
            {
                await _commandService.ExecuteAsync(context, argPos, _provider);
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException)
                {
                    if (!message.ToString().Trim().Equals("."))
                    {
                        Logger.LogInformation("User {user} attempted to use command {command}", context.User, message.ToString()[1..message.ToString().Length]);
                        await context.Message.ReplyAsync($"The following command, `{message.ToString()[1..message.ToString().Length]}` does not exist.");
                    }
                }
                else
                {
                    await context.Message.ReplyAsync($"An error occurred: `{e.Message}`\n");
                }
            }
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);

            if (!command.IsSpecified || result.IsSuccess)
                return;

            await context.Message.ReplyAsync($"An error occurred: `{result.ErrorReason}`");
        }
    }
}