using BluetoothLibrary;
using BluetoothLibrary.Log;
using BluetoothLibrary.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowedApplication
{
    public class CSVManager : InstanceManagerBase
    {

        /// <summary>
        /// The number of errors to keep track of
        /// </summary>
        public uint ErrorMax { get; set; } = 30;

        private string _DesiredName;

        /// <summary>
        /// The current log for this manager
        /// </summary>
        public CSVScoutingLog<Message2022> CurrentLog { get => _Log; }

        private List<string> _Errors = new List<string>();

        private readonly object ERROR_LOCK = new object();

        private CSVScoutingLog<Message2022> _Log;

        /// <summary>
        /// Gets the number of errors currently
        /// </summary>
        public int ErrorCount { get => _Errors.Count; }

        /// <summary>
        /// File path for this csv manager
        /// </summary>
        /// <param name="path">
        /// desired path for this csv manager
        /// </param>
        public CSVManager(string path)
        {
            _Log = new CSVScoutingLog<Message2022>(path);
            var sep = ".";
            var index = path.LastIndexOf(sep);
            if (index > -1)
            {
                _DesiredName = path.Substring(0, index);
            }
            else
            {
                _DesiredName = path;
            }
        }

        /// <summary>
        /// Raises an error with the specified text
        /// </summary>
        /// <param name="text">
        /// error text
        /// </param>
        private void _RaiseError(string text)
        {
            lock (ERROR_LOCK)
            {
                text = $"{DateTime.Now:HH:mm} - {text}";
                _Errors.Insert(0, text);
                if (_Errors.Count > ErrorCount)
                    _Errors.RemoveAt(_Errors.Count - 1);
            }
        }

        /// <summary>
        /// Gets the ith error
        /// </summary>
        /// <param name="i">
        /// Index of error to get
        /// </param>
        /// <returns>
        /// null if index out of bounds
        /// </returns>
        public string GetErrorAt(int i)
        {
            lock (ERROR_LOCK)
            {
                if (i < _Errors.Count && i >= 0)
                    return _Errors[i];
                return null;
            }
        }

        protected override void WriteLog(string s)
        {
            ScoutingMessageBase m;
            try
            {
                m = ScoutingMessageBase.FromCSV<Message2022>(s);
            }
            catch
            {
                // error getting scouting message
                _RaiseError("Error: Bad scouting message");
                return;
            }

            // valid scouting message

            if (!CurrentLog.Init())
            {
                _RaiseError("Error: Bad log file, creating new one");
                do
                {
                    _Log = new CSVScoutingLog<Message2022>(CurrentLog.GetUseablePath(_DesiredName));
                } while (!CurrentLog.Init()); // loop until a new log is initialized
                var name = CurrentLog.FileName;
                var index = name.LastIndexOf(Path.DirectorySeparatorChar);
                _RaiseError($"Resolved: created new log file ({name.Substring(index + 1)})");
            }

            // valid scouting message and log

            if (!CurrentLog.Write(m))
            {
                _RaiseError("Error: Couldn't write log message");
            }
        }
    }
}
