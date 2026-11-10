using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SEMI_FINAL_ASPX
{
    /// <summary>
    /// Dịch vụ quản lý JWT: Sinh Access Token, Refresh Token và xác thực Token.
    /// </summary>
    public static class JwtTokenService
    {
        // Khóa bí mật mặc định (tối thiểu 256-bit / 32 ký tự)
        private const string DefaultSecretKey = "SEMI_FINAL_ASPX_Super_Secret_Key_For_JWT_Authentication_2026_Minimum_256_Bits!";
        public const int AccessTokenExpirationMinutes = 15;
        public const int RefreshTokenExpirationDays = 7;

        private static string GetSecretKey()
        {
            string key = ConfigurationManager.AppSettings["Jwt:SecretKey"];
            return string.IsNullOrEmpty(key) ? DefaultSecretKey : key;
        }

        private static string GetIssuer()
        {
            string issuer = ConfigurationManager.AppSettings["Jwt:Issuer"];
            return string.IsNullOrEmpty(issuer) ? "SEMI_FINAL_ASPX" : issuer;
        }

        private static string GetAudience()
        {
            string audience = ConfigurationManager.AppSettings["Jwt:Audience"];
            return string.IsNullOrEmpty(audience) ? "SEMI_FINAL_ASPX_Clients" : audience;
        }

        /// <summary>
        /// Tạo Access Token (JWT) có thời hạn sống 15 phút, chứa các Claim người dùng và vai trò.
        /// </summary>
        public static string GenerateAccessToken(int userId, string username, string fullName, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] keyBytes = Encoding.UTF8.GetBytes(GetSecretKey());

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", userId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, fullName),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
                Issuer = GetIssuer(),
                Audience = GetAudience(),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Sinh chuỗi ngẫu nhiên bảo mật cao làm Refresh Token.
        /// </summary>
        public static string GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        /// <summary>
        /// Băm Refresh Token bằng SHA-256 để lưu trữ an toàn trong CSDL.
        /// </summary>
        public static string HashRefreshToken(string rawToken)
        {
            if (string.IsNullOrEmpty(rawToken)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của Access Token.
        /// </summary>
        public static bool ValidateAccessToken(string token, out ClaimsPrincipal principal, bool validateLifetime = true)
        {
            principal = null;
            if (string.IsNullOrEmpty(token)) return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] keyBytes = Encoding.UTF8.GetBytes(GetSecretKey());

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = validateLifetime,
                    ClockSkew = TimeSpan.FromMinutes(1) // Cho phép lệch đồng hồ tối đa 1 phút
                };

                principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
