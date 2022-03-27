using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLibrary.Messages
{
    /// <summary>
    /// Denotes a scouting property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ScoutingProperty : Attribute
    {

        /// <summary>
        /// Header for the CSV/XLSX
        /// </summary>
        public string Header { get;  }

        /// <summary>
        /// Index of this column in CSV/XLSX
        /// </summary>
        public ulong Index { get; }

        /// <summary>
        /// Creates a new scouting property
        /// </summary>
        /// <param name="index">
        /// index for this property
        /// </param>
        /// <param name="header">
        /// header for this property, cannot contain commas
        /// </param>
        public ScoutingProperty(ulong index, string header)
        {
            Index = index;

            // validate header
            if (header.Contains(','))
                throw new ArgumentException("CSV/XLSX header cannot contain comma");

            Header = header;
        }

    }
}
