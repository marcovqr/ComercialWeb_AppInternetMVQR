USE master;
SET NOCOUNT ON;

DECLARE @Backup nvarchar(4000)=N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\Respaldo_BD_Comercial_19072026.bak';
IF DB_ID(N'comercial') IS NOT NULL THROW 51000, 'La base comercial ya existe. No se sobrescribió.', 1;

CREATE TABLE #Files(
 LogicalName nvarchar(128), PhysicalName nvarchar(260), [Type] char(1), FileGroupName nvarchar(128) NULL,
 Size numeric(20,0), MaxSize numeric(20,0), FileId bigint, CreateLSN numeric(25,0), DropLSN numeric(25,0) NULL,
 UniqueId uniqueidentifier, ReadOnlyLSN numeric(25,0) NULL, ReadWriteLSN numeric(25,0) NULL,
 BackupSizeInBytes bigint, SourceBlockSize int, FileGroupId int, LogGroupGUID uniqueidentifier NULL,
 DifferentialBaseLSN numeric(25,0) NULL, DifferentialBaseGUID uniqueidentifier NULL, IsReadOnly bit,
 IsPresent bit, TDEThumbprint varbinary(32) NULL, SnapshotUrl nvarchar(360) NULL
);
DECLARE @FileListSql nvarchar(max)=N'RESTORE FILELISTONLY FROM DISK=N'''+REPLACE(@Backup,'''','''''')+N'''';
INSERT #Files EXEC sys.sp_executesql @FileListSql;

DECLARE @DataLogical sysname=(SELECT TOP(1) LogicalName FROM #Files WHERE [Type]='D' ORDER BY FileId);
DECLARE @LogLogical sysname=(SELECT TOP(1) LogicalName FROM #Files WHERE [Type]='L' ORDER BY FileId);
IF @DataLogical IS NULL OR @LogLogical IS NULL THROW 51001, 'El respaldo no contiene archivos de datos y log reconocibles.', 1;

DECLARE @DataPath nvarchar(4000)=CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultDataPath'))+N'comercial.mdf';
DECLARE @LogPath nvarchar(4000)=CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultLogPath'))+N'comercial_log.ldf';
DECLARE @Sql nvarchar(max)=N'RESTORE DATABASE [comercial] FROM DISK=N'''+REPLACE(@Backup,'''','''''')+
 N''' WITH MOVE N'''+REPLACE(@DataLogical,'''','''''')+N''' TO N'''+REPLACE(@DataPath,'''','''''')+
 N''', MOVE N'''+REPLACE(@LogLogical,'''','''''')+N''' TO N'''+REPLACE(@LogPath,'''','''''')+N''', RECOVERY, STATS=5;';
PRINT N'Restaurando archivos lógicos: '+@DataLogical+N' / '+@LogLogical;
EXEC sys.sp_executesql @Sql;
DBCC CHECKDB(N'comercial') WITH NO_INFOMSGS;
SELECT name,state_desc,user_access_desc,recovery_model_desc FROM sys.databases WHERE name=N'comercial';
