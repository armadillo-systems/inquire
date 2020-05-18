using System;
using System.IO;
using System.Text;

namespace iNQUIRE.Helper
{
    public static class LogHelper
    {
        private static readonly object _locker = new Object();
        public static string ErrorLogFileName { get; set; }
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


        static LogHelper()
        {
            ErrorLogFileName = "inquire.csv";
        }

        /// <summary>
        /// Returns the log for the current month
        /// </summary>
        /// <returns></returns>
        public static string GetLog()
        {
            return GetLog(DateTime.Now);
        }

        public static string GetLog(int month, int year)
        {
            var dt = new DateTime(year, month, 1);
            return GetLog(dt);
        }

        public static string GetLog(DateTime dt)
        {
            lock (_locker)
            {
                var file = MakeLogFileFullPath(dt);
                if (File.Exists(file))
                    return File.ReadAllText(file);
                else
                    return string.Format("Could not find file {0}", file);
            }
        }

        private static string MakeLogFileFullPath(DateTime dt)
        {
            var filename = GetLogFileNameFromMonthAndYear(dt);
            return string.Format("{0}{1}{2}", LogFileDirectory, Path.DirectorySeparatorChar, filename);
        }

        private static string GetLogFileNameFromMonthAndYear(DateTime dt)
        {
            string cur_month = dt.Month.ToString();
            string cur_year = dt.Year.ToString();

            if (cur_month.Length == 1)
                cur_month = "0" + cur_month;

            var file = Path.GetFileNameWithoutExtension(ErrorLogFileName);
            var ext = Path.GetExtension(ErrorLogFileName);
            return string.Format(@"{0}_{1}{2}{3}", file, cur_month, cur_year, ext);
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
                        var cur_date = DateTime.Now;
                        var filename = MakeLogFileFullPath(cur_date);

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

                        lock (_locker)
                        {
                            File.AppendAllText(filename, sb.ToString());
                        }
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
