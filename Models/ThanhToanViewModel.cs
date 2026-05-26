using System.Collections.Generic;
using System.Linq;

namespace dienthoai.Models
{
    public class ThanhToanViewModel
    {
        public TaiKhoan? TaiKhoan { get; set; }
        public List<ChiTietGioHang> Items { get; set; } = new();
        public string PhuongThucThanhToan { get; set; } = "COD";
        public string? MaGiamGia { get; set; }
        public string? MaGiamGiaDaApDung { get; set; }
        public string? ThongBaoMaGiamGia { get; set; }
        public string? LoiMaGiamGia { get; set; }
        public decimal TamTinh { get; set; }
        public decimal GiamGia { get; set; }
        public List<CheckoutDiscountOption> GoiYMaGiamGia { get; set; } = new();

        public int TongSoLuong => Items.Sum(x => x.SoLuong);

        public decimal TongThanhToan
        {
            get
            {
                var total = TamTinh - GiamGia;
                return total > 0 ? total : 0;
            }
        }
    }

    public class CheckoutDiscountOption
    {
        public string Code { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
