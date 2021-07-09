﻿using System;
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
using Schema;
using Serilog;

namespace ShionBot
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly Users _users;
        private readonly Balances _balances;
        private readonly Experiences _experiences;

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config, Servers servers, Users users, Balances balances, Experiences experiences) : base(client, logger)
        {
            _provider = provider;
            _commandService = commandService;
            _config = config;
            _servers = servers;
            _users = users;
            _balances = balances;
            _experiences = experiences;
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
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? ".";

            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;

            // Ignore if no command was specified
            if (message.ToString().Trim().Equals(prefix)) return;

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

            switch (result.Error)
            {
                case CommandError.ParseFailed:
                    await context.Message.ReplyAsync($"One or more of the arguments entered in your command `{command.Value.Name}`, were invalid.");
                    break;
                default:
                    await context.Message.ReplyAsync($"An unknown error occurred: `{result.ErrorReason}`");
                    break;
            }
        }
    }
}