using Discord;

namespace Shion.Core.Structures
{
    public struct EmbedInfo
    {
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

        public Color Color { get; init; }

        public string? Author { get; init; }

        public string? Title { get; init; }

        public string? Description { get; init; }

        public string? Footer { get; init; }

        public DateTimeOffset? Timestamp { get; init; }
    }
}