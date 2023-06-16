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

    public virtual DbSet<CMN_PER_PersonalInfo> CMN_PER_PersonalInfo { get; set; }

    public virtual DbSet<USR_Users> USR_Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Playground;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CMN_PER_PersonalInfo>(entity =>
        {
            entity.HasKey(e => e.CMN_PER_PersonalInfo1);

            entity.Property(e => e.CMN_PER_PersonalInfo1).HasColumnName("CMN_PER_PersonalInfo");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.USR_User_Ref).WithMany(p => p.CMN_PER_PersonalInfo)
                .HasForeignKey(d => d.USR_User_RefID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CMN_PER_PersonalInfo_USR_Users");
        });

        modelBuilder.Entity<USR_Users>(entity =>
        {
            entity.HasKey(e => e.USR_UserID);

            entity.Property(e => e.CreationDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
