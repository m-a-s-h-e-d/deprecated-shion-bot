using Discord;

namespace Shion.Core.Structures;

public struct EmbedInfo
{
    public EmbedInfo(Color color, string? author, string? title, string? description, string? footer, DateTimeOffset? timestamp)
    {
        Color = color;
        Author = author;
        Title = title;
        Description = description;
        Footer = footer;
        Timestamp = timestamp;
        // Image URL
        // Thumbnail URL
        // URL
        // Timestamp
    }

    public Color Color { get; init; }
    public string? Author { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Footer { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
}