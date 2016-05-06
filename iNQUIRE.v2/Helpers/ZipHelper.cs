using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace iNQUIRE.Helpers
{
    public static class ZipHelper
    {
        public static void ZipAdd(HttpResponseBase response, ZipOutputStream zipOutputStream, String file_name, Stream file_stream)
        {
            byte[] buffer = new byte[4096];
            ZipEntry entry = new ZipEntry(ZipEntry.CleanName(file_name));
            zipOutputStream.UseZip64 = UseZip64.Off;
            // entry.Size = file_stream.Length;
            // Setting the Size provides WinXP built-in extractor compatibility,
            //  but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.

            zipOutputStream.PutNextEntry(entry);

            if (file_stream != null)
            {
                int count = file_stream.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    zipOutputStream.Write(buffer, 0, count);
                    count = file_stream.Read(buffer, 0, buffer.Length);
                    if (!response.IsClientConnected)
                        break;
                    response.Flush();
                }
                file_stream.Close();
            }
        }
    }
}