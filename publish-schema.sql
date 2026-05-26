CREATE TABLE [TaiKhoan] (
    [MaTaiKhoan] int NOT NULL IDENTITY,
    [TenDangNhap] varchar(50) NOT NULL,
    [MatKhauHash] varchar(255) NOT NULL,
    [HoTen] nvarchar(100) NULL,
    [Email] varchar(100) NULL,
    [SoDienThoai] varchar(15) NULL,
    [DiaChi] nvarchar(500) NULL,
    [VaiTro] nvarchar(30) NULL,
    [TrangThai] bit NULL DEFAULT CAST(1 AS bit),
    [Avatar] nvarchar(max) NULL,
    [NgayTao] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__TaiKhoan__AD7C6529473A4591] PRIMARY KEY ([MaTaiKhoan])
);
GO


CREATE TABLE [ThuongHieu] (
    [MaThuongHieu] int NOT NULL IDENTITY,
    [TenThuongHieu] nvarchar(100) NOT NULL,
    [MoTa] nvarchar(255) NULL,
    CONSTRAINT [PK__ThuongHi__A3733E2C9A9C821E] PRIMARY KEY ([MaThuongHieu])
);
GO


CREATE TABLE [TuCam_DanhGia] (
    [MaTu] int NOT NULL IDENTITY,
    [TuKhoa] nvarchar(100) NOT NULL,
    [GhiChu] nvarchar(255) NULL,
    CONSTRAINT [PK__TuCam_Da__2725005AD91E46FD] PRIMARY KEY ([MaTu])
);
GO


CREATE TABLE [DonHang] (
    [MaDonHang] int NOT NULL IDENTITY,
    [MaTaiKhoan] int NULL,
    [TenNguoiNhan] nvarchar(100) NOT NULL,
    [SoDienThoai] varchar(15) NOT NULL,
    [DiaChiGiaoHang] nvarchar(500) NOT NULL,
    [TongTien] decimal(18,0) NOT NULL,
    [PhuongThucThanhToan] nvarchar(50) NULL,
    [TrangThaiDonHang] nvarchar(50) NULL DEFAULT N'Chờ xác nhận',
    [LyDoHuy] nvarchar(500) NULL,
    [NgayDatHang] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__DonHang__129584AD591FEB27] PRIMARY KEY ([MaDonHang]),
    CONSTRAINT [FK__DonHang__MaTaiKh__72C60C4A] FOREIGN KEY ([MaTaiKhoan]) REFERENCES [TaiKhoan] ([MaTaiKhoan])
);
GO


CREATE TABLE [GioHang] (
    [MaGioHang] int NOT NULL IDENTITY,
    [MaTaiKhoan] int NULL,
    [NgayTao] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__GioHang__F5001DA3FDB9169B] PRIMARY KEY ([MaGioHang]),
    CONSTRAINT [FK__GioHang__MaTaiKh__6A30C649] FOREIGN KEY ([MaTaiKhoan]) REFERENCES [TaiKhoan] ([MaTaiKhoan])
);
GO


CREATE TABLE [SanPham] (
    [MaSP] int NOT NULL IDENTITY,
    [MaThuongHieu] int NULL,
    [TenSP] nvarchar(255) NOT NULL,
    [GiaBan] decimal(18,0) NOT NULL,
    [HinhAnh] varchar(255) NULL,
    [Chipset] nvarchar(100) NULL,
    [RAM] nvarchar(50) NULL,
    [ROM] nvarchar(50) NULL,
    [TinhTrang] nvarchar(50) NULL,
    [NguonGoc] nvarchar(50) NULL,
    [SoLuongTon] int NULL DEFAULT 0,
    [TrangThai] bit NULL DEFAULT CAST(1 AS bit),
    [NgayThem] datetime NULL DEFAULT ((getdate())),
    [Camera] nvarchar(255) NULL,
    [GiaGoc] decimal(18,0) NULL,
    [Pin] nvarchar(100) NULL,
    CONSTRAINT [PK__SanPham__2725081C8AD49326] PRIMARY KEY ([MaSP]),
    CONSTRAINT [FK__SanPham__MaThuon__6477ECF3] FOREIGN KEY ([MaThuongHieu]) REFERENCES [ThuongHieu] ([MaThuongHieu])
);
GO


