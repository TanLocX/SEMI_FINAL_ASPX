using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SEMI_FINAL_ASPX
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // =========================================================================================
            // 🛡️ PHÒNG THỦ LUỒNG 5: TRANG ĐĂNG XUẤT ĐỘC LẬP - HỦY PHIÊN TOÀN DIỆN
            // =========================================================================================
            // 1. Xóa toàn bộ dữ liệu biến trong Session
            Session.Clear();

            // 2. Hủy phiên làm việc trên Web Server
            Session.Abandon();

            // 3. Xóa cookie phiên ASP.NET_SessionId
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }

            // 4. Vô hiệu hóa cache trình duyệt
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            // 5. Chuyển hướng người dùng về trang Đăng nhập
            Response.Redirect("Login.aspx");
        }
    }
}
