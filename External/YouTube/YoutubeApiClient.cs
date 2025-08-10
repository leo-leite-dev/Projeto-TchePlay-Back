using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TchePlay.Api.Config;

namespace TchePlay.Api.External.YouTube
{
    public sealed class YouTubeApiClient
    {
        private readonly HttpClient _http;
        private readonly YouTubeSettings _settings;

        public YouTubeApiClient(HttpClient http, IOptions<YouTubeSettings> settings)
        {
            _http = http;
            _settings = settings.Value ?? throw new InvalidOperationException("YouTube settings not configured.");
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException("YouTube:ApiKey is missing.");
        }

        public async Task<SearchResponse> SearchMoviesAsync(
            string? q,
            string region = "BR",
            int max = 50,
            string? page = null,
            CancellationToken ct = default)
        {
            var url = QueryHelpers.AddQueryString("https://www.googleapis.com/youtube/v3/search",
                new Dictionary<string, string?>
                {
                    ["key"] = _settings.ApiKey,
                    ["part"] = "snippet",
                    ["type"] = "video",
                    ["videoType"] = "movie",
                    ["videoDuration"] = "long",
                    ["regionCode"] = region,
                    ["relevanceLanguage"] = "pt",
                    ["maxResults"] = max.ToString(),
                    ["q"] = q ?? "",
                    ["pageToken"] = page
                });

            return await _http.GetFromJsonAsync<SearchResponse>(url, ct)
                   ?? new SearchResponse(null, new());
        }

        public async Task<VideosResponse> GetVideosAsync(IEnumerable<string> ids, CancellationToken ct = default)
        {
            var idList = ids?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray() ?? Array.Empty<string>();
            if (idList.Length == 0) return new VideosResponse(new());

            var url = QueryHelpers.AddQueryString("https://www.googleapis.com/youtube/v3/videos",
                new Dictionary<string, string?>
                {
                    ["key"] = _settings.ApiKey,
                    ["part"] = "snippet,contentDetails,status",
                    ["id"] = string.Join(",", idList)
                });

            return await _http.GetFromJsonAsync<VideosResponse>(url, ct) ?? new VideosResponse(new());
        }
    }
}