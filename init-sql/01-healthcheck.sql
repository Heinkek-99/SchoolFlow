-- ============================================
-- Script de vérification SQL Server
-- ============================================

-- Vérifier que SQL Server accepte les connexions
SELECT 
    @@VERSION AS SqlServerVersion,
    SERVERPROPERTY('Edition') AS Edition,
    SERVERPROPERTY('ProductLevel') AS ProductLevel,
    SERVERPROPERTY('ProductVersion') AS ProductVersion,
    GETDATE() AS CurrentTime;

-- Vérifier la base master
USE master;
GO

-- Afficher les bases existantes
SELECT name, database_id, create_date 
FROM sys.databases;
GO

PRINT 'SQL Server is ready!';