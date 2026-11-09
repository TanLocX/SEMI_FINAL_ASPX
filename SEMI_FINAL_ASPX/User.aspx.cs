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
    public partial class User : System.Web.UI.Page
    {
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. KIỂM TRA ĐĂNG NHẬP (AUTHENTICATION)
            // Nếu chưa đăng nhập thì đuổi về Login.aspx
            if (Session["Username"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // 2. HIỂN THỊ TÊN NGƯỜI DÙNG LÊN NAVBAR
                lblUserName.Text = Session["FullName"] != null ? Session["FullName"].ToString() : Session["Username"].ToString();

                // 3. ĐỔ DỮ LIỆU TỪ CSDL VÀO REPEATER
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

        // 4. XỬ LÝ NÚT ĐĂNG XUẤT (LOGOUT)
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();    // Xóa hết dữ liệu phiên
            Session.Abandon();  // Hủy phiên làm việc
            Response.Redirect("Login.aspx");
        }
    }
}