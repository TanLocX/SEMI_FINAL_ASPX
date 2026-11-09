<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SEMI_FINAL_ASPX.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
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
                        <h4 class="fw-bold">LẬP TRÌNH WEB ASPX</h4>
                    </div>
                    <div>
                        <div class="fw-bold mb-3" style="font-size: 2.5rem; line-height: 1.2;">
                            <p>Start your day with</p>
                            
                            <p>everything in one place.</p>
                        </div>
                        
                        <p class="lead" style="font-size: 1.1rem; opacity: 0.85;">
                            Projects, docs, and conversations — organized so your team can focus on the work that matters.
                        </p>
                    </div>
                    <div>
                        <p class="fst-italic" style="font-size: 0.9rem; opacity: 0.85;">
                            “The cleanest tool we have ever rolled out.” — Dana K., Ops Lead
                        </p>
                    </div>
                </div>
                


                <div class="col-md-6 d-flex align-items-center justify-content-center bg-white p-4">
    
                    <!-- Hộp giới hạn độ rộng để form đẹp và gọn -->
                    <div style="max-width: 400px; width: 100%;">
        
                        <!-- 1. Tiêu đề form -->
                        <div class="mb-4">
                            <h2 class="fw-bold text-dark">Welcome back</h2>
                            <p class="text-muted small">Sign in to continue to your workspace.</p>
                        </div>

                        <!-- 2. Ô nhập Email / Tên đăng nhập -->
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Email address</label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control py-2" placeholder="you@company.com"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" 
                                ControlToValidate="txtUsername" 
                                
                                CssClass="text-danger small mt-1 d-block" 
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>
                        </div>

                        <!-- 3. Ô nhập Mật khẩu -->
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control py-2" TextMode="Password" placeholder="••••••••"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" 
                                ControlToValidate="txtPassword" 
                                
                                CssClass="text-danger small mt-1 d-block" 
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>
                        </div>

                        <!-- 4. Hàng Remember me & Quên mật khẩu (dàn đều 2 bên) -->
                        <div class="d-flex justify-content-between align-items-center mb-4">
                            <div class="form-check">
                                <asp:CheckBox ID="chkRemember" runat="server" CssClass="form-check-input" />
                                <label class="form-check-label text-muted small" for="chkRemember">Remember me</label>
                            </div>
                            <a href="#" class="small text-decoration-none fw-semibold" style="color: #4f46e5;">Forgot password?</a>
                        </div>

                        <!-- 5. Thông báo lỗi khi đăng nhập sai -->
                        <asp:Label ID="lblThongBao" runat="server" CssClass="text-danger small d-block text-center mb-3 fw-semibold"></asp:Label>

                        <!-- 6. Nút Đăng nhập -->
                        <asp:Button ID="btnLogin" runat="server" Text="Sign in" 
                            CssClass="btn w-100 py-2 fw-semibold text-white mb-4" 
                            style="background-color: #4f46e5;" 
                            OnClick="btnLogin_Click" />

                        <!-- 7. Đăng ký tài khoản mới -->
                        <div class="text-center small text-muted">
                            New here? <a href="#" class="text-decoration-none fw-semibold" style="color: #4f46e5;">Create an account</a>
                        </div>

                        

                    </div>

                </div>
            </div>
        </div>
    </form>
</body>
</html>
