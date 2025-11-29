using MarketPlace.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Favourite> Favourites { get; set; }

    public virtual DbSet<MasterSkill> MasterSkills { get; set; }

    public virtual DbSet<MastersInfo> MastersInfos { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Job> Jobs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cities_pkey");
        });

        modelBuilder.Entity<Favourite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.JobId }).HasName("favourites_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Job).WithMany(p => p.Favourites).HasConstraintName("favourites_service_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Favourites).HasConstraintName("favourites_user_id_fkey");
        });


        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("services_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Category).WithMany(p => p.Jobs)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("services_category_id_fkey");

            entity.HasOne(d => d.Master).WithMany(p => p.Jobs).HasConstraintName("services_master_id_fkey");

            entity.HasMany(d => d.Tags).WithMany(p => p.Jobs)
                .UsingEntity<Dictionary<string, object>>(
                    "JobTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("services_tags_tag_id_fkey"),
                    l => l.HasOne<Job>().WithMany()
                        .HasForeignKey("JobId")
                        .HasConstraintName("services_tags_service_id_fkey"),
                    j =>
                    {
                        j.HasKey("JobId", "TagId").HasName("job_tags_pkey");
                        j.ToTable("job_tags");
                        j.IndexerProperty<Guid>("JobId").HasColumnName("job_id");
                        j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<MasterSkill>(entity =>
        {
            entity.HasOne(d => d.Master).WithMany().HasConstraintName("master_skills_master_id_fkey");

            entity.HasOne(d => d.Skill).WithMany()
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("master_skills_skill_id_fkey");
        });

        modelBuilder.Entity<MastersInfo>(entity =>
        {
            entity.HasOne(d => d.Master)
            .WithMany()
            .HasConstraintName("masters_info_master_id_fkey");
        });


        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.HasOne(d => d.Job).WithMany(p => p.Orders).HasConstraintName("orders_service_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Orders).HasConstraintName("orders_user_id_fkey");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("skills_pkey");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.CityNavigation).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("users_city_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
