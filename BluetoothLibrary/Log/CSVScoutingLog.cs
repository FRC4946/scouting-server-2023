using BluetoothLibrary.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLibrary.Log
{
    /// <summary>
    /// Scouting log for CSV files
    /// </summary>
    /// <typeparam name="T">
    /// type of scouting message for the log
    /// </typeparam>
    public class CSVScoutingLog<T> : ScoutingLogBase<T> where T : ScoutingMessageBase
    { 

        /// <summary>
        /// Current filename for scouting log
        /// </summary>
        public string FileName { get; } 

        /// <summary>
        /// Whether the current log file exists
        /// </summary>
        public bool Exists { get => LogFile.Exists(FileName); }

        /// <summary>
        /// Whether the current log file is writeable
        /// </summary>
        public bool Writeable { get => LogFile.Writeable(FileName); }

        /// <summary>
        /// Whether the current file has correct headers
        /// </summary>
        public bool CorrectHeader { get => _CorrectHeader(FileName); }

        /// <summary>
        /// Creates a new csv scouting log
        /// </summary>
        /// <param name="s">
        /// filename for the log
        /// </param>
        public CSVScoutingLog(string s)
        {
            FileName = s;
        }

        /// <summary>
        /// Makes the provided enumerable of strings into a CSV line
        /// </summary>
        /// <param name="s">
        /// enumerable of strings
        /// </param>
        /// <returns>
        /// CSV representation of strings
        /// </returns>
        private static string _MakeCSV(IEnumerable<string> s)
        {
            return s.Aggregate((a, b) => $"{a},{b}") + ",\n";
        }

        public override bool Exit()
        {
            return true;
        }

        /// <summary>
        /// Gets a useable csv path
        /// </summary>
        /// <param name="pathBase">
        /// path base to look in
        /// </param>
        /// <returns>
        /// useable path within specified path
        /// </returns>
        public string GetUseablePath(string pathBase)
        {
            var path = pathBase + ".csv";
            if (!LogFile.Exists(path) || (LogFile.Writeable(path) && _CorrectHeader(path)))
                return path;

            uint i = 0;

            do
            {
                path = pathBase + " (" + ++i + ").csv";
            }
            while (LogFile.Exists(path) && (!LogFile.Writeable(path) || !_CorrectHeader(path)));

            return path;
        }

        /// <summary>
        /// return whether the file at the specified path has the correct headers
        /// </summary>
        /// <param name="path">
        /// file to look for headers in
        /// </param>
        /// <returns>
        /// whether the file has the correct headers
        /// </returns>
        private bool _CorrectHeader(string path)
        {
            var headers = _MakeCSV(ScoutingMessageBase.GetHeaders<T>());
            try
            {
                var t = File.ReadAllText(path);
                return t.StartsWith(headers);
            }
            catch
            {
                return false;
            }
        }

        public override bool Init()
        {
            var headers = _MakeCSV(ScoutingMessageBase.GetHeaders<T>());
            if (!Exists)
            {
                // file doesn't exist, create it
                try
                {
                    File.WriteAllText(FileName, headers);
                    return true;
                }    
                catch
                {
                    return false;
                } 
            }
            return Writeable && CorrectHeader;
        }

        public override bool Write(ScoutingMessageBase b)
        {
            if (!Writeable)
                return false;

            try
            {
                File.AppendAllText(FileName, _MakeCSV(b.GetFields()));
                return true;
            } 
            catch
            {
                return false;
            }
        }
    }
}
