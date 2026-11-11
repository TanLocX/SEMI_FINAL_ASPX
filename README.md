# SEMI_FINAL_ASPX

Hệ thống quản lý người dùng và phân quyền (RBAC) trên nền tảng ASP.NET Web Forms (.NET Framework 4.7.2). Dự án triển khai xác thực bằng Argon2id, JWT lưu trữ qua HttpOnly Cookie và lớp phân quyền tập trung `BasePage`.

---

## Mục lục

1. [Tổng quan](#tổng-quan)
2. [Kiến trúc & Công nghệ](#kiến-trúc--công-nghệ)
3. [Cấu trúc Thư mục](#cấu-trúc-thư-mục)
4. [Cơ chế Bảo mật & Phân quyền](#cơ-chế-bảo-mật--phân-quyền)
5. [Cài đặt & Khởi chạy](#cài-đặt--khởi-chạy)
6. [Tài khoản Thử nghiệm](#tài-khoản-thử-nghiệm)
7. [Kiểm thử Tự động](#kiểm-thử-tự-động)
8. [Tài liệu Kỹ thuật](#tài-liệu-kỹ-thuật)

---

## Tổng quan

Đồ án học phần **Lập Trình Web ASPX** (Bài tập 6: Authentication & Authorization).

Mục tiêu:
* Xây dựng chức năng Đăng nhập (Login) và Đăng xuất (Logout) an toàn.
* Phân quyền truy cập dựa trên vai trò (**Role-Based Access Control**):
  * **Admin:** Quản trị danh sách người dùng, có quyền thao tác quản trị.
  * **User:** Chỉ xem thông tin danh sách (Read-only), bị chặn khi truy cập khu vực quản trị.
* Xử lý xác thực không lưu mật khẩu thô, bảo vệ token trước nguy cơ XSS và CSRF.

---

## Kiến trúc & Công nghệ

* **Framework:** ASP.NET Web Forms (.NET Framework 4.7.2)
* **Ngôn ngữ:** C# 7.3
* **Cơ sở dữ liệu:** Microsoft SQL Server (LocalDB / Express)
* **Frontend:** Bootstrap 5.3.8
* **Thư viện xác thực & mật mã:**
  * `Konscious.Security.Cryptography.Argon2` (1.3.1): Băm mật khẩu Argon2id
  * `System.IdentityModel.Tokens.Jwt` (6.35.0): Quản lý JSON Web Token
  * `Microsoft.IdentityModel.Tokens` (6.35.0): Ký số HMAC-SHA256

---

## Cấu trúc Thư mục

Mã nguồn được phân chia theo từng tầng trách nhiệm:

```
SEMI_FINAL_ASPX/
├── Common/
│   ├── BasePage.cs                 # Lớp cơ sở kiểm tra RBAC tại OnInit & Silent Refresh
│   └── SecurityHelper.cs           # Tiện ích xác thực và đăng xuất
├── Data/
│   ├── database.sql                # Script khởi tạo CSDL và bảng RefreshToken
│   └── RefreshTokenRepository.cs   # Truy vấn, lưu hash và xoay vòng Refresh Token
├── Security/
│   ├── Argon2PasswordHasher.cs     # Băm và xác minh mật khẩu bằng Argon2id (chuẩn PHC)
│   ├── JwtTokenService.cs          # Sinh Access Token (15m) và Refresh Token (7d)
│   └── TokenCookieManager.cs       # Quản lý HttpOnly Cookie cho token
├── Content/                        # Tệp CSS, Bootstrap
├── Scripts/                        # Tệp JavaScript, Bootstrap bundle
├── AccessDenied.aspx               # Trang thông báo 403 Forbidden
├── Admin.aspx                      # Trang quản trị (yêu cầu Role = Admin)
├── Login.aspx                      # Trang đăng nhập
├── Logout.aspx                     # Trang điều hướng đăng xuất
├── User.aspx                       # Trang người dùng thường (Role = User)
├── Global.asax                     # Vòng đời ứng dụng
└── Web.config                      # Cấu hình chuỗi kết nối, JWT secret, HTTP cookies
Tests/
└── test_security.ps1               # Script PowerShell kiểm thử 8 kịch bản bảo mật
```

---

## Cơ chế Bảo mật & Phân quyền

### 1. Băm mật khẩu (Argon2id)
* Mật khẩu được băm bằng thuật toán **Argon2id** (OWASP khuyến nghị) trước khi lưu vào cột `MatKhau VARCHAR(255)`.
* Tham số cấu hình:
  * Memory: 64 MB (65,536 KB)
  * Iterations: 3 vòng
  * Parallelism: 2 luồng
  * Salt: 16 bytes ngẫu nhiên từ `RNGCryptoServiceProvider`
* Định dạng lưu trữ: Chuỗi PHC `$argon2id$v=19$m=65536,t=3,p=2$...`
* Xác minh mật khẩu dùng thuật toán so sánh thời gian cố định (`SlowEquals`) nhằm triệt tiêu tấn công Timing Attack.

### 2. Xác thực Token kép (Dual-Token) & HttpOnly Cookie
* **Access Token:** JWT có thời hạn 15 phút, ký bằng `HmacSha256`, chứa các claim: `userId`, `username`, `name`, `role`, `jti`.
* **Refresh Token:** Chuỗi ngẫu nhiên 32 bytes, hạn 7 ngày. CSDL chỉ lưu mã băm SHA-256 (`TokenHash`) của token này để phòng chống rò rỉ dữ liệu.
* **Lưu trữ qua Cookie:** Cả 2 token được lưu trong Cookie trình duyệt với thuộc tính:
  * `HttpOnly = true`: Chặn mã độc JavaScript truy cập trực tiếp vào token qua XSS.
  * `SameSite = Lax`: Giảm thiểu rủi ro tấn công CSRF.
  * `Path = /`: Áp dụng cho toàn bộ ứng dụng.

### 3. Phân quyền tập trung (`Common/BasePage.cs`)
Thay vì kiểm tra thủ công trong từng trang `.aspx.cs`, các trang được bảo vệ sẽ kế thừa `BasePage`:
* Tại sự kiện `OnInit`:
  1. Đọc và kiểm tra tính hợp lệ của `jwt_access_token`.
  2. Nếu Access Token hết hạn, hệ thống tự động kiểm tra `jwt_refresh_token` trong CSDL để cấp lại Access Token mới (**Silent Refresh**) và xoay vòng Refresh Token (**Token Rotation**).
  3. Nếu không có token hợp lệ hoặc token đã bị thu hồi, chuyển hướng về `Login.aspx`.
  4. Nếu hợp lệ, đối chiếu `RequiredRole`:
     * Trang `Admin.aspx`: `RequiredRole = "Admin"`. Nếu người dùng mang vai trò `User`, hệ thống chặn truy cập và chuyển hướng sang `AccessDenied.aspx` (403).
     * Trang `User.aspx`: Không yêu cầu quyền đặc biệt, cho phép mọi tài khoản đã đăng nhập.

---

## Cài đặt & Khởi chạy

### Yêu cầu
* Windows 10/11
* Visual Studio 2019/2022 với workload **ASP.NET and web development**
* SQL Server (LocalDB hoặc SQL Server Express)

### Các bước thực hiện

1. **Khởi tạo cơ sở dữ liệu:**
   Mở SQL Server và chạy tệp `SEMI_FINAL_ASPX/Data/database.sql`.  
   Lệnh chạy nhanh qua CLI với LocalDB:
   ```cmd
   sqlcmd -S "(localdb)\MSSQLLocalDB" -i "SEMI_FINAL_ASPX\Data\database.sql"
   ```

2. **Cấu hình chuỗi kết nối:**
   Kiểm tra tệp `SEMI_FINAL_ASPX/Web.config`:
   ```xml
   <connectionStrings>
     <add name="QLNguoiDungConn"
          connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=QL_NguoiDung;Integrated Security=True"
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

3. **Mở và biên dịch dự án:**
   * Mở `SEMI_FINAL_ASPX.slnx` bằng Visual Studio.
   * Chọn cấu hình **Debug** và nhấn **F5**.
   * Ứng dụng sẽ tự động mở trang `https://localhost:44320/Login.aspx`.

---

## Tài khoản Thử nghiệm

Tất cả tài khoản mẫu có mật khẩu mặc định: **`123`**

| Tên đăng nhập | Mật khẩu | Họ và tên | Vai trò (Role) | Trang đích |
| :--- | :---: | :--- | :---: | :---: |
| `admin` | `123` | Quản Trị Viên Hệ Thống | Admin | `Admin.aspx` |
| `locadmin` | `123` | Nguyễn Tấn Lộc (Admin) | Admin | `Admin.aspx` |
| `user` | `123` | Người Dùng Mẫu | User | `User.aspx` |
| `giahung` | `123` | Phan Trần Gia Hưng | User | `User.aspx` |
| `vanan` | `123` | Trần Văn An | User | `User.aspx` |
| `thuhuong` | `123` | Lê Thị Thu Hương | User | `User.aspx` |

---

## Kiểm thử Tự động

Dự án cung cấp bộ kịch bản kiểm thử tự động tại `Tests/test_security.ps1` nhằm thẩm tra các ca kiểm thử bảo mật cốt lõi:

```powershell
powershell -File Tests/test_security.ps1
```

Kịch bản bao gồm:
* Trích xuất `__VIEWSTATE` và token hợp lệ từ `Login.aspx`.
* Đăng nhập người dùng thường với mật khẩu Argon2id, kiểm tra cookie `HttpOnly`.
* Kiểm tra RBAC: Tài khoản `User` truy cập trái phép vào `Admin.aspx` (kỳ vọng chuyển hướng `AccessDenied.aspx`).
* Truy cập hợp lệ vào `User.aspx` (HTTP 200).
* Đăng xuất qua `Logout.aspx`, thu hồi Refresh Token trong CSDL.
* Thử truy cập lại tài nguyên sau khi đăng xuất (kỳ vọng chuyển hướng `Login.aspx`).
* Đăng nhập tài khoản `Admin` và truy cập thành công `Admin.aspx` (HTTP 200).

---

## Tài liệu Kỹ thuật

Báo cáo phân tích chuyên sâu, sơ đồ luồng dữ liệu, giải thích chi tiết lớp `BasePage` và câu hỏi phản biện xem tại:
* [BAO_CAO_CHI_TIET_PHIEN_LAM_VIEC_SECURITY.md](BAO_CAO_CHI_TIET_PHIEN_LAM_VIEC_SECURITY.md)
