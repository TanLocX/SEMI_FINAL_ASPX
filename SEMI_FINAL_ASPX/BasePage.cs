using System;
using System.Security.Claims;
using System.Web.UI;

namespace SEMI_FINAL_ASPX
{
    /// <summary>
    /// Lớp phân quyền dùng chung (Authorization Base Page).
    /// Áp dụng cơ chế RBAC (Role-Based Access Control) qua JWT và tự động Silent Refresh.
    /// </summary>
    public abstract class BasePage : Page
    {
        /// <summary>
        /// Quyền (Role) bắt buộc để truy cập trang này.
        /// - null (mặc định): Cho phép bất kỳ người dùng nào đã đăng nhập.
        /// - "Admin": Chỉ cho phép Quản trị viên truy cập.
        /// - "User": Chỉ cho phép Người dùng thường truy cập.
        /// </summary>
        public virtual string RequiredRole => null;

        /// <summary>
        /// Tên đăng nhập của người dùng hiện tại từ JWT Claim.
        /// </summary>
        public string CurrentUsername { get; private set; }

        /// <summary>
        /// Họ và tên của người dùng hiện tại từ JWT Claim.
        /// </summary>
        public string CurrentFullName { get; private set; }

        /// <summary>
        /// Vai trò (Role) của người dùng hiện tại từ JWT Claim.
        /// </summary>
        public string CurrentRole { get; private set; }

        /// <summary>
        /// Mã định danh người dùng trong CSDL.
        /// </summary>
        public int CurrentUserId { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Thực hiện quy trình Xác thực (Authentication) và Phân quyền (Authorization)
            AuthenticateAndAuthorize();
        }

        private void AuthenticateAndAuthorize()
        {
            string accessToken = TokenCookieManager.GetAccessToken(Request);
            ClaimsPrincipal principal;

            // 1. Kiểm tra Access Token
            if (!string.IsNullOrEmpty(accessToken) && JwtTokenService.ValidateAccessToken(accessToken, out principal))
            {
                ApplyPrincipal(principal);
            }
            else
            {
                // 2. Access Token hết hạn hoặc không có -> Thử khôi phục bằng Refresh Token (Silent Refresh)
                string refreshToken = TokenCookieManager.GetRefreshToken(Request);

                if (!string.IsNullOrEmpty(refreshToken) &&
                    RefreshTokenRepository.TryRotateRefreshToken(
                        refreshToken,
                        out int userId,
                        out string username,
                        out string fullName,
                        out string role,
                        out string newRefreshToken))
                {
                    // Cấp Access Token mới
                    string newAccessToken = JwtTokenService.GenerateAccessToken(userId, username, fullName, role);

                    // Cập nhật lại Cookie cho trình duyệt
                    TokenCookieManager.SetAuthCookies(Response, Request, newAccessToken, newRefreshToken);

                    // Thiết lập thông tin người dùng từ token mới
                    CurrentUserId = userId;
                    CurrentUsername = username;
                    CurrentFullName = fullName;
                    CurrentRole = role;

                    if (JwtTokenService.ValidateAccessToken(newAccessToken, out principal))
                    {
                        Context.User = principal;
                    }
                }
                else
                {
                    // 3. Cả Access Token lẫn Refresh Token đều không hợp lệ -> Chưa đăng nhập
                    TokenCookieManager.ClearAuthCookies(Response, Request);
                    Response.Redirect("Login.aspx", true);
                    return;
                }
            }

            // 4. KIỂM TRA PHÂN QUYỀN (AUTHORIZATION / RBAC)
            if (!string.IsNullOrEmpty(RequiredRole))
            {
                if (!string.Equals(CurrentRole, RequiredRole, StringComparison.OrdinalIgnoreCase))
                {
                    // Không đủ quyền hạn -> Đá sang trang 403 Access Denied
                    Response.Redirect("AccessDenied.aspx", true);
                }
            }
        }

        private void ApplyPrincipal(ClaimsPrincipal principal)
        {
            Context.User = principal;

            CurrentUsername = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            CurrentFullName = principal.FindFirst(ClaimTypes.Name)?.Value ?? CurrentUsername;
            CurrentRole = principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            string userIdClaim = principal.FindFirst("userId")?.Value;
            if (int.TryParse(userIdClaim, out int uid))
            {
                CurrentUserId = uid;
            }
        }

        /// <summary>
        /// Đăng xuất an toàn: Thu hồi Refresh Token trong DB và hủy toàn bộ Cookie.
        /// </summary>
        public void SignOut()
        {
            string refreshToken = TokenCookieManager.GetRefreshToken(Request);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                RefreshTokenRepository.RevokeRefreshToken(refreshToken);
            }

            TokenCookieManager.ClearAuthCookies(Response, Request);
            Session?.Clear();
            Session?.Abandon();

            Response.Redirect("Login.aspx", true);
        }
    }
}
