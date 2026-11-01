USE [telegram_bot]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tgb_devices_sessions]
(
    [deviceId]      [bigint]        NOT NULL,
    [deviceTitle]   [nvarchar](512) NOT NULL,
    [FormUri]       [nvarchar](512) NOT NULL,
    [QualifiedName] [nvarchar](512) NOT NULL,
    CONSTRAINT [PK_tgb_devices_sessions_1] PRIMARY KEY CLUSTERED ([deviceId] ASC)
)
GO

CREATE TABLE [dbo].[tgb_devices_sessions_data]
(
    [Id]       [uniqueidentifier] NOT NULL CONSTRAINT [DF_tgb_devices_session_data_Id] DEFAULT (newid()),
    [deviceId] [bigint]           NOT NULL,
    [key]      [nvarchar](512)    NOT NULL,
    [value]    [nvarchar](max)    NOT NULL,
    [type]     [nvarchar](512)    NOT NULL,
    CONSTRAINT [PK_tgb_devices_session_data] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

-- Speeds up loading and removing the state of a single device (used with session limits).
CREATE NONCLUSTERED INDEX [IX_tgb_devices_sessions_data_deviceId] ON [dbo].[tgb_devices_sessions_data] ([deviceId])
GO
