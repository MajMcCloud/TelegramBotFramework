USE [telegram_bot]
GO
/****** Object:  Table [dbo].[tgb_devices_sessions]    Script Date: 30.06.2022 16:22:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tgb_devices_sessions](
	[deviceId] [bigint] NOT NULL,
	[deviceTitle] [nvarchar](512) NOT NULL,
	[FormUri] [nvarchar](512) NOT NULL,
	[QualifiedName] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_tgb_devices_sessions_1] PRIMARY KEY CLUSTERED 
(
	[deviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tgb_devices_sessions_data]    Script Date: 30.06.2022 16:22:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tgb_devices_sessions_data](
	[Id] [uniqueidentifier] NOT NULL,
	[deviceId] [bigint] NOT NULL,
	[key] [nvarchar](512) NOT NULL,
	[value] [nvarchar](max) NOT NULL,
	[type] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_tgb_devices_session_data] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[tgb_devices_sessions_data] ADD  CONSTRAINT [DF_tgb_devices_session_data_Id]  DEFAULT (newid()) FOR [Id]
GO
