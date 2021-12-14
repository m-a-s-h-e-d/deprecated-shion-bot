using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Shion.Core.Extensions
{
    /// <summary>
    /// Class storing static extension methods for Discord <see cref="Embed"/> and <see cref="EmbedBuilder"/> objects.
    /// </summary>
    public static class EmbedExtensions
    {
        /// <summary>
        /// Builds the passed <see cref="EmbedBuilder"/> and sends it to the specified <see cref="ISocketMessageChannel"/>.
        /// </summary>
        /// <param name="builder">The <see cref="EmbedBuilder"/> to be passed.</param>
        /// <param name="channel">The <see cref="ISocketMessageChannel"/> to be passed.</param>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation. Produces a <see cref="RestUserMessage"/>.</returns>
        public static async Task<RestUserMessage> BuildAndSendEmbed(this EmbedBuilder builder, ISocketMessageChannel channel)
        {
            var sentMessage = await channel.SendMessageAsync(embed: builder.Build());
            return await Task.FromResult(sentMessage);
        }

        /// <summary>
        /// Builds the passed <see cref="EmbedBuilder"/> and replies to the specified <see cref="IUserMessage"/>.
        /// </summary>
        /// <param name="builder">The <see cref="EmbedBuilder"/> to be passed.</param>
        /// <param name="message">The <see cref="IUserMessage"/> to be passed.</param>
        /// <returns>A <see cref="Task"/> representing the results of the asynchronous operation. Produces a <see cref="IUserMessage"/>.</returns>
        public static async Task<IUserMessage> BuildAndReplyEmbed(this EmbedBuilder builder, IUserMessage message)
        {
            var sentMessage = await message.ReplyAsync(embed: builder.Build());
            return await Task.FromResult(sentMessage);
        }
    }
}
