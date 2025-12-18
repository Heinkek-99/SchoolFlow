-- ============================================
-- Configuration SQL Server pour Docker
-- ============================================

-- Désactiver le chiffrement forcé
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
GO

EXEC sp_configure 'remote access', 1;
RECONFIGURE;
GO

-- Activer les connexions TCP/IP
EXEC xp_instance_regwrite 
    N'HKEY_LOCAL_MACHINE', 
    N'Software\Microsoft\MSSQLServer\MSSQLServer', 
    N'LoginMode', 
    REG_DWORD, 
    2;
GO

-- Afficher la configuration
SELECT 
    @@SERVERNAME AS ServerName,
    @@VERSION AS Version,
    SERVERPROPERTY('Edition') AS Edition,
    SERVERPROPERTY('ProductLevel') AS ProductLevel;
GO

PRINT 'SQL Server configured successfully for Docker!';
GO