using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SEMI_FINAL_ASPX
{
    public partial class Admin : BasePage
    {
        // Yêu cầu bắt buộc Role "Admin" (BasePage tự động kiểm tra trong OnInit)
        public override string RequiredRole => "Admin";

        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Lấy thông tin họ tên từ JWT Claim (được trích xuất tự động qua BasePage)
                lblAdminName.Text = CurrentFullName;

                // Nạp danh sách người dùng
                LoadDanhSachNguoiDung();
            }
        }

        private void LoadDanhSachNguoiDung()
        {
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                string sql = "SELECT Id, TenDangNhap, HoTen, Quyen, TrangThai FROM NguoiDung";
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    rptNguoiDung.DataSource = dt;
                    rptNguoiDung.DataBind();
                }
            }
        }

        // Xử lý nút đăng xuất bảo mật
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            SignOut();
        }
    }
}