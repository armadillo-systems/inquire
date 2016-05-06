using System;
using System.IO;
using System.Text;

namespace iNQUIRE.Helper
{
    public static class LogHelper
    {
        public static string LogFileDirectory { get; set; }

        public static Guid ApplicationID { get; set; }

        public static Boolean IsLogStatsEnabled
        {
            get { return (_isLogStatsEnabledDb || _isLogStatsEnabledCsv); }
        }

        private static Boolean _isLogStatsEnabledDb;
        public static Boolean IsLogStatsEnabledDb
        {
            get { return _isLogStatsEnabledDb; }
            set { _isLogStatsEnabledDb = value; }
        }

        private static Boolean _isLogStatsEnabledCsv = true;
        public static Boolean IsLogStatsEnabledCsv
        {
            get { return _isLogStatsEnabledCsv; }
            set { _isLogStatsEnabledCsv = value; }
        }

        public static void StatsLog(Guid? object_id, String object_name, String event_type, String value1, String value2)
        {
            if (IsLogStatsEnabled)
            {
                if (_isLogStatsEnabledDb)
                {
                    try
                    {
                        //DataAccess.AddLogEvent(ApplicationID, object_id, object_name, event_type, value1, value2);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Format("Warning: Could not add log event to database: {0}", ex.Message));
                    }
                }

                if (_isLogStatsEnabledCsv)
                {
                    // could be expanded in future to call webservice to log stat, but this will be significantly more overhead than file append, would need to be asynchronous too
                    // will fail in an xbap, so catch clause could contain xbap friendly, web service based code
                    try
                    {
                        // break logs down in to monthly to avoid file appends to giant files
                        DateTime cur_date = DateTime.Now;
                        string cur_month = cur_date.Month.ToString();
                        string cur_year = cur_date.Year.ToString();

                        if (cur_month.Length == 1)
                            cur_month = "0" + cur_month;

                        string filename = String.Format(@"{0}{1}{2}.csv", LogFileDirectory, cur_month, cur_year);

                        if (File.Exists(filename) == false)
                            File.AppendAllText(filename, "Date;Time;ApplicationID;ObjectID;ObjectName;EventType;Value1;Value2\r\n");

                        if (value1 == null)
                            value1 = "";

                        if (value2 == null)
                            value2 = "";

                        var sb = new StringBuilder(cur_date.ToShortDateString());
                        sb.Append(";");
                        sb.Append(cur_date.ToLongTimeString());
                        sb.Append(";");
                        sb.Append("\"");
                        sb.Append(ApplicationID);
                        sb.Append("\"");
                        sb.Append(";");
                        sb.Append("\"");
                        sb.Append(object_id);
                        sb.Append("\"");
                        sb.Append(";");
                        sb.Append("\"");
                        sb.Append(object_name);
                        sb.Append("\"");
                        sb.Append(";");
                        sb.Append("\"");
                        sb.Append(event_type);
                        sb.Append("\"");
                        sb.Append(";");
                        sb.Append("\"");
                        sb.Append(value1.Replace(";", "^").Replace("\"", "'")); // user inputted semi colons and double quotes may interfere with importing of csv (as using semicolon as delimiter, and double quote as text qualifier)
                        sb.Append("\"");
                        sb.Append(";");
                        sb.Append("\"");
                        sb.Append(value2.Replace(";", "^").Replace("\"", "'"));
                        sb.Append("\"");
                        sb.Append("\r\n");

                        File.AppendAllText(filename, sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Format("Warning: Could not write to stats file: {0}", ex.Message));
                    }
                }
            }
        }
    }
}
