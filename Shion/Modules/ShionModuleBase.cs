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
    public abstract class ShionModuleBase : ModuleBase<ShardedCommandContext>
    {
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