CREATE TABLE [DanhGia] (
    [MaDanhGia] int NOT NULL IDENTITY,
    [PhanHoi] nvarchar(max) NULL,
    [MaSP] int NULL,
    [MaTaiKhoan] int NULL,
    [SoSao] int NULL,
    [NoiDung] nvarchar(max) NULL,
    [HinhAnhDG] varchar(255) NULL,
    [TrangThaiHienThi] bit NULL DEFAULT CAST(1 AS bit),
    [NgayDanhGia] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__DanhGia__AA9515BFAC0A8F8B] PRIMARY KEY ([MaDanhGia]),
    CONSTRAINT [FK__DanhGia__MaSP__7B5B524B] FOREIGN KEY ([MaSP]) REFERENCES [SanPham] ([MaSP]),
    CONSTRAINT [FK__DanhGia__MaTaiKh__7C4F7684] FOREIGN KEY ([MaTaiKhoan]) REFERENCES [TaiKhoan] ([MaTaiKhoan])
);
GO


CREATE TABLE [HinhAnhSanPham] (
    [MaHinhAnh] int NOT NULL IDENTITY,
    [MaSp] int NOT NULL,
    [DuongDanAnh] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_HinhAnhSanPham] PRIMARY KEY ([MaHinhAnh]),
    CONSTRAINT [FK_HinhAnhSanPham_SanPham_MaSp] FOREIGN KEY ([MaSp]) REFERENCES [SanPham] ([MaSP]) ON DELETE CASCADE
);
GO


CREATE TABLE [SanPhamBienThe] (
    [MaBienThe] int NOT NULL IDENTITY,
    [MaSP] int NOT NULL,
    [DungLuong] nvarchar(30) NOT NULL,
    [MauSac] nvarchar(80) NOT NULL,
    [MaMau] nvarchar(20) NULL,
    [GiaBan] decimal(18,0) NOT NULL,
    [GiaGoc] decimal(18,0) NULL,
    [HinhAnh] nvarchar(255) NULL,
    [SoLuongTon] int NULL,
    [LaMacDinh] bit NOT NULL,
    [TrangThai] bit NOT NULL,
    CONSTRAINT [PK_SanPhamBienThe] PRIMARY KEY ([MaBienThe]),
    CONSTRAINT [FK_SanPhamBienThe_SanPham] FOREIGN KEY ([MaSP]) REFERENCES [SanPham] ([MaSP])
);
GO


CREATE TABLE [DanhGiaPhanHoi] (
    [MaPhanHoi] int NOT NULL IDENTITY,
    [MaDanhGia] int NOT NULL,
    [MaTaiKhoan] int NOT NULL,
    [NoiDung] nvarchar(1000) NOT NULL,
    [NgayPhanHoi] datetime NOT NULL DEFAULT ((getdate())),
    [TrangThai] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_DanhGiaPhanHoi] PRIMARY KEY ([MaPhanHoi]),
    CONSTRAINT [FK_DanhGiaPhanHoi_DanhGia] FOREIGN KEY ([MaDanhGia]) REFERENCES [DanhGia] ([MaDanhGia]),
    CONSTRAINT [FK_DanhGiaPhanHoi_TaiKhoan] FOREIGN KEY ([MaTaiKhoan]) REFERENCES [TaiKhoan] ([MaTaiKhoan])
);
GO


CREATE TABLE [ChiTietDonHang] (
    [MaChiTietDonHang] int NOT NULL IDENTITY,
    [MaDonHang] int NOT NULL,
    [MaSP] int NOT NULL,
    [MaBienThe] int NULL,
    [SoLuong] int NOT NULL,
    [DonGia] decimal(18,0) NOT NULL,
    CONSTRAINT [PK_ChiTietDonHang] PRIMARY KEY ([MaChiTietDonHang]),
    CONSTRAINT [FK_ChiTietDonHang_SanPhamBienThe] FOREIGN KEY ([MaBienThe]) REFERENCES [SanPhamBienThe] ([MaBienThe]),
    CONSTRAINT [FK__ChiTietDo__MaDon__778AC167] FOREIGN KEY ([MaDonHang]) REFERENCES [DonHang] ([MaDonHang]),
    CONSTRAINT [FK__ChiTietDon__MaSP__787EE5A0] FOREIGN KEY ([MaSP]) REFERENCES [SanPham] ([MaSP])
);
GO


