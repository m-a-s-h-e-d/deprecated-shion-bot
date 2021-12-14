using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Shion.Core.Preconditions
{
    /// <summary>
    /// The <see cref="PreconditionAttribute"/> to check if the command was issued by the bot owner.
    /// </summary>
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        /// <summary>
        /// Checks if the <see cref="ICommandContext"/> user is the bot owner.
        /// </summary>
        /// <param name="context">The <see cref="ICommandContext"/> to be passed.</param>
        /// <param name="command">The <see cref="CommandInfo"/> to be passed.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> to be passed.</param>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation. Produces a <see cref="PreconditionResult"/>.</returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context,
            CommandInfo command,
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
