using Microsoft.EntityFrameworkCore;
using System.Xml;
using TchePlay.Api.Data;

namespace TchePlay.Api.Features.Movies
{
    public sealed class MovieRepository(AppDbContext db) : IMovieRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> GetAllAsync(
       int page, int pageSize, CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Movies.AsNoTracking()
                .Where(m => m.Approved);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(m => m.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieListItemResponse(
                    m.VideoId, m.Title, m.ChannelTitle, m.ThumbnailUrl,
                    XmlConvert.ToString(m.Duration), m.PublishedAt))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> SearchRecentAsync(
            int days, int page, int pageSize, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var query = _db.Movies.AsNoTracking()
                .Where(m => m.Approved && m.PublishedAt >= since);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(m => m.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieListItemResponse(
                    m.VideoId, m.Title, m.ChannelTitle, m.ThumbnailUrl,
                    XmlConvert.ToString(m.Duration), m.PublishedAt))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> SearchByTitleAsync(
            string? q, int page, int pageSize, CancellationToken ct)
        {
            var query = _db.Movies.AsNoTracking()
                .Where(m => m.Approved);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(m => EF.Functions.ILike(m.Title, $"%{q}%"));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(m => m.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieListItemResponse(
                    m.VideoId, m.Title, m.ChannelTitle, m.ThumbnailUrl,
                    XmlConvert.ToString(m.Duration), m.PublishedAt))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<MovieListItemResponse> Items, int Total)> GetByYearAsync(
      int year, int page, int pageSize, CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddYears(1);

            var query = _db.Movies.AsNoTracking()
                .Where(m => m.Approved && m.PublishedAt >= start && m.PublishedAt < end);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(m => m.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovieListItemResponse(
                    m.VideoId, m.Title, m.ChannelTitle, m.ThumbnailUrl,
                    XmlConvert.ToString(m.Duration), m.PublishedAt))
                .ToListAsync(ct);

            return (items, total);
        }
    }
}