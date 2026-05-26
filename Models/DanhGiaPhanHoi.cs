using System;

namespace dienthoai.Models;

public partial class DanhGiaPhanHoi
{
    public int MaPhanHoi { get; set; }

    public int MaDanhGia { get; set; }

    public int MaTaiKhoan { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime NgayPhanHoi { get; set; }

    public bool TrangThai { get; set; }

    public virtual DanhGium MaDanhGiaNavigation { get; set; } = null!;

    public virtual TaiKhoan MaTaiKhoanNavigation { get; set; } = null!;
}

