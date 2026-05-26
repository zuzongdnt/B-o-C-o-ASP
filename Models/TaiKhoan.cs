using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class TaiKhoan
{
    public int MaTaiKhoan { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public string? VaiTro { get; set; }

    public bool? TrangThai { get; set; }
// Thêm dòng này vào cùng danh sách với HoTen, Email...
public string? Avatar { get; set; }
    public DateTime? NgayTao { get; set; }
// Them dong nay vao Models/TaiKhoan.cs
public virtual ICollection<DanhGiaPhanHoi> DanhGiaPhanHois { get; set; } = new List<DanhGiaPhanHoi>();


    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
}
