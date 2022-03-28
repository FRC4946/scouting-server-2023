using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLibrary
{
    /// <summary>
    /// Static library for managing bluetooth
    /// </summary>
    public static class Bluetooth
    {

        /// <summary>
        /// Whether bluetooth is currently enabled
        /// </summary>
        public static bool Enabled
        {
            get
            {
                var enabled = true;
                BluetoothRadio myRadio = null;
                BluetoothClient client = null;

                // if this throws no exeptions, bluetooth is enabled
                try
                {
                    // tries to get local bluetooth radio
                    myRadio = BluetoothRadio.PrimaryRadio;
                    // tries to create a new bluetooth client for the hardware
                    client = new BluetoothClient();
                }
                catch
                {
                    enabled = false;
                }

                if (myRadio == null)
                {
                    enabled = false;
                }

                return enabled;
            }
        }

        /// <summary>
        /// Gets the MAC address of this machine
        /// </summary>
        public static string MAC
        {
            get
            {
                return _Interpolate(BluetoothRadio.PrimaryRadio.LocalAddress.ToString(), 2, ':');
            }
        }

        /// <summary>
        /// Interpolates a string with the specified char
        /// </summary>
        /// <param name="s">
        /// string to interpolate
        /// </param>
        /// <param name="iteration">
        /// frequency of interpolation
        /// </param>
        /// <param name="insert">
        /// char to insert
        /// </param>
        /// <returns>
        /// the interpolated string
        /// </returns>
        private static string _Interpolate(string s, int iteration, char insert)
        {
            List<char> charList = new List<char>(s.ToCharArray());

            int counter = 0;
            int compensation = 0;

            while (counter + compensation < charList.Count)
            {
                if (counter > 0 && ((counter + compensation) % iteration) == (compensation % iteration))
                {
                    charList.Insert(counter + compensation, insert);
                    compensation += 1;
                }
                counter += 1;
            }

            return new string(charList.ToArray());

        }

    }
}
