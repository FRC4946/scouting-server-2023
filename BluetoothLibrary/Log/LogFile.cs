using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLibrary.Log
{
    /// <summary>
    /// Static class for log files
    /// </summary>
    public static class LogFile
    {

        /// <summary>
        /// Whether a log file at a path exists
        /// </summary>
        /// <param name="path">
        /// The log file path to look at
        /// </param>
        /// <returns>
        /// True if exists
        /// </returns>
        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Whether the log file at the specified path is writeable
        /// </summary>
        /// <param name="path">
        /// path to check at
        /// </param>
        /// <returns>
        /// true if writeable
        /// </returns>
        public static bool Writeable(string path)
        {
            FileStream f = null;
            try
            {
                f = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            } 
            catch
            {
                return false;
            }
            finally
            {
                if (f != null)
                    f.Close();
            }
            return true;
        }

        /// <summary>
        /// Gets a new available path from the specified path base and extension
        /// </summary>
        /// <param name="pathBase">
        /// base to use
        /// </param>
        /// <param name="extension">
        /// extension to use
        /// </param>
        /// <returns>
        /// a path that is writeable
        /// </returns>
        public static string GetNewName(string pathBase, string extension)
        {
            var path = pathBase + "." + extension;
            if (!Exists(path) || Writeable(path))
                return path;

            uint i = 0;

            do
            {
                path = pathBase + " (" + ++i + ")." + extension;
            } 
            while (Exists(path) && !Writeable(path));

            return path;
        }

    }
}
