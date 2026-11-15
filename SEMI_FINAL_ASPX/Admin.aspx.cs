using System;
using System.Collections.Generic;
using System.Configuration; // Thư viện đọc Web.config
using System.Data;
using System.Data.SqlClient; // Thư viện kết nối CSDL SQL Server
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SEMI_FINAL_ASPX
{
    public partial class Admin : System.Web.UI.Page
    {
        // Lấy chuỗi kết nối từ file Web.config
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // =========================================================================================
            // 🛡️ PHÒNG THỦ LUỒNG 2: VÔ HIỆU HÓA CACHE TRÌNH DUYỆT (CHỐNG NÚT "BACK" SAU LOGOUT)
            // =========================================================================================
            // Khi Admin bấm Đăng xuất, nếu không tắt cache, kẻ sau bấm nút Back (<-) sẽ xem lại được dữ liệu cũ.
            Response.Cache.SetCacheability(HttpCacheability.NoCache); // Yêu cầu trình duyệt không lưu cache
            Response.Cache.SetNoStore();                             // Cấm lưu trữ tài liệu vào bộ nhớ đệm
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1)); // Đặt thời gian hết hạn trong quá khứ

            // =========================================================================================
            // 🛡️ PHÒNG THỦ LUỒNG 1: CHỐNG TRUY CẬP TRỰC TIẾP BẰNG URL (FORCE BROWSING)
            // =========================================================================================
            
            // BƯỚC 1: KIỂM TRA AUTHENTICATION (XÁC THỰC DANH TÍNH)
            // Nếu Session["Username"] bằng null -> Người dùng chưa đăng nhập -> Chuyển về Login.aspx
            if (Session["Username"] == null)
            {
                Response.Redirect("Login.aspx");
                return; // Dừng thực thi toàn bộ code bên dưới ngay lập tức
            }

            // BƯỚC 2: KIỂM TRA AUTHORIZATION (PHÂN QUYỀN TRUY CẬP)
            // Đã đăng nhập nhưng Role KHÔNG PHẢI "Admin" -> Chặn ngay và chuyển sang trang AccessDenied.aspx (403)
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("AccessDenied.aspx");
                return; // Dừng thực thi, tuyệt đối không tải dữ liệu quản trị
            }

            // Chỉ nạp dữ liệu lần đầu tiên tải trang (tránh nạp lại khi nhấn các nút thao tác PostBack)
            if (!IsPostBack)
            {
                // Hiển thị tên Admin lên thanh điều hướng Navbar
                lblAdminName.Text = Session["FullName"] != null ? Session["FullName"].ToString() : Session["Username"].ToString();

                // Nạp danh sách người dùng từ CSDL vào bảng giao diện
                LoadDanhSachNguoiDung();
            }
        }

        // =========================================================================================
        // 🛡️ PHÒNG THỦ LUỒNG 4: KIỂM TRA QUYỀN 2 LỚP TRÊN SERVER (CHỐNG CAN THIỆP F12 / TÂM LÝ CLIENT)
        // =========================================================================================
        // Cho dù kẻ tấn công có dùng F12 sửa DOM để gửi yêu cầu giả mạo, hàm này vẫn kiểm tra lại Role trên Server
        private bool KiemTraQuyenAdminServerSide()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                // Nếu phát hiện giả mạo quyền -> hủy bỏ và đá ra trang từ chối truy cập
                Response.Redirect("AccessDenied.aspx");
                return false;
            }
            return true;
        }

        // Hàm nạp danh sách người dùng hiển thị lên bảng
        private void LoadDanhSachNguoiDung()
        {
            // Kiểm tra an toàn trước khi truy xuất dữ liệu
            if (!KiemTraQuyenAdminServerSide()) return;

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                string sql = "SELECT Id, TenDangNhap, HoTen, Quyen, TrangThai FROM NguoiDung";
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt); // Đổ dữ liệu vào bảng tạm DataTable

                    rptNguoiDung.DataSource = dt; // Gán dữ liệu cho Repeater
                    rptNguoiDung.DataBind();       // Render dữ liệu ra giao diện HTML
                }
            }
        }

        // =========================================================================================
        // 🛡️ PHÒNG THỦ LUỒNG 5: HỦY SẠCH PHIÊN KHI ĐĂNG XUẤT (CHỐNG ĐÁNH CẮP LẠI PHIÊN CŨ)
        // =========================================================================================
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();    // Xóa sạch toàn bộ biến lưu trữ trong Session
            Session.Abandon();  // Hủy bỏ phiên làm việc hiện tại trên Web Server (Server RAM)

            // Xóa cookie định danh phiên ASP.NET_SessionId phía trình duyệt
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            // Chuyển người dùng quay trở về trang Đăng nhập
            Response.Redirect("Login.aspx");
        }
    }
}
