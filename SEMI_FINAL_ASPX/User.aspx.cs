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
    public partial class User : BasePage
    {
        // BasePage tự động kiểm tra Authentication và trích xuất Claims trong OnInit
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Hiển thị tên người dùng từ JWT Claim
                lblUserName.Text = CurrentFullName;

                // Đổ dữ liệu từ CSDL vào Repeater
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

                    // Đổ dữ liệu vào Repeater
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