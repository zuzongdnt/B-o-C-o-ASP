using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class SanPham
{
    public int MaSp { get; set; }

    public int? MaThuongHieu { get; set; }

    public string TenSp { get; set; } = null!;

    public decimal GiaBan { get; set; }

    public string? HinhAnh { get; set; }

    public string? Chipset { get; set; }

    public string? Ram { get; set; }

    public string? Rom { get; set; }

    public string? TinhTrang { get; set; }

    public string? NguonGoc { get; set; }

    public int? SoLuongTon { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? NgayThem { get; set; }
public string? Camera { get; set; }

    public decimal? GiaGoc { get; set; }
    public virtual ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; } = new List<HinhAnhSanPham>();
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();
// Them 2 dong nay vao Models/SanPham.cs

public string? Pin { get; set; }

public virtual ICollection<SanPhamBienThe> SanPhamBienThes { get; set; } = new List<SanPhamBienThe>();


    public virtual ThuongHieu? MaThuongHieuNavigation { get; set; }
}
