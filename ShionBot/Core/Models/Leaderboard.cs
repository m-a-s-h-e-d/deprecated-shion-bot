using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Schema;

namespace ShionBot.Core.Models
{
    public class LeaderboardUser
    {
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public long? Balance { get; set; }
        public long? Level { get; set; }
        public long? Experience { get; set; }
    }

    public class Leaderboard
    {
        private static readonly Color _botEmbedColor = new(4, 28, 99);

        private static async Task<EmbedBuilder> BuildLeaderboardEmbed(List<LeaderboardUser> userList, SchemaContext dbContext, string type, ulong serverId, IGuild guild = null)
        {
            var embed = new EmbedBuilder()
                .WithTitle($"{(serverId == 0 ? "Global" : guild.Name)} Leaderboard")
                .WithColor(_botEmbedColor)
                .WithCurrentTimestamp();

            int currentPosition = 1;
            foreach(LeaderboardUser user in userList)
            {
                string field = "";
                var name = await dbContext.Users.AsAsyncEnumerable()
                    .Where(x => x.UserId == user.UserId)
                    .Select(x => x.Username)
                    .FirstOrDefaultAsync();
                if (type.Equals("balance", StringComparison.OrdinalIgnoreCase))
                {
                    field = $"**{user.Balance} :coin:**";
                }
                else
                {
                    field = $"**Level {user.Level} - {user.Experience} EXP**";
                }
                embed = embed.AddField($"{currentPosition}. {name}", field, false);
                currentPosition++;
            }

            return await Task.FromResult(embed);
        }

        private static async Task<List<LeaderboardUser>> GetBalanceUsers(SchemaContext dbContext, ulong serverId)
        {
            var users = dbContext.ServerUsers.AsQueryable()
                    .Join(
                        dbContext.Balances,
                        serveruser => serveruser.UserId,
                        balance => balance.UserId,
                        (serveruser, balance) => new
                        {
                            serveruser.ServerId,
                            serveruser.UserId,
                            Balance = balance.Bal
                        });
            if (serverId != 0)
            {
                users = users
                    .Where(x => x.ServerId == serverId);
            }
            List<LeaderboardUser> list = users
                .Select(x => new LeaderboardUser()
                {
                    UserId = x.UserId,
                    Balance = x.Balance
                })
                .Distinct()
                .Take(10)
                .OrderByDescending(x => x.Balance)
                .ToList();

            return await Task.FromResult(list);
        }

        public static async Task<Embed> GetBalanceLeaderboard(SchemaContext dbContext, ulong serverId, IGuild guild)
        {
            List<LeaderboardUser> userList = await GetBalanceUsers(dbContext, serverId);
            return await Task.FromResult((await BuildLeaderboardEmbed(userList, dbContext, "balance", serverId, guild)).Build());
            
        }

        private static async Task<List<LeaderboardUser>> GetLevelUsers(SchemaContext dbContext, ulong serverId)
        {
            var users = dbContext.ServerUsers.AsQueryable()
                    .Join(
                        dbContext.Experiences,
                        serveruser => serveruser.UserId,
                        experience => experience.UserId,
                        (serveruser, experience) => new
                        {
                            serveruser.ServerId,
                            serveruser.UserId,
                            experience.Level,
                            Experience = experience.Exp
                        });
            if (serverId != 0)
            {
                users = users
                    .Where(user => user.ServerId == serverId);
            }

            List<LeaderboardUser> list = users
                .Select(user => new LeaderboardUser()
                {
                    UserId = user.UserId,
                    Level = user.Level,
                    Experience = user.Experience
                })
                .Distinct()
                .Take(10)
                .OrderByDescending(x => x.Level)
                .ToList();

            return await Task.FromResult(list);
        }

        public static async Task<Embed> GetLevelLeaderboard(SchemaContext dbContext, ulong serverId, IGuild guild)
        {
            List<LeaderboardUser> userList = await GetLevelUsers(dbContext, serverId);
            return await Task.FromResult((await BuildLeaderboardEmbed(userList, dbContext, "level", serverId, guild)).Build());
        }
    }
}
