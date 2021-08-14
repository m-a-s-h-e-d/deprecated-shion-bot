using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schema;
using ShionBot.Extensions;
using ShionBot.Utilities;

namespace ShionBot.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly Servers _servers;
        private readonly Users _users;
        private readonly ServerUsers _serverusers;
        private readonly Balances _balances;
        private readonly Experiences _experiences;
        private readonly Color _botEmbedColor = new(4, 28, 99);

        public General(ILogger<General> logger, Servers servers, Users users, ServerUsers serverusers, Balances balances, Experiences experiences)
        {
            _logger = logger;
            _servers = servers;
            _users = users;
            _serverusers = serverusers;
            _balances = balances;
            _experiences = experiences;
        }

        // Temporary for adding a user to the database
        [Command("tempdb")]
        public async Task TempDb([Remainder] SocketGuildUser user = null)
        {
            user ??= (SocketGuildUser)Context.User;

            if (user == null)
                throw new ArgumentException("No user was specified.");

            await _serverusers.AddServerUser(user.Id, Context.Guild.Id);
        }

        [Command("ping")]
        [Alias("pong", "ms", "latency")]
        public async Task PingAsync()
        {
            _logger.LogInformation("Current ping: {ping}ms.", (Context.Client).Latency);
            await Context.Channel.SendMessageAsync($"Responded in {(Context.Client).Latency}ms.");
        }

        [Command("info")] // This will be modified to look better in the future, along with balance and level.
        [Alias("user")] // Level and balance commands will be separated from this for neatness.
        public async Task Info([Remainder]SocketGuildUser user = null)
        {
            user ??= (SocketGuildUser)Context.User;

            await new EmbedBuilder()
                .WithTitle($"{UserUtil.GetFullUsername(user)}'s User Info")
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .AddField("User ID", user.Id, true)
                .AddField("Discriminator", user.Discriminator, true)
                .AddField("Created at", user.CreatedAt.ToString("MMMM dd, yyyy"), true)
                .AddField("Joined at", (user).JoinedAt.Value.ToString("MMMM dd, yyyy"), true)
                .WithColor(new Color(await _users.GetEmbedColor(user.Id, UserUtil.GetFullUsername(user))))
                .WithCurrentTimestamp()
                .BuildAndSendEmbed(Context.Channel);
        }

        [Command("updateusername")]
        public async Task UpdateUsername()
        {
            var user = Context.User;

            await _users.ModifyUsername(user.Id, UserUtil.GetFullUsername(user));

            await new EmbedBuilder()
                .WithTitle("Updated Username")
                .WithDescription($"Your username was updated in the database.")
                .AddField("New username", UserUtil.GetFullUsername(user), true)
                .WithColor(_botEmbedColor)
                .WithCurrentTimestamp()
                .BuildAndReplyEmbed(Context.Message);
        }

        [Command("rep")]
        public async Task RepAsync([Remainder] SocketGuildUser user = null)
        {
            var lastRep = await _users.GetLastRep(Context.User.Id, UserUtil.GetFullUsername(Context.User));

            var now = DateTime.Now;

            user ??= (SocketGuildUser)Context.User;

            if (user == null)
                throw new ArgumentException("No user was specified.");
            if (user.Id == Context.User.Id)
            {
                await new EmbedBuilder()
                    .WithTitle($"{user.Username}'s Reputation Count")
                    .AddField("Reputation", $"{await _users.GetRepCount(user.Id, UserUtil.GetFullUsername(user))} 🔼", true)
                    .WithColor(new Color(await _users.GetEmbedColor(user.Id, UserUtil.GetFullUsername(user))))
                    .WithCurrentTimestamp()
                    .BuildAndReplyEmbed(Context.Message);
                return;
            }

            if (!TimerUtil.CheckValidDaily(now, lastRep))
            {
                TimeSpan? span = TimerUtil.GetTimeDifference(now, lastRep);
                if (span == null)
                    throw new ArgumentException("An unexpected null error has occurred!");
                TimeSpan differenceSpan = new TimeSpan(23, 0, 0).Subtract(span.Value);

                await new EmbedBuilder()
                    .WithTitle("You need to wait to give your next rep!")
                    .AddField("Time remaining", TimerUtil.FormattedSpan(differenceSpan), true)
                    .WithColor(new Color(await _users.GetEmbedColor(Context.User.Id, UserUtil.GetFullUsername(Context.User))))
                    .WithCurrentTimestamp()
                    .BuildAndReplyEmbed(Context.Message);
                return;
            }

            await _users.ModifyRep(user.Id, 1, UserUtil.GetFullUsername(user));
            await _users.ModifyLastRep(Context.User.Id, now, UserUtil.GetFullUsername(user));

            await new EmbedBuilder()
                .WithTitle($"{Context.User.Username} => {user.Username}")
                .WithDescription($"{Context.User.Username} repped {user.Username}!")
                .AddField("New rep count", $"{await _users.GetRepCount(user.Id, UserUtil.GetFullUsername(user))} 🔼", true)
                .WithColor(new Color(await _users.GetEmbedColor(user.Id, UserUtil.GetFullUsername(user))))
                .WithCurrentTimestamp()
                .BuildAndReplyEmbed(Context.Message);
        }

        [Command("status")]
        public async Task Status([Remainder] SocketGuildUser user = null)
        {
            user ??= (SocketGuildUser)Context.User;
            var activity = user.Activity.Name ?? "Nothing";

            await new EmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s Current Status")
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .AddField("Currently", activity, true)
                .WithColor(new Color(await _users.GetEmbedColor(user.Id, UserUtil.GetFullUsername(user))))
                .WithCurrentTimestamp()
                .BuildAndSendEmbed(Context.Channel);
        }

        [Command("setembed")]
        public async Task ModifyEmbedColor(string hexColor)
        {
            if (!Regex.IsMatch(hexColor, @"^[0-9a-fA-F]{6}$"))
            {
                await ReplyAsync("Not a valid hex code for color. (e.g: 000000 - FFFFFF)");
                return;
            }

            var user = Context.User;

            await _users.ModifyEmbedColor(user.Id, hexColor, user.Username);

            await new EmbedBuilder()
                .WithTitle($"Updated {user.Username}#{user.Discriminator}'s Embed Color")
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .AddField("New Color", hexColor, true)
                .WithColor(new Color(await _users.GetEmbedColor(user.Id, UserUtil.GetFullUsername(user))))
                .WithCurrentTimestamp()
                .BuildAndSendEmbed(Context.Channel);
        }

        [Command("botinfo")]
        [Alias("bot")]
        public async Task BotInfo()
        {
            await new EmbedBuilder()
                .WithTitle("Shion Bot Information")
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
                })
                .WithDescription("Shion was created by Mashed#7999, contact them on Discord if you have any inquiries regarding this bot.")
                .WithColor(_botEmbedColor)
                .WithCurrentTimestamp()
                .BuildAndSendEmbed(Context.Channel);
        }

        [Command("serverinfo")]
        [Alias("server")]
        public async Task ServerInfo()
        {
            await new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(_botEmbedColor)
                .AddField("Created at", Context.Guild.CreatedAt.ToString("MMMM dd, yyyy"), true)
                .AddField("Member Count", (Context.Guild).MemberCount + " members", true)
                .AddField("Online Users", (Context.Guild).Users.Where(x => x.Status == UserStatus.Online).Count() + " members", true)
                .BuildAndSendEmbed(Context.Channel);
        }
    }
}
