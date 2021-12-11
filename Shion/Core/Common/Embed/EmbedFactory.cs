using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Shion.Core.Structures;

namespace Shion.Core.Common.Embed
{
    public static class EmbedFactory
    {
        public static EmbedBuilder CreateEmbedBuilder(EmbedInfo embedInfo) =>
            new EmbedBuilder()
                .WithColor(embedInfo.Color)
                .WithAuthor(embedInfo.Author)
                .WithTitle(embedInfo.Title)
                .WithDescription(embedInfo.Description)
                .WithFooter(embedInfo.Footer);
    }
}
