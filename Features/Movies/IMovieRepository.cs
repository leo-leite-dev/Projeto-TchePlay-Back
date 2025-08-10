namespace TchePlay.Api.Features.Movies
{
    public interface IMovieRepository
    {
        Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken ct);
        Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> SearchRecentAsync(int days, int page, int pageSize, CancellationToken ct);
        Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> SearchByTitleAsync(string? q, int page, int pageSize, CancellationToken ct);
        Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> GetByYearAsync(int year, int page, int pageSize, CancellationToken ct);
    }
}