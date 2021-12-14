using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Shion.Core.Structures;
using Shion.Modules.Utility;

namespace Shion.Modules
{
    /// <summary>
    /// The common <see cref="ModuleBase"/> with methods used in other modules.
    /// </summary>
    public abstract class ShionModuleBase : ModuleBase<ShardedCommandContext>
    {
        /// <summary>
        /// Configures settings based on the passed <see cref="EmbedInfo"/> and returns a new <see cref="EmbedBuilder"/>.
        /// </summary>
        /// <param name="embedInfo">The <see cref="EmbedInfo"/> to be passed.</param>
        /// <returns>A configured <see cref="EmbedBuilder"/> representing a discord embed builder.</returns>
        public static EmbedBuilder CreateEmbedBuilder(EmbedInfo embedInfo)
        {
            var builder = new EmbedBuilder()
                .WithColor(embedInfo.Color)
                .WithAuthor(embedInfo.Author)
                .WithTitle(embedInfo.Title)
                .WithDescription(embedInfo.Description)
                .WithFooter(embedInfo.Footer);

            return (embedInfo.Timestamp != null) ? builder.WithTimestamp((DateTimeOffset)embedInfo.Timestamp) : builder.WithCurrentTimestamp();
        }
    }
}
