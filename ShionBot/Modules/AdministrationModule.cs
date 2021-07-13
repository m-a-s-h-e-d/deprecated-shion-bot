using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Schema;
using ShionBot.Utilities;

namespace ShionBot.Modules
{
    public class AdministrationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<AdministrationModule> _logger;
        private readonly IHost _host;
        private readonly SchemaContext _dbContext;
        private readonly Servers _servers;
        private readonly Users _users;
        private readonly ServerUsers _serverusers;
        private readonly Balances _balances;
        private readonly Experiences _experiences;
        private readonly Color _botEmbedColor = new(4, 28, 99);

        public AdministrationModule(IHost host, ILogger<AdministrationModule> logger, SchemaContext dbContext, Servers servers, Users users, ServerUsers serverusers, Balances balances, Experiences experiences)
        {
            _host = host;
            _logger = logger;
            _dbContext = dbContext;
            _servers = servers;
            _users = users;
            _serverusers = serverusers;
            _balances = balances;
            _experiences = experiences;
        }

        [Command("compensate")]
        public async Task CompensateMoney(long balanceInjected, [Remainder] SocketGuildUser user = null)
        {
            if (Context.User.Id != 285106328790237195)
            {
                await ReplyAsync("You do not have sufficient permissions to use this command.\nAuthority Level: **[Bot Owner]**");
                return;
            }

            //TODO Make an @everyone check
            if (user == null)
                throw new ArgumentException("No user was specified.");

            // Add money to the target user
            await _balances.ModifyBalance(user.Id, +balanceInjected);

            var builder = new EmbedBuilder()
                .WithColor(new Color(await _users.GetEmbedColor(user.Id, UserUtil.GetFullUsername(user))))
                .WithTitle($"{user.Username} Received Money")
                .AddField("Amount Transferred", $"{balanceInjected} :coin:", true)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: builder.Build());
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await ReplyAsync($"{messages.Count()} messages were deleted.");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? ".";
                await ReplyAsync($"The current prefix of this bot is `{guildPrefix}`.");
                return;
            }

            if (prefix.Length > 8)
            {
                await ReplyAsync($"The length of the new prefix is too long!");
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The new prefix of this bot is `{prefix}`.");
        }

        [Command("log")]
        public async Task TestLogs()
        {
            if (Context.User.Id != 285106328790237195)
            {
                await ReplyAsync("You do not have sufficient permissions to use this command.\nAuthority Level: **[Bot Owner]**");
                return;
            }

            _logger.LogTrace("This is a trace log");
            _logger.LogDebug("This is a debug log");
            _logger.LogInformation("This is an information log");
            _logger.LogWarning("This is a warning log");
            _logger.LogError(new InvalidOperationException("Invalid Operation"), "This is a error log with exception");
            _logger.LogCritical(new InvalidOperationException("Invalid Operation"), "This is a critical load with exception");

            _logger.Log(GetLogLevel(LogSeverity.Error), "Error logged from a Discord LogSeverity.Error");
            _logger.Log(GetLogLevel(LogSeverity.Info), "Information logged from Discord LogSeverity.Info ");
        }

        [Command("shutdown")]
        public async Task Stop()
        {
            if (Context.User.Id != 285106328790237195)
            {
                await ReplyAsync("You do not have sufficient permissions to use this command.\nAuthority Level: *[Bot Owner]*");
                return;
            }

            _ = _host.StopAsync();
        }

        [Command("eval", RunMode = RunMode.Async)]
        public async Task EvaluateAsync([Remainder] string code)
        {
            if (Context.User.Id != 285106328790237195)
            {
                await ReplyAsync("You do not have sufficient permissions to use this command.\nAuthority Level: **[Bot Owner]**");
                return;
            }

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf("\n", cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.", nameof(code));

            code = code[cs1..cs2];
            _logger.LogInformation("User {user} executed the following evaluation: {code}\n", Context.User, code);

            var embed = new EmbedBuilder()
                .WithTitle("Evaluating...")
                .WithColor(_botEmbedColor);
            var msg = await ReplyAsync(null, false, embed.Build());

            var globals = new EvaluationEnvironment(Context);
            var sopts = ScriptOptions.Default
                .WithImports("System", "System.Collections.Generic", "System.Diagnostics", "System.Linq", "System.Text",
                             "System.Threading.Tasks", "System.Net.Sockets", "System.Net", "Discord", "Discord.Commands", "Discord.WebSocket",
                             "Microsoft.CodeAnalysis", "Microsoft.CodeAnalysis.CSharp.Scripting", "Microsoft.CodeAnalysis.Scripting",
                             "Microsoft.Extensions.Hosting", "Microsoft.Extensions.Logging", "Schema", "ShionBot.Core.Models", "ShionBot.Utilities")
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var sw1 = Stopwatch.StartNew();
            var cs = CSharpScript.Create(code, sopts, typeof(EvaluationEnvironment));
            var csc = cs.Compile();

            if (csc.Any(xd => xd.Severity == DiagnosticSeverity.Error))
            {
                embed = new EmbedBuilder()
                    .WithTitle("Compilation failed")
                    .WithDescription($"Compilation failed after {sw1.ElapsedMilliseconds:#.##0}ms with {csc.Length:#,##0} errors.")
                    .WithColor(_botEmbedColor);

                foreach (var xd in csc.Take(3))
                {
                    var ls = xd.Location.GetLineSpan();
                    embed.AddField(string.Concat("Error at ", ls.StartLinePosition.Line.ToString("#,##0"), ", ", ls.StartLinePosition.Character.ToString("#,##0")), Format.Code(xd.GetMessage()));
                }
                if (csc.Length > 3)
                {
                    embed.AddField("Some errors omitted", string.Concat((csc.Length - 3).ToString("#,##0"), " more errors not displayed"), false);
                }
                await msg.ModifyAsync(m => {
                    m.Embed = embed.Build();
                });
                return;
            }

            Exception rex = null;
            ScriptState<object> css = null;
            var sw2 = Stopwatch.StartNew();
            try
            {
                css = await cs.RunAsync(globals);
                rex = css.Exception;
            }
            catch (Exception ex)
            {
                rex = ex;
            }
            sw2.Stop();
            if (rex != null)
            {
                embed = new EmbedBuilder()
                    .WithTitle("Execution failed")
                    .WithDescription($"Execution failed after {sw2.ElapsedMilliseconds:#,##0}ms with `{rex.GetType()}: {rex.Message}`.")
                    .WithColor(_botEmbedColor);
                await msg.ModifyAsync(m => {
                    m.Embed = embed.Build();
                });
                return;
            }

            embed = new EmbedBuilder()
                .WithTitle("Evaluation successful")
                .WithColor(_botEmbedColor)
                .AddField("Result", css.ReturnValue != null ? css.ReturnValue.ToString() : "No value returned", false)
                .AddField("Compilation time", string.Concat(sw1.ElapsedMilliseconds.ToString("#,##0"), "ms"), true)
                .AddField("Execution time", string.Concat(sw2.ElapsedMilliseconds.ToString("#,##0"), "ms"), true);

            if (css.ReturnValue != null)
                embed.AddField("Return type", css.ReturnValue.GetType().ToString(), true);

            await msg.ModifyAsync(m => {
                m.Embed = embed.Build();
            });
        }

        private static LogLevel GetLogLevel(LogSeverity severity)
            => (LogLevel)Math.Abs((int)severity - 5);

        public sealed class EvaluationEnvironment
        {
            public ICommandContext Context { get; }

            public IUserMessage Message => this.Context.Message;
            public IMessageChannel Channel => this.Context.Channel;
            public IGuild Guild => this.Context.Guild;
            public IUser User => this.Context.User;
            public IGuildUser Member => this.Context.User as IGuildUser;
            public IDiscordClient Client => this.Context.Client;

            public EvaluationEnvironment(ICommandContext Context)
            {
                this.Context = Context;
            }
        }
    }
}
