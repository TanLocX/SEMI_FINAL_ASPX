<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="SEMI_FINAL_ASPX.Admin" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Trang Quản Trị - Admin</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server">
        

        <nav class="navbar navbar-expand-lg navbar-dark bg-dark px-4 py-3 shadow-sm">
            <div class="container-fluid">
                <span class="navbar-brand fw-bold text-uppercase">Hệ Thống Quản Trị (Admin Panel)</span>

                <div class="d-flex align-items-center gap-3">
                    <span class="text-white">Xin chào, 
                        <asp:Label ID="lblAdminName" runat="server" CssClass="fw-bold text-warning">

                        </asp:Label>

                    </span>
                    <span class="badge bg-danger px-2 py-1">Admin</span>
                    

                    <asp:Button ID="btnLogout" runat="server" Text="Đăng xuất" CssClass="btn btn-outline-light btn-sm" OnClick="btnLogout_Click" />
                </div>
            </div>
        </nav>



        <div class="container my-5">
            <div class="card shadow-sm border-0 rounded-3">
                <div class="card-header bg-white py-3 d-flex justify-content-between align-items-center">
                    <h5 class="mb-0 fw-bold text-primary">Danh sách Người dùng / Sản phẩm</h5>



                    <button type="button" class="btn btn-primary btn-sm fw-semibold">+ Thêm Mới</button>
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
                                        <th>Quyen</th>
                                        <th>Trạng thái</th>
                                        <th class="text-center">Thao tác quản trị</th>
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
                                    <span class="badge bg-success"><%# Eval("TrangThai") %></span>
                                </td>
                                <td class="text-center">
                                    <button type="button" class="btn btn-warning btn-sm me-1">Sửa</button>
                                    <button type="button" class="btn btn-danger btn-sm">Xóa</button>
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