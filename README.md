# SEMI_FINAL_ASPX - Hệ Thống Phân Quyền & Bảo Mật Web Forms

> **Học phần:** Lập Trình Web ASPX  
> **Chủ đề:** Authentication & Authorization - Phân quyền Role-Based Access Control (Admin / User)  
> **Nền tảng:** ASP.NET Web Forms (.NET Framework 4.7.2), C#, SQL Server, Bootstrap 5  
> **Tiêu chuẩn bảo mật:** OWASP Top 10, Argon2id Password Hashing, JWT (Access & Refresh Token), HttpOnly Cookies

---

## 📌 1. Giới thiệu Dự án

**SEMI_FINAL_ASPX** là ứng dụng web quản lý tài khoản và phân quyền người dùng được xây dựng trên nền tảng **ASP.NET Web Forms (.NET 4.7.2)**. 

Dự án không dừng lại ở mức quản lý phiên làm việc (Session) thông thường, mà được nâng cấp toàn diện lên kiến trúc bảo mật cấp doanh nghiệp theo chuẩn **OWASP**:
* **Băm mật khẩu Argon2id:** Thuật toán bộ nhớ cứng thắng giải *Password Hashing Competition*, vô hiệu hóa hoàn toàn dàn trâu cày GPU/ASIC brute-force.
* **Xác thực Token kép (JWT Dual-Token):** Access Token (15 phút) và Refresh Token (7 ngày).
* **Bảo vệ Cookie HttpOnly:** Miễn nhiễm trước các cuộc tấn công đánh cắp phiên qua XSS (Cross-Site Scripting).
* **Lớp Phân quyền Nền tảng (`BasePage`):** Kiến trúc hướng đối tượng (OOP) sạch, tự động kiểm tra quyền hạn và tự động làm mới phiên (**Silent Refresh**) khi Access Token hết hạn.

---

## ✨ 2. Tính năng Nổi bật

### 🔐 Xác thực (Authentication)
- Form đăng nhập giao diện hiện đại, chia 2 cột với Bootstrap 5.
- Chống tấn công **SQL Injection** 100% bằng câu lệnh tham số hóa (`SqlParameter`).
- Băm và kiểm tra mật khẩu bằng **Argon2id** với so sánh thời gian cố định (**Constant-time comparison**) chống tấn công kênh kề (Timing Attack).
- Kiểm tra trạng thái tài khoản: Tự động khóa và từ chối truy cập nếu `TrangThai == N'Bị khóa'`.
- Thông báo lỗi chung để tránh lộ thông tin tồn tại tài khoản (**Username Enumeration Prevention**).

### 🛡️ Phân quyền (Authorization - RBAC)
- Phân định rõ ràng 2 vai trò (**Role**):
  - **Quản trị viên (`Admin`):** Toàn quyền truy cập bảng điều khiển quản trị `Admin.aspx`, xem toàn bộ danh sách và có các nút chức năng Thêm / Sửa / Xóa.
  - **Người dùng thường (`User`):** Chỉ được xem danh sách thành viên ở chế độ đọc (**Read-only**) tại `User.aspx`.
- **Cơ chế chặn vượt quyền:** Khi tài khoản `User` cố tình gõ link `Admin.aspx` trên thanh địa chỉ, hệ thống lập tức chặn đứng và chuyển hướng sang trang lỗi **`AccessDenied.aspx (403 Forbidden)`**.

### 🔄 Tự động Làm mới Phiên (Silent Refresh) & Xoay vòng Token
- Access Token có thời hạn sống ngắn (15 phút) nhằm giảm thiểu rủi ro khi bị lộ.
- Khi Access Token hết hạn, lớp `BasePage` tự động đọc Refresh Token trong Cookie để cấp lại Access Token mới trong suốt, người dùng không bị văng ra trang Login khi đang thao tác.
- **Refresh Token Rotation:** Mỗi lần Refresh Token được sử dụng, token cũ sẽ bị thu hồi (`IsRevoked = 1`) và hệ thống cấp ngay một token mới để chống tấn công dùng lại token (Token Replay Attack).

### 🚪 Đăng xuất Toàn diện (Secure Logout)
- Hủy và thu hồi Refresh Token tương ứng trong CSDL SQL Server.
- Xóa sạch toàn bộ Cookie token trên trình duyệt (đặt hạn sử dụng về quá khứ).
- Hủy và xóa sạch phiên làm việc `Session`.

