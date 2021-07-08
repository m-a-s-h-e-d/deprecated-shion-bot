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

namespace ShionBot.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly Color _botEmbedColor = new(4, 28, 99);

        public General(ILogger<General> logger)
        {
            _logger = logger;
        }

        [Command("ping")]
        [Alias("pong", "ms", "latency")]
        public async Task PingAsync()
        {
            _logger.LogInformation("Current ping: {ping}ms.", (Context.Client).Latency);
            await Context.Channel.SendMessageAsync($"Responded in {(Context.Client).Latency}ms.");
        }

        [Command("info")]
        [Alias("user")]
        public async Task Info([Remainder]SocketGuildUser user = null)
        {
            var rnd = new Random();
            var randomEmbedColor = new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
            user ??= (SocketGuildUser)Context.User;

            var builder = new EmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s User Info")
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .AddField("User ID", user.Id, true)
                .AddField("Discriminator", user.Discriminator, true)
                .AddField("Created at", user.CreatedAt.ToString("MMMM dd, yyyy"), true)
                .AddField("Joined at", (user).JoinedAt.Value.ToString("MMMM dd, yyyy"), true)
                .WithColor(randomEmbedColor)
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("status")]
        public async Task Status([Remainder] SocketGuildUser user = null)
        {
            var rnd = new Random();
            var randomEmbedColor = new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
            user ??= (SocketGuildUser)Context.User;

            var builder = new EmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}'s Current Status")
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .AddField("Currently", user.Activity.Name, true)
                .WithColor(randomEmbedColor)
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("mcstatus")]
        [Alias("mc", "minecraft")]
        public async Task MinecraftStatus([Remainder] string address)
        {
            // Check if there is a colon in the address
            int colonIndex = address.IndexOf(':');

            // Check if a port is specified, if no port, use default 25565
            int port;
            if (colonIndex == -1)
            {
                port = 25565;
                address += ":25565"; // Jank way of restructuring the address to simplify code
                colonIndex = address.IndexOf(':');
            }
            else
                port = Convert.ToInt32(address.Substring(colonIndex + 1));

            // If port is valid, if not throw an exception
            if (port < 0 || port > 65535)
                throw new ArgumentException("Invalid port, check and try again.");

            // Check if the host is formatted as an ip or a string
            string host = "";
            if (!IPAddress.TryParse(address.Substring(0, colonIndex), out IPAddress dns))
            {
                host = address.Substring(0, colonIndex);
            }

            TcpClient MinecraftServer = new TcpClient();
            string hostString = (host.Equals("")) ? dns.ToString() : host;
            _logger.LogInformation("Checking for server status of {dns}:{port}", hostString, port);
            var msg = await Context.Channel.SendMessageAsync($":yellow_circle: Checking server status of {hostString}:{port}...");

            // Attempt connection
            var sw = Stopwatch.StartNew();
            try
            {
                if (host.Equals(""))
                    MinecraftServer.Connect(dns, port);
                else
                    MinecraftServer.Connect(host, port);
                sw.Stop();
                await msg.ModifyAsync(m => {
                    m.Content = $":green_circle: Successfully found a connection in {sw.ElapsedMilliseconds}ms. If the port is correct, the server should be up.";
                });
            }
            catch
            {
                sw.Stop();
                await msg.ModifyAsync(m => {
                    m.Content = $":red_circle: Could not establish a connection after {sw.ElapsedMilliseconds}ms. The server may be offline or the port may be closed.";
                });
            }
        }

        [Command("bot-info")]
        [Alias("bot")]
        public async Task BotInfo()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Shion Bot Information")
                .WithAuthor(author =>
                {
                    author
                    .WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
                })
                .WithDescription("Shion was created by Mashed#7999, contact them on Discord if you have any inquiries regarding this bot.")
                .WithColor(_botEmbedColor)
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("server-info")]
        [Command("server")]
        public async Task ServerInfo()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(_botEmbedColor)
                .AddField("Created at", Context.Guild.CreatedAt.ToString("MMMM dd, yyyy"), true)
                .AddField("Member Count", (Context.Guild).MemberCount + " members", true)
                .AddField("Online Users", (Context.Guild).Users.Where(x => x.Status == UserStatus.Online).Count() + " members", true);
            
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
    }
}
