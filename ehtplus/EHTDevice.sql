USE [ehtplus]
GO

/****** Object:  Table [dbo].[EHTDevice]    Script Date: 3/1/2024 4:10:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EHTDevice](
	[DeviceGuid] [dbo].[GUID_IDENTIFIER] NOT NULL,
	[EHTServerGuid] [dbo].[GUID_IDENTIFIER] NOT NULL,
	[BreakerPanelGuid] [dbo].[GUID_IDENTIFIER] NULL,
	[ControllerPanelGuid] [dbo].[GUID_IDENTIFIER] NULL,
	[LocationGuid] [dbo].[GUID_IDENTIFIER] NULL,
	[DeviceTag] [dbo].[DeviceTagType] NULL,
	[DeviceType] [dbo].[DeviceType] NULL,
	[BreakerNumber] [char](10) NULL,
	[ControllerPosition] [char](10) NULL,
	[DeviceAddress] [int] NULL,
	[Port] [tinyint] NULL,
	[ComFailureCount] [int] NULL,
	[ComRetryCount] [int] NULL,
	[ComSuccessCount] [int] NULL,
	[ComTxDelay] [int] NULL,
	[ComRetries] [int] NULL,
	[FirmwareVersion] [float] NULL,
	[Offline] [bit] NULL,
	[DeleteChildren] [bit] NULL,
	[IPAddress] [dbo].[IP_Address] NULL,
	[InstalledDate] [datetime] NULL,
	[DevicePriority] [tinyint] NULL,
	[DeviceModel] [smallint] NULL,
	[IsSteamable] [bit] NULL,
	[DeviceData] [varbinary](7168) NULL,
	[ReadTimeout] [int] NULL,
	[WriteTimeout] [int] NULL,
	[WirelessCommOptions] [int] NULL,
	[ControlSetpoint] [float] NULL,
)