using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Core.DB.Database;

public partial class PlaygroundContext : DbContext
{
    public PlaygroundContext()
    {
    }

    public PlaygroundContext(DbContextOptions<PlaygroundContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AUTH_Accounts> AUTH_Accounts { get; set; }

    public virtual DbSet<AUTH_Sessions> AUTH_Sessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Playground;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AUTH_Accounts>(entity =>
        {
            entity.HasKey(e => e.AUTH_AccountID);

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AUTH_Sessions>(entity =>
        {
            entity.HasKey(e => e.AUTH_SessionID);

            entity.HasIndex(e => e.Account_RefID, "IX_Account_RefID");

            entity.Property(e => e.SessionToken)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Account_Ref).WithMany(p => p.AUTH_Sessions)
                .HasForeignKey(d => d.Account_RefID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AUTH_Sessions_AUTH_Accounts");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
