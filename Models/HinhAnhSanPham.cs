using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema; 

namespace dienthoai.Models;

[Table("HinhAnhSanPham")] 
public partial class HinhAnhSanPham
{
    [Key] 
    public int MaHinhAnh { get; set; }

    public int MaSp { get; set; }

    public string DuongDanAnh { get; set; } = null!;

    // 👇 THÊM ĐÚNG DÒNG NÀY ĐỂ ÉP NÓ DÙNG CỘT MaSp LÀM KHÓA NGOẠI
    [ForeignKey("MaSp")] 
    public virtual SanPham? MaSpNavigation { get; set; }
}