using Microsoft.EntityFrameworkCore;
using System.Xml;
using TchePlay.Api.Data;
using TchePlay.Api.Data.Entities;
using TchePlay.Api.External.YouTube;

namespace TchePlay.Api.Features.Movies
{
    public sealed class MovieIngestService(
    AppDbContext db,
    YouTubeApiClient yt,
    ILogger<MovieIngestService> logger)
    {
        private readonly AppDbContext _db = db;
        private readonly YouTubeApiClient _yt = yt;
        private readonly ILogger<MovieIngestService> _log = logger;

        public async Task<int> IngestAsync(string? q, string region = "BR", int maxResults = 50, CancellationToken ct = default)
        {
            var queries = string.IsNullOrWhiteSpace(q)
                ? new[] { "filme completo dublado", "filme dublado pt-br", "filme dublado português" }
                : new[] { q! };

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var term in queries)
            {
                string? pageToken = null;

                while (ids.Count < maxResults)
                {
                    var sr = await _yt.SearchMoviesAsync(term, region: region, max: 50, page: pageToken, ct: ct);

                    foreach (var id in sr.Items.Select(i => i.Id.VideoId).Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        ids.Add(id!);
                        if (ids.Count >= maxResults) break;
                    }

                    if (ids.Count >= maxResults) break;

                    pageToken = sr.NextPageToken;
                    if (string.IsNullOrEmpty(pageToken)) break;
                }

                if (ids.Count >= maxResults) break;
            }

            if (ids.Count == 0)
            {
                _log.LogInformation("Ingest: sem resultados para q='{Q}', region={Region}", q, region);
                return 0;
            }

            static IEnumerable<string[]> ChunkIds(IEnumerable<string> source, int size)
            {
                var batch = new List<string>(size);
                foreach (var s in source)
                {
                    batch.Add(s);
                    if (batch.Count == size) { yield return batch.ToArray(); batch.Clear(); }
                }
                if (batch.Count > 0) yield return batch.ToArray();
            }

            var details = new List<VideoFull>(ids.Count);
            foreach (var batch in ChunkIds(ids, 50))
            {
                var vr = await _yt.GetVideosAsync(batch, ct);
                if (vr?.Items is { Count: > 0 }) details.AddRange(vr.Items);
            }

            if (details.Count == 0)
            {
                _log.LogInformation("Ingest: sem detalhes retornados pelo YouTube. q='{Q}', region={Region}", q, region);
                return 0;
            }

            var filtered = details.Where(v =>
                v.Status?.Embeddable == true &&
                ParseIso(v.ContentDetails?.Duration) >= TimeSpan.FromMinutes(20) &&
                !LooksLikeReview(v.Snippet?.Title, v.Snippet?.Description) &&
                IsPortugueseDubbed(v.Snippet?.Title, v.Snippet?.Description)
            ).ToList();

            if (filtered.Count == 0)
            {
                _log.LogInformation("Ingest: nenhum vídeo aprovado após filtros. q='{Q}'", q);
                return 0;
            }

            var videoIds = filtered.Select(v => v.Id!).ToArray();
            var existing = await _db.Movies
                .Where(m => videoIds.Contains(m.VideoId))
                .ToDictionaryAsync(m => m.VideoId, ct);

            var upserts = 0;

            foreach (var v in filtered)
            {
                if (v.Id is null) continue;

                var title = v.Snippet?.Title?.Trim() ?? "";
                var channel = v.Snippet?.ChannelTitle?.Trim() ?? "";
                var publishedAt = v.Snippet?.PublishedAt ?? DateTime.UtcNow;
                var duration = ParseIso(v.ContentDetails?.Duration);
                var thumb = BuildThumbnailUrl(v.Id);

                if (existing.TryGetValue(v.Id, out var entity))
                {
                    entity.Title = title;
                    entity.ChannelTitle = channel;
                    entity.PublishedAt = publishedAt;
                    entity.Duration = duration;
                    entity.ThumbnailUrl = thumb;
                    entity.Embeddable = true;
                    entity.Approved = true;
                    entity.IngestedAt = DateTime.UtcNow;
                }
                else
                {
                    entity = new Movie
                    {
                        VideoId = v.Id,
                        Title = title,
                        ChannelTitle = channel,
                        PublishedAt = publishedAt,
                        Duration = duration,
                        ThumbnailUrl = thumb,
                        Embeddable = true,
                        Approved = true,
                        IngestedAt = DateTime.UtcNow
                    };
                    await _db.Movies.AddAsync(entity, ct);
                }

                upserts++;
            }

            await _db.SaveChangesAsync(ct);
            _log.LogInformation("Ingest: {Count} upserts (q='{Q}', region={Region})", upserts, q, region);
            return upserts;
        }

        static TimeSpan ParseIso(string? iso) => XmlConvert.ToTimeSpan(iso ?? "PT0S");

        static bool LooksLikeReview(string? t, string? d)
        {
            var s = string.Concat(t, " ", d).ToLowerInvariant();
            return s.Contains("review") || s.Contains("resenha") || s.Contains("análise");
        }

        static string BuildThumbnailUrl(string videoId)
            => $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

        static bool IsPortugueseDubbed(string? title, string? description)
        {
            var text = string.Concat(title, " ", description).ToLowerInvariant();

            bool hasDubPt =
                text.Contains("dublado") || text.Contains("dublada") || text.Contains("dublagem") ||
                text.Contains("pt-br") || text.Contains("ptbr") ||
                text.Contains("português") || text.Contains("portugues") ||
                text.Contains("áudio português") || text.Contains("audio português") || text.Contains("audio portugues");

            bool saysSubOnly =
                (text.Contains("legendado") || text.Contains("leg.") || text.Contains("leg ")
                 || text.Contains("[leg]") || text.Contains("subtitulado") || text.Contains("com legendas"))
                && !(text.Contains("dublado") || text.Contains("dublada") || text.Contains("dublagem"));

            if (saysSubOnly) return false;
            return hasDubPt;
        }
    }
}