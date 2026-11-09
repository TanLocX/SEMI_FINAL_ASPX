<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="SEMI_FINAL_ASPX.AccessDenied" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>403 - Từ chối truy cập</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light d-flex align-items-center justify-content-center" style="min-height: 100vh;">
    <form id="form1" runat="server" class="text-center p-4">
        <div class="card shadow border-0 rounded-4 p-5" style="max-width: 520px;">
            <div class="mb-3">
                <span class="display-1 text-danger fw-bold">403</span>
            </div>
            <h3 class="fw-bold text-dark mb-2">TỪ CHỐI TRUY CẬP (ACCESS DENIED)</h3>
            <p class="text-muted mb-4">
                Tài khoản của bạn không có đủ quyền hạn để truy cập vào khu vực Quản trị này.
            </p>
            <div class="d-flex justify-content-center gap-3">
                <a href="User.aspx" class="btn btn-primary px-4">Quay về trang Người dùng</a>
                <a href="Login.aspx" class="btn btn-outline-secondary px-4">Đổi tài khoản</a>
            </div>
        </div>
    </form>
</body>
</html>