IF DB_ID(N'Kargonomi_JS') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [Kargonomi_JS]');
END;
GO

USE [Kargonomi_JS];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ApplicationMetadata', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApplicationMetadata
    (
        MetadataKey NVARCHAR(100) NOT NULL CONSTRAINT PK_ApplicationMetadata PRIMARY KEY,
        MetadataValue NVARCHAR(500) NOT NULL,
        UpdatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_ApplicationMetadata_UpdatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID(N'dbo.WebhookEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WebhookEvents
    (
        Id BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_WebhookEvents PRIMARY KEY,
        IdempotencyKey NVARCHAR(160) NOT NULL,
        EventType NVARCHAR(160) NOT NULL,
        PayloadHash CHAR(64) NOT NULL,
        PayloadJson NVARCHAR(MAX) NOT NULL,
        ReceivedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_WebhookEvents_ReceivedAtUtc DEFAULT SYSUTCDATETIME(),
        ProcessedAtUtc DATETIME2(7) NULL,
        LastError NVARCHAR(2048) NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT UQ_WebhookEvents_IdempotencyKey UNIQUE (IdempotencyKey),
        CONSTRAINT CK_WebhookEvents_PayloadJson CHECK (ISJSON(PayloadJson) = 1)
    );

    CREATE INDEX IX_WebhookEvents_ReceivedAtUtc ON dbo.WebhookEvents (ReceivedAtUtc DESC);
    CREATE INDEX IX_WebhookEvents_ProcessedAtUtc ON dbo.WebhookEvents (ProcessedAtUtc) WHERE ProcessedAtUtc IS NULL;
END;
GO

IF OBJECT_ID(N'dbo.ApiRequestLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiRequestLogs
    (
        Id BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ApiRequestLogs PRIMARY KEY,
        CorrelationId UNIQUEIDENTIFIER NOT NULL,
        HttpMethod NVARCHAR(10) NOT NULL,
        RequestPath NVARCHAR(1024) NOT NULL,
        StatusCode SMALLINT NULL,
        DurationMilliseconds INT NULL,
        CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_ApiRequestLogs_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_ApiRequestLogs_StatusCode CHECK (StatusCode IS NULL OR StatusCode BETWEEN 100 AND 599),
        CONSTRAINT CK_ApiRequestLogs_Duration CHECK (DurationMilliseconds IS NULL OR DurationMilliseconds >= 0)
    );

    CREATE UNIQUE INDEX UX_ApiRequestLogs_CorrelationId ON dbo.ApiRequestLogs (CorrelationId);
    CREATE INDEX IX_ApiRequestLogs_CreatedAtUtc ON dbo.ApiRequestLogs (CreatedAtUtc DESC);
END;
GO

MERGE dbo.ApplicationMetadata WITH (HOLDLOCK) AS Target
USING
(
    VALUES
        (N'project.name', N'Kargonomi API'),
        (N'project.runtime', N'Node.js'),
        (N'project.version', N'2.5.0-Enterprise'),
        (N'project.license', N'GPL-3.0'),
        (N'project.authors', N'Hamza Deniz Yılmaz and Beyza Gül'),
        (N'project.sponsor', N'Bilhost')
) AS Source (MetadataKey, MetadataValue)
ON Target.MetadataKey = Source.MetadataKey
WHEN MATCHED AND Target.MetadataValue <> Source.MetadataValue THEN
    UPDATE SET MetadataValue = Source.MetadataValue, UpdatedAtUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (MetadataKey, MetadataValue) VALUES (Source.MetadataKey, Source.MetadataValue);
GO
