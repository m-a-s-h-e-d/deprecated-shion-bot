using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Shion.Core.Preconditions
{
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider provider)
        {
            if (context.User is SocketGuildUser gUser)
            {
                return Task.FromResult(gUser.Id == Convert.ToUInt64(Environment.GetEnvironmentVariable("BOT_OWNER_ID")) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("You do not have sufficient permissions to use this command."));
            }

            return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
