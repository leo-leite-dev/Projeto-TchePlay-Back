namespace TchePlay.Api.Features.Movies
{
    public sealed record MovieListItemResponse(string VideoId, string Title, string ChannelTitle, string ThumbnailUrl, string DurationIso, DateTime PublishedAt);
    public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
}