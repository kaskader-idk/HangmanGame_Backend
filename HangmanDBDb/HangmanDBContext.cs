using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HangmanDBDb;

public partial class HangmanDBContext : DbContext
{
    public HangmanDBContext(DbContextOptions<HangmanDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HangmanWoerter> HangmanWoerters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HangmanWoerter>(entity =>
        {
            entity.ToTable("HANGMAN_WOERTER");

            entity.HasIndex(e => e.Wort, "UQ_HANGMAN_WOERTER_WORT").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Beschreibung)
                .HasMaxLength(255)
                .HasColumnName("BESCHREIBUNG");
            entity.Property(e => e.Schwierigkeit)
                .HasMaxLength(10)
                .HasColumnName("SCHWIERIGKEIT");
            entity.Property(e => e.Wort)
                .HasMaxLength(50)
                .HasColumnName("WORT");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
