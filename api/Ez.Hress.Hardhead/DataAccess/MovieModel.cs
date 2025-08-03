namespace Ez.Hress.Hardhead.DataAccess;

using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;

using Ez.Hress.Hardhead.DataAccess.Models;

public partial class MovieModel : DbContext
{
    private readonly string _connectionString;

    public MovieModel(string connectionstring)
    {
        _connectionString = connectionstring;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString, options => 
            options.EnableRetryOnFailure()
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        );
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual IEnumerable<Event> Movies => Events.Where(m => m.TypeId == 49 && m.ParentId != null);

    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<Text> Texts { get; set; }
    public virtual DbSet<Count> Counts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .HasMany(e => e.Images)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Texts)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Counts)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Event entity to work with database triggers
        modelBuilder.Entity<Event>()
            .ToTable("rep_Event")
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Text>()
            .ToTable("rep_Text")
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        //modelBuilder.Entity<Text>()
        //    .Property(e => e.TextValue)
        //    .IsUnicode(false);

        //modelBuilder.Entity<Text>()
        //    .HasMany(e => e.Texts)
        //    .WithOptional(e => e.Parent)
        //    .HasForeignKey(e => e.ParentId);
    }
}
