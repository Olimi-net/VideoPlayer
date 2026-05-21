using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/**
 * @Author Lada 
 * @Created 2018.08.22 
 */

namespace VideoPlayer
{
    public class PipeModule
    {
        public static string SortPipe(string[] files)
        {
            var date = DateTime.Today;
            string fname = "";

            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                var dt = fi.CreationTime;
                if (dt > date)
                {
                    date = dt;
                    fname = f;
                }
            }
            string res = ReadPipe(fname);

            foreach (var f in files)
            {
                DeleteFile(f);
            }
            return res;
        }

        public static void WritePipe(string fname, string w)
        {
            try
            {
                using (var sw = new StreamWriter(fname))
                {
                    sw.WriteLine(w);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static string ReadPipe(string fname)
        {
            try
            {
                using (var sr = new StreamReader(fname))
                {
                    return sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        private static void DeleteFile(string fname)
        {
            try
            {
                File.Delete(fname);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
