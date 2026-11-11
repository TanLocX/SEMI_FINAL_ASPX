using System;
using System.Configuration;
using System.Data.SqlClient;
using SEMI_FINAL_ASPX.Security;

namespace SEMI_FINAL_ASPX.Data
{
    /// <summary>
    /// Thao tác CSDL cho Refresh Token (Lưu trữ, Xác thực xoay vòng và Thu hồi token).
    /// </summary>
    public static class RefreshTokenRepository
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;
        }

        /// <summary>
        /// Lưu Refresh Token mới vào CSDL.
        /// </summary>
        public static void SaveRefreshToken(int userId, string rawToken, DateTime expiryDate)
        {
            string tokenHash = JwtTokenService.HashRefreshToken(rawToken);

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                string sql = @"INSERT INTO RefreshToken (UserId, TokenHash, ExpiryDate, IsRevoked, CreatedDate)
                               VALUES (@userId, @tokenHash, @expiry, 0, GETDATE())";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@tokenHash", tokenHash);
                    cmd.Parameters.AddWithValue("@expiry", expiryDate);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Xác thực Refresh Token và thực hiện cơ chế Xoay vòng Token (Refresh Token Rotation).
        /// Cũ sẽ bị thu hồi và cấp lại một cặp token mới.
        /// </summary>
        public static bool TryRotateRefreshToken(
            string oldRawToken,
            out int userId,
            out string username,
            out string fullName,
            out string role,
            out string newRawRefreshToken)
        {
            userId = 0;
            username = string.Empty;
            fullName = string.Empty;
            role = string.Empty;
            newRawRefreshToken = string.Empty;

            if (string.IsNullOrEmpty(oldRawToken)) return false;

            string oldHash = JwtTokenService.HashRefreshToken(oldRawToken);

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // 1. Tìm token chưa bị thu hồi và còn hạn sử dụng
                string selectSql = @"SELECT r.Id, r.UserId, u.TenDangNhap, u.HoTen, u.Quyen, u.TrangThai
                                     FROM RefreshToken r
                                     INNER JOIN NguoiDung u ON r.UserId = u.Id
                                     WHERE r.TokenHash = @tokenHash 
                                       AND r.IsRevoked = 0 
                                       AND r.ExpiryDate > GETDATE()";

                int tokenId = 0;
                string trangThai = string.Empty;

                using (var cmd = new SqlCommand(selectSql, conn))
                {
                    cmd.Parameters.AddWithValue("@tokenHash", oldHash);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tokenId = Convert.ToInt32(reader["Id"]);
                            userId = Convert.ToInt32(reader["UserId"]);
                            username = reader["TenDangNhap"].ToString();
                            fullName = reader["HoTen"].ToString();
                            role = reader["Quyen"].ToString();
                            trangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "";
                        }
                        else
                        {
                            return false; // Token không hợp lệ hoặc đã hết hạn
                        }
                    }
                }

                // 2. Kiểm tra tài khoản có bị khóa không
                if (trangThai == "Bị khóa")
                {
                    return false;
                }

                // 3. Thu hồi token cũ (Refresh Token Rotation)
                string revokeOldSql = "UPDATE RefreshToken SET IsRevoked = 1 WHERE Id = @tokenId";
                using (var revokeCmd = new SqlCommand(revokeOldSql, conn))
                {
                    revokeCmd.Parameters.AddWithValue("@tokenId", tokenId);
                    revokeCmd.ExecuteNonQuery();
                }

                // 4. Sinh Refresh Token mới
                newRawRefreshToken = JwtTokenService.GenerateRefreshToken();
                string newHash = JwtTokenService.HashRefreshToken(newRawRefreshToken);
                DateTime newExpiry = DateTime.UtcNow.AddDays(JwtTokenService.RefreshTokenExpirationDays);

                string insertNewSql = @"INSERT INTO RefreshToken (UserId, TokenHash, ExpiryDate, IsRevoked, CreatedDate)
                                        VALUES (@userId, @tokenHash, @expiry, 0, GETDATE())";

                using (var insertCmd = new SqlCommand(insertNewSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@userId", userId);
                    insertCmd.Parameters.AddWithValue("@tokenHash", newHash);
                    insertCmd.Parameters.AddWithValue("@expiry", newExpiry);
                    insertCmd.ExecuteNonQuery();
                }

                return true;
            }
        }

        /// <summary>
        /// Thu hồi Refresh Token khi người dùng chủ động Đăng xuất.
        /// </summary>
        public static void RevokeRefreshToken(string rawToken)
        {
            if (string.IsNullOrEmpty(rawToken)) return;

            string tokenHash = JwtTokenService.HashRefreshToken(rawToken);

            using (var conn = new SqlConnection(GetConnectionString()))
            {
                string sql = "UPDATE RefreshToken SET IsRevoked = 1 WHERE TokenHash = @tokenHash";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tokenHash", tokenHash);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
