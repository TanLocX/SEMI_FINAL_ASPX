<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="User.aspx.cs" Inherits="SEMI_FINAL_ASPX.User" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Trang Người Dùng - Cổng Thông Tin</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server">
        

        <nav class="navbar navbar-expand-lg navbar-dark px-4 py-3 shadow-sm" style="background-color: #4f46e5;">
            <div class="container-fluid">
                <span class="navbar-brand fw-bold text-uppercase">CỔNG THÔNG TIN NGƯỜI DÙNG</span>
                
                <div class="d-flex align-items-center gap-3">

                    <span class="text-white">Xin chào, 
                        <asp:Label ID="lblUserName" runat="server" CssClass="fw-bold text-warning"></asp:Label>
                    </span>

                    <span class="badge bg-light text-dark px-2 py-1">Quyền: User</span>
                    

                    <asp:Button ID="btnLogout" runat="server" Text="Đăng xuất" CssClass="btn btn-outline-light btn-sm" OnClick="btnLogout_Click" />
                </div>
            </div>
        </nav>


        <div class="container my-5">
            

            <div class="alert alert-primary border-0 shadow-sm d-flex justify-content-between align-items-center mb-4">
                <div>
                    Bạn đang đăng nhập với tư cách <strong>Người dùng thường (User)</strong>. Bạn chỉ có quyền tra cứu danh sách.
                </div>

                <a href="Admin.aspx" class="btn btn-sm btn-outline-primary">Thử vào trang Admin</a>
            </div>


            <div class="card shadow-sm border-0 rounded-3">
                <div class="card-header bg-white py-3">
                    <h5 class="mb-0 fw-bold text-dark">Danh sách Thành viên trong Hệ thống</h5>
                </div>
                <div class="card-body p-0">
                    
                    <asp:Repeater ID="rptNguoiDung" runat="server">
                        <HeaderTemplate>
                            <table class="table table-hover table-striped mb-0 align-middle">
                                <thead class="table-light">
                                    <tr>
                                        <th>#</th>
                                        <th>Tên tài khoản</th>
                                        <th>Họ và tên</th>
                                        <th>Vai trò</th>
                                        <th>Trạng thái</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <tr>
                                <td><%# Container.ItemIndex + 1 %></td>
                                <td><strong><%# Eval("TenDangNhap") %></strong></td>
                                <td><%# Eval("HoTen") %></td>
                                <td>
                                    <span class='badge <%# Eval("Quyen").ToString() == "Admin" ? "bg-danger" : "bg-secondary" %>'>
                                        <%# Eval("Quyen") %>
                                    </span>
                                </td>
                                <td>
                                    <span class="badge bg-success">Đang hoạt động</span>
                                </td>
                            </tr>
                        </ItemTemplate>

                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>

                </div>
            </div>

        </div>
    </form>
</body>
</html>