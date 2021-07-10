using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Schema;

namespace ShionBot.Core.Models
{
    public class Leaderboard
    {
        public static Task GetBalanceLeaderboard(SchemaContext dbContext, ulong serverId)
        {
            var users = dbContext.ServerUsers.AsAsyncEnumerable()
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
                Console.WriteLine(users
                    .Where(x => x.ServerId == serverId)
                    .OrderByDescending(x => x.Balance)
                    .Take(10));
            }
            else
            {
                Console.WriteLine(users
                    .OrderByDescending(x => x.Balance)
                    .Take(10));
            }
            return Task.CompletedTask;
        }

        public static Task GetLevelLeaderboard(SchemaContext dbContext, ulong serverId)
        {
            var users = dbContext.ServerUsers.AsAsyncEnumerable()
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
                Console.WriteLine(users
                    .Where(x => x.ServerId == serverId)
                    .OrderByDescending(x => x.Level)
                    .Take(10));
            }
            else
            {
                Console.WriteLine(users
                    .OrderByDescending(x => x.Level)
                    .Take(10));
            }
            return Task.CompletedTask;
        }
    }
}
