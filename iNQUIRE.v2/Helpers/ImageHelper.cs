using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Net;
using iNQUIRE.Helper;
using iNQUIRE.Models;

namespace iNQUIRE.Helpers
{
    public static class ImageHelper
    {
        private static readonly BackgroundWorker _bw;
        private static readonly List<KeyValuePair<string, string>> _bwFiles = new List<KeyValuePair<string, string>>();

        public static string DjatokaHome { get; set; }
        public static string JavaHome { get; set; }

        // used to replace djatoka namespace with IIP image server path, to avoid having to re-import BL crimea Solr data from Sql Server, as the current data import handler is broken with the new version of solr
        // might be a useful thing to have anyway, to allow easy modification of file names/paths that come out of Solr?
        public static string Jpeg2000NamespaceReplace { get; set; } 
        public static string Jpeg2000Namespace { get; set; }
        public static string Jpeg2000Directory { get; set; }
        public static string ImageDirectory { get; set; }
        public static string ImageFilenameAppend { get; set; }

        static ImageHelper()
        {
            _bw = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _bw.RunWorkerCompleted += onBw_RunWorkerCompleted;
            _bw.DoWork += onBw_DoWork;
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static int[] GetImageDimensions(string file)
        {
            try
            {
                if (JP2HelperBase.IsAudioOrVideo(file))
                    file = Path.ChangeExtension(file, ".jpg"); // replace audio/video file extension with .jpg so thumbnail can be read for metadata

                file = String.Format("{0}{1}", ImageDirectory, file);

                var bitmapFrame = BitmapFrame.Create(new Uri(file), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                return new[] { bitmapFrame.PixelWidth, bitmapFrame.PixelHeight };
            }
            catch (Exception)
            {
                return new[] { 0, 0 };
            }
        }

        public static string CreateJpeg2000(string file)
        {
            var source_file = String.Format("{0}{1}", ImageDirectory, file);
            var dest_file = String.Format("{0}{1}.jp2", Jpeg2000Directory, StripFileExtension(file));

            var files = new KeyValuePair<string, string>(source_file, dest_file);

            var db = new DjatokaDataContext();
            var resource = db.DjatokaResources.SingleOrDefault(r => r.imageFile == dest_file);

            // check file doesn't exist physically, and also not present in database linked to an identifier, trying to avoid duplicate encoding here
            if (resource == null)
            {
                if (File.Exists(dest_file) == false)
                {
                    if (!_bwFiles.Contains(files))
                        _bwFiles.Add(files);

                    if (!IsBwBusy)
                        _bw.RunWorkerAsync();
                }
                else
                    LogHelper.StatsLog(null, "CreateJpeg2000", "WARNING! Skipped file creation, destination file exists already, but no database entry: " + dest_file, null, null);

                return "";
            }

            LogHelper.StatsLog(null, "CreateJpeg2000", "skipped file creation, database entry exists for: " + source_file, null, null);
            return resource.identifier;
        }

        public static bool IsBwBusy
        {
            get { return (_bw != null) && (_bw.IsBusy); }
        }

        public static void BwTidyUp()
        {
            if (IsBwBusy)
            {
                _bw.CancelAsync();
                _bw.Dispose();
            }
        }

        static void onBw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_bwFiles.Count > 0)
            {
                var source_file = _bwFiles[0].Key;
                var dest_file = _bwFiles[0].Value;

                LogHelper.StatsLog(null, "onBw_DoWork", "processing " + source_file, null, null);

                createJpeg2000(source_file, dest_file);
                #region convert to jpeg
                //var b = new BitmapImage(new Uri(source_file));

                //using (var stream = new FileStream(dest_file, FileMode.Create))
                //{
                //    var encoder = new JpegBitmapEncoder { QualityLevel = 80 };
                //    encoder.Frames.Add(BitmapFrame.Create(b));
                //    encoder.Save(stream);
                //}
                #endregion convert to jpeg

                _bwFiles.RemoveAt(0);
            }
            else
                LogHelper.StatsLog(null, "onBw_DoWork", "Nothing to do", null, null);
        }

        [DebuggerNonUserCodeAttribute]
        private static void createJpeg2000(string input_file, string output_file)
        {
            try
            {
                var p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    },
                    EnableRaisingEvents = true
                };

                string KAKADU_HOME = String.Format("{0}\\bin\\Win32", DjatokaHome);

                string JAVA_OPTS = String.Format("-Djava.awt.headless=true  -Xmx512M -Xms64M -Dkakadu.home=\"{0}\" -Djava.library.path=\"{0}\"", KAKADU_HOME);
                string CLASSPATH = String.Format(@".;{0}\lib\adore-djatoka-1.1.jar;{0}\lib\commons-cli-1.1.jar;{0}\lib\ij.jar;{0}\lib\jai_codec.jar;{0}\lib\jai_core.jar;{0}\lib\kdu_jni.jar;{0}\lib\log4j-1.2.8.jar;{0}\lib\mlibwrapper_jai.jar;{0}\lib\oom.jar;{0}\lib\oomRef.jar;{0}\lib\uk.co.mmscomputing.imageio.tiff.jar;{0}\lib\ij-ImageIO.jar", DjatokaHome);
                string cmd = String.Format(@"""{0}""", JavaHome);

                p.StartInfo.FileName = cmd;
                p.StartInfo.Arguments = String.Format("{2} -classpath {3} gov.lanl.adore.djatoka.DjatokaCompress -i\"{0}\" -o\"{1}\"", input_file, output_file, JAVA_OPTS, CLASSPATH);

                try
                {
                    p.Start();
                }
                catch (Exception exp)
                {
                    LogHelper.StatsLog(null, "createJpeg2000() process start failed: " + exp.Message, String.Format("{0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments), null, null);
                }

                LogHelper.StatsLog(null, "createJpeg2000()", String.Format("{0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments), null, null);

                using (StreamReader error = p.StandardError)
                {
                    var error_output = error.ReadToEnd();
                    p.WaitForExit();

                    if ((error_output.Length > 0) && (error_output.Length != 153))
                        throw new Exception("error detected: " + error_output);


                    LogHelper.StatsLog(null, "createJpeg2000()", "Ok!: " + output_file, null, null);
                }

                var jpeg2000_identifier = String.Format("{0}{1}", Jpeg2000Namespace,
                                                        Path.GetFileNameWithoutExtension(output_file));

                var db = new DjatokaDataContext();
                var r = new DjatokaResource { identifier = jpeg2000_identifier, imageFile = output_file };
                db.DjatokaResources.InsertOnSubmit(r);
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                LogHelper.StatsLog(null, "createJpeg2000()", "createJpeg2000() failed: " + ex.Message, null, null);
                throw new Exception("createJpeg2000() error", ex.InnerException);
            }
        }

        [DebuggerNonUserCodeAttribute]
        static void onBw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                LogHelper.StatsLog(null, "onBw_RunWorkerCompleted", "File processed ok", null, null);
            else
                LogHelper.StatsLog(null, "onBw_RunWorkerCompleted", "Problem processing file " + e.Result, null, null);

            if ((!IsBwBusy) && (_bwFiles.Count > 0))
                _bw.RunWorkerAsync();
        }

        public static String GetFileExtension(String file_path)
        {
            char[] c = { '.' };
            String[] segments = file_path.Split(c);
            return segments[(segments.Length - 1)];
        }

        public static String StripFileExtension(String file_path)
        {
            int p = file_path.LastIndexOf(".");
            file_path = file_path.Remove(p, file_path.Length - p);
            return file_path;
        }

        public static Stream GetImageStream(string img_src, string image_not_found_src)
        {
            try
            {
                return WebRequest.Create(img_src).GetResponse().GetResponseStream();
            }
            catch (Exception e)
            {
                LogHelper.StatsLog(null, "GetImageStream", String.Format("Error: WebRequest stream for {0}, {1}", img_src, e.Message), null, null);
                try
                {
                    return WebRequest.Create(image_not_found_src).GetResponse().GetResponseStream();
                }
                catch (Exception ex)
                {
                    LogHelper.StatsLog(null, "GetImageStream", String.Format("Error level 2: WebRequest stream for {0}, {1}", img_src, ex.Message), null, null);
                    return null;
                }
            }
        }

        public static byte[] GetImageBytes(string img_src, string img_not_found_src)
        {
            using (Stream s = ImageHelper.GetImageStream(img_src, img_not_found_src))
            {
                return ReadFully(s);
            }
        }
    }
}