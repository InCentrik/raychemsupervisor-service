USE [ehtplus]
GO

/****** Object:  Table [dbo].[Tda9be7b1_c75d_484d_946f_4c4f83b4861e]    Script Date: 3/5/2024 5:01:10 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Tda9be7b1_c75d_484d_946f_4c4f83b4861e](
	[TrendDataId] [int] IDENTITY(1,1) NOT NULL,
	[DeviceGuid] [dbo].[GUID_IDENTIFIER] NULL,
	[TrendDateTime] [datetime] NULL,
	[TrendData] [varbinary](7168) NULL
) ON [PRIMARY]
GO