CREATE TABLE [ChiTietGioHang] (
    [MaChiTietGioHang] int NOT NULL IDENTITY,
    [MaGioHang] int NOT NULL,
    [MaSP] int NOT NULL,
    [MaBienThe] int NULL,
    [SoLuong] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK_ChiTietGioHang] PRIMARY KEY ([MaChiTietGioHang]),
    CONSTRAINT [FK_ChiTietGioHang_SanPhamBienThe] FOREIGN KEY ([MaBienThe]) REFERENCES [SanPhamBienThe] ([MaBienThe]),
    CONSTRAINT [FK__ChiTietGi__MaGio__6E01572D] FOREIGN KEY ([MaGioHang]) REFERENCES [GioHang] ([MaGioHang]),
    CONSTRAINT [FK__ChiTietGio__MaSP__6EF57B66] FOREIGN KEY ([MaSP]) REFERENCES [SanPham] ([MaSP])
);
GO


CREATE INDEX [IX_ChiTietDonHang_MaBienThe] ON [ChiTietDonHang] ([MaBienThe]);
GO


CREATE INDEX [IX_ChiTietDonHang_MaDonHang] ON [ChiTietDonHang] ([MaDonHang]);
GO


CREATE INDEX [IX_ChiTietDonHang_MaSP] ON [ChiTietDonHang] ([MaSP]);
GO


CREATE INDEX [IX_ChiTietGioHang_MaBienThe] ON [ChiTietGioHang] ([MaBienThe]);
GO


CREATE INDEX [IX_ChiTietGioHang_MaSP] ON [ChiTietGioHang] ([MaSP]);
GO


CREATE UNIQUE INDEX [UX_ChiTietGioHang_GioHang_SanPham_BienThe] ON [ChiTietGioHang] ([MaGioHang], [MaSP], [MaBienThe]) WHERE [MaBienThe] IS NOT NULL;
GO


CREATE INDEX [IX_DanhGia_MaSP] ON [DanhGia] ([MaSP]);
GO


CREATE INDEX [IX_DanhGia_MaTaiKhoan] ON [DanhGia] ([MaTaiKhoan]);
GO


CREATE INDEX [IX_DanhGiaPhanHoi_MaDanhGia] ON [DanhGiaPhanHoi] ([MaDanhGia]);
GO


CREATE INDEX [IX_DanhGiaPhanHoi_MaTaiKhoan] ON [DanhGiaPhanHoi] ([MaTaiKhoan]);
GO


CREATE INDEX [IX_DonHang_MaTaiKhoan] ON [DonHang] ([MaTaiKhoan]);
GO


CREATE INDEX [IX_GioHang_MaTaiKhoan] ON [GioHang] ([MaTaiKhoan]);
GO


CREATE INDEX [IX_HinhAnhSanPham_MaSp] ON [HinhAnhSanPham] ([MaSp]);
GO


CREATE INDEX [IX_SanPham_MaThuongHieu] ON [SanPham] ([MaThuongHieu]);
GO


CREATE INDEX [IX_SanPhamBienThe_MaSP] ON [SanPhamBienThe] ([MaSP]);
GO


CREATE UNIQUE INDEX [UQ__TaiKhoan__55F68FC0FD5255D4] ON [TaiKhoan] ([TenDangNhap]);
GO


CREATE UNIQUE INDEX [UQ__ThuongHi__98D6A83459B998F2] ON [ThuongHieu] ([TenThuongHieu]);
GO


CREATE UNIQUE INDEX [UQ__TuCam_Da__2E7DF67585A6073D] ON [TuCam_DanhGia] ([TuKhoa]);
GO


