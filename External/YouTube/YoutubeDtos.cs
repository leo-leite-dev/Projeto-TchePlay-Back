namespace TchePlay.Api.External.YouTube
{
    public sealed record SearchResponse(string? NextPageToken, List<SearchItem> Items)
    {
        public SearchResponse() : this(null, new List<SearchItem>()) { }
    }

    public sealed record SearchItem(SearchId Id, SearchSnippet Snippet);
    public sealed record SearchId(string? VideoId);
    public sealed record SearchSnippet(string? Title, string? Description, string? ChannelTitle, DateTime? PublishedAt);

    public sealed record VideosResponse(List<VideoFull> Items)
    {
        public VideosResponse() : this(new List<VideoFull>()) { }
    }

    public sealed record VideoFull(string? Id, VideoSnippet Snippet, VideoContentDetails ContentDetails, VideoStatus Status);
    public sealed record VideoSnippet(string? Title, string? Description, string? ChannelTitle, DateTime? PublishedAt);
    public sealed record VideoContentDetails(string? Duration);
    public sealed record VideoStatus(bool Embeddable);
}