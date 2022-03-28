using BluetoothLibrary.Log;
using BluetoothLibrary.Messages;
using InTheHand.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothLibrary
{
    /// <summary>
    /// Manager base for bluetooth instances
    /// </summary>
    public abstract class InstanceManagerBase : IEnumerable<BluetoothInstance>
    {
        /// <summary>
        /// Queue of strings to process into scouting messages
        /// </summary>
        private Queue<string> _Queue = new Queue<string>();

        /// <summary>
        /// Instances for this manager
        /// </summary>
        private List<BluetoothInstance> _Instances = new List<BluetoothInstance>();

        /// <summary>
        /// UUID for this application
        /// </summary>
        public Guid UUID { get; set; } = new Guid("{39675b0d-6dd8-4622-847f-3e5acc607e27}");

        /// <summary>
        /// Whether this manager is listening for new connections
        /// </summary>
        public bool Listening { get => _Listening; }

        private bool _Listening = false, _Logging = true;

        private Thread _Listener, _Logger;

        private readonly object LISTEN_LOCK = new object(), QUEUE_LOCK = new object(), LOG_LOCK = new object();

        private ManualResetEvent _Exit = new ManualResetEvent(false), _NewMessages = new ManualResetEvent(false);

        /// <summary>
        /// Starts a new thread that listens for connections
        /// </summary>
        public void Listen()
        {
            _Listener = new Thread(_Listen);
            _Listener.Start();
        }

        /// <summary>
        /// Enqueues the specified string for processing
        /// </summary>
        /// <param name="s">
        /// string to enqueue
        /// </param>
        internal void Enqueue(string s)
        {
            lock (QUEUE_LOCK)
                _Queue.Enqueue(s);
            lock (LOG_LOCK)
                _NewMessages.Set();
        }

        /// <summary>
        /// Listens for connections
        /// </summary>
        private void _Listen()
        {
            var listener = new BluetoothListener(UUID);
            lock (LISTEN_LOCK)
            {
                _Listening = true;
                _Exit.Reset();
            }
            listener.Start();

            while (Bluetooth.Enabled && Listening)
            {
                var t = listener.BeginAcceptBluetoothClient(_ListenCallback, listener);
                WaitHandle.WaitAny(new WaitHandle[] { t.AsyncWaitHandle, _Exit });
            }

            lock (LISTEN_LOCK)
                _Listening = false;
            listener.Stop();
        }

        /// <summary>
        /// Starts a new thread that logs messages
        /// </summary>
        public void Log()
        {
            _Logger = new Thread(_LogMessages);
            _Logger.Start();
        }

        /// <summary>
        /// Logs all messages
        /// </summary>
        private void _LogMessages()
        {
            lock (LOG_LOCK)
            {
                _Logging = true;
                _NewMessages.Set();
            }

            while (_Logging)
            {
                _NewMessages.WaitOne();

                if (_Queue.Count > 0)
                {
                    // process queue item
                    lock (QUEUE_LOCK)
                    {
                        var s = _Queue.Dequeue();
                        // process item
                        WriteLog(s);
                    }
                }
                else
                {
                    // no new items
                    lock (LOG_LOCK)
                        _NewMessages.Reset();
                }
            }

            lock (LOG_LOCK)
            {
                _Logging = false;
            }
        }

        /// <summary>
        /// Writes the specified csv string to a log
        /// </summary>
        /// <param name="s">
        /// string to write
        /// </param>
        protected abstract void WriteLog(string s);

        /// <summary>
        /// Called when a connection is made
        /// </summary>
        /// <param name="r">
        /// result from making connection
        /// </param>
        private void _ListenCallback(IAsyncResult r)
        {
            try
            {
                var l = (BluetoothListener)r.AsyncState;
                var c = l.EndAcceptBluetoothClient(r);
                _AddInstance(c);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Adds an instance to this 
        /// </summary>
        /// <param name="c">
        /// client to use
        /// </param>
        private void _AddInstance(BluetoothClient c)
        {
            var instance = new BluetoothInstance(this);
            _Instances.Add(instance);
            instance.HandleConnection(c);
        }

        /// <summary>
        /// Stops listening
        /// </summary>
        public void StopListening()
        {
            lock (LISTEN_LOCK)
            {
                _Listening = false;
                _Exit.Set();
            }
        }

        /// <summary>
        /// Stops loggin
        /// </summary>
        public void StopLogging()
        {
            lock (LOG_LOCK)
            {
                _Logging = false;
                _NewMessages.Set();
            }
        }

        public IEnumerator<BluetoothInstance> GetEnumerator()
        {
            return _Instances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
