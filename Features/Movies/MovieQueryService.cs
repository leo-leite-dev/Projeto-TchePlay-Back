namespace TchePlay.Api.Features.Movies
{
    public sealed class MoviesQueryService(IMovieRepository repo)
    {
        private readonly IMovieRepository _repo = repo;

        public async Task<PagedResponse<MovieListItemResponse>> GetAllAsync(
            int page, int pageSize, CancellationToken ct)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 || pageSize > 100 ? 24 : pageSize;

            var (items, total) = await _repo.GetAllAsync(page, pageSize, ct);
            return new(items, page, pageSize, total);
        }

        public async Task<PagedResponse<MovieListItemResponse>> GetRecentAsync(
            int days, int page, int pageSize, CancellationToken ct)
        {
            days = days <= 0 ? 30 : days;
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 || pageSize > 100 ? 24 : pageSize;

            var (items, total) = await _repo.SearchRecentAsync(days, page, pageSize, ct);
            return new(items, page, pageSize, total);
        }

        public async Task<PagedResponse<MovieListItemResponse>> SearchAsync(
            string? q, int page, int pageSize, CancellationToken ct)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 || pageSize > 100 ? 24 : pageSize;

            var (items, total) = await _repo.SearchByTitleAsync(q, page, pageSize, ct);
            return new(items, page, pageSize, total);
        }

        public async Task<PagedResponse<MovieListItemResponse>> GetByYearAsync(
             int year, int page, int pageSize, CancellationToken ct)
        {
            var currentYear = DateTime.UtcNow.Year;
            if (year < 1900 || year > currentYear + 1) year = currentYear;

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 || pageSize > 100 ? 24 : pageSize;

            var (items, total) = await _repo.GetByYearAsync(year, page, pageSize, ct);
            return new(items, page, pageSize, total);
        }
    }
}