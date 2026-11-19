<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SEMI_FINAL_ASPX.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>ĐĂNG NHẬP HỆ THỐNG</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container-fluid">
            <div class="row">
                <div class="col-md-6 text-white d-flex flex-column justify-content-between p-5" 
                    style="background: linear-gradient(135deg, #4f46e5, #7c3aed); min-height: 100vh;">
                    <div>
                        <div class="d-flex align-items-center gap-2 mb-2">
                            <span class="badge bg-white text-primary px-3 py-2 fw-bold">NHÓM 6</span>
                            <span class="text-white-50">|</span>
                            <h5 class="fw-bold mb-0 text-white">LẬP TRÌNH WEB ASPX</h5>
                        </div>
                    </div>
                    <div>
                        <div class="fw-bold mb-3" style="font-size: 2.3rem; line-height: 1.25;">
                            <p class="mb-2">AUTHENTICATION &amp;</p>
                            <p class="mb-0">AUTHORIZATION</p>
                        </div>
                        
                        <p class="lead" style="font-size: 1.05rem; opacity: 0.9; line-height: 1.6;">
                            Cơ chế kiểm soát an ninh 2 tầng: Xác thực người dùng (Authentication) và Phân quyền theo vai trò (Role-Based Authorization), phòng chống truy cập trái phép và bảo vệ an toàn dữ liệu.
                        </p>
                    </div>
                    <div>
                        <p class="fst-italic mb-0" style="font-size: 0.9rem; opacity: 0.85;">
                            “Authentication xác định Bạn là ai — Authorization quyết định Bạn được phép làm gì.”
                        </p>
                        <small class="text-white-50">Đề tài giữa kỳ: Authentication / Authorization (Login + Role)</small>
                    </div>
                </div>
                


                <div class="col-md-6 d-flex align-items-center justify-content-center bg-white p-4">
    
                    <!-- Hộp giới hạn độ rộng để form đẹp và gọn -->
                    <div style="max-width: 400px; width: 100%;">
        
                        <!-- 1. Tiêu đề form -->
                        <div class="mb-4">
                            <h2 class="fw-bold text-dark">Đăng nhập hệ thống</h2>
                            <p class="text-muted small">Xác thực danh tính để phân quyền truy cập chức năng.</p>
                        </div>

                        <!-- 2. Ô nhập Tên đăng nhập -->
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Tên đăng nhập (Username)</label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control py-2" placeholder="admin hoặc user"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" 
                                ControlToValidate="txtUsername" 
                                ErrorMessage=""
                                CssClass="text-danger small mt-1 d-block" 
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>
                        </div>

                        <!-- 3. Ô nhập Mật khẩu -->
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Mật khẩu (Password)</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control py-2" TextMode="Password" placeholder="Nhập mật khẩu"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" 
                                ControlToValidate="txtPassword" 
                                ErrorMessage=""
                                CssClass="text-danger small mt-1 d-block" 
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>
                        </div>

                        <!-- 5. Thông báo lỗi khi đăng nhập sai -->
                        <asp:Label ID="lblThongBao" runat="server" CssClass="text-danger small d-block text-center mb-3 fw-semibold"></asp:Label>

                        <!-- 6. Nút Đăng nhập -->
                        <asp:Button ID="btnLogin" runat="server" Text="Đăng nhập hệ thống" 
                            CssClass="btn w-100 py-2 fw-semibold text-white mb-3" 
                            style="background-color: #4f46e5;" 
                            OnClick="btnLogin_Click" />

                       

                        

                    </div>

                </div>
            </div>
        </div>
    </form>
</body>
</html>
