IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QL_NguoiDung')
BEGIN
    CREATE DATABASE QL_NguoiDung;
END
GO

USE QL_NguoiDung;
GO

-- 1. BẢNG NGƯỜI DÙNG (Users)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NguoiDung')
BEGIN
    CREATE TABLE NguoiDung (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
        MatKhau VARCHAR(255) NOT NULL, -- Lưu chuỗi hash Argon2id định dạng PHC (~90-120 ký tự)
        HoTen NVARCHAR(100) NOT NULL,
        Email VARCHAR(100),
        Quyen VARCHAR(20) NOT NULL, -- 'Admin' hoặc 'User'
        TrangThai NVARCHAR(30) DEFAULT N'Đang hoạt động'
    );
END
ELSE
BEGIN
    -- Đảm bảo độ dài cột MatKhau đủ chứa chuỗi Argon2id
    ALTER TABLE NguoiDung ALTER COLUMN MatKhau VARCHAR(255) NOT NULL;
END
GO

-- 2. BẢNG REFRESH TOKEN (Quản lý phiên làm việc & Thu hồi Token)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshToken')
BEGIN
    CREATE TABLE RefreshToken (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL FOREIGN KEY REFERENCES NguoiDung(Id) ON DELETE CASCADE,
        TokenHash VARCHAR(64) NOT NULL, -- Lưu mã băm SHA-256 của Refresh Token
        ExpiryDate DATETIME NOT NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

DELETE FROM RefreshToken;
DELETE FROM NguoiDung;
GO

-- Mật khẩu mặc định của tất cả tài khoản mẫu là '123'
-- Đã được băm bằng thuật toán Argon2id (Memory: 64MB, Iterations: 3, Parallelism: 2)
-- Chuỗi PHC: $argon2id$v=19$m=65536,t=3,p=2$U0BsdF8yMDI2X1NlY3VyMyE=$FCSXORh4NmZxWBHgYkwEFfDAaTSWbqt+ga/ahTEo9pE=
INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, Email, Quyen, TrangThai) 
VALUES 
-- Nhóm tài khoản Admin (Quản trị viên)
('admin', '$argon2id$v=19$m=65536,t=3,p=2$U0BsdF8yMDI2X1NlY3VyMyE=$FCSXORh4NmZxWBHgYkwEFfDAaTSWbqt+ga/ahTEo9pE=', N'Quản Trị Viên Hệ Thống', 'admin@northwind.com', 'Admin', N'Đang hoạt động'),
('locadmin', '$argon2id$v=19$m=65536,t=3,p=2$U0BsdF8yMDI2X1NlY3VyMyE=$FCSXORh4NmZxWBHgYkwEFfDAaTSWbqt+ga/ahTEo9pE=', N'Nguyễn Tấn Lộc (Admin)', 'tanloc@northwind.com', 'Admin', N'Đang hoạt động'),

-- Nhóm tài khoản User (Người dùng thường)
('user', '$argon2id$v=19$m=65536,t=3,p=2$U0BsdF8yMDI2X1NlY3VyMyE=$FCSXORh4NmZxWBHgYkwEFfDAaTSWbqt+ga/ahTEo9pE=', N'Người Dùng Mẫu', 'user@company.com', 'User', N'Đang hoạt động'),
('giahung', '$argon2id$v=19$m=65536,t=3,p=2$U0BsdF8yMDI2X1NlY3VyMyE=$FCSXORh4NmZxWBHgYkwEFfDAaTSWbqt+ga/ahTEo9pE=', N'Phan Trần Gia Hưng', 'giahung@company.com', 'User', N'Đang hoạt động'),
('vanan', '$argon2id$v=19$m=65536,t=3,p=2$U0BsdF8yMDI2X1NlY3VyMyE=$FCSXORh4NmZxWBHgYkwEFfDAaTSWbqt+ga/ahTEo9pE=', N'Trần Văn An', 'vanan@gmail.com', 'User', N'Đang hoạt động'),
('thuhuong', '$argon2id$v=19$m=65536,t=3,p=2$U0BsdF8yMDI2X1NlY3VyMyE=$FCSXORh4NmZxWBHgYkwEFfDAaTSWbqt+ga/ahTEo9pE=', N'Lê Thị Thu Hương', 'thuhuong@gmail.com', 'User', N'Đang hoạt động');
GO

SELECT * FROM NguoiDung;
SELECT * FROM RefreshToken;
GO
