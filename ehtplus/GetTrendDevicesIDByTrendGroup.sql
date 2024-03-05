USE [ehtplus]
GO

/****** Object:  StoredProcedure [dbo].[GetTrendDevicesIDDataByTrendGroup]    Script Date: 3/1/2024 4:24:57 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetTrendDevicesIDDataByTrendGroup]

	(
		@TrendGroupGuid uniqueidentifier
	)

AS
	SET NOCOUNT ON 
	declare @ChildrenDevices table (DeviceGuid uniqueidentifier)
	declare @ResultDevices table (
                           DeviceGuid       uniqueidentifier ,
                           EHTServerGuid	uniqueidentifier,
                           DeviceTag        nvarchar(300),
                           DeviceType       varchar(20),
                           DeviceAddress	int,
                           IPAddress        varchar(39),
                           Port				tinyint,
                           DeviceModel		smallint,
                           IsSteamable      bit
                        )
    declare @DeviceGuid uniqueidentifier
	
	insert @ResultDevices 
			select a.DeviceGuid, b.EHTServerGuid, dbo.AffixParentTag(a.DeviceGuid, b.DeviceTag), b.DeviceType,b.DeviceAddress,b.IPAddress, b.Port, b.DeviceModel,b.IsSteamable
				From TrendDevice a 
				Inner join EHTdevice b
				on a.DeviceGuid = b.DeviceGuid
				where (a.TrendGroupGuid = @TrendGroupGuid)
  
	select * from @ResultDevices order by DeviceTag
	RETURN
GO

