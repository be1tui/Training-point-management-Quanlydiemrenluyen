using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;
using System.Text;
using Slugify;

namespace QUANLYDIEMRENLUYEN.Utilities
{
    public static class Functions
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        // Gọi hàm này 1 lần trong Startup hoặc Program.cs để gán context
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private static ISession? Session => _httpContextAccessor?.HttpContext?.Session;

        // Các thuộc tính lấy từ Session
        public static int AccountId => Session?.GetInt32("AccountId") ?? 0;
        public static string Username => Session?.GetString("Username") ?? string.Empty;
        public static string Email => Session?.GetString("Email") ?? string.Empty;
        public static string FullName => Session?.GetString("FullName") ?? string.Empty;
        public static string Password => Session?.GetString("Password") ?? string.Empty;
        public static string Avatar => Session?.GetString("Avatar") ?? string.Empty;
        public static string Role => Session?.GetString("Role") ?? string.Empty;
        public static bool IsActive => bool.TryParse(Session?.GetString("IsActive"), out var b) ? b : true;
        public static DateTime CreatedAt => DateTime.TryParse(Session?.GetString("CreatedAt"), out var dt) ? dt : DateTime.Now;

        // Thêm biến Message để sử dụng trong LoginController
        public static string Message
        {
            get => Session?.GetString("Message") ?? string.Empty;
            set => Session?.SetString("Message", value);
        }

        // Tạo Slug cho tiêu đề bài viết
        public static string TitleSlugGeneration(string type, string? title, long id)
        {
            var slugHelper = new SlugHelper();
            return type + "-" + slugHelper.GenerateSlug(title) + "-" + id.ToString() + ".html";
        }

        // Mã hóa MD5 đơn giản
        public static string MD5Hash(string text)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(text);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // Mã hóa mật khẩu với nhiều vòng lặp
        public static string MD5Password(string text)
        {
            string str = MD5Hash(text);
            for (int i = 0; i < 5; i++)
            {
                str = MD5Hash(str + str);
            }
            return str;
        }

        public static string AvatarDefault()
        {
            return "/Avatar/default-avatar.png";
        }
    }
}
