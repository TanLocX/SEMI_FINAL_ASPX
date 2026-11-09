using System;
using System.Collections.Generic;
using System.Configuration; // Thư viện đọc Web.config
using System.Data.SqlClient; // Thư viện làm việc với SQL Server
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SEMI_FINAL_ASPX
{
    public partial class Login : System.Web.UI.Page
    {
        // Lấy chuỗi kết nối từ Web.config
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Nếu người dùng đã đăng nhập từ trước rồi thì tự động đẩy về trang tương ứng
            if (!IsPostBack)
            {
                if (Session["Role"] != null)
                {
                    if (Session["Role"].ToString() == "Admin")
                        Response.Redirect("Admin.aspx");
                    else
                        Response.Redirect("User.aspx");
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
                string sql = "SELECT HoTen, Quyen FROM NguoiDung WHERE TenDangNhap = @u AND MatKhau = @p";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Truyền tham số để tránh SQL Injection
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);

                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            // 1. ĐĂNG NHẬP THÀNH CÔNG -> ĐỌC THÔNG TIN
                            string hoTen = reader["HoTen"].ToString();
                            string quyen = reader["Quyen"].ToString();

                            // 2. LƯU VÀO SESSION (Authentication & Authorization)
                            Session["Username"] = username;
                            Session["FullName"] = hoTen;
                            Session["Role"] = quyen;

                            // 3. ĐIỀU HƯỚNG THEO ROLE (PHÂN QUYỀN)
                            if (quyen == "Admin")
                            {
                                Response.Redirect("Admin.aspx");
                            }
                            else
                            {
                                Response.Redirect("User.aspx");
                            }
                        }
                        else
                        {
                            // Đăng nhập thất bại
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