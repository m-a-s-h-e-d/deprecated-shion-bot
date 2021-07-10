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
    public class MinecraftModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<MinecraftModule> _logger;
        private readonly Servers _servers;
        private readonly Users _users;
        private readonly Balances _balances;
        private readonly Experiences _experiences;
        private readonly Color _botEmbedColor = new(4, 28, 99);

        public MinecraftModule(ILogger<MinecraftModule> logger, Servers servers, Users users, Balances balances, Experiences experiences)
        {
            _logger = logger;
            _servers = servers;
            _users = users;
            _balances = balances;
            _experiences = experiences;
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
                port = Convert.ToInt32(address[(colonIndex + 1)..]);

            // If port is valid, if not throw an exception
            if (port < 0 || port > 65535)
                throw new ArgumentException("Invalid port, check and try again.");

            // Check if the host is formatted as an ip or a string
            string host = "";
            if (!IPAddress.TryParse(address.Substring(0, colonIndex), out IPAddress dns))
            {
                host = address.Substring(0, colonIndex);
            }

            TcpClient MinecraftServer = new();
            string hostString = (host.Equals("")) ? dns.ToString() : host;
            _logger.LogInformation("Checking for server status of {dns}:{port}", hostString, port);
            var msg = await Context.Channel.SendMessageAsync($":yellow_circle: Checking server status of {hostString}:{port}...");

            // Attempt connection
            var sw = Stopwatch.StartNew();
            try
            {
                if (!MinecraftServer.ConnectAsync(hostString, port).Wait(TimeSpan.FromSeconds(5)))
                    throw new Exception();
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
    }
}
