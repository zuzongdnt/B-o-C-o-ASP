SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.ChiTietGioHang', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.ChiTietGioHang', N'MaChiTietGioHang') IS NULL
    BEGIN
        DECLARE @pkName sysname;

        SELECT @pkName = kc.name
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID(N'dbo.ChiTietGioHang')
          AND kc.[type] = N'PK';

        IF @pkName IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.ChiTietGioHang DROP CONSTRAINT [' + @pkName + N']');
        END;

        ALTER TABLE dbo.ChiTietGioHang
        ADD MaChiTietGioHang INT IDENTITY(1,1) NOT NULL;

        ALTER TABLE dbo.ChiTietGioHang
        ADD CONSTRAINT PK_ChiTietGioHang PRIMARY KEY (MaChiTietGioHang);
    END;

    IF COL_LENGTH(N'dbo.ChiTietGioHang', N'MaBienThe') IS NULL
    BEGIN
        ALTER TABLE dbo.ChiTietGioHang
        ADD MaBienThe INT NULL;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_ChiTietGioHang_SanPhamBienThe'
          AND parent_object_id = OBJECT_ID(N'dbo.ChiTietGioHang')
    )
    BEGIN
        EXEC(N'ALTER TABLE dbo.ChiTietGioHang
        ADD CONSTRAINT FK_ChiTietGioHang_SanPhamBienThe
        FOREIGN KEY (MaBienThe) REFERENCES dbo.SanPhamBienThe(MaBienThe)');
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_ChiTietGioHang_GioHang_SanPham_BienThe'
          AND object_id = OBJECT_ID(N'dbo.ChiTietGioHang')
    )
    BEGIN
        EXEC(N'CREATE UNIQUE INDEX UX_ChiTietGioHang_GioHang_SanPham_BienThe
        ON dbo.ChiTietGioHang(MaGioHang, MaSP, MaBienThe)
        WHERE MaBienThe IS NOT NULL');
    END;
END;
