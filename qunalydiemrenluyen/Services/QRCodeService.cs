using System.Web;

namespace QUANLYDIEMRENLUYEN.Services
{
    public static class QRCodeService
    {
        public static string GenerateQRCodeUrl(string data, int size = 200)
        {
            // Mã hóa dữ liệu để đảm bảo dữ liệu an toàn cho URL
            string encoded = HttpUtility.UrlEncode(data);
            return $"https://api.qrserver.com/v1/create-qr-code/?size={size}x{size}&data={encoded}";
        }
    }
}