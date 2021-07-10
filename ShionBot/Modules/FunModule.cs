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
using ShionBot.Core.Models;
using ShionBot.Utilities;

namespace ShionBot.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<FunModule> _logger;
        private readonly SchemaContext _dbContext;
        private readonly Servers _servers;
        private readonly Users _users;
        private readonly ServerUsers _serverusers;
        private readonly Balances _balances;
        private readonly Experiences _experiences;
        private readonly Color _botEmbedColor = new(4, 28, 99);

        public FunModule(ILogger<FunModule> logger, SchemaContext dbContext, Servers servers, Users users, ServerUsers serverusers, Balances balances, Experiences experiences)
        {
            _logger = logger;
            _dbContext = dbContext;
            _servers = servers;
            _users = users;
            _serverusers = serverusers;
            _balances = balances;
            _experiences = experiences;
        }

        [Command("tempdb")]
        public async Task TempDb()
        {
            await _serverusers.AddServerUser(Context.User.Id, Context.Guild.Id);
        }

        [Command("leaderboard")]
        [Alias("lb")]
        public async Task GetLeaderboardAsync(string type, Optional<string> view)
        {
            // This is so god damn ugly what the hell.
            string invalidParameterMsg = "Invalid parameter was given. (e.g: leaderboard {balance | level} {server | global})";
            string specifiedView = "server";

            if (view.IsSpecified)
            {
                specifiedView = view.ToString().ToLower();
                if (specifiedView.Equals("server") || !specifiedView.Equals("global"))
                {
                    await ReplyAsync(invalidParameterMsg);
                    return;
                }
            }

            switch (type.ToLower())
            {
                case "balance":
                    await Leaderboard.GetBalanceLeaderboard(_dbContext, (specifiedView.Equals("server") ? Context.Guild.Id : 0));
                    return;
                case "level":
                    await Leaderboard.GetLevelLeaderboard(_dbContext, (specifiedView.Equals("server") ? Context.Guild.Id : 0));
                    return;
                default:
                    await ReplyAsync(invalidParameterMsg);
                    return;
            }
        }

        [Command("daily")]
        [Alias("claim")]
        public async Task DailyClaim([Remainder] SocketGuildUser user = null)
        {
            var lastClaim = await _balances.GetLastClaim(Context.User.Id);

            var now = DateTime.Now;

            if (!TimerUtil.CheckValidDaily(now, lastClaim))
            {
                TimeSpan? span = TimerUtil.GetTimeDifference(now, lastClaim);
                if (span == null)
                    throw new ArgumentException("An unexpected null error has occurred!");
                TimeSpan differenceSpan = new TimeSpan(23, 0, 0).Subtract(span.Value);

                var invalidBuilder = new EmbedBuilder()
                    .WithTitle("You need to wait for your next daily!")
                    .AddField("Time remaining", TimerUtil.FormattedSpan(differenceSpan), true)
                    .WithColor(new Color(await _users.GetEmbedColor(Context.User.Id)))
                    .WithCurrentTimestamp();

                await ReplyAsync(null, false, invalidBuilder.Build());
                return;
            }

            user ??= (SocketGuildUser)Context.User;

            if (user == null)
                throw new ArgumentException("No user was specified.");

            string message = $"{Context.User.Username}#{Context.User.Discriminator} claimed their daily!";

            var claimAmount = 100 + (25 * (await _experiences.GetLevel(Context.User.Id) - 1));
            // Increase balance by daily top-off
            await _balances.ModifyBalance(user.Id, claimAmount);
            await _balances.ModifyLastClaim(user.Id, now);

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
            // TODO Allow amounts as percentages / all / half (Probably convert this in to a utility for checking value)
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
