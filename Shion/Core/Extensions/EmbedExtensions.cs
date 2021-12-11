namespace Shion.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Rest;
    using Discord.WebSocket;

    public static class EmbedExtensions
    {
        public static Task<RestUserMessage> BuildAndSendEmbed(this EmbedBuilder builder, ISocketMessageChannel channel)
        {
            return channel.SendMessageAsync(embed: builder.Build());
        }

        public static Task<IUserMessage> BuildAndReplyEmbed(this EmbedBuilder builder, IUserMessage message)
        {
            return message.ReplyAsync(embed: builder.Build());
        }
    }
}
