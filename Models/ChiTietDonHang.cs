using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class ChiTietDonHang
{
    public int MaChiTietDonHang { get; set; }

    public int MaDonHang { get; set; }

    public int MaSp { get; set; }

    public int? MaBienThe { get; set; }

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;

    public virtual SanPhamBienThe? MaBienTheNavigation { get; set; }
}
