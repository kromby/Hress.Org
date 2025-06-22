namespace Ez.Hress.Hardhead.DataAccess;

using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;

using Ez.Hress.Hardhead.DataAccess.Models;

public partial class MovieModel : DbContext
{
    public MovieModel(string connectionstring)
        : base(connectionstring)
    {
        Events = Set<Event>();
        Images = Set<Image>();
        Texts = Set<Text>();
        Counts = Set<Count>();
        
        var type = typeof(SqlProviderServices);
        if (type == null)
            throw new SystemException("Do not remove, ensures static reference to System.Data.Entity.SqlServer");
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual IEnumerable<Event> Movies => Events.Where(m => m.TypeId == 49 && m.ParentId != null);

    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<Text> Texts { get; set; }
    public virtual DbSet<Count> Counts { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>()
            .HasMany(e => e.Images)
            .WithRequired(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Texts)
            .WithRequired(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Counts)
            .WithRequired(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .WillCascadeOnDelete(false);

        //modelBuilder.Entity<Text>()
        //    .Property(e => e.TextValue)
        //    .IsUnicode(false);

        //modelBuilder.Entity<Text>()
        //    .HasMany(e => e.Texts)
        //    .WithOptional(e => e.Parent)
        //    .HasForeignKey(e => e.ParentId);
    }
}
