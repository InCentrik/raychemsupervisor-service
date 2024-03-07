USE [ehtplus]
GO

/****** Object:  Table [dbo].[Te89cefc4_0011_4a6c_8524_49bdac06858d]    Script Date: 3/5/2024 5:02:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Te89cefc4_0011_4a6c_8524_49bdac06858d](
	[TrendDataId] [int] IDENTITY(1,1) NOT NULL,
	[DeviceGuid] [dbo].[GUID_IDENTIFIER] NULL,
	[TrendDateTime] [datetime] NULL,
	[TrendData] [varbinary](7168) NULL
) ON [PRIMARY]
GO

