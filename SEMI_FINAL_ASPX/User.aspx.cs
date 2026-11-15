using System;
using System.Collections.Generic;
using System.Configuration; // Thư viện đọc file Web.config
using System.Data;
using System.Data.SqlClient; // Thư viện kết nối CSDL SQL Server
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SEMI_FINAL_ASPX
{
    public partial class User : System.Web.UI.Page
    {
        // Lấy chuỗi kết nối từ file Web.config
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // =========================================================================================
            // 🛡️ PHÒNG THỦ LUỒNG 2: VÔ HIỆU HÓA CACHE TRÌNH DUYỆT (CHỐNG NÚT "BACK" SAU LOGOUT)
            // =========================================================================================
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
            Response.Expires = -1000;
            Response.CacheControl = "no-cache";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate, max-age=0");
            Response.AddHeader("Pragma", "no-cache");

            // =========================================================================================
            // 🛡️ PHÒNG THỦ LUỒNG 1: CHỐNG TRUY CẬP TRỰC TIẾP BẰNG URL (FORCE BROWSING)
            // =========================================================================================
            // KIỂM TRA AUTHENTICATION: Chưa đăng nhập (Session Username bị null) -> Đuổi về trang Login
            if (Session["Username"] == null)
            {
                Response.Redirect("Login.aspx");
                return; // Dừng thực thi toàn bộ code bên dưới
            }

            // =========================================================================================
            // 🛡️ PHÒNG THỦ LUỒNG 4: NGUYÊN TẮC QUYỀN TỐI THIỂU PHÍA CLIENT (LEAST PRIVILEGE)
            // =========================================================================================
            // Trang User.aspx chỉ hiển thị dữ liệu dạng tra cứu (Read-only).
            // Mã nguồn HTML không hề sinh ra các nút Sửa/Xóa của Admin -> Kẻ xấu dùng F12 cũng không có gì để can thiệp.

            if (!IsPostBack)
            {
                // Hiển thị tên người dùng lên thanh Navbar
                lblUserName.Text = Session["FullName"] != null ? Session["FullName"].ToString() : Session["Username"].ToString();

                // Đổ dữ liệu từ CSDL vào Repeater để người dùng tra cứu
                LoadDanhSachNguoiDung();
            }
        }

        // Hàm nạp danh sách dữ liệu tra cứu cho User
        private void LoadDanhSachNguoiDung()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                string sql = "SELECT Id, TenDangNhap, HoTen, Quyen, TrangThai FROM NguoiDung";
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt); // Lấy dữ liệu từ CSDL

                    rptNguoiDung.DataSource = dt; // Gán nguồn dữ liệu cho Repeater
                    rptNguoiDung.DataBind();       // Đổ dữ liệu ra bảng giao diện
                }
            }
        }

        // =========================================================================================
        // 🛡️ PHÒNG THỦ LUỒNG 5: HỦY SẠCH PHIÊN KHI ĐĂNG XUẤT (LOGOUT AN TOÀN)
        // =========================================================================================
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();    // Xóa sạch toàn bộ biến lưu trong bộ nhớ Session
            Session.Abandon();  // Hủy bỏ hoàn toàn phiên làm việc hiện tại trên Web Server

            // Xóa cookie định danh phiên ASP.NET_SessionId trên máy client
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            // Chuyển người dùng quay trở về trang Login
            Response.Redirect("Login.aspx");
        }
    }
}
