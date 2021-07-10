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
using ShionBot.Utilities;

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

        /*[Command("leaderboard")]
        [Alias("lb")]
        public async Task Leaderboard(string type)
        {
            if (!type.Equals("balance", StringComparison.OrdinalIgnoreCase) || !type.Equals("level", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Channel.SendMessageAsync("Invalid usage of `.leaderboard`, you must specify `balance | level` as a parameter.");
                return;
            }
        }*/

        /*[Command("global-leaderboard")]
        [Alias("glb")]
        public async Task GlobalLeaderboard(string type)
        {
            if (!type.Equals("balance", StringComparison.OrdinalIgnoreCase) || !type.Equals("level", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Channel.SendMessageAsync("Invalid usage of `.global-leaderboard`, you must specify `balance | level` as a parameter.");
                return;
            }
        }*/

        [Command("daily")]
        [Alias("claim")]
        public async Task DailyClaim([Remainder] SocketGuildUser user = null)
        {
            //TODO Check for 23 hours since last claim
            user ??= (SocketGuildUser)Context.User;

            if (user == null)
                throw new ArgumentException("No user was specified.");

            string message = $"{Context.User.Username}#{Context.User.Discriminator} claimed their daily!";

            var claimAmount = 100 + (25 * (await _experiences.GetLevel(Context.User.Id) - 1));
            // Increase balance by daily top-off
            await _balances.ModifyBalance(user.Id, claimAmount);

            if (user.Id != Context.User.Id)
                message = $"{Context.User.Username}#{Context.User.Discriminator} gave their daily to {user.Username}#{user.Discriminator}!";

            var builder = new EmbedBuilder()
                .WithTitle(message)
                .AddField("Daily Earnings", $"{claimAmount} :coin:", true)
                .WithColor(new Color(await _users.GetEmbedColor(user.Id)))
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
        }

        [Command("bet-flip")]
        [Alias("bf")]
        public async Task BetCoinFlip(int bet, string betFace)
        {
            if (bet < 1)
            {
                await ReplyAsync("Please enter a valid amount to bet.");
                return;
            }

            var balance = await _balances.GetBalance(Context.User.Id);

            if (bet > balance)
            {
                await ReplyAsync("You do not have sufficient funds to bet.");
                return;
            }

            Coin coin = new();
            coin.Flip();

            bool result;
            string face = coin.GetFace();

            try
            {
                result = coin.Equals(betFace);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case ArgumentException:
                        await ReplyAsync("You did not enter a valid coin face. (e.g: Heads or Tails)");
                        return;
                    default:
                        await ReplyAsync("An unknown error occurred!");
                        return;
                }
            }

            // Deduct the bet cost and then evaluate earnings
            await _balances.ModifyBalance(Context.User.Id, -bet);
            var betEarnings = result ? (2 * bet) : 0;
            await _balances.ModifyBalance(Context.User.Id, betEarnings);

            var builder = new EmbedBuilder()
                .WithTitle($"{Context.User.Username}#{Context.User.Discriminator} flipped the coin")
                .WithDescription($"{(face == "Heads" ? "🔼" : "🔽")} It landed on {face.ToLower()}! {(result ? "You won!" : "Tough luck.")}")
                .AddField("Earnings", $"{betEarnings} :coin:", true)
                .WithColor(new Color(await _users.GetEmbedColor(Context.User.Id)))
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
        }

        [Command("coin-flip")]
        [Alias("flip")]
        public async Task CoinFlip(int times = 1)
        {
            if (times < 1 || times > 10)
            {
                await ReplyAsync("Please enter a value between 1 and 10 for number of coin flips.");
                return;
            }

            Coin coin = new();
            string faceResults = "";

            for (int i = 0; i < times; i++)
            {
                coin.Flip();
                faceResults += $"{(coin.GetFace() == "Heads" ? "🔼" : "🔽")} ";
            }

            var builder = new EmbedBuilder()
                .WithTitle($"{Context.User.Username}#{Context.User.Discriminator} flipped the coin {times} {(times == 1 ? "time" : "times")}")
                .WithDescription(faceResults)
                .WithColor(new Color(await _users.GetEmbedColor(Context.User.Id)))
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
        }

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
                .WithColor(new Color(await _users.GetEmbedColor(user.Id)))
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
                .WithColor(new Color(await _users.GetEmbedColor(Context.User.Id)))
                .AddField("Current Balance", $"{balance} :coin:", true)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }
    }
}
