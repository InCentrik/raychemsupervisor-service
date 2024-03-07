SET IDENTITY_INSERT Te89cefc4_0011_4a6c_8524_49bdac06858d OFF
SET IDENTITY_INSERT Tda9be7b1_c75d_484d_946f_4c4f83b4861e ON
Go
Insert Into Tda9be7b1_c75d_484d_946f_4c4f83b4861e (TrendDataId,DeviceGuid,TrendDateTime,TrendData)

Select TrendDataId,DeviceGuid,TrendDateTime, Cast(TrendData As VarBinary(7168))

FROM [ehtplus].[dbo].[Tda9be7b1_c75d_484d_946f_4c4f83b4861e_FLAT]
