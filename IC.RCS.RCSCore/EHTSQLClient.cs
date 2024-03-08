using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using EHT.EHTDataObjectModel;
using System.Globalization;
using EHT.EHTCommandFormatter;
using EHT.EHTTools;
using System.Collections;
using EHT.CommonUI;
using System.Runtime.Serialization;
using ProtoBuf;


namespace IC.RCS.RCSCore
{
    //Accepts a SQL login to an EHT server to then make queries as needed
    public class EHTSQLClient
    {
        public string serverName;
        public string databaseName;
        public string username;
        public string password;

        public EHTSQLClient(string serverName, string databaseName, string username, string password)
        {

            this.serverName = serverName;
            this.databaseName = databaseName;
            this.username = username;
            this.password = password;

        }

        public string GetConnectionString()
        {

            return string.Format("data source={0};initial catalog={1};integrated security = False; User ID = {2}; Password = {3}", serverName, databaseName, username, password);

        }

        public bool IsDBTableExists(string tableName)
        {
            bool rc = false;

            if (tableName.Length == 0)
                throw new Exception("Parameter is invalid.");

            // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.  
            string cmdStr = "select case when exists((select * from information_schema.tables where table_name = '" + tableName + "')) then 1 else 0 end";
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(cmdStr, conn);

                conn.Open();
                rc = (int)cmd.ExecuteScalar() == 1;

            }
            return rc;
        }

        public bool RemoveBigTrendDataTableByName(string tableName)
        {
            return this.RemoveBigTrendGroupTable(tableName);
        }

        private bool RemoveBigTrendGroupTable(string tableName)
        {
            bool rc = false;

            string trendGroupTableName = tableName;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            SqlCommand Cmd = Conn.CreateCommand();

            Cmd.CommandText = "DROP TABLE " + trendGroupTableName;
            Cmd.CommandType = CommandType.Text;

            try
            {
                Conn.Open();
                Cmd.ExecuteNonQuery();
                rc = true;
            }
            catch (SqlException sqlex)
            {
                rc = false;
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }


        public string GetFakeTrendGroupTableName(Guid trendGroupId)
        {
            return "z" + trendGroupId.ToString().ToLower(CultureInfo.InvariantCulture).Replace("-", "_");
        }

        public string GetTrendGroupTableName(Guid trendGroupId)
        {
            return "T" + trendGroupId.ToString().ToLower(CultureInfo.InvariantCulture).Replace("-", "_");
        }

        public bool TestConnection()
        {

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }

        }

        private int ExecuteSQLQuery(string commandText, CommandType commandType)
        {
            int rowsAffected;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            SqlCommand Cmd = Conn.CreateCommand();

            Cmd.CommandText = commandText;
            Cmd.CommandType = commandType;

            try
            {
                Conn.Open();
                rowsAffected = Cmd.ExecuteNonQuery();
            }
            catch (SqlException sqlex)
            {
                Debug.WriteLine(sqlex.Message);
                rowsAffected = -1;
            }
            finally
            {
                Conn.Close();
            }
            return rowsAffected;
        }

        public int TransferDeserializeEHTData(Guid trendgroupId)
        {
            int rowsAffected = 0;

            //Get trend devices
            EHTDeviceIDData[] devices = new EHTDeviceIDData[0];



            return rowsAffected;
        }

        public int CreateFakeTrendTable(Guid trendgroupId)
        {
            string trendGroupTableName = GetFakeTrendGroupTableName(trendgroupId);

            string commandText = "CREATE TABLE " + trendGroupTableName + " (" +
                            "TrendDataId int NOT NULL IDENTITY(1,1), " +        // C# auto_increment setting
                            "DeviceGuid GUID_IDENTIFIER, " +
                            "TrendDateTime datetime, " +
                            Constants.SQLTrendDataColumns +
                            ") " +
                            "CREATE INDEX idx_deviceId ON " + trendGroupTableName + " (DeviceGuid); ";

            CommandType commandType = CommandType.Text;

            return ExecuteSQLQuery(commandText, commandType);
        }

        public void GetTrendDevicesIdDataByTrendGroup(Guid trendgroupId, out EHT.EHTDataObjectModel.EHTDeviceIDData[] devices)
        {
            //Calls sp, but will this SP always exist?
            SqlConnection Conn = new SqlConnection(GetConnectionString());
            SqlCommand Cmd = Conn.CreateCommand();
            Cmd.CommandText = "GetTrendDevicesIDDataByTrendGroup";
            Cmd.CommandType = CommandType.StoredProcedure;
            SqlParameter parm = new SqlParameter();
            parm.ParameterName = "@TrendGroupGuid";
            parm.Value = trendgroupId;
            parm.SqlDbType = SqlDbType.UniqueIdentifier;
            Cmd.Parameters.Add(parm);

            SqlDataAdapter SqlDA = new SqlDataAdapter();
            SqlDA.SelectCommand = Cmd;

            DataSet ds = new DataSet();
            SqlDA.Fill(ds, "EHTDeviceIDData");
            devices = null;
            if (ds != null)
            {
                int i = 0, cnt;
                if (ds.Tables[0] == null)
                    return;
                cnt = ds.Tables[0].Rows.Count;
                if (cnt > 0)
                {
                    devices = new EHTDeviceIDData[cnt];
                    for (i = 0; i < cnt; i++)
                    {

                        devices[i] = new EHTDeviceIDData();
                        devices[i].DeviceGuid = (Guid)(ds.Tables[0].Rows[i]["DeviceGuid"]);
                        devices[i].EHTServerGuid = (Guid)(ds.Tables[0].Rows[i]["EHTServerGuid"]);
                        devices[i].DeviceTag = ((string)(ds.Tables[0].Rows[i]["DeviceTag"])).Trim();
                        devices[i].DeviceType = ((string)(ds.Tables[0].Rows[i]["DeviceType"])).Trim();
                        devices[i].DeviceAddress = (int)(ds.Tables[0].Rows[i]["DeviceAddress"]);
                        devices[i].IPAddress = ((string)(ds.Tables[0].Rows[i]["IPAddress"])).Trim();
                        devices[i].Port = (byte)(ds.Tables[0].Rows[i]["Port"]);
                        devices[i].DeviceModel = (ushort)(short)(ds.Tables[0].Rows[i]["DeviceModel"]);
                        devices[i].IsSteamable = (bool)(ds.Tables[0].Rows[i]["IsSteamable"]);

                    }
                }



            }
        }

        public tbEHTTrendData[] GetBigTrendDeviceTrendDataByTime(Guid trendGroupId, Guid trendDeviceId, string trendDeviceType, bool useSmallDataSet, int maxRecords, System.DateTime startTime, bool readOldData)
        {
            string trendGroupTableName = GetTrendGroupTableName(trendGroupId);
            string strTrendDeviceGuid = trendDeviceId.ToString().Replace("'", "''");
            string strStartTime = startTime.Year + "-" + startTime.Month + "-" + startTime.Day +
                " " + startTime.Hour + ":" + startTime.Minute + ":" + startTime.Second +
                "." + startTime.Millisecond;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            ArrayList list = new ArrayList();
            using (Conn)
            {
                SqlCommand Cmd = Conn.CreateCommand();
                Conn.Open();
                if (readOldData == true)
                    Cmd.CommandText = string.Format("select top({0}) [TrendDateTime], [TrendData] from [ehtplus].[dbo].[{1}] where [DeviceGuid] = '{2}' and [TrendDateTime] < '{3}'", maxRecords, trendGroupTableName, strTrendDeviceGuid, strStartTime);
                else
                    Cmd.CommandText = string.Format("select top({0}) [TrendDateTime], [TrendData] from [ehtplus].[dbo].[{1}] where [DeviceGuid] = '{2}' and [TrendDateTime] > '{3}'", maxRecords, trendGroupTableName, strTrendDeviceGuid, strStartTime);
                Cmd.CommandType = CommandType.Text;
                SqlDataReader reader = Cmd.ExecuteReader();

                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var t = new tbEHTTrendData();
                        t.DeviceGuid = string.Empty;
                        t.TrendDateTime = (DateTime)(reader["TrendDateTime"]);
                        t.TrendDeviceTag = string.Empty;
                        t.TrendDeviceType = string.Empty;
                        t.TrendInterval = 0;
                        if (reader["TrendData"] != null)
                        {
                            try
                            {
                                byte[] byTrendData = (byte[])(reader["TrendData"]);

                                // protobuf deserialization
                                t.TrendData = ProtobufSerializer.ProtoDeserialTrendData(trendDeviceType, byTrendData, useSmallDataSet);
                            }
                            catch
                            {
                                t.TrendData = null;
                            }

                        }
                        else
                            t.TrendData = null;
                        list.Add(t);
                    }
                    reader.NextResult();
                }
            }
            return (tbEHTTrendData[])list.ToArray(typeof(tbEHTTrendData));
        }

        public List<string> GetEHTDeviceModelValuesTypes()
        {
            //TODO decide if this is worth doing to dynamically map types to SQL command text
            //Returns the type of each value in the device model
            Type ehtDeviceModelType = typeof(EHT.EHTDeviceModel.tinyEHTHTC2Values3P);
            var test = ehtDeviceModelType.GetFields().Select(field => field.Name);
            List<string> typesStrings = (List<string>) ehtDeviceModelType.GetFields().Select(field => field.Name).ToList();

            return typesStrings;

        }

        public DataSet GetTrendGroups()
        {
            //Calls sp, but will this SP always exist?
            SqlConnection Conn = new SqlConnection(GetConnectionString());
            SqlCommand Cmd = Conn.CreateCommand();
            Cmd.CommandText = "Select * From "+databaseName+".dbo.EHTTrendGroup";
            Cmd.CommandType = CommandType.Text;

            SqlDataAdapter SqlDA = new SqlDataAdapter();
            SqlDA.SelectCommand = Cmd;

            DataSet ds = new DataSet();
            SqlDA.Fill(ds, "EHTTrendGroup");

            return ds;
        }

        #region misc helpers
        private string BooleanToBitString(bool state)
        {
            string str = state ? "1" : "0";
            return str;
        }

        #endregion

        #region bigvalues
        public bool AddEHTTrendDataTest(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.EHTHTCValues tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "'" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Year + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Month + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Day +
                " " + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Hour + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Minute + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Second +
                "." + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.MaxMinLastUpdate.Year + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Month + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Day +
                " " + tbEHTTrendDataArg.MaxMinLastUpdate.Hour + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Minute + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Second +
                "." + tbEHTTrendDataArg.MaxMinLastUpdate.Millisecond + "'" + ","
                + (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "'" + tbEHTTrendDataArg.LastGCCReadTime.Year + "-" + tbEHTTrendDataArg.LastGCCReadTime.Month + "-" + tbEHTTrendDataArg.LastGCCReadTime.Day +
                " " + tbEHTTrendDataArg.LastGCCReadTime.Hour + ":" + tbEHTTrendDataArg.LastGCCReadTime.Minute + ":" + tbEHTTrendDataArg.LastGCCReadTime.Second +
                "." + tbEHTTrendDataArg.LastGCCReadTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor3Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor4Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControllerStatusBits.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.GroundFaultCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.Line2LineVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Resistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Power.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakLoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + tbEHTTrendDataArg.LimiterTSTempAvg.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LimiterTSTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LimiterTSTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Failed.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Failed.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Installed.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Installed.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + "0" + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + "0"   //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddHTCTrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.EHTHTCValues tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LimiterTSTempMax.ToString(CultureInfo.InvariantCulture) + ","   // local
                + tbEHTTrendDataArg.LimiterTSTempMin.ToString(CultureInfo.InvariantCulture) + ","   // local
                + "'" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Year + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Month + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Day +
                " " + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Hour + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Minute + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Second +
                "." + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.MaxMinLastUpdate.Year + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Month + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Day +
                " " + tbEHTTrendDataArg.MaxMinLastUpdate.Hour + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Minute + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Second +
                "." + tbEHTTrendDataArg.MaxMinLastUpdate.Millisecond + "'" + ","
                + (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "'" + tbEHTTrendDataArg.LastGCCReadTime.Year + "-" + tbEHTTrendDataArg.LastGCCReadTime.Month + "-" + tbEHTTrendDataArg.LastGCCReadTime.Day +
                " " + tbEHTTrendDataArg.LastGCCReadTime.Hour + ":" + tbEHTTrendDataArg.LastGCCReadTime.Minute + ":" + tbEHTTrendDataArg.LastGCCReadTime.Second +
                "." + tbEHTTrendDataArg.LastGCCReadTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor3Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor4Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControllerStatusBits.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.GroundFaultCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Voltage.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Resistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Power.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakLoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + tbEHTTrendDataArg.LimiterTSTempAvg.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddHTC3TrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.EHTHTC2Values3P tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "'" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Year + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Month + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Day +
                " " + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Hour + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Minute + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Second +
                "." + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.MaxMinLastUpdate.Year + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Month + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Day +
                " " + tbEHTTrendDataArg.MaxMinLastUpdate.Hour + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Minute + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Second +
                "." + tbEHTTrendDataArg.MaxMinLastUpdate.Millisecond + "'" + ","
                + (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "'" + tbEHTTrendDataArg.LastParentReadTime.Year + "-" + tbEHTTrendDataArg.LastParentReadTime.Month + "-" + tbEHTTrendDataArg.LastParentReadTime.Day +
                " " + tbEHTTrendDataArg.LastParentReadTime.Hour + ":" + tbEHTTrendDataArg.LastParentReadTime.Minute + ":" + tbEHTTrendDataArg.LastParentReadTime.Second +
                "." + tbEHTTrendDataArg.LastParentReadTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor3Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor4Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Line2LineVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TracingResistance1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnEffectiveLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerAccumulator1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddHTC2TrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.EHTHTC2Values tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "'" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Year + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Month + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Day +
                " " + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Hour + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Minute + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Second +
                "." + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Day +
                " " + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Second +
                "." + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.MaxMinLastUpdate.Year + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Month + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Day +
                " " + tbEHTTrendDataArg.MaxMinLastUpdate.Hour + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Minute + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Second +
                "." + tbEHTTrendDataArg.MaxMinLastUpdate.Millisecond + "'" + ","
                + (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "'" + tbEHTTrendDataArg.LastParentReadTime.Year + "-" + tbEHTTrendDataArg.LastParentReadTime.Month + "-" + tbEHTTrendDataArg.LastParentReadTime.Day +
                " " + tbEHTTrendDataArg.LastParentReadTime.Hour + ":" + tbEHTTrendDataArg.LastParentReadTime.Minute + ":" + tbEHTTrendDataArg.LastParentReadTime.Second +
                "." + tbEHTTrendDataArg.LastParentReadTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor3Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor4Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.FixedVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TracingResistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddIOTrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.M3KIOValues tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.AverageAnalogsLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.UnitMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.SwitchMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.MaxMinLastUpdate
                + "NULL" + "," // (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastParentReadTime
                + "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS3Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS4Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource1
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource2
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource3
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource4
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource5
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource6
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource7
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource8
                + "NULL" + "," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.EffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingResistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerConsumption.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + "NULL" + "," // tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddSLIMTrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.M3KLimiterValues tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.AverageAnalogsLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.UnitMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.SwitchMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.MaxMinLastUpdate
                + "NULL" + "," // (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastParentReadTime
                + "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS3Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource1
                + tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource2
                + tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource3
                + tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource4
                + tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource5
                + tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource6
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource7
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource8
                + "NULL" + "," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.EffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingResistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerConsumption.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","    // RawSSROutput
                + (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","  // RawContactorOutput
                + "NULL" + "," // tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddNGC30Circuit2TrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.EHTCircuit2Values tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TotalCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MaxControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MinControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.LocalTSTempMax.ToString(CultureInfo.InvariantCulture) + ","   // local
                + "NULL" + "," //  tbEHTTrendDataArg.LocalTSTempMin.ToString(CultureInfo.InvariantCulture) + ","   // local
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Year + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Month + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Day +
                               //" " + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Hour + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Minute + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Second +
                               //"." + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Day +
                               // " " + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Second +
                               // "." + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Day +
                               // " " + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Second +
                               // "." + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," // + "'" + tbEHTTrendDataArg.MaxMinLastUpdate.Year + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Month + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Day +
                               // " " + tbEHTTrendDataArg.MaxMinLastUpdate.Hour + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Minute + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Second +
                               // "." + tbEHTTrendDataArg.MaxMinLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," //  (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.LastGCCReadTime.Year + "-" + tbEHTTrendDataArg.LastGCCReadTime.Month + "-" + tbEHTTrendDataArg.LastGCCReadTime.Day +
                               //" " + tbEHTTrendDataArg.LastGCCReadTime.Hour + ":" + tbEHTTrendDataArg.LastGCCReadTime.Minute + ":" + tbEHTTrendDataArg.LastGCCReadTime.Second +
                               //"." + tbEHTTrendDataArg.LastGCCReadTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD1Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD2Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD3Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD4Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ComputedPASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ComputedPASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.NextRelaySwitch.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RelayState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL," // tbEHTTrendDataArg.ControllerStatusBits.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Current.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.GroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Voltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.Resistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Power.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TotalHeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL,"   // (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL,"   // (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL,"   // (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD1Fail) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD2Fail) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD3Fail) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD4Fail) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD1Install) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD2Install) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD3Install) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.RTD4Install) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.PowerInputState) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.CBTripState) + ","
                + this.BooleanToBitString(tbEHTTrendDataArg.UserAlarmState) + ","
                + tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL,"   // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }
        #endregion

        #region tinyvalues
        public bool AddtinyHTC3TrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.tinyEHTHTC2Values3P tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.AverageAnalogsLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.UnitMaintenanceLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.SwitchMaintenanceLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.MaxMinLastUpdate
                + "NULL" + "," //+ (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastParentReadTime
                + "NULL" + "," // tbEHTTrendDataArg.LastSetpointUpdateTime
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor3Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor4Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.InstantLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Line2LineVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.TracingResistance1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnEffectiveLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PowerAccumulator1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PeakLineCurrent1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL" + "," //+ (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL" + "," //+ (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL" + "," //+ (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine("AddTinyHTC3 .. " + ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddtinyHTCTrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.tinyEHTHTCValues tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.AverageAnalogsLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.UnitMaintenanceLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.SwitchMaintenanceLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.MaxMinLastUpdate
                + "NULL" + "," //+ (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastParentReadTime
                + "NULL" + "," // tbEHTTrendDataArg.LastSetpointUpdateTime
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor3Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor4Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.LoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.GroundFaultCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Voltage.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Resistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Power.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PeakLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine("AddTinyHTC .. " + ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddtinyHTC2TrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.tinyEHTHTC2Values tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.AverageAnalogsLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.UnitMaintenanceLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.SwitchMaintenanceLastUpdate
                + "NULL" + "," //+ tbEHTTrendDataArg.MaxMinLastUpdate
                + "NULL" + "," //+ (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //+ tbEHTTrendDataArg.LastParentReadTime
                + "NULL" + "," // tbEHTTrendDataArg.LastSetpointUpdateTime
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor1Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor2Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor3Temp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TempSensor4Temp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.EffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.FixedVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TracingResistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.PowerConsumption.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PeakLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + "NULL,"   //+ tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine("AddTinyHTC2 .. " + ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddtinyIOTrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.tinyM3KIOValues tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.AverageAnalogsLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.UnitMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.SwitchMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.MaxMinLastUpdate
                + "NULL" + "," // (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastParentReadTime
                + "NULL,"   // tbEHTTrendDataArg.LastSetpointUpdateTime
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS3Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS4Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource1
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource2
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource3
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource4
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource5
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource6
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource7
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource8
                + "NULL" + "," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.EffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingResistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerConsumption.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL,"   //+ (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + "NULL" + "," // tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //+ tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine("AddTinyIO .. " + ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddtinySLIMTrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.tinyM3KLimiterValues tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + "NULL" + "," // + "NULL" + "," // tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // + "NULL" + "," // tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // + tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // + tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // + tbEHTTrendDataArg.ContactorCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.AverageAnalogsLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.UnitMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.SwitchMaintenanceLastUpdate
                + "NULL" + "," // tbEHTTrendDataArg.MaxMinLastUpdate
                + "NULL" + "," // (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastParentReadTime
                + "NULL" + "," // tbEHTTrendDataArg.LastSetpointUpdateTime
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS1Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS2Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.TS3Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // + tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource1
                + "NULL" + "," // + tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource2
                + "NULL" + "," // + tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource3
                + "NULL" + "," // + tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource4
                + "NULL" + "," // + tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource5
                + "NULL" + "," // + tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource6
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + "," // TempSource7
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + "," // TempSource8
                + "NULL" + "," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCNextSwitchAction.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PASCOutputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingControlStatus.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.EffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantanousGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TracingResistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerConsumption.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.HeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL" + "," // + (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL" + "," // + (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","    // RawSSROutput
                + "NULL" + "," // + (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","  // RawContactorOutput
                + "NULL" + "," // tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // + (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // + (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine("AddTinySLIM .. " + ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }

        public bool AddtinyNGC30Circuit2TrendData(string deviceGuid, string trendGroupTableName, EHT.EHTDeviceModel.tinyEHTCircuit2Values tbEHTTrendDataArg, DateTime trendDateTime)
        {
            bool rc = false;

            SqlConnection Conn = new SqlConnection(GetConnectionString());
            Conn.Open();
            SqlCommand sqlCmd = Conn.CreateCommand();
            sqlCmd.CommandText = "insert into " + trendGroupTableName + " values  ("
                + "N'" + deviceGuid.Replace("'", "''") + "'" + ","
                + "'" + trendDateTime.Year + "-" + trendDateTime.Month + "-" + trendDateTime.Day +
                " " + trendDateTime.Hour + ":" + trendDateTime.Minute + ":" + trendDateTime.Second +
                "." + trendDateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.ControlSetpoint.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.HoursSinceReset.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.HoursInUse.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.TotalCycleCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.MaxControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.MinControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.LocalTSTempMax.ToString(CultureInfo.InvariantCulture) + ","   // local
                + "NULL" + "," //  tbEHTTrendDataArg.LocalTSTempMin.ToString(CultureInfo.InvariantCulture) + ","   // local
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Year + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Month + "-" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Day +
                               //" " + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Hour + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Minute + ":" + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Second +
                               //"." + tbEHTTrendDataArg.AverageAnalogsLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Day +
                               // " " + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Second +
                               // "." + tbEHTTrendDataArg.UnitMaintenanceLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Year + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Month + "-" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Day +
                               // " " + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Hour + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Minute + ":" + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Second +
                               // "." + tbEHTTrendDataArg.SwitchMaintenanceLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," // + "'" + tbEHTTrendDataArg.MaxMinLastUpdate.Year + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Month + "-" + tbEHTTrendDataArg.MaxMinLastUpdate.Day +
                               // " " + tbEHTTrendDataArg.MaxMinLastUpdate.Hour + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Minute + ":" + tbEHTTrendDataArg.MaxMinLastUpdate.Second +
                               // "." + tbEHTTrendDataArg.MaxMinLastUpdate.Millisecond + "'" + ","
                + "NULL" + "," //  (tbEHTTrendDataArg.ControlTempFailed ? "0" : "1") + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommPolls.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommResponses.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommRetries.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.CommFailures.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  "'" + tbEHTTrendDataArg.LastGCCReadTime.Year + "-" + tbEHTTrendDataArg.LastGCCReadTime.Month + "-" + tbEHTTrendDataArg.LastGCCReadTime.Day +
                               //" " + tbEHTTrendDataArg.LastGCCReadTime.Hour + ":" + tbEHTTrendDataArg.LastGCCReadTime.Minute + ":" + tbEHTTrendDataArg.LastGCCReadTime.Second +
                               //"." + tbEHTTrendDataArg.LastGCCReadTime.Millisecond + "'" + ","
                + "NULL" + "," //   "'" + tbEHTTrendDataArg.LastSetpointUpdateTime.Year + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Month + "-" + tbEHTTrendDataArg.LastSetpointUpdateTime.Day +
                               //" " + tbEHTTrendDataArg.LastSetpointUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastSetpointUpdateTime.Second +
                               //"." + tbEHTTrendDataArg.LastSetpointUpdateTime.Millisecond + "'" + ","
                + "'" + tbEHTTrendDataArg.LastUpdateTime.Year + "-" + tbEHTTrendDataArg.LastUpdateTime.Month + "-" + tbEHTTrendDataArg.LastUpdateTime.Day +
                " " + tbEHTTrendDataArg.LastUpdateTime.Hour + ":" + tbEHTTrendDataArg.LastUpdateTime.Minute + ":" + tbEHTTrendDataArg.LastUpdateTime.Second +
                "." + tbEHTTrendDataArg.LastUpdateTime.Millisecond + "'" + ","
                + tbEHTTrendDataArg.ErrorStatus.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD1Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD2Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD3Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RTD4Temperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp1.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp5.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp6.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp7.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TSTemp8.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.AmbientTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LocalTempSensorTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControlOutputDutyCycle.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.ComputedPASCOnCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.ComputedPASCOffCount.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.NextRelaySwitch.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.RelayState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.ControllerStatusBits.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Current.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.EffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.GroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.InstantLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.InstantLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Voltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.Resistance.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TracingResistance3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.FixedFrequency.ToString(CultureInfo.InvariantCulture) + ","
                + tbEHTTrendDataArg.Power.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerConsumption3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnEffectiveLineCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.LastOnEffectiveLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnControlTemp.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LastOnGFI.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.TotalPowerAllPhases_KWH.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PowerAccumulator3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLoadCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent2.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakLineCurrent3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   //tbEHTTrendDataArg.PeakGFCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," //  tbEHTTrendDataArg.TotalHeaterTime.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawDigitalInputLocRem ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawAlarmOutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawSSROutput ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.RawContactorOutput ? "0" : "0") + ","
                + "NULL" + "," //  tbEHTTrendDataArg.LimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS1TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS1TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS2TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL,"   // tbEHTTrendDataArg.TS2TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS3TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMax.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.TS4TempMin.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinVoltage.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxCurrent.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxGroundFault.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MaxLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.MinLimiterTemperature.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Fail.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD1Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD2Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD3Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.RTD4Install.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.PowerInputState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.CBTripState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.UserAlarmState.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusLow.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.StatusHigh.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus3.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // tbEHTTrendDataArg.LoopStatus4.ToString(CultureInfo.InvariantCulture) + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterNonLatching ? "0" : "1") + ","
                + "NULL" + "," // (tbEHTTrendDataArg.LimiterTripped ? "0" : "0") + ","
                + 0 + "," //tbEHTTrendDataArg.BulkAlarm1.ToString(CultureInfo.InvariantCulture) + ","
                + 0 //tbEHTTrendDataArg.BulkAlarm2.ToString(CultureInfo.InvariantCulture) + ","
                + ")";

            try
            {
                sqlCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rc = true;
                System.Diagnostics.Trace.WriteLine("AddTinyCCT .. " + ex.Message);
            }
            finally
            {
                Conn.Close();
            }
            return rc;
        }
        #endregion
    }
}
