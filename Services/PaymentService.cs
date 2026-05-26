using System.Globalization;
using System.Linq;

namespace dienthoai.Services
{
    public sealed record BankTransferInfo(string BankName, string BankCode, string AccountNumber, string AccountName);

    public interface IPaymentService
    {
        BankTransferInfo TransferInfo { get; }
        string GenerateSePayQrUrl(decimal soTien, int maDonHang);
        string GenerateSePayQrUrl(string soTien, string maDonHang);
    }

    public class PaymentService : IPaymentService
    {
        private const string BankCode = "MBBank";
        private const string AccountNumber = "0984875162";
        private const string AccountName = "VO MINH THUAN";
        private const string TransferPrefix = "THANHTOAN";

        public BankTransferInfo TransferInfo { get; } =
            new("MBBank", BankCode, AccountNumber, AccountName);

        public string GenerateSePayQrUrl(decimal soTien, int maDonHang)
        {
            return GenerateSePayQrUrl(
                soTien.ToString("0", CultureInfo.InvariantCulture),
                maDonHang.ToString(CultureInfo.InvariantCulture));
        }

        public string GenerateSePayQrUrl(string soTien, string maDonHang)
        {
            var amount = NormalizeAmount(soTien);
            var orderCode = NormalizeOrderCode(maDonHang);
            var noiDung = $"{TransferPrefix}{orderCode}";

            return "https://qr.sepay.vn/img"
                + $"?acc={TransferInfo.AccountNumber}"
                + $"&bank={TransferInfo.BankCode}"
                + $"&amount={Uri.EscapeDataString(amount)}"
                + $"&des={Uri.EscapeDataString(noiDung)}";
        }

        private static string NormalizeAmount(string? input)
        {
            var digits = new string((input ?? "").Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? "0" : digits;
        }

        private static string NormalizeOrderCode(string? input)
        {
            var cleaned = new string((input ?? "").Where(char.IsLetterOrDigit).ToArray());
            return string.IsNullOrEmpty(cleaned) ? "DONHANG" : cleaned.ToUpperInvariant();
        }
    }
}
