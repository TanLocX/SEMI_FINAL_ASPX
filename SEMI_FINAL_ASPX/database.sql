
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QL_NguoiDung')
BEGIN
    CREATE DATABASE QL_NguoiDung;
END
GO

USE QL_NguoiDung;
GO


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NguoiDung')
BEGIN
    CREATE TABLE NguoiDung (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
        MatKhau VARCHAR(50) NOT NULL,
        HoTen NVARCHAR(100) NOT NULL,
        Email VARCHAR(100),
        Quyen VARCHAR(20) NOT NULL, -- 'Admin' hoặc 'User'
        TrangThai NVARCHAR(30) DEFAULT N'Đang hoạt động'
    );
END
GO


DELETE FROM NguoiDung;
GO


INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email, Quyen, TrangThai) 
VALUES 
-- Nhóm tài khoản Admin (Quản trị viên)
('admin', '123', N'Quản Trị Viên Hệ Thống', 'admin@northwind.com', 'Admin', N'Đang hoạt động'),
('locadmin', '123', N'Nguyễn Tấn Lộc (Admin)', 'tanloc@northwind.com', 'Admin', N'Đang hoạt động'),

-- Nhóm tài khoản User (Người dùng thường)
('user', '123', N'Người Dùng Mẫu', 'user@company.com', 'User', N'Đang hoạt động'),
('giahung', '123', N'Phan Trần Gia Hưng', 'giahung@company.com', 'User', N'Đang hoạt động'),
('vanan', '123', N'Trần Văn An', 'vanan@gmail.com', 'User', N'Đang hoạt động'),
('thuhuong', '123', N'Lê Thị Thu Hương', 'thuhuong@gmail.com', 'User', N'Đang hoạt động');
GO


SELECT * FROM NguoiDung;
GO