using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class DanhGium
{
    // Thêm dòng này vào dưới các thuộc tính hiện có
public string? PhanHoi { get; set; }
    public int MaDanhGia { get; set; }

    public int? MaSp { get; set; }

    public int? MaTaiKhoan { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public string? HinhAnhDg { get; set; }
// Them dong nay vao Models/DanhGium.cs
public virtual ICollection<DanhGiaPhanHoi> DanhGiaPhanHois { get; set; } = new List<DanhGiaPhanHoi>();


    public bool? TrangThaiHienThi { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }

    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }
}
