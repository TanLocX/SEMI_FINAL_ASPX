using System;
using System.Collections.Generic;
using System.Configuration; // Thư viện đọc chuỗi kết nối trong Web.config
using System.Data.SqlClient; // Thư viện kết nối và thực thi truy vấn SQL Server
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SEMI_FINAL_ASPX
{
    public partial class Login : System.Web.UI.Page
    {
        // Lấy chuỗi kết nối từ file cấu hình Web.config
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Nếu người dùng đã đăng nhập từ trước (Session còn sống) -> tự động điều hướng đúng trang
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
            // Lấy thông tin tài khoản và mật khẩu từ ô nhập liệu giao diện
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Mở kết nối đến Cơ sở dữ liệu SQL Server
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                // =========================================================================================
                // 🛡️ PHÒNG THỦ LUỒNG 3: CHỐNG TẤN CÔNG SQL INJECTION BẰNG PARAMETERIZED QUERY
                // =========================================================================================
                // ❌ SAI LẦM NGUY HIỂM: Nối chuỗi trực tiếp "WHERE TenDangNhap = '" + username + "'"
                //    Kẻ xấu nhập: admin' -- hoặc ' OR 1=1 -- sẽ đăng nhập được mà không cần mật khẩu!
                //
                // ✔️ GIẢI PHÁP AN TOÀN: Dùng dấu giữ chỗ tham số @u và @p trong câu lệnh SQL.
                // =========================================================================================
                string sql = "SELECT HoTen, Quyen FROM NguoiDung WHERE TenDangNhap = @u AND MatKhau = @p";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Gán giá trị vào tham số: SQL Server sẽ xử lý nội dung người dùng nhập 100% như DỮ LIỆU THUẦN
                    // Mọi ký tự đặc biệt như dấu nháy đơn ('), dấu gạch ngang (--) đều bị vô hiệu hóa
                    cmd.Parameters.AddWithValue("@u", username); // Tham số tên đăng nhập
                    cmd.Parameters.AddWithValue("@p", password); // Tham số mật khẩu

                    try
                    {
                        conn.Open(); // Mở kết nối CSDL
                        SqlDataReader reader = cmd.ExecuteReader(); // Thực thi truy vấn đọc dữ liệu

                        if (reader.Read())
                        {
                            // 1. ĐỌC DỮ LIỆU TỪ CSDL KHI ĐĂNG NHẬP THÀNH CÔNG
                            string hoTen = reader["HoTen"].ToString(); // Lấy họ tên người dùng
                            string quyen = reader["Quyen"].ToString(); // Lấy quyền: "Admin" hoặc "User"

                            // =========================================================================================
                            // 🛡️ PHÒNG THỦ LUỒNG 5: CHỐNG TẤN CÔNG CỐ ĐỊNH PHIÊN (ANTI SESSION FIXATION)
                            // =========================================================================================
                            // Xóa sạch toàn bộ dữ liệu phiên cũ trước khi gán phiên đăng nhập mới
                            Session.Clear();

                            // Lưu thông tin nhận dạng vào Session (Lưu trữ an toàn trên bộ nhớ RAM của Server)
                            Session["Username"] = username;  // Dùng để xác thực (Authentication)
                            Session["FullName"] = hoTen;     // Dùng để hiển thị chào mừng
                            Session["Role"] = quyen;         // Dùng để phân quyền (Authorization)

                            // 2. ĐIỀU HƯỚNG THEO ROLE (PHÂN QUYỀN HỆ THỐNG)
                            if (quyen == "Admin")
                            {
                                Response.Redirect("Admin.aspx"); // Quyền Admin chuyển về trang Quản trị
                            }
                            else
                            {
                                Response.Redirect("User.aspx");  // Quyền User chuyển về trang Người dùng
                            }
                        }
                        else
                        {
                            // Đăng nhập thất bại: Tài khoản hoặc mật khẩu không khớp trong CSDL
                            lblThongBao.Text = "Tài khoản hoặc mật khẩu không chính xác!";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Bắt lỗi nếu có sự cố kết nối tới SQL Server
                        lblThongBao.Text = "Lỗi kết nối CSDL: " + ex.Message;
                    }
                }
            }
        }
    }
}
