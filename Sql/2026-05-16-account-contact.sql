IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.TaiKhoan', N'SoDienThoai') IS NULL
    BEGIN
        ALTER TABLE dbo.TaiKhoan
        ADD SoDienThoai VARCHAR(15) NULL;
    END;

    IF COL_LENGTH(N'dbo.TaiKhoan', N'DiaChi') IS NULL
    BEGIN
        ALTER TABLE dbo.TaiKhoan
        ADD DiaChi NVARCHAR(500) NULL;
    END;
END;
