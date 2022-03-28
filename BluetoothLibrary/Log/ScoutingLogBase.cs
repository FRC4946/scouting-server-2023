using BluetoothLibrary.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLibrary.Log
{
    /// <summary>
    /// Base for scouting logs, shouldn't keep any resources open
    /// </summary>
    /// <typeparam name="T">
    /// type of scouting message for this scouting log
    /// </typeparam>
    public abstract class ScoutingLogBase<T> where T : ScoutingMessageBase
    {
        /// <summary>
        /// Setup this scouting log
        /// </summary>
        /// <returns>
        /// Whether this method runs correctly
        /// </returns>
        public abstract bool Init();

        /// <summary>
        /// Writes a scouting message to this log
        /// </summary>
        /// <param name="b">
        /// The message to write
        /// </param>
        /// <returns>
        /// Whether this method runs correctly
        /// </returns>
        public abstract bool Write(ScoutingMessageBase b);

        /// <summary>
        /// Exit this scouting log
        /// </summary>
        /// <returns>
        /// Whether this method runs correctly
        /// </returns>
        public abstract bool Exit();
    }
}