---

## 🏗️ 3. Cấu trúc Thư mục Chuẩn (Clean Architecture)

Dự án được cấu trúc mạch lạc, tách biệt trách nhiệm (**Separation of Concerns**):

```
SEMI_FINAL_ASPX/
│
├── Security/                      👉 CORE BẢO MẬT
│   ├── Argon2PasswordHasher.cs    # Băm Argon2id chuẩn PHC (Memory 64MB, Iterations 3, Parallelism 2)
│   ├── JwtTokenService.cs         # Cấp & xác thực Access Token (15m, HMAC-SHA256) / Refresh Token (7d)
│   └── TokenCookieManager.cs      # Đóng gói Token vào HttpOnly Cookies (Chống XSS, CSRF Lax)
│
├── Data/                          👉 TRUY XUẤT CƠ SỞ DỮ LIỆU
│   ├── RefreshTokenRepository.cs  # Thao tác bảng RefreshToken (Lưu SHA-256, xoay vòng, thu hồi)
│   └── database.sql               # Script CSDL QL_NguoiDung + Dữ liệu mẫu Argon2id
│
├── Common/                        👉 LỚP NỀN TẢNG DÙNG CHUNG
│   ├── BasePage.cs                # "Lớp phân quyền" cốt lõi: Tự động kiểm tra RBAC & Silent Refresh
│   └── SecurityHelper.cs          # Tiện ích auth & Đăng xuất toàn diện
│
├── Content/                       👉 Giao diện (Bootstrap 5 CSS)
├── Scripts/                       👉 Javascript thư viện Bootstrap
├── Properties/                    👉 Cấu hình Assembly dự án
│
├── AccessDenied.aspx / .cs        👉 Giao diện trang 403 (Từ chối truy cập)
├── Admin.aspx / .cs               👉 Giao diện trang Quản trị (Role: Admin)
├── Login.aspx / .cs               👉 Giao diện trang Đăng nhập
├── Logout.aspx / .cs              👉 Trang xử lý Đăng xuất
├── User.aspx / .cs                👉 Giao diện trang Người dùng (Role: User)
├── Global.asax                    👉 Vòng đời ứng dụng ASP.NET
├── Web.config                     👉 Cấu hình Secret Key, HttpOnly Cookies, Connection String
└── packages.config                👉 Quản lý thư viện NuGet
│
Tests/                             👉 KIỂM THỬ TỰ ĐỘNG
└── test_security.ps1              # Script kiểm thử tự động 8 kịch bản bảo mật
│
BAO_CAO_CHI_TIET_PHIEN_LAM_VIEC_SECURITY.md  👉 TÀI LIỆU CHI TIẾT BÁO CÁO & THUYẾT TRÌNH
README.md                          👉 TÀI LIỆU DỰ ÁN NÀY
```

---

## 💻 4. Yêu cầu Hệ thống & Công nghệ (Tech Stack)

* **IDE:** Visual Studio 2019 / 2022 / Insiders (đã cài workload *ASP.NET and web development*).
* **Target Framework:** .NET Framework 4.7.2.
* **Database:** SQL Server (LocalDB `(localdb)\MSSQLLocalDB` hoặc SQL Server Express / Enterprise).
* **Thư viện NuGet chính:**
  * `Konscious.Security.Cryptography.Argon2` (1.3.1) & `Blake2` (1.1.1)
  * `System.IdentityModel.Tokens.Jwt` (6.35.0)
  * `Microsoft.IdentityModel.Tokens` (6.35.0)
  * `bootstrap` (5.3.8)

---

## 🚀 5. Hướng dẫn Cài đặt & Chạy Dự án (Quick Start)

### Bước 1: Khởi tạo Cơ sở Dữ liệu
1. Mở SQL Server Management Studio (SSMS) hoặc cửa sổ dòng lệnh PowerShell.
2. Chạy file script: `SEMI_FINAL_ASPX\Data\database.sql` vào SQL Server của bạn.  
   *(Nếu dùng LocalDB, bạn có thể chạy trực tiếp lệnh: `sqlcmd -S "(localdb)\MSSQLLocalDB" -i "SEMI_FINAL_ASPX\Data\database.sql"`)*.

