using Discord;
using Shion.Modules;

namespace Shion.Core.Structures
{
    /// <summary>
    /// Struct that stores information for an embed, used in <see cref="ShionModuleBase"/>.
    /// </summary>
    public struct EmbedInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmbedInfo"/> struct.
        /// </summary>
        /// <param name="color">A nullable Discord <see cref="Color"/> to be passed.</param>
        /// <param name="author">A nullable author <see cref="string"/> to be passed.</param>
        /// <param name="title">A nullable title <see cref="string"/> to be passed.</param>
        /// <param name="description">A nullable description <see cref="string"/> to be passed.</param>
        /// <param name="footer">A nullable footer <see cref="string"/> to be passed.</param>
        /// <param name="timestamp">A nullable timestamp <see cref="DateTimeOffset"/> to be passed.</param>
        public EmbedInfo(Color color, string? author, string? title, string? description, string? footer, DateTimeOffset? timestamp)
        {
            this.Color = color;
            this.Author = author;
            this.Title = title;
            this.Description = description;
            this.Footer = footer;
            this.Timestamp = timestamp;

            /*
            Image URL
            Thumbnail URL
            URL
            Timestamp
            */
        }

        /// <summary>
        /// Gets the discord <see cref="Color"/> for the embed color.
        /// </summary>
        public Color Color { get; init; }

        /// <summary>
        /// Gets the author <see cref="string"/> for the author of the embed.
        /// </summary>
        public string? Author { get; init; }

        /// <summary>
        /// Gets the title <see cref="string"/> for the title of the embed.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Gets the description <see cref="string"/> for the description of the embed.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Gets the footer <see cref="string"/> for the footer of the embed.
        /// </summary>
        public string? Footer { get; init; }

        /// <summary>
        /// Gets the timestamp <see cref="DateTimeOffset"/> for the timestamp of the embed.
        /// </summary>
        public DateTimeOffset? Timestamp { get; init; }
    }
}