USE [comercial];
SET XACT_ABORT ON;
BEGIN TRANSACTION;

/*
  Este ajuste no actualiza ninguna fila histórica.
  Los detalles anteriores conservan producto, cantidad, precio y estado.
  Las columnas de mensualidad y período solo se llenan en facturas nuevas.
*/

IF COL_LENGTH('dbo.factura_det', 'fac_detalle_id') IS NULL
    ALTER TABLE dbo.factura_det
        ADD fac_detalle_id bigint IDENTITY(1,1) NOT NULL;

IF COL_LENGTH('dbo.factura_det', 'fac_linea') IS NULL
    ALTER TABLE dbo.factura_det ADD fac_linea int NULL;

IF COL_LENGTH('dbo.factura_det', 'men_codigo') IS NULL
    ALTER TABLE dbo.factura_det ADD men_codigo int NULL;

IF COL_LENGTH('dbo.factura_det', 'fac_desde') IS NULL
    ALTER TABLE dbo.factura_det ADD fac_desde date NULL;

IF COL_LENGTH('dbo.factura_det', 'fac_hasta') IS NULL
    ALTER TABLE dbo.factura_det ADD fac_hasta date NULL;

DECLARE @PrimaryKey sysname =
(
    SELECT kc.name
    FROM sys.key_constraints AS kc
    WHERE kc.parent_object_id = OBJECT_ID(N'dbo.factura_det')
      AND kc.[type] = 'PK'
);

DECLARE @PrimaryKeyIsDetalleId bit =
(
    SELECT CASE WHEN COUNT(*) = 1 AND MAX(c.name) = N'fac_detalle_id' THEN 1 ELSE 0 END
    FROM sys.key_constraints AS kc
    INNER JOIN sys.index_columns AS ic
        ON ic.object_id = kc.parent_object_id
       AND ic.index_id = kc.unique_index_id
    INNER JOIN sys.columns AS c
        ON c.object_id = ic.object_id
       AND c.column_id = ic.column_id
    WHERE kc.parent_object_id = OBJECT_ID(N'dbo.factura_det')
      AND kc.[type] = 'PK'
);

IF @PrimaryKey IS NOT NULL AND ISNULL(@PrimaryKeyIsDetalleId, 0) = 0
BEGIN
    DECLARE @DropPrimaryKeySql nvarchar(max) =
        N'ALTER TABLE dbo.factura_det DROP CONSTRAINT ' + QUOTENAME(@PrimaryKey) + N';';
    EXEC sys.sp_executesql @DropPrimaryKeySql;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.factura_det')
      AND [type] = 'PK'
)
BEGIN
    EXEC sys.sp_executesql N'
        ALTER TABLE dbo.factura_det
            ADD CONSTRAINT PK_factura_det PRIMARY KEY (fac_detalle_id);';
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.factura_det')
      AND name = N'UX_factura_det_numero_linea'
)
BEGIN
    EXEC sys.sp_executesql N'
        CREATE UNIQUE INDEX UX_factura_det_numero_linea
            ON dbo.factura_det (fac_numero, fac_linea)
            WHERE fac_linea IS NOT NULL;';
END;

COMMIT TRANSACTION;