### Bước 2: Kiểm tra Chuỗi kết nối trong `Web.config`
Mở file `SEMI_FINAL_ASPX\Web.config` và kiểm tra thẻ `connectionStrings`:
```xml
<connectionStrings>
    <add name="QLNguoiDungConn"
         connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=QL_NguoiDung;Integrated Security=True"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```
*(Thay đổi `Data Source` phù hợp với máy của bạn nếu không dùng LocalDB)*.

### Bước 3: Mở và Chạy Dự án
1. Mở file solution `SEMI_FINAL_ASPX.slnx` trong Visual Studio.
2. Nhấn phím **F5** (hoặc nút **Start Debugging** trên thanh công cụ).
3. Visual Studio và IIS Express sẽ tự động khởi động và mở thẳng trang Đăng nhập tại địa chỉ:
   `https://localhost:44320/Login.aspx` (hoặc `http://localhost:61546/Login.aspx`).

---

## 👥 6. Danh sách Tài khoản Thử nghiệm (Test Accounts)

Tất cả tài khoản dưới đây đều đã được tạo sẵn trong file `database.sql` với mật khẩu mặc định là: **`123`** (đã băm bằng Argon2id).

| Tên đăng nhập | Mật khẩu | Họ và tên | Vai trò (Role) | Trang đích sau khi Login |
| :--- | :---: | :--- | :---: | :---: |
| **`admin`** | `123` | Quản Trị Viên Hệ Thống | **Admin** | `Admin.aspx` |
| **`locadmin`** | `123` | Nguyễn Tấn Lộc (Admin) | **Admin** | `Admin.aspx` |
| **`user`** | `123` | Người Dùng Mẫu | **User** | `User.aspx` |
| **`giahung`** | `123` | Phan Trần Gia Hưng | **User** | `User.aspx` |
| **`vanan`** | `123` | Trần Văn An | **User** | `User.aspx` |
| **`thuhuong`**| `123` | Lê Thị Thu Hương | **User** | `User.aspx` |

---

## 🧪 7. Kiểm thử Tự động (Automated Testing)

Dự án đi kèm một script kiểm thử tự động toàn diện được viết bằng PowerShell (`Tests/test_security.ps1`), kiểm tra end-to-end qua 8 kịch bản bảo mật:

```powershell
powershell -File Tests/test_security.ps1
```

### Các kịch bản được kiểm thử:
1. **[TEST 1]** Lấy ViewState & Form Token từ `Login.aspx`.
2. **[TEST 2]** Đăng nhập User thường bằng Argon2id $\rightarrow$ Trả về cặp cookie `jwt_access_token` và `jwt_refresh_token` đều có cờ **`HttpOnly: True`**.
3. **[TEST 3]** Kiểm tra Phân quyền RBAC: Tài khoản User cố tình truy cập `Admin.aspx` $\rightarrow$ Chặn đứng, trả về mã điều hướng sang `/AccessDenied.aspx` (403 Forbidden).
4. **[TEST 4]** User truy cập `User.aspx` hợp lệ $\rightarrow$ Trả về HTTP 200 OK.
5. **[TEST 5]** Đăng xuất qua `Logout.aspx` $\rightarrow$ Thu hồi token trong SQL và xóa cookie, điều hướng về `/Login.aspx`.
6. **[TEST 6]** Thử truy cập lại sau khi đăng xuất $\rightarrow$ Chặn truy cập, điều hướng về `/Login.aspx`.
7. **[TEST 7]** Đăng nhập tài khoản Admin $\rightarrow$ Điều hướng thành công về `/Admin.aspx`.
8. **[TEST 8]** Admin truy cập trang `Admin.aspx` $\rightarrow$ Trả về HTTP 200 OK.

---

## 📚 8. Tài liệu Báo cáo & Thuyết trình

Chi tiết lý thuyết so sánh, sơ đồ phân luồng, hướng dẫn code lớp `BasePage.cs`, kịch bản báo cáo trước lớp và các câu hỏi phản biện của giảng viên được trình bày đầy đủ tại:
👉 **`BAO_CAO_CHI_TIET_PHIEN_LAM_VIEC_SECURITY.md`**
