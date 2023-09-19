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