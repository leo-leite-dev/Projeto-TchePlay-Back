using Microsoft.EntityFrameworkCore;
using TchePlay.Api.Config;
using TchePlay.Api.Data;
using TchePlay.Api.External.YouTube;
using TchePlay.Api.Features.Movies;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "DefaultCors";
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Dev: libera Angular local
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<YouTubeSettings>(builder.Configuration.GetSection("YouTube"));

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<YouTubeApiClient>();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<MoviesQueryService>();
builder.Services.AddScoped<MovieIngestService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.Run();