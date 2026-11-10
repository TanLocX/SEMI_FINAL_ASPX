using System;
using System.Web;

namespace SEMI_FINAL_ASPX
{
    /// <summary>
    /// Quản lý việc lưu trữ và thu hồi Access Token & Refresh Token qua HttpOnly Cookie.
    /// Giúp Web Forms bảo mật tuyệt đối trước XSS (JavaScript không thể đọc Cookie).
    /// </summary>
    public static class TokenCookieManager
    {
        public const string AccessTokenCookieName = "jwt_access_token";
        public const string RefreshTokenCookieName = "jwt_refresh_token";

        /// <summary>
        /// Ghi cặp token vào Cookie của trình duyệt.
        /// </summary>
        public static void SetAuthCookies(HttpResponse response, HttpRequest request, string accessToken, string refreshToken)
        {
            if (response == null) return;

            bool isSecure = request != null && request.IsSecureConnection;

            // 1. Access Token Cookie (15 phút)
            var accessCookie = new HttpCookie(AccessTokenCookieName, accessToken)
            {
                HttpOnly = true,
                Secure = isSecure,
                Path = "/",
                Expires = DateTime.Now.AddMinutes(JwtTokenService.AccessTokenExpirationMinutes),
                SameSite = SameSiteMode.Lax
            };
            response.Cookies.Add(accessCookie);

            // 2. Refresh Token Cookie (7 ngày)
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshCookie = new HttpCookie(RefreshTokenCookieName, refreshToken)
                {
                    HttpOnly = true,
                    Secure = isSecure,
                    Path = "/",
                    Expires = DateTime.Now.AddDays(JwtTokenService.RefreshTokenExpirationDays),
                    SameSite = SameSiteMode.Lax
                };
                response.Cookies.Add(refreshCookie);
            }
        }

        /// <summary>
        /// Lấy Access Token từ Cookie của request.
        /// </summary>
        public static string GetAccessToken(HttpRequest request)
        {
            return request?.Cookies[AccessTokenCookieName]?.Value;
        }

        /// <summary>
        /// Lấy Refresh Token từ Cookie của request.
        /// </summary>
        public static string GetRefreshToken(HttpRequest request)
        {
            return request?.Cookies[RefreshTokenCookieName]?.Value;
        }

        /// <summary>
        /// Xóa cặp Token khỏi Cookie khi người dùng Đăng xuất (Set hạn về quá khứ).
        /// </summary>
        public static void ClearAuthCookies(HttpResponse response, HttpRequest request)
        {
            if (response == null) return;

            bool isSecure = request != null && request.IsSecureConnection;

            var accessCookie = new HttpCookie(AccessTokenCookieName, string.Empty)
            {
                HttpOnly = true,
                Secure = isSecure,
                Path = "/",
                Expires = DateTime.Now.AddDays(-1),
                SameSite = SameSiteMode.Lax
            };
            response.Cookies.Add(accessCookie);

            var refreshCookie = new HttpCookie(RefreshTokenCookieName, string.Empty)
            {
                HttpOnly = true,
                Secure = isSecure,
                Path = "/",
                Expires = DateTime.Now.AddDays(-1),
                SameSite = SameSiteMode.Lax
            };
            response.Cookies.Add(refreshCookie);
        }
    }
}
