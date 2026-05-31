using CADSFINANCE.Models;
using Microsoft.EntityFrameworkCore;

namespace CADSFINANCE.Infrastructure.Persistence;

public sealed class CadsFinanceDbContext : DbContext
{
    public CadsFinanceDbContext(DbContextOptions<CadsFinanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<DonViTinh> DonViTinhs => Set<DonViTinh>();

    public DbSet<StmPshh> StmPshhs => Set<StmPshh>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DonViTinh>(entity =>
        {
            entity.ToTable("LST_DonViTinh", "dbo");
            entity.HasKey(item => item.MaDvt);

            entity.Property(item => item.MaDvt)
                .HasColumnName("MA_DVT")
                .HasMaxLength(10);

            entity.Property(item => item.TenDvt)
                .HasColumnName("TEN_DVT")
                .HasMaxLength(50);

            entity.Property(item => item.UserId)
                .HasColumnName("USER_ID");

            entity.Property(item => item.QuyCach)
                .HasColumnName("QUY_CACH");

            entity.Property(item => item.IsActive)
                .HasColumnName("isActive");
        });

        modelBuilder.Entity<StmPshh>(entity =>
        {
            entity.ToTable("STM_PSHH", "dbo");
            entity.HasKey(item => item.IdPs);

            entity.Property(item => item.IdPs).HasColumnName("ID_PS");
            entity.Property(item => item.SoHd).HasColumnName("SO_HD");
            entity.Property(item => item.SoPhieu).HasColumnName("SO_PHIEU");
            entity.Property(item => item.MaDt).HasColumnName("MA_DT");
            entity.Property(item => item.NgayCt).HasColumnName("NGAY_CT");
            entity.Property(item => item.MaLoaiCt).HasColumnName("MA_LOAI_CT");
            entity.Property(item => item.DienGiai).HasColumnName("DIEN_GIAI");
            entity.Property(item => item.Tien).HasColumnName("TIEN");
            entity.Property(item => item.TienNt).HasColumnName("TIEN_NT");
            entity.Property(item => item.TienVat).HasColumnName("TIEN_VAT");
        });
    }
}
