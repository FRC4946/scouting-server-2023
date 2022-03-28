using BluetoothLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowedApplication
{
    /// <summary>
    /// Main form for application
    /// </summary>
    public partial class MainForm : Form
    {

        /// <summary>
        /// Get manager for this window
        /// </summary>
        public CSVManager Manager { get => _Manager; }

        CSVManager _Manager;

        private bool _DesiredRunning = false;

        private bool _TreeInitialized = false;

        private TreeNode _Application, _Bluetooth, _Logging, _AppRunning, _BluetoothEnabled, _MAC, _ListenerRunning, _GUID,
            _LogFile, _LoggerRunning;

        public MainForm()
        {
            InitializeComponent();
        }

        private void strip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "run")
            {
                // run button clicked
                run_Clicked(e.ClickedItem);
            }
        }

        /// <summary>
        /// Determines whether the manager should be
        /// </summary>
        /// <returns>
        /// whether the manager should be running
        /// </returns>
        private bool _Running()
        {
            return _DesiredRunning;
        }

        /// <summary>
        /// Logic periodically called from ui update timer
        /// </summary>
        private void _PeriodicLogic()
        {
            if (_Running())
            {
                if (!Manager.Listening)
                {
                    if (Bluetooth.Enabled)
                        Manager.Listen();
                }
                if (!Manager.Logging)
                {
                    Manager.Log();
                }
            }
        }

        /// <summary>
        /// Sets up the nodes on the status tree
        /// </summary>
        private void _SetupStatusTree()
        {
            statusTree.BeginUpdate();
            if (_TreeInitialized)
                return; // end if tree is already set up
            _TreeInitialized = true;
            // handle status tree
            statusTree.Nodes.Clear();

            // application node
            _Application = new TreeNode("Application");

            _AppRunning = new TreeNode("App Running");

            _Application.Nodes.AddRange(new TreeNode[] { _AppRunning });

            // bluetooth node
            _Bluetooth = new TreeNode("Bluetooth");

            _ListenerRunning = new TreeNode("Listener");

            _GUID = new TreeNode("GUID");

            _BluetoothEnabled = new TreeNode("Bluetooth Enabled");

            _MAC = new TreeNode("MAC");

            _Bluetooth.Nodes.AddRange(new TreeNode[] { _BluetoothEnabled, _MAC, _GUID,
                _ListenerRunning });

            // logging node
            _Logging = new TreeNode("Logging");

            _LogFile = new TreeNode("Log File");

            _LoggerRunning = new TreeNode("Logger");

            _Logging.Nodes.AddRange(new TreeNode[] { _LogFile, _LoggerRunning });

            // add to tree
            statusTree.Nodes.AddRange(new TreeNode[] { _Application, _Bluetooth, _Logging });
        }

        /// <summary>
        /// Updates the appearance of ui items based on manager state
        /// </summary>
        private void _UpdateStatus()
        {
            _SetupStatusTree();
            _BluetoothEnabled.Text = "Bluetooth: " + (Bluetooth.Enabled ? "Enabled" : "Disabled");
            _MAC.Text = $"MAC: {Bluetooth.MAC}";
            if (!_Running())
            {
                // not running
                run.Text = "RUN";
                managerStatus.Text = "Server: Not Running";
                loggerStatus.Text = "Logger: Not Running";
                errors.Items.Clear();
                errors.Items.Add("Status: Not Running");
                connections.Items.Clear();

                // tree nodes
                _AppRunning.Text = "Status: Not Running";

                _ListenerRunning.Text = "Listener: Not Running";
                _GUID.Text = "GUID: Not Running";

                _LogFile.Text = "Log File: Not Running";
                _LoggerRunning.Text = "Logger: Not Running";
            }
            else
            {
                // should be running
                run.Text = "STOP";
                _AppRunning.Text = "Status: Running";

                // listener status
                if (!Bluetooth.Enabled)
                {
                    managerStatus.Text = "Server: Bluetooth Not Enabled";
                    _ListenerRunning.Text = "Listener: Bluetooth Not Enabled";
                    _GUID.Text = "GUID: Bluetooth Not Enabled";
                }
                else if (Manager == null)
                {
                    managerStatus.Text = "Server: Manager Not Initialized";
                    _ListenerRunning.Text = "Listener: Manager Not Initialized";
                    _GUID.Text = "GUID: Manager Not Initialized";
                }
                else if (!Manager.Listening)
                {
                    managerStatus.Text = "Server: Manager Listener Not Running";
                    _ListenerRunning.Text = "Listener: Not Running";
                    _GUID.Text = "GUID: " + Manager.UUID.ToString();
                }
                else
                {
                    managerStatus.Text = $"Server: Running On {Bluetooth.MAC}";
                    _ListenerRunning.Text = "Listener: Running";
                    _GUID.Text = "GUID: " + Manager.UUID.ToString();
                }

                // logger status
                if (Manager == null)
                {
                    loggerStatus.Text = "Logger: Manager Not Initialized";
                    _LogFile.Text = "Log File: Manager Not Initialized";
                    _LoggerRunning.Text = "Logger: Manager Not Initialized";
                }
                else if (Manager.Logging)
                {
                    var fileName = Manager.CurrentLog.FileName;
                    _LogFile.Text = "Log File: " + fileName;
                    fileName = fileName.Substring(fileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    loggerStatus.Text = $"Logger: Running On {fileName}";
                    _LoggerRunning.Text = "Logger: Running";
                }
                else
                {
                    loggerStatus.Text = "Logger: Manager Logger Not Running";
                    _LogFile.Text = "Log File: " + Manager.CurrentLog.FileName;
                    _LoggerRunning.Text = "Logger: Not Running";
                }

                // errors
                errors.BeginUpdate();
                errors.Items.Clear();
                if (Manager == null)
                {
                    errors.Items.Add("Error: Manager Not Initialized");
                }
                else
                {
                    for (int i = 0; i < _Manager.ErrorCount; i++)
                    {
                        errors.Items.Add(_Manager.GetErrorAt(i));
                    }
                }
                errors.EndUpdate();

                // connections
                connections.BeginUpdate();
                connections.Items.Clear();
                if (Manager != null)
                {
                    foreach (var b in Manager)
                    {
                        connections.Items.Add("Connection: " + b.DeviceName);
                    }
                }
                connections.EndUpdate();
            }
            statusTree.EndUpdate();
        }

        /// <summary>
        /// Starts the listener and other components for this application
        /// </summary>
        /// <param name="fileName">
        /// filename to start with
        /// </param>
        private void _Start(string fileName)
        {
            _DesiredRunning = true;
            _Manager = new CSVManager(fileName);

            try
            {
                _Manager.Log();
                if (Bluetooth.Enabled)
                    _Manager.Listen();
            }
            catch
            {

            }
        }

        /// <summary>
        /// Stops the listener and other components
        /// </summary>
        private void _Stop()
        {
            _DesiredRunning = false;
            _Manager?.StopListening();
            _Manager?.StopLogging();
            _Manager = null;
        }

        private void run_Clicked(ToolStripItem i)
        {
            if (!_Running())
            {
                using (var d = new SaveFileDialog())
                {
                    d.Filter = "Comma Separated Values (*.csv)|*.csv";
                    if (d.ShowDialog() == DialogResult.OK)
                    {
                        _Start(d.FileName);   
                    }
                }
            } 
            else
            {
                _Stop();
            }
            _UpdateStatus();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            refreshTimer.Start();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            _UpdateStatus();
            _PeriodicLogic();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _Stop();
        }
    }
}
