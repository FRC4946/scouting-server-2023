using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLibrary.Messages
{
    /// <summary>
    /// Base for a scouting message
    /// </summary>
    public class ScoutingMessageBase
    {

        /// <summary>
        /// Gets the scouting properties for this message base
        /// </summary>
        /// <param name="t">
        /// Type of scouting message to use
        /// </param>
        /// <returns>
        /// Array of property info for this scouting message
        /// </returns>
        private static (PropertyInfo property, ScoutingProperty scouting)[] _GetScoutingProperties(Type t)
        {
            return t.GetProperties()
                .Select(x => (x, x.GetCustomAttribute(typeof(ScoutingProperty)) as ScoutingProperty))
                .Where(x => x.Item2 != null)
                .ToArray();
        }

        /// <summary>
        /// Gets the headers for this scouting message
        /// </summary>
        /// <param name="t">
        /// Type of scouting message to get headers for
        /// </param>
        /// <returns>
        /// the headers for this scouting message
        /// </returns>
        private static string _GetHeaders(Type t)
        {
            return _MakeCSV(_GetScoutingProperties(t)
                .OrderBy(x => x.scouting.Index)
                .Select(x => x.scouting.Header));
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

        /// <summary>
        /// Gets the headers for this scouting message
        /// </summary>
        /// <typeparam name="T">
        /// type of scouting message to get headers for
        /// </typeparam>
        /// <returns>
        /// the headers for this scouting message
        /// </returns>
        public static string GetHeaders<T>() where T : ScoutingMessageBase
        {
            return _GetHeaders(typeof(T));
        }

        /// <summary>
        /// Gets the headers for this scouting message
        /// </summary>
        /// <returns>
        /// headers for this scouting message
        /// </returns>
        public string GetHeaders()
        {
            return _GetHeaders(GetType());
        }

        /// <summary>
        /// Gets the CSV representation of this scouting message
        /// </summary>
        /// <returns>
        /// CSV representation of this scouting message
        /// </returns>
        public string GetCSV()
        {
            return _MakeCSV(_GetScoutingProperties(GetType())
                .OrderBy(x => x.scouting.Index)
                .Select(x => (string)x.property.GetValue(this)));
        }
    }
}
