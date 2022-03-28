using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothLibrary
{
    /// <summary>
    /// Abstract class for interfacing with bluetooth client
    /// </summary>
    public class BluetoothInstance
    {

        /// <summary>
        /// Gets the name of the device connected to this bluetooth instance
        /// </summary>
        public string DeviceName { get => _DeviceName; }

        /// <summary>
        /// Gets when the connection was opened
        /// </summary>
        public DateTime Opened { get => _Opened; }

        /// <summary>
        /// Gets the timespan this instance has been open for
        /// </summary>
        public TimeSpan Elapsed { get => DateTime.Now - Opened; }

        /// <summary>
        /// Gets the timespan since last accessed
        /// </summary>
        public TimeSpan LastAccessedElapsed { get => DateTime.Now - LastAccessed; }

        /// <summary>
        /// How long before a connection times out
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Maximum duration a connection should be open
        /// </summary>
        public TimeSpan MaxTime { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>
        /// When this instance was last accessed
        /// </summary>
        public DateTime LastAccessed { get => _LastAccessed; }

        /// <summary>
        /// Milliseconds to sleep when no data is on socket
        /// </summary>
        public int NoDataSleep { get; set; } = 100;

        /// <summary>
        /// Gets whether this connection is open
        /// </summary>
        public bool Open { get => _Open; }

        /// <summary>
        /// The manager for this instance
        /// </summary>
        public InstanceManagerBase Manager { get; }

        private string _DeviceName;

        private bool _Open = false;

        private DateTime _Opened, _LastAccessed;

        private readonly object CLOSE_LOCK = new object();

        private Thread _Handling;

        /// <summary>
        /// Creates a new bluetooth instance
        /// </summary>
        /// <param name="m">
        /// manager for this instance
        /// </param>
        internal BluetoothInstance(InstanceManagerBase m)
        {
            Manager = m;
        }

        /// <summary>
        /// Closes this instance if it is open
        /// </summary>
        public void Close()
        {
            lock (CLOSE_LOCK)
                _Open = false;  
        }

        /// <summary>
        /// Handles a connectino from the specified bluetooth client on a new thread
        /// </summary>
        /// <param name="conn">
        /// The connection to handle
        /// </param>
        internal void HandleConnection(BluetoothClient conn)
        {
            _Handling = new Thread(() => _HandleConnection(conn));
            _Handling.Start();
        }

        /// <summary>
        /// Handles a connection from the specified bluetooth client
        /// </summary>
        /// <param name="conn">
        /// bluetooth client to handle connection from
        /// </param>
        /// <returns>
        /// The lines received from the client
        /// </returns>
        private void _HandleConnection(BluetoothClient conn)
        {
            lock (CLOSE_LOCK)
                _Open = true;

            _Opened = DateTime.Now;
            _LastAccessed = DateTime.Now;

            var builder = new StringBuilder();
            var stream = conn.GetStream(); // byte stream between server and client

            byte[] deviceNameBytes = new byte[1024]; //bytes will be written to this array
            int bytesRead = stream.Read(deviceNameBytes, 0, deviceNameBytes.Length);

            _DeviceName = Encoding.ASCII.GetString(deviceNameBytes, 0, bytesRead);

            while (_IsReceivingState()) // as long as this connection should be receiving
            {
                if (stream.DataAvailable)
                {
                    _LastAccessed = DateTime.Now;

                    // read data
                    byte[] streamData = new byte[8192];
                    bytesRead = stream.Read(streamData, 0, streamData.Length);

                    builder.Append(Encoding.ASCII.GetString(streamData, 0, bytesRead));

                    _ProcessMessages(ref builder);
                }
                else
                {
                    Thread.Sleep(NoDataSleep);
                }
            }

            // lets the client know the message was recieved
            try
            {
                stream.Write(Encoding.ASCII.GetBytes("end"), 0, Encoding.ASCII.GetByteCount("end"));
            }
            catch
            {

            }
            finally
            {
                conn.Close(); //end the connection once the device disconnects

                lock (CLOSE_LOCK)
                    _Open = false;
            }

            // removes this instance from management
            Manager.RemoveInstance(this);
        }

        /// <summary>
        /// Processes the read data
        /// </summary>
        /// <param name="b">
        /// Stringbuilder containing the data
        /// </param>
        /// <param name="lines">
        /// list of lines to add to
        /// </param>
        private void _ProcessMessages(ref StringBuilder b)
        {
            // run while there are full lines
            var index = b.ToString().IndexOf('\n');
            while (index > -1)
            {
                var length = index + 1;
                var line = b.ToString().Substring(0, length);
                
                // check whether to close

                if (line.Trim().Replace("\n", "").Replace("\r", "") == "end")
                {
                    // end connection
                    lock (CLOSE_LOCK)
                        _Open = false;
                }

                // handle line
                Manager.Enqueue(line);

                b.Remove(0, length);

                index = b.ToString().IndexOf('\n');
            }
        }

        /// <summary>
        /// Whether this instance is in a receiving state
        /// </summary>
        /// <returns>
        /// whether this instance is in a state to receive
        /// </returns>
        private bool _IsReceivingState()
        {
            return Bluetooth.Enabled && Open && LastAccessedElapsed < Timeout && Elapsed < MaxTime;
        }
    }
}
