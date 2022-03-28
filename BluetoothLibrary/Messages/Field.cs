using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLibrary.Messages
{
    /// <summary>
    /// Class bundling together scouting field and scouting header
    /// </summary>
    public class Field
    {

        /// <summary>
        /// The value in this scouting field
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The header for this scouting field
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Creates a new field
        /// </summary>
        /// <param name="header">
        /// field header
        /// </param>
        /// <param name="value">
        /// field value
        /// </param>
        public Field(string header, string value)
        {
            Header = header;
            Value = value;
        }

    }
}
