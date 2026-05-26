IF OBJECT_ID(N'dbo.ChiTietDonHang', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.ChiTietDonHang', N'MaChiTietDonHang') IS NULL
    BEGIN
        DECLARE @pkName sysname;

        SELECT @pkName = kc.name
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID(N'dbo.ChiTietDonHang')
          AND kc.[type] = N'PK';

        IF @pkName IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.ChiTietDonHang DROP CONSTRAINT [' + @pkName + N']');
        END;

        ALTER TABLE dbo.ChiTietDonHang
        ADD MaChiTietDonHang INT IDENTITY(1,1) NOT NULL;

        ALTER TABLE dbo.ChiTietDonHang
        ADD CONSTRAINT PK_ChiTietDonHang PRIMARY KEY (MaChiTietDonHang);
    END;

    IF COL_LENGTH(N'dbo.ChiTietDonHang', N'MaBienThe') IS NULL
    BEGIN
        ALTER TABLE dbo.ChiTietDonHang
        ADD MaBienThe INT NULL;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_ChiTietDonHang_SanPhamBienThe'
          AND parent_object_id = OBJECT_ID(N'dbo.ChiTietDonHang')
    )
    BEGIN
        EXEC(N'ALTER TABLE dbo.ChiTietDonHang
        ADD CONSTRAINT FK_ChiTietDonHang_SanPhamBienThe
        FOREIGN KEY (MaBienThe) REFERENCES dbo.SanPhamBienThe(MaBienThe)');
    END;
END;

IF OBJECT_ID(N'dbo.DonHang', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.DonHang', N'LyDoHuy') IS NULL
    BEGIN
        ALTER TABLE dbo.DonHang
        ADD LyDoHuy NVARCHAR(500) NULL;
    END;
END;
