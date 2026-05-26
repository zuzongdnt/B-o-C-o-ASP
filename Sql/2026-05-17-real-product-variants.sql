SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.SanPhamBienThe', N'U') IS NULL
BEGIN
    THROW 50001, N'Bang dbo.SanPhamBienThe khong ton tai.', 1;
END;

IF OBJECT_ID(N'tempdb..#RealVariants') IS NOT NULL
    DROP TABLE #RealVariants;

IF OBJECT_ID(N'dbo.NguonAnhSanPham', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NguonAnhSanPham
    (
        TenFile NVARCHAR(255) NOT NULL PRIMARY KEY,
        TenSanPham NVARCHAR(255) NOT NULL,
        MauLienQuan NVARCHAR(120) NULL,
        LinkNguon NVARCHAR(1000) NOT NULL,
        NhaCungCap NVARCHAR(120) NOT NULL,
        NgayCapNhat DATETIME NOT NULL CONSTRAINT DF_NguonAnhSanPham_NgayCapNhat DEFAULT (GETDATE())
    );
END;

MERGE dbo.NguonAnhSanPham AS target
USING (VALUES
    (N'zz_apple_iphone17pro_official.png', N'iPhone 17 Pro / Pro Max', N'Xanh đậm, Bạc, Cam vũ trụ', N'https://www.apple.com/v/iphone-17-pro/f/images/meta/iphone-17-pro_overview__eumhhclcpuaa_og.png', N'Apple'),
    (N'zz_samsung_s26ultra_colors.jpg', N'Samsung Galaxy S26 Ultra', N'Đen, Trắng, Xanh bầu trời, Tím cobalt', N'https://images.samsung.com/is/image/samsung/assets/us/smartphones/galaxy-s26-ultra/images/galaxy-s26-ultra-features-colors-durability-b.jpg', N'Samsung'),
    (N'zz_xiaomi17ultra_official.png', N'Xiaomi 17 Ultra', N'Đen, Trắng, Bạc', N'https://i02.appmifile.com/mi-com-product/fly-birds/xiaomi-17-ultra/pc/3daf9b115eb928eb378d9a0039d43176.png', N'Xiaomi'),
    (N'zz_oppo_findx8pro_pearlwhite.png', N'OPPO Find X8 Pro', N'Trắng ngọc trai', N'https://www.oppo.com/content/dam/oppo/common/mkt/v2-2/find-x8-series-en/find-x8-pro/products/932-720.png', N'OPPO'),
    (N'zz_vivo_x200ultra_official.png', N'vivo X200 Ultra', N'Đen, Bạc, Đỏ', N'https://asia-exstatic-vivofs.vivo.com/PSee2l50xoirPK7y/1740478524253/a4dac6af710c541803b1ab533d221b4f.png', N'vivo'),
    (N'zz_honor_magic8pro_gold.png', N'HONOR Magic8 Pro', N'Vàng / ảnh đại diện chính thức', N'https://www-file.honor.com/content/dam/honor/common/product-list/product-series/honor-magic8-pro/magic8-pro-id-gold-back.png', N'HONOR')
) AS source (TenFile, TenSanPham, MauLienQuan, LinkNguon, NhaCungCap)
    ON target.TenFile = source.TenFile
WHEN MATCHED THEN
    UPDATE SET
        target.TenSanPham = source.TenSanPham,
        target.MauLienQuan = source.MauLienQuan,
        target.LinkNguon = source.LinkNguon,
        target.NhaCungCap = source.NhaCungCap,
        target.NgayCapNhat = GETDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (TenFile, TenSanPham, MauLienQuan, LinkNguon, NhaCungCap)
    VALUES (source.TenFile, source.TenSanPham, source.MauLienQuan, source.LinkNguon, source.NhaCungCap);

CREATE TABLE #RealVariants
(
    MaBienThe INT NOT NULL PRIMARY KEY,
    MaSP INT NOT NULL,
    DungLuong NVARCHAR(30) NOT NULL,
    MauSac NVARCHAR(80) NOT NULL,
    MaMau NVARCHAR(20) NULL,
    GiaBan DECIMAL(18, 0) NOT NULL,
    GiaGoc DECIMAL(18, 0) NULL,
    HinhAnh NVARCHAR(255) NULL,
    SoLuongTon INT NULL,
    LaMacDinh BIT NOT NULL,
    TrangThai BIT NOT NULL
);

INSERT INTO #RealVariants
    (MaBienThe, MaSP, DungLuong, MauSac, MaMau, GiaBan, GiaGoc, HinhAnh, SoLuongTon, LaMacDinh, TrangThai)
VALUES
-- Apple
(1, 1, N'512GB', N'Xanh đậm', N'#1f3a5f', 33000000, 34990000, N'zz_apple_iphone17pro_official.png', 20, 1, 1),
(2, 1, N'512GB', N'Bạc', N'#d1d5db', 33000000, 34990000, N'zz_apple_iphone17pro_official.png', 12, 0, 1),
(3, 1, N'512GB', N'Cam vũ trụ', N'#f97316', 33000000, 34990000, N'zz_apple_iphone17pro_official.png', 10, 0, 1),
(4, 1, N'256GB', N'Xanh đậm', N'#1f3a5f', 31000000, 32990000, N'zz_apple_iphone17pro_official.png', 15, 0, 1),
(5, 1, N'256GB', N'Bạc', N'#d1d5db', 31000000, 32990000, N'zz_apple_iphone17pro_official.png', 12, 0, 1),

(6, 2, N'256GB', N'Xanh đậm', N'#1f3a5f', 28990000, 31500000, N'zz_apple_iphone17pro_official.png', 15, 1, 1),
(7, 2, N'256GB', N'Bạc', N'#d1d5db', 28990000, 31500000, N'zz_apple_iphone17pro_official.png', 9, 0, 1),
(8, 2, N'256GB', N'Cam vũ trụ', N'#f97316', 28990000, 31500000, N'zz_apple_iphone17pro_official.png', 8, 0, 1),
(9, 2, N'512GB', N'Xanh đậm', N'#1f3a5f', 31490000, 34000000, N'zz_apple_iphone17pro_official.png', 10, 0, 1),

(10, 3, N'256GB', N'Đen', N'#1f2937', 22990000, 24000000, N'zz_apple_iphone17pro_official.png', 25, 1, 1),
(11, 3, N'256GB', N'Trắng', N'#f7f7f2', 22990000, 24000000, N'zz_apple_iphone17pro_official.png', 15, 0, 1),
(12, 3, N'256GB', N'Xanh lưu ly', N'#3f6f8f', 22990000, 24000000, N'zz_apple_iphone17pro_official.png', 13, 0, 1),
(13, 3, N'256GB', N'Hồng nhạt', N'#f6c8d1', 22990000, 24000000, N'zz_apple_iphone17pro_official.png', 11, 0, 1),
(14, 3, N'128GB', N'Đen', N'#1f2937', 21490000, 22500000, N'zz_apple_iphone17pro_official.png', 18, 0, 1),

(15, 4, N'256GB', N'Đen Titan', N'#1f2937', 26500000, NULL, N'ip16_promax_cu_den.jpg', 12, 1, 1),
(16, 4, N'256GB', N'Trắng Titan', N'#f4f4f0', 26500000, NULL, N'ip16_promax_cu.jpg', 7, 0, 1),
(17, 4, N'256GB', N'Titan tự nhiên', N'#c7b8a5', 26500000, NULL, N'ip16_promax_cu.jpg', 6, 0, 1),
(18, 4, N'256GB', N'Titan sa mạc', N'#c8a77d', 26500000, NULL, N'ip16_promax_cu.jpg', 5, 0, 1),
(19, 4, N'512GB', N'Đen Titan', N'#1f2937', 29000000, NULL, N'ip16_promax_cu_den.jpg', 5, 0, 1),

(20, 5, N'128GB', N'Đen', N'#1f2937', 18500000, NULL, N'ip16_cu.jpg', 10, 1, 1),
(21, 5, N'128GB', N'Trắng', N'#f7f7f2', 18500000, NULL, N'ip16_cu.jpg', 7, 0, 1),
(22, 5, N'128GB', N'Xanh lưu ly', N'#3b6f8f', 18500000, NULL, N'ip16_cu.jpg', 8, 0, 1),
(23, 5, N'128GB', N'Hồng', N'#f5a6bd', 18500000, NULL, N'ip16_cu.jpg', 6, 0, 1),

(24, 6, N'256GB', N'Đen Titan', N'#1f2937', 21500000, NULL, N'ip15_promax_cu.jpg', 8, 1, 1),
(25, 6, N'256GB', N'Trắng Titan', N'#f4f4f0', 21500000, NULL, N'ip15_promax_cu.jpg', 5, 0, 1),
(26, 6, N'256GB', N'Xanh Titan', N'#44586b', 21500000, NULL, N'ip15_promax_cu.jpg', 5, 0, 1),
(27, 6, N'256GB', N'Titan tự nhiên', N'#c7b8a5', 21500000, NULL, N'ip15_promax_cu.jpg', 5, 0, 1),

(28, 7, N'128GB', N'Đen', N'#1f2937', 14500000, NULL, N'ip15_cu.jpg', 14, 1, 1),
(29, 7, N'128GB', N'Xanh dương', N'#7aa7c7', 14500000, NULL, N'ip15_cu.jpg', 9, 0, 1),
(30, 7, N'128GB', N'Xanh lá', N'#a8c2a1', 14500000, NULL, N'ip15_cu.jpg', 7, 0, 1),
(31, 7, N'128GB', N'Hồng', N'#f6c8d1', 14500000, NULL, N'ip15_cu.jpg', 8, 0, 1),

(32, 8, N'128GB', N'Đen không gian', N'#1f2937', 15200000, NULL, N'ip14pro_98_den.jpg', 5, 1, 1),
(33, 8, N'128GB', N'Bạc', N'#d6d6d1', 15200000, NULL, N'ip14pro_98.jpg', 4, 0, 1),
(34, 8, N'128GB', N'Vàng', N'#d8bd75', 15200000, NULL, N'ip14pro_98.jpg', 3, 0, 1),
(35, 8, N'128GB', N'Tím đậm', N'#5f4b77', 15200000, NULL, N'ip14pro_98.jpg', 3, 0, 1),

(36, 9, N'128GB', N'Midnight', N'#1f2937', 9400000, NULL, N'iphone14_plus_den.jpg', 8, 1, 1),
(37, 9, N'128GB', N'Starlight', N'#f1eee7', 9400000, NULL, N'iphone14_plus.jpg', 5, 0, 1),
(38, 9, N'128GB', N'Xanh dương', N'#9fb7d8', 9400000, NULL, N'iphone14_plus.jpg', 5, 0, 1),
(39, 9, N'128GB', N'Tím', N'#c8b6d8', 9400000, NULL, N'iphone14_plus.jpg', 4, 0, 1),

(40, 10, N'128GB', N'Midnight', N'#1f2937', 9500000, NULL, N'ip13_98_den.jpg', 10, 1, 1),
(41, 10, N'128GB', N'Starlight', N'#f1eee7', 9500000, NULL, N'ip13_98.jpg', 7, 0, 1),
(42, 10, N'128GB', N'Xanh dương', N'#5f89b5', 9500000, NULL, N'ip13_98.jpg', 7, 0, 1),
(43, 10, N'128GB', N'Hồng', N'#f2c6cf', 9500000, NULL, N'ip13_98.jpg', 6, 0, 1),

(44, 11, N'64GB', N'Đen', N'#1f2937', 5200000, NULL, N'ip11_97_den.jpg', 30, 1, 1),
(45, 11, N'64GB', N'Trắng', N'#f7f7f2', 5200000, NULL, N'ip11_97.jpg', 18, 0, 1),
(46, 11, N'64GB', N'Xanh lá', N'#9bbfa3', 5200000, NULL, N'ip11_97.jpg', 15, 0, 1),
(47, 11, N'64GB', N'Tím', N'#c8b6d8', 5200000, NULL, N'ip11_97.jpg', 12, 0, 1),

-- Samsung
(48, 13, N'512GB', N'Đen', N'#1f2937', 31990000, 34990000, N'zz_samsung_s26ultra_colors.jpg', 10, 1, 1),
(49, 13, N'512GB', N'Trắng', N'#f7f7f2', 31990000, 34990000, N'zz_samsung_s26ultra_colors.jpg', 7, 0, 1),
(50, 13, N'512GB', N'Xanh bầu trời', N'#9fb7d8', 31990000, 34990000, N'zz_samsung_s26ultra_colors.jpg', 6, 0, 1),
(51, 13, N'512GB', N'Tím cobalt', N'#62517a', 31990000, 34990000, N'zz_samsung_s26ultra_colors.jpg', 5, 0, 1),
(52, 13, N'256GB', N'Đen', N'#1f2937', 30490000, 33490000, N'zz_samsung_s26ultra_colors.jpg', 8, 0, 1),

(53, 14, N'256GB', N'Đen Titan', N'#1f2937', 23500000, NULL, N's25_ultra_cu_den.jpg', 8, 1, 1),
(54, 14, N'256GB', N'Xám Titan', N'#8d918f', 23500000, NULL, N's25_ultra_cu.jpg', 6, 0, 1),
(55, 14, N'256GB', N'Bạc Titan', N'#d5d7d2', 23500000, NULL, N's25_ultra_cu.jpg', 5, 0, 1),

(56, 15, N'256GB', N'Đen Titan', N'#1f2937', 18500000, NULL, N's24_ultra_98_den.jpg', 15, 1, 1),
(57, 15, N'256GB', N'Xám Titan', N'#8d918f', 18500000, NULL, N's24_ultra_98.jpg', 8, 0, 1),
(58, 15, N'256GB', N'Tím Titan', N'#7b6a86', 18500000, NULL, N's24_ultra_98.jpg', 6, 0, 1),

(59, 16, N'128GB', N'Đen Phantom', N'#1f2937', 6850000, NULL, N's23_cu.jpg', 10, 1, 1),
(60, 16, N'128GB', N'Kem', N'#eee7d6', 6850000, NULL, N's23_cu.jpg', 7, 0, 1),
(61, 16, N'128GB', N'Xanh lá', N'#5d7b67', 6850000, NULL, N's23_cu.jpg', 5, 0, 1),

(62, 17, N'128GB', N'Xanh navy', N'#1f2937', 8990000, 9990000, N'samsung_a55.jpg', 20, 1, 1),
(63, 17, N'128GB', N'Xanh băng', N'#a9c5dc', 8990000, 9990000, N'samsung_a55.jpg', 12, 0, 1),
(64, 17, N'128GB', N'Tím lilac', N'#c8b5d7', 8990000, 9990000, N'samsung_a55.jpg', 10, 0, 1),

(65, 18, N'128GB', N'Đen Phantom', N'#1f2937', 7500000, NULL, N's21_ultra_97.jpg', 12, 1, 1),
(66, 18, N'128GB', N'Bạc Phantom', N'#d6d8d6', 7500000, NULL, N's21_ultra_97.jpg', 7, 0, 1),

-- Xiaomi / Redmi
(67, 19, N'512GB', N'Đen', N'#1f2937', 28500000, NULL, N'zz_xiaomi17ultra_official.png', 10, 1, 1),
(68, 19, N'512GB', N'Trắng', N'#f7f7f2', 28500000, NULL, N'zz_xiaomi17ultra_official.png', 6, 0, 1),
(69, 19, N'512GB', N'Bạc', N'#d1d5db', 28500000, NULL, N'zz_xiaomi17ultra_official.png', 5, 0, 1),
(70, 19, N'256GB', N'Đen', N'#1f2937', 27000000, NULL, N'zz_xiaomi17ultra_official.png', 8, 0, 1),

(71, 20, N'256GB', N'Đen', N'#1f2937', 16500000, NULL, N'xiaomi_16pro_cu.jpg', 12, 1, 1),
(72, 20, N'256GB', N'Trắng', N'#f7f7f2', 16500000, NULL, N'xiaomi_16pro_cu.jpg', 8, 0, 1),
(73, 20, N'256GB', N'Xanh lá', N'#6f8b77', 16500000, NULL, N'xiaomi_16pro_cu.jpg', 6, 0, 1),

(74, 21, N'256GB', N'Đen', N'#1f2937', 15250000, 16990000, N'xiaomi15.jpg', 18, 1, 1),
(75, 21, N'256GB', N'Trắng', N'#f7f7f2', 15250000, 16990000, N'xiaomi15.jpg', 12, 0, 1),
(76, 21, N'256GB', N'Xanh lá', N'#7c9c8b', 15250000, 16990000, N'xiaomi15.jpg', 8, 0, 1),
(77, 21, N'512GB', N'Đen', N'#1f2937', 17750000, 19490000, N'xiaomi15.jpg', 10, 0, 1),

(78, 22, N'256GB', N'Đen', N'#1f2937', 13500000, NULL, N'xiaomi_15_cu.jpg', 15, 1, 1),
(79, 22, N'256GB', N'Trắng', N'#f7f7f2', 13500000, NULL, N'xiaomi_15_cu.jpg', 8, 0, 1),
(80, 22, N'256GB', N'Xanh lá', N'#7c9c8b', 13500000, NULL, N'xiaomi_15_cu.jpg', 6, 0, 1),

(81, 23, N'512GB', N'Đen', N'#1f2937', 14500000, NULL, N'xiaomi14t_pro.jpg', 9, 1, 1),
(82, 23, N'512GB', N'Xám Titan', N'#8d918f', 14500000, NULL, N'xiaomi14t_pro.jpg', 6, 0, 1),
(83, 23, N'512GB', N'Xanh đậm', N'#536979', 14500000, NULL, N'xiaomi14t_pro.jpg', 5, 0, 1),

(84, 24, N'256GB', N'Đen', N'#1f2937', 6950000, 7950000, N'redmi_k80.jpg', 20, 1, 1),
(85, 24, N'256GB', N'Trắng', N'#f7f7f2', 6950000, 7950000, N'redmi_k80.jpg', 12, 0, 1),
(86, 24, N'256GB', N'Xanh', N'#5d879f', 6950000, 7950000, N'redmi_k80.jpg', 9, 0, 1),
(87, 24, N'512GB', N'Đen', N'#1f2937', 9450000, 10450000, N'redmi_k80.jpg', 10, 0, 1),

(88, 25, N'256GB', N'Đen', N'#1f2937', 6500000, NULL, N'redmi_note14pro.jpg', 25, 1, 1),
(89, 25, N'256GB', N'Tím', N'#8f7aa7', 6500000, NULL, N'redmi_note14pro.jpg', 12, 0, 1),
(90, 25, N'256GB', N'Xanh lá', N'#7c9c8b', 6500000, NULL, N'redmi_note14pro.jpg', 10, 0, 1),

(91, 26, N'128GB', N'Đen', N'#1f2937', 4500000, NULL, N'xiaomi_12_98.jpg', 8, 1, 1),
(92, 26, N'128GB', N'Xanh dương', N'#6f9fbd', 4500000, NULL, N'xiaomi_12_98.jpg', 5, 0, 1),
(93, 26, N'128GB', N'Tím', N'#b3a1c7', 4500000, NULL, N'xiaomi_12_98.jpg', 4, 0, 1),

-- Vivo
(94, 27, N'512GB', N'Đen', N'#1f2937', 24500000, NULL, N'vivo_x300pro.jpg', 10, 1, 1),
(95, 27, N'512GB', N'Trắng', N'#f7f7f2', 24500000, NULL, N'vivo_x300pro.jpg', 6, 0, 1),
(96, 27, N'512GB', N'Xanh dương', N'#5f89b5', 24500000, NULL, N'vivo_x300pro.jpg', 5, 0, 1),
(97, 27, N'256GB', N'Đen', N'#1f2937', 23000000, NULL, N'vivo_x300pro.jpg', 8, 0, 1),

(98, 28, N'512GB', N'Đen', N'#1f2937', 24150000, 29500000, N'zz_vivo_x200ultra_official.png', 4, 1, 1),
(99, 28, N'512GB', N'Bạc', N'#d1d5db', 24150000, 29500000, N'zz_vivo_x200ultra_official.png', 3, 0, 1),
(100, 28, N'512GB', N'Đỏ', N'#9f1d2d', 24150000, 29500000, N'zz_vivo_x200ultra_official.png', 2, 0, 1),

(101, 29, N'512GB', N'Đen', N'#1f2937', 24150000, 29500000, N'zz_vivo_x200ultra_official.png', 5, 1, 1),
(102, 29, N'512GB', N'Bạc', N'#d1d5db', 24150000, 29500000, N'zz_vivo_x200ultra_official.png', 3, 0, 1),
(103, 29, N'512GB', N'Đỏ', N'#9f1d2d', 24150000, 29500000, N'zz_vivo_x200ultra_official.png', 2, 0, 1),

(104, 30, N'256GB', N'Đen', N'#1f2937', 11450000, NULL, N'vivox100s.jpg', 12, 1, 1),
(105, 30, N'256GB', N'Trắng', N'#f7f7f2', 11450000, NULL, N'vivox100s.jpg', 8, 0, 1),
(106, 30, N'256GB', N'Xanh', N'#5f89b5', 11450000, NULL, N'vivox100s.jpg', 6, 0, 1),

(107, 31, N'256GB', N'Đen', N'#1f2937', 12500000, NULL, N'vivo_x100_cu.jpg', 8, 1, 1),
(108, 31, N'256GB', N'Trắng', N'#f7f7f2', 12500000, NULL, N'vivo_x100_cu.jpg', 5, 0, 1),
(109, 31, N'256GB', N'Xanh', N'#5f89b5', 12500000, NULL, N'vivo_x100_cu.jpg', 4, 0, 1),

(110, 32, N'256GB', N'Đen', N'#1f2937', 9500000, 10990000, N'vivo_v30.jpg', 15, 1, 1),
(111, 32, N'256GB', N'Trắng', N'#f7f7f2', 9500000, 10990000, N'vivo_v30.jpg', 8, 0, 1),
(112, 32, N'256GB', N'Xanh', N'#6f9fbd', 9500000, 10990000, N'vivo_v30.jpg', 7, 0, 1),

(113, 33, N'256GB', N'Đen', N'#1f2937', 8500000, NULL, N'vivo_x80pro_98.jpg', 6, 1, 1),
(114, 33, N'256GB', N'Cam', N'#c56d43', 8500000, NULL, N'vivo_x80pro_98.jpg', 3, 0, 1),
(115, 33, N'256GB', N'Xanh', N'#5f89b5', 8500000, NULL, N'vivo_x80pro_98.jpg', 3, 0, 1),

-- Oppo
(116, 34, N'512GB', N'Đen không gian', N'#1f2937', 26990000, NULL, N'zz_oppo_findx8pro_pearlwhite.png', 8, 1, 1),
(117, 34, N'512GB', N'Trắng ngọc trai', N'#f7f7f2', 26990000, NULL, N'zz_oppo_findx8pro_pearlwhite.png', 5, 0, 1),
(118, 34, N'512GB', N'Xanh đại dương', N'#4f7186', 26990000, NULL, N'zz_oppo_findx8pro_pearlwhite.png', 4, 0, 1),
(119, 34, N'256GB', N'Đen không gian', N'#1f2937', 25490000, NULL, N'zz_oppo_findx8pro_pearlwhite.png', 6, 0, 1),

(120, 35, N'256GB', N'Đen', N'#1f2937', 17500000, NULL, N'oppo_findx7ultra_cu.jpg', 10, 1, 1),
(121, 35, N'256GB', N'Nâu da', N'#9a6b47', 17500000, NULL, N'oppo_findx7ultra_cu.jpg', 5, 0, 1),
(122, 35, N'256GB', N'Xanh đại dương', N'#4f7186', 17500000, NULL, N'oppo_findx7ultra_cu.jpg', 4, 0, 1),

(123, 36, N'256GB', N'Đen', N'#1f2937', 11500000, 13000000, N'oppo_reno12pro.jpg', 18, 1, 1),
(124, 36, N'256GB', N'Bạc', N'#d1d5db', 11500000, 13000000, N'oppo_reno12pro.jpg', 10, 0, 1),
(125, 36, N'256GB', N'Tím', N'#b3a1c7', 11500000, 13000000, N'oppo_reno12pro.jpg', 9, 0, 1),

(126, 37, N'256GB', N'Đen', N'#1f2937', 6500000, NULL, N'oppo_reno10_cu.jpg', 12, 1, 1),
(127, 37, N'256GB', N'Xám bạc', N'#bfc5c6', 6500000, NULL, N'oppo_reno10_cu.jpg', 7, 0, 1),
(128, 37, N'256GB', N'Xanh', N'#5f89b5', 6500000, NULL, N'oppo_reno10_cu.jpg', 6, 0, 1),

(129, 38, N'128GB', N'Đen', N'#1f2937', 3500000, NULL, N'oppo_reno8z_97.jpg', 20, 1, 1),
(130, 38, N'128GB', N'Vàng bình minh', N'#d8bd75', 3500000, NULL, N'oppo_reno8z_97.jpg', 9, 0, 1),

-- Honor / OnePlus / RedMagic
(131, 39, N'512GB', N'Đen', N'#1f2937', 25500000, NULL, N'zz_honor_magic8pro_gold.png', 7, 1, 1),
(132, 39, N'512GB', N'Trắng', N'#f7f7f2', 25500000, NULL, N'zz_honor_magic8pro_gold.png', 4, 0, 1),
(133, 39, N'512GB', N'Xanh lá', N'#6f8b77', 25500000, NULL, N'zz_honor_magic8pro_gold.png', 3, 0, 1),
(134, 39, N'256GB', N'Đen', N'#1f2937', 24000000, NULL, N'zz_honor_magic8pro_gold.png', 5, 0, 1),

(135, 40, N'256GB', N'Đen', N'#1f2937', 16500000, NULL, N'honor_magic7_cu.jpg', 9, 1, 1),
(136, 40, N'256GB', N'Trắng', N'#f7f7f2', 16500000, NULL, N'honor_magic7_cu.jpg', 5, 0, 1),
(137, 40, N'256GB', N'Xanh lá', N'#6f8b77', 16500000, NULL, N'honor_magic7_cu.jpg', 4, 0, 1),

(138, 41, N'512GB', N'Đen', N'#1f2937', 12990000, NULL, N'honor_200pro.jpg', 15, 1, 1),
(139, 41, N'512GB', N'Trắng ánh trăng', N'#f7f7f2', 12990000, NULL, N'honor_200pro.jpg', 8, 0, 1),
(140, 41, N'512GB', N'Xanh biển', N'#5f89b5', 12990000, NULL, N'honor_200pro.jpg', 7, 0, 1),

(141, 42, N'256GB', N'Đen', N'#1f2937', 6500000, NULL, N'honor_90_cu.jpg', 10, 1, 1),
(142, 42, N'256GB', N'Bạc kim cương', N'#d1d5db', 6500000, NULL, N'honor_90_cu.jpg', 6, 0, 1),
(143, 42, N'256GB', N'Xanh lục bảo', N'#3f7f68', 6500000, NULL, N'honor_90_cu.jpg', 5, 0, 1),

(144, 43, N'128GB', N'Đen', N'#1f2937', 3200000, NULL, N'honor_50_98.jpg', 15, 1, 1),
(145, 43, N'128GB', N'Bạc', N'#d1d5db', 3200000, NULL, N'honor_50_98.jpg', 8, 0, 1),
(146, 43, N'128GB', N'Xanh ngọc', N'#7ab8aa', 3200000, NULL, N'honor_50_98.jpg', 7, 0, 1),

(147, 44, N'256GB', N'Đen', N'#1f2937', 6650000, NULL, N'oneplus_ace_5.jpg', 15, 1, 1),
(148, 44, N'256GB', N'Bạc', N'#d1d5db', 6650000, NULL, N'oneplus_ace_5.jpg', 8, 0, 1),
(149, 44, N'256GB', N'Xanh lá', N'#6f8b77', 6650000, NULL, N'oneplus_ace_5.jpg', 7, 0, 1),

(150, 45, N'512GB', N'Đen', N'#1f2937', 13000000, 15500000, N'redmagic_9pro.jpg', 5, 1, 1),
(151, 45, N'512GB', N'Bạc', N'#d1d5db', 13000000, 15500000, N'redmagic_9pro.jpg', 3, 0, 1),
(152, 45, N'512GB', N'Trong suốt', N'#e5e7eb', 13000000, 15500000, N'redmagic_9pro.jpg', 2, 0, 1),
(153, 45, N'256GB', N'Đen', N'#1f2937', 11500000, 14000000, N'redmagic_9pro.jpg', 4, 0, 1);

UPDATE dbo.SanPhamBienThe
SET TrangThai = 0,
    LaMacDinh = 0
WHERE MaBienThe NOT IN (SELECT MaBienThe FROM #RealVariants);

SET IDENTITY_INSERT dbo.SanPhamBienThe ON;

MERGE dbo.SanPhamBienThe AS target
USING #RealVariants AS source
    ON target.MaBienThe = source.MaBienThe
WHEN MATCHED THEN
    UPDATE SET
        target.MaSP = source.MaSP,
        target.DungLuong = source.DungLuong,
        target.MauSac = source.MauSac,
        target.MaMau = source.MaMau,
        target.GiaBan = source.GiaBan,
        target.GiaGoc = source.GiaGoc,
        target.HinhAnh = source.HinhAnh,
        target.SoLuongTon = source.SoLuongTon,
        target.LaMacDinh = source.LaMacDinh,
        target.TrangThai = source.TrangThai
WHEN NOT MATCHED BY TARGET THEN
    INSERT (MaBienThe, MaSP, DungLuong, MauSac, MaMau, GiaBan, GiaGoc, HinhAnh, SoLuongTon, LaMacDinh, TrangThai)
    VALUES (source.MaBienThe, source.MaSP, source.DungLuong, source.MauSac, source.MaMau, source.GiaBan, source.GiaGoc, source.HinhAnh, source.SoLuongTon, source.LaMacDinh, source.TrangThai);

SET IDENTITY_INSERT dbo.SanPhamBienThe OFF;

;WITH DuplicatedDefaults AS
(
    SELECT MaSP, MaBienThe,
           ROW_NUMBER() OVER (PARTITION BY MaSP ORDER BY LaMacDinh DESC, MaBienThe ASC) AS rn
    FROM dbo.SanPhamBienThe
    WHERE TrangThai = 1
)
UPDATE v
SET LaMacDinh = CASE WHEN d.rn = 1 THEN 1 ELSE 0 END
FROM dbo.SanPhamBienThe v
INNER JOIN DuplicatedDefaults d ON d.MaBienThe = v.MaBienThe;

SELECT MaSP, COUNT(*) AS SoBienTheDangBan
FROM dbo.SanPhamBienThe
WHERE TrangThai = 1
GROUP BY MaSP
ORDER BY MaSP;
