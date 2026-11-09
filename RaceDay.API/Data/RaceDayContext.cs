using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<RouteWeatherInfo> RouteWeatherInfos => Set<RouteWeatherInfo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store the Role enum as text ("Organiser"/"Participant") so the column
            // matches the CHECK constraint used in the Part 1 SQL script.
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Enrolment>()
                .Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Event>()
                .Property(e => e.EventDate)
                .HasColumnType("date");

            modelBuilder.Entity<Event>()
                .Property(e => e.EventType)
                .HasConversion<string>()
                .HasMaxLength(20);

            // A Participant can only enrol once per Category - same rule as the
            // UQ_Enrolment constraint in Part 1.
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantID, e.CategoryID })
                .IsUnique();

            // One Result per Enrolment.
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            // Avoid EF Core generating cascade-delete cycles across Users -> Events -> Enrolments -> Results.
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.EventsOrganised)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.CapturedBy)
                .WithMany()
                .HasForeignKey(r => r.CapturedByID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
