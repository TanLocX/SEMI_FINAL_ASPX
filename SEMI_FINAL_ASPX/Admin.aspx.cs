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
    public partial class Admin : System.Web.UI.Page
    {
        private string strConn = ConfigurationManager.ConnectionStrings["QLNguoiDungConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. KIỂM TRA AUTHENTICATION: Chưa đăng nhập thì về Login
            if (Session["Username"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // 2. KIỂM TRA AUTHORIZATION: Đã đăng nhập nhưng KHÔNG PHẢI ADMIN -> Đẩy sang AccessDenied.aspx
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("AccessDenied.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // 3. HIỂN THỊ TÊN ADMIN LÊN NAVBAR
                lblAdminName.Text = Session["FullName"] != null ? Session["FullName"].ToString() : Session["Username"].ToString();

                // 4. NẠP DỮ LIỆU CSDL VÀO REPEATER
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

        // 5. NÚT ĐĂNG XUẤT
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}