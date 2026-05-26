using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class DonHang
{
    public int MaDonHang { get; set; }

    public int? MaTaiKhoan { get; set; }

    public string TenNguoiNhan { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string DiaChiGiaoHang { get; set; } = null!;

    public decimal TongTien { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public string? TrangThaiDonHang { get; set; }

    public string? LyDoHuy { get; set; }

    public DateTime? NgayDatHang { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }
}
