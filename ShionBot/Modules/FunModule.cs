using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schema;

namespace ShionBot.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<FunModule> _logger;
        private readonly Servers _servers;
        private readonly Users _users;
        private readonly Balances _balances;
        private readonly Experiences _experiences;
        private readonly Color _botEmbedColor = new(4, 28, 99);

        public FunModule(ILogger<FunModule> logger, Servers servers, Users users, Balances balances, Experiences experiences)
        {
            _logger = logger;
            _servers = servers;
            _users = users;
            _balances = balances;
            _experiences = experiences;
        }

        //TODO Leaderboard for balance and level
        /*[Command("leaderboard")]
        [Alias("lb")]
        public async Task Leaderboard(string type)
        {
            if (!type.Equals("balance", StringComparison.OrdinalIgnoreCase) || !type.Equals("level", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Channel.SendMessageAsync("Invalid usage of `.leaderboard`, you must specify `balance | level` as a parameter.");
                return;
            }
        }

        [Command("global-leaderboard")]
        [Alias("glb")]
        public async Task GlobalLeaderboard(string type)
        {
            if (!type.Equals("balance", StringComparison.OrdinalIgnoreCase) || !type.Equals("level", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Channel.SendMessageAsync("Invalid usage of `.global-leaderboard`, you must specify `balance | level` as a parameter.");
                return;
            }
        }*/

        [Command("give")]
        [Alias("pay", "transfer")]
        public async Task TransferMoney(long balanceTransferred, [Remainder] SocketGuildUser user = null)
        {
            if (user == null)
                throw new ArgumentException("No user was specified.");

            // Check if the user is trying to give themselves money (That is stupid)
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync("You cannot give yourself money.");
                return;
            }

            // Check if amount being transferred is less than 1 (Don't let them rob the other person)
            if (balanceTransferred < 1)
            {
                await ReplyAsync("Please specify a valid amount to be transferred (e.g: 1 or more).");
                return;
            }

            // Check if the transferrer has enough money
            if (await _balances.GetBalance(Context.User.Id) < balanceTransferred)
            {
                await ReplyAsync("You do not have enough money to make the transfer.");
                return;
            }

            // Remove money from the transferrer
            await _balances.ModifyBalance(Context.User.Id, -balanceTransferred);
            // Add money to the transferee
            await _balances.ModifyBalance(user.Id, +balanceTransferred);

            var builder = new EmbedBuilder()
                .WithColor(_botEmbedColor)
                .WithTitle($"{Context.User.Username} => {user.Username}")
                .AddField("Amount Transferred", $"{balanceTransferred} :coin:", true)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }

        [Command("balance")]
        [Alias("$", "money", "bal")]
        public async Task GetBalance([Remainder] SocketGuildUser user = null)
        {
            user ??= (SocketGuildUser)Context.User;

            var balance = await _balances.GetBalance(user.Id);
            var builder = new EmbedBuilder()
                .WithTitle($"{user.Username}'s Current Balance")
                .WithColor(_botEmbedColor)
                .AddField("Current Balance", $"{balance} :coin:", true)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }
    }
}
