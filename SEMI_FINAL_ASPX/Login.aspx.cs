using System;
using System.Collections.Generic;
using System.Configuration; // Thư viện đọc Web.config
using System.Data.SqlClient; // Thư viện làm việc với SQL Server
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SEMI_FINAL_ASPX.Common;
using SEMI_FINAL_ASPX.Data;
using SEMI_FINAL_ASPX.Security;

namespace SEMI_FINAL_ASPX
{
    public partial class Login : System.Web.UI.Page
    {
        // Lấy chuỗi kết nối từ Web.config
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Nếu đã đăng nhập hợp lệ qua JWT Cookie, tự động chuyển đến đúng trang
                if (SecurityHelper.IsAuthenticated(Request))
                {
                    string role = SecurityHelper.GetCurrentRole(Request);
                    Response.Redirect(role == "Admin" ? "Admin.aspx" : "User.aspx", true);
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Kiểm tra trong CSDL
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                // Lấy thông tin tài khoản theo tên đăng nhập (chống SQL Injection)
                string sql = "SELECT Id, MatKhau, HoTen, Quyen, TrangThai FROM NguoiDung WHERE TenDangNhap = @u";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);

                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["Id"]);
                                string storedHash = reader["MatKhau"].ToString();
                                string hoTen = reader["HoTen"].ToString();
                                string quyen = reader["Quyen"].ToString();
                                string trangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "";

                                // 1. Kiểm tra trạng thái tài khoản
                                if (trangThai == "Bị khóa")
                                {
                                    lblThongBao.Text = "Tài khoản của bạn đã bị khóa! Vui lòng liên hệ quản trị viên.";
                                    return;
                                }

                                // 2. Xác thực mật khẩu bằng Argon2id (hỗ trợ cả plain-text cũ nếu có)
                                if (Argon2PasswordHasher.VerifyPassword(password, storedHash))
                                {
                                    reader.Close(); // Đóng reader để lưu Refresh Token

                                    // 3. Sinh cặp JWT Token: Access Token (15m) + Refresh Token (7d)
                                    string accessToken = JwtTokenService.GenerateAccessToken(userId, username, hoTen, quyen);
                                    string refreshToken = JwtTokenService.GenerateRefreshToken();
                                    DateTime refreshExpiry = DateTime.UtcNow.AddDays(JwtTokenService.RefreshTokenExpirationDays);

                                    // 4. Lưu mã băm của Refresh Token vào CSDL (bảo mật thu hồi)
                                    RefreshTokenRepository.SaveRefreshToken(userId, refreshToken, refreshExpiry);

                                    // 5. Ghi Token vào HttpOnly Cookies (Chống XSS hoàn toàn)
                                    TokenCookieManager.SetAuthCookies(Response, Request, accessToken, refreshToken);

                                    // 6. Lưu Session song song để tương thích
                                    Session["Username"] = username;
                                    Session["FullName"] = hoTen;
                                    Session["Role"] = quyen;

                                    // 7. Điều hướng theo phân quyền RBAC
                                    if (string.Equals(quyen, "Admin", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Response.Redirect("Admin.aspx", true);
                                    }
                                    else
                                    {
                                        Response.Redirect("User.aspx", true);
                                    }
                                    return;
                                }
                            }

                            // Thông báo chung để chống tấn công dò tìm tài khoản (Username Enumeration)
                            lblThongBao.Text = "Tài khoản hoặc mật khẩu không chính xác!";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblThongBao.Text = "Lỗi kết nối CSDL: " + ex.Message;
                    }
                }
            }
        }
    }
}