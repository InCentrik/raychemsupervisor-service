USE [ehtplus]
GO

/****** Object:  Table [dbo].[EHTTrendGroup]    Script Date: 3/1/2024 4:24:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EHTTrendGroup](
	[TrendGroupGuid] [dbo].[GUID_IDENTIFIER] NOT NULL,
	[PlantAreaGuid] [dbo].[GUID_IDENTIFIER] NULL,
	[TrendGroupName] [nvarchar](50) NULL,
	[TrendGroupDescription] [nvarchar](1024) NULL,
	[TrendReadInterval] [int] NULL,
	[TrendLogMaximumReads] [int] NULL,
	[TrendLogOptions] [int] NULL,
	[TrendDataProcessVariableEnable] [int] NULL,
	[TrendDataMinMaxEnable] [int] NULL,
	[TrendDataMaintenanceEnable] [int] NULL,
	[TrendDataLimiterMinMaxEnable] [int] NULL,
	[ReadCounter] [int] NULL,
	[TrendDataAlarmStatusEnable] [int] NULL,
 CONSTRAINT [PK_EHTTRENDGROUP] PRIMARY KEY CLUSTERED 
(
	[TrendGroupGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[EHTTrendGroup]  WITH CHECK ADD  CONSTRAINT [FK_EHTTREND_RELATIONS_EHTPLANT] FOREIGN KEY([PlantAreaGuid])
REFERENCES [dbo].[EHTPlantArea] ([PlantAreaGuid])
GO

ALTER TABLE [dbo].[EHTTrendGroup] CHECK CONSTRAINT [FK_EHTTREND_RELATIONS_EHTPLANT]
GO

