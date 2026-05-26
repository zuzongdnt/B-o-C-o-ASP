using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class SanPhamBienThe
{
    public int MaBienThe { get; set; }

    public int MaSp { get; set; }

    public string DungLuong { get; set; } = null!;

    public string MauSac { get; set; } = null!;

    public string? MaMau { get; set; }

    public decimal GiaBan { get; set; }

    public decimal? GiaGoc { get; set; }

    public string? HinhAnh { get; set; }

    public int? SoLuongTon { get; set; }

    public bool LaMacDinh { get; set; }

    public bool TrangThai { get; set; }

    public virtual SanPham MaSpNavigation { get; set; } = null!;

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
}
