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
    public abstract class ScoutingMessageBase
    {

        /// <summary>
        /// Creates a new scouting message from the specified csv
        /// </summary>
        /// <typeparam name="T">
        /// type of scouting message to create
        /// </typeparam>
        /// <param name="csv">
        /// csv to create scouting message with
        /// </param>
        /// <returns>
        /// the created scouting message
        /// </returns>
        public static T FromCSV<T>(string csv) where T : ScoutingMessageBase, new()
        {
            if (csv.EndsWith("\n"))
                csv = csv.Remove(csv.Length - 1, 1);
            var t = new T();
            var fields = csv.Split(',');
            var info = _GetScoutingProperties(typeof(T));
            foreach (var p in info)
            {
                var index = (int)p.scouting.Index;
                if (index < fields.Length)
                    p.property.SetValue(t, fields[index]);
                else
                    throw new ArgumentException("Invalid CSV string passed to from CSV");
            }
            return t;
        }

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
        private static IEnumerable<string> _GetHeaders(Type t)
        {
            return _GetScoutingProperties(t)
                .OrderBy(x => x.scouting.Index)
                .Select(x => x.scouting.Header);
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
        public static IEnumerable<string> GetHeaders<T>() where T : ScoutingMessageBase
        {
            return _GetHeaders(typeof(T));
        }

        /// <summary>
        /// Gets the headers for this scouting message
        /// </summary>
        /// <returns>
        /// headers for this scouting message
        /// </returns>
        public IEnumerable<string> GetHeaders()
        {
            return _GetHeaders(GetType());
        }

        /// <summary>
        /// Gets the fields for this scouting message
        /// </summary>
        /// <returns>
        /// Fields for this scouting message
        /// </returns>
        public IEnumerable<string> GetFields()
        {
            return _GetScoutingProperties(GetType())
                .OrderBy(x => x.scouting.Index)
                .Select(x => (string)x.property.GetValue(this));
        }

        /// <summary>
        /// Gets the fields and header for this scouting message
        /// </summary>
        /// <returns>
        /// enumerable of fields for this scouting message
        /// </returns>
        public IEnumerable<Field> GetFieldsAndHeader()
        {
            return _GetScoutingProperties(GetType())
                .OrderBy(x => x.scouting.Index)
                .Select(x => new Field(x.scouting.Header, 
                (string)x.property.GetValue(this)));
        }
    }
}
