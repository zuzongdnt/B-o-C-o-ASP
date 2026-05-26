using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class GioHang
{
    public int MaGioHang { get; set; }

    public int? MaTaiKhoan { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }
}
