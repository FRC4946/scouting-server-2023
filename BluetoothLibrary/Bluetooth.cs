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

    }
}
