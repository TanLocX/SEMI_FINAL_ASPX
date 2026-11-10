using System;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using SEMI_FINAL_ASPX.Data;
using SEMI_FINAL_ASPX.Security;

namespace SEMI_FINAL_ASPX.Common
{
    /// <summary>
    /// Lớp tiện ích bảo mật dùng chung cho toàn bộ hệ thống.
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// Kiểm tra nhanh xem request hiện tại đã đăng nhập hay chưa (qua Access Token hoặc Refresh Token).
        /// </summary>
        public static bool IsAuthenticated(HttpRequest request)
        {
            if (request == null) return false;

            string accessToken = TokenCookieManager.GetAccessToken(request);
            if (!string.IsNullOrEmpty(accessToken) && JwtTokenService.ValidateAccessToken(accessToken, out _))
            {
                return true;
            }

            string refreshToken = TokenCookieManager.GetRefreshToken(request);
            return !string.IsNullOrEmpty(refreshToken);
        }

        /// <summary>
        /// Lấy Role của người dùng hiện tại từ Access Token.
        /// </summary>
        public static string GetCurrentRole(HttpRequest request)
        {
            if (request == null) return string.Empty;

            string accessToken = TokenCookieManager.GetAccessToken(request);
            if (!string.IsNullOrEmpty(accessToken) && JwtTokenService.ValidateAccessToken(accessToken, out ClaimsPrincipal principal))
            {
                return principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Đăng xuất: Thu hồi token trong CSDL và xóa sạch Cookie.
        /// </summary>
        public static void SignOut(Page page)
        {
            if (page == null) return;

            string refreshToken = TokenCookieManager.GetRefreshToken(page.Request);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                RefreshTokenRepository.RevokeRefreshToken(refreshToken);
            }

            TokenCookieManager.ClearAuthCookies(page.Response, page.Request);
            page.Session?.Clear();
            page.Session?.Abandon();

            page.Response.Redirect("Login.aspx", true);
        }
    }
}
