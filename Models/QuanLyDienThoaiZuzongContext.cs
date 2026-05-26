using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace dienthoai.Models;

public partial class QuanLyDienThoaiZuzongContext : DbContext
{
    public QuanLyDienThoaiZuzongContext()
    {
    }

    public QuanLyDienThoaiZuzongContext(DbContextOptions<QuanLyDienThoaiZuzongContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }
public virtual DbSet<DanhGiaPhanHoi> DanhGiaPhanHois { get; set; }

    public virtual DbSet<ThuongHieu> ThuongHieus { get; set; }

    public virtual DbSet<TuCamDanhGium> TuCamDanhGia { get; set; }
public virtual DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; }
public virtual DbSet<SanPhamBienThe> SanPhamBienThes { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=zuzong\\SQLEXPRESS;Database=QuanLyDienThoai_Zuzong;User Id=sa;Password=123;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietDonHang).HasName("PK_ChiTietDonHang");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaChiTietDonHang).ValueGeneratedOnAdd();
            entity.Property(e => e.MaBienThe).HasColumnName("MaBienThe");
            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__778AC167");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDon__MaSP__787EE5A0");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaBienThe)
                .HasConstraintName("FK_ChiTietDonHang_SanPhamBienThe");
        });
modelBuilder.Entity<DanhGiaPhanHoi>(entity =>
{
    entity.HasKey(e => e.MaPhanHoi);

    entity.ToTable("DanhGiaPhanHoi");

    entity.Property(e => e.NoiDung).HasMaxLength(1000);
    entity.Property(e => e.NgayPhanHoi)
        .HasDefaultValueSql("(getdate())")
        .HasColumnType("datetime");
    entity.Property(e => e.TrangThai).HasDefaultValue(true);

    entity.HasOne(d => d.MaDanhGiaNavigation)
        .WithMany(p => p.DanhGiaPhanHois)
        .HasForeignKey(d => d.MaDanhGia)
        .OnDelete(DeleteBehavior.ClientSetNull)
        .HasConstraintName("FK_DanhGiaPhanHoi_DanhGia");

    entity.HasOne(d => d.MaTaiKhoanNavigation)
        .WithMany(p => p.DanhGiaPhanHois)
        .HasForeignKey(d => d.MaTaiKhoan)
        .OnDelete(DeleteBehavior.ClientSetNull)
        .HasConstraintName("FK_DanhGiaPhanHoi_TaiKhoan");
});
        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietGioHang).HasName("PK_ChiTietGioHang");

            entity.ToTable("ChiTietGioHang");

            entity.HasIndex(e => new { e.MaGioHang, e.MaSp, e.MaBienThe }, "UX_ChiTietGioHang_GioHang_SanPham_BienThe")
                .IsUnique()
                .HasFilter("[MaBienThe] IS NOT NULL");

            entity.Property(e => e.MaChiTietGioHang).ValueGeneratedOnAdd();
            entity.Property(e => e.MaBienThe).HasColumnName("MaBienThe");
            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaGioHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGi__MaGio__6E01572D");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietGio__MaSP__6EF57B66");

            entity.HasOne(d => d.MaBienTheNavigation).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.MaBienThe)
                .HasConstraintName("FK_ChiTietGioHang_SanPhamBienThe");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BFAC0A8F8B");

            entity.Property(e => e.HinhAnhDg)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("HinhAnhDG");
            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThaiHienThi).HasDefaultValue(true);

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK__DanhGia__MaSP__7B5B524B");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK__DanhGia__MaTaiKh__7C4F7684");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD591FEB27");

            entity.ToTable("DonHang");

            entity.Property(e => e.DiaChiGiaoHang).HasMaxLength(500);
            entity.Property(e => e.NgayDatHang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TenNguoiNhan).HasMaxLength(100);
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThaiDonHang)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ xác nhận");
            entity.Property(e => e.LyDoHuy).HasMaxLength(500);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK__DonHang__MaTaiKh__72C60C4A");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.MaGioHang).HasName("PK__GioHang__F5001DA3FDB9169B");

            entity.ToTable("GioHang");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK__GioHang__MaTaiKh__6A30C649");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSp).HasName("PK__SanPham__2725081C8AD49326");

            entity.ToTable("SanPham");
entity.Property(e => e.Pin).HasMaxLength(100);
entity.Property(e => e.Camera).HasMaxLength(255);

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.Chipset).HasMaxLength(100);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaGoc).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.HinhAnh)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayThem)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguonGoc).HasMaxLength(50);
            entity.Property(e => e.Ram)
                .HasMaxLength(50)
                .HasColumnName("RAM");
            entity.Property(e => e.Rom)
                .HasMaxLength(50)
                .HasColumnName("ROM");
            entity.Property(e => e.SoLuongTon).HasDefaultValue(0);
            entity.Property(e => e.TenSp)
                .HasMaxLength(255)
                .HasColumnName("TenSP");
            entity.Property(e => e.TinhTrang).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaThuongHieuNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.MaThuongHieu)
                .HasConstraintName("FK__SanPham__MaThuon__6477ECF3");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529473A4591");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0FD5255D4").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.DiaChi).HasMaxLength(500);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.VaiTro).HasMaxLength(30);
        });

        modelBuilder.Entity<ThuongHieu>(entity =>
        {
            entity.HasKey(e => e.MaThuongHieu).HasName("PK__ThuongHi__A3733E2C9A9C821E");

            entity.ToTable("ThuongHieu");

            entity.HasIndex(e => e.TenThuongHieu, "UQ__ThuongHi__98D6A83459B998F2").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenThuongHieu).HasMaxLength(100);
        });
modelBuilder.Entity<SanPhamBienThe>(entity =>
{
    entity.HasKey(e => e.MaBienThe);

    entity.ToTable("SanPhamBienThe");

    entity.Property(e => e.MaBienThe).HasColumnName("MaBienThe");
    entity.Property(e => e.MaSp).HasColumnName("MaSP");
    entity.Property(e => e.DungLuong).HasMaxLength(30);
    entity.Property(e => e.MauSac).HasMaxLength(80);
    entity.Property(e => e.MaMau).HasMaxLength(20);
    entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");
    entity.Property(e => e.GiaGoc).HasColumnType("decimal(18, 0)");
    entity.Property(e => e.HinhAnh).HasMaxLength(255);

    entity.HasOne(d => d.MaSpNavigation)
        .WithMany(p => p.SanPhamBienThes)
        .HasForeignKey(d => d.MaSp)
        .OnDelete(DeleteBehavior.ClientSetNull)
        .HasConstraintName("FK_SanPhamBienThe_SanPham");
});

        modelBuilder.Entity<TuCamDanhGium>(entity =>
        {
            entity.HasKey(e => e.MaTu).HasName("PK__TuCam_Da__2725005AD91E46FD");

            entity.ToTable("TuCam_DanhGia");

            entity.HasIndex(e => e.TuKhoa, "UQ__TuCam_Da__2E7DF67585A6073D").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.TuKhoa).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
