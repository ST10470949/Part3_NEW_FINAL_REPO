using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RaceDay.API.Data;
using RaceDay.API.Models;

#nullable disable

namespace RaceDay.API.Migrations
{
    [DbContext(typeof(RaceDayContext))]
    partial class InitialRaceDay
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("RaceDay.API.Models.Category", b =>
            {
                b.Property<int>("CategoryID").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<decimal>("EntryFee").HasColumnType("decimal(8,2)");
                b.Property<int>("EventID").HasColumnType("int");
                b.Property<int>("MaxParticipants").ValueGeneratedOnAdd().HasColumnType("int").HasDefaultValue(100);
                b.Property<string>("Name").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
                b.HasKey("CategoryID");
                b.HasIndex("EventID");
                b.ToTable("Categories");
            });

            modelBuilder.Entity("RaceDay.API.Models.Event", b =>
            {
                b.Property<int>("EventID").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<DateTime>("CreatedAt").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.Property<string>("Description").HasColumnType("nvarchar(max)");
                b.Property<string>("BannerImageUrl").HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<decimal>("DistanceKm").HasColumnType("decimal(6,2)");
                b.Property<DateTime>("EventDate").HasColumnType("date");
                b.Property<string>("EventType").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.Property<string>("Location").IsRequired().HasMaxLength(150).HasColumnType("nvarchar(150)");
                b.Property<string>("Name").IsRequired().HasMaxLength(150).HasColumnType("nvarchar(150)");
                b.Property<int>("OrganiserID").HasColumnType("int");
                b.HasKey("EventID");
                b.HasIndex("OrganiserID");
                b.ToTable("Events");
            });

            modelBuilder.Entity("RaceDay.API.Models.Enrolment", b =>
            {
                b.Property<int>("EnrolmentID").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("CategoryID").HasColumnType("int");
                b.Property<DateTime>("EnrolmentDate").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.Property<int>("ParticipantID").HasColumnType("int");
                b.Property<EnrolmentStatus>("Status").HasConversion<string>().HasMaxLength(20).HasColumnType("nvarchar(20)").HasDefaultValue(EnrolmentStatus.Confirmed);
                b.HasKey("EnrolmentID");
                b.HasIndex("CategoryID");
                b.HasIndex("ParticipantID", "CategoryID").IsUnique();
                b.ToTable("Enrolments");
            });

            modelBuilder.Entity("RaceDay.API.Models.Result", b =>
            {
                b.Property<int>("ResultID").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("CapturedByID").HasColumnType("int");
                b.Property<int>("EnrolmentID").HasColumnType("int");
                b.Property<TimeSpan?>("FinishTime").HasColumnType("time");
                b.Property<int?>("Position").HasColumnType("int");
                b.HasKey("ResultID");
                b.HasIndex("CapturedByID");
                b.HasIndex("EnrolmentID").IsUnique();
                b.ToTable("Results");
            });

            modelBuilder.Entity("RaceDay.API.Models.RouteWeatherInfo", b =>
            {
                b.Property<int>("RouteInfoID").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<int>("EventID").HasColumnType("int");
                b.Property<string>("RouteMapURL").HasMaxLength(255).HasColumnType("nvarchar(255)");
                b.Property<DateTime>("UpdatedAt").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.Property<string>("WeatherForecast").HasMaxLength(255).HasColumnType("nvarchar(255)");
                b.HasKey("RouteInfoID");
                b.HasIndex("EventID").IsUnique();
                b.ToTable("RouteWeatherInfos");
            });

            modelBuilder.Entity("RaceDay.API.Models.User", b =>
            {
                b.Property<int>("UserID").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<DateTime>("CreatedAt").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
                b.Property<string>("Email").IsRequired().HasMaxLength(150).HasColumnType("nvarchar(150)");
                b.Property<string>("FullName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<string>("ProfilePictureUrl").HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<string>("PasswordHash").IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
                b.Property<UserRole>("Role").HasConversion<string>().HasMaxLength(20).HasColumnType("nvarchar(20)");
                b.HasKey("UserID");
                b.HasIndex("Email").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("RaceDay.API.Models.Category", b =>
            {
                b.HasOne("RaceDay.API.Models.Event", "Event")
                    .WithMany("Categories")
                    .HasForeignKey("EventID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Event");
            });

            modelBuilder.Entity("RaceDay.API.Models.Event", b =>
            {
                b.HasOne("RaceDay.API.Models.User", "Organiser")
                    .WithMany("EventsOrganised")
                    .HasForeignKey("OrganiserID")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
                b.Navigation("Organiser");
                b.Navigation("Categories");
                b.Navigation("WeatherInfo");
            });

            modelBuilder.Entity("RaceDay.API.Models.Enrolment", b =>
            {
                b.HasOne("RaceDay.API.Models.Category", "Category")
                    .WithMany("Enrolments")
                    .HasForeignKey("CategoryID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("RaceDay.API.Models.User", "Participant")
                    .WithMany("Enrolments")
                    .HasForeignKey("ParticipantID")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
                b.Navigation("Category");
                b.Navigation("Participant");
                b.Navigation("Result");
            });

            modelBuilder.Entity("RaceDay.API.Models.Result", b =>
            {
                b.HasOne("RaceDay.API.Models.User", "CapturedBy")
                    .WithMany()
                    .HasForeignKey("CapturedByID")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
                b.HasOne("RaceDay.API.Models.Enrolment", "Enrolment")
                    .WithOne("Result")
                    .HasForeignKey("RaceDay.API.Models.Result", "EnrolmentID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("CapturedBy");
                b.Navigation("Enrolment");
            });

            modelBuilder.Entity("RaceDay.API.Models.RouteWeatherInfo", b =>
            {
                b.HasOne("RaceDay.API.Models.Event", "Event")
                    .WithOne("WeatherInfo")
                    .HasForeignKey("RaceDay.API.Models.RouteWeatherInfo", "EventID")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Event");
            });

            modelBuilder.Entity("RaceDay.API.Models.User", b =>
            {
                b.Navigation("Enrolments");
                b.Navigation("EventsOrganised");
            });
        }
    }
}
