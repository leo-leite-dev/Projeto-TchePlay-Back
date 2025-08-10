namespace TchePlay.Api.Data.Entities
{
    public sealed class Movie
    {
        public Guid Id { get; init; }
        public string VideoId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string ChannelTitle { get; set; } = default!;
        public string ThumbnailUrl { get; set; } = default!;
        public TimeSpan Duration { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool Embeddable { get; set; }
        public bool Approved { get; set; } = true;
        public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    }
}