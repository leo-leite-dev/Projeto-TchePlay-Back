using Microsoft.EntityFrameworkCore;
using TchePlay.Api.Data.Entities;

namespace TchePlay.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("uuid-ossp");

        b.Entity<Movie>(e =>
        {
            e.ToTable("movies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.VideoId)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.Title)
                .HasMaxLength(300)
                .IsRequired();

            e.Property(x => x.ChannelTitle)
                .HasMaxLength(200)
                .IsRequired();

            e.Property(x => x.ThumbnailUrl)
                .HasMaxLength(500)
                .IsRequired();

            e.Property(x => x.Duration).IsRequired();

            e.Property(x => x.PublishedAt).IsRequired();
            e.Property(x => x.Embeddable).IsRequired();
            e.Property(x => x.Approved).HasDefaultValue(true).IsRequired();

            e.HasIndex(x => x.VideoId).IsUnique();
            e.HasIndex(x => new { x.Approved, x.PublishedAt });
        });
    }
}