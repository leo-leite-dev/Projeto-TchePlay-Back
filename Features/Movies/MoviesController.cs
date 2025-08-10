using Microsoft.AspNetCore.Mvc;

namespace TchePlay.Api.Features.Movies
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class MoviesController : ControllerBase
    {
        private readonly MoviesQueryService _queries;
        private readonly MovieIngestService _ingest;

        public MoviesController(MoviesQueryService queries, MovieIngestService ingest)
        {
            _queries = queries;
            _ingest = ingest;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> Recent(
            [FromQuery] int days = 30,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 24,
            CancellationToken ct = default)
            => Ok(await _queries.GetRecentAsync(days, page, pageSize, ct));

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 24,
            CancellationToken ct = default)
            => Ok(await _queries.GetAllAsync(page, pageSize, ct));

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 24,
            CancellationToken ct = default)
            => Ok(await _queries.SearchAsync(q, page, pageSize, ct));

        [HttpPost("ingest")]
        public async Task<IActionResult> Ingest(
            [FromQuery] string? q,
            [FromQuery] string region = "BR",
            [FromQuery] int maxResults = 50,
            CancellationToken ct = default)
        {
            const int MaxCap = 1000;

            if (maxResults <= 0) maxResults = 50;
            if (maxResults > MaxCap) maxResults = MaxCap;

            var upserts = await _ingest.IngestAsync(q, region, maxResults, ct);
            return Ok(new { upserts });
        }

        [HttpGet("by-year/{year:int}")]
        public async Task<IActionResult> GetByYear(
            [FromRoute] int year,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 24,
            CancellationToken ct = default)
        {
            var result = await _queries.GetByYearAsync(year, page, pageSize, ct);
            return Ok(result);
        }
    }
}