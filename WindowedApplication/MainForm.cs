using BluetoothLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        /// Determines whether the manager is running
        /// </summary>
        /// <returns>
        /// whether the manager is running
        /// </returns>
        private bool _Running()
        {
            return _Manager != null && _Manager.Listening && Manager.Logging;
        }

        /// <summary>
        /// Updates the appearance of ui items based on manager state
        /// </summary>
        private void _UpdateStatus()
        {
            var b = _RunButton();
            var s = _Status();
            if (_Running())
            {
                b.Text = "STOP";
                s.Text = $"Server: Running On {Bluetooth.MAC}";
                errors.BeginUpdate();
                errors.Items.Clear();
                for (int i = 0; i < _Manager.ErrorCount; i++)
                {
                    errors.Items.Add(_Manager.GetErrorAt(i));
                }
                errors.EndUpdate();
            }
            else
            {
                b.Text = "RUN";
                s.Text = "Server: Not Running";
                errors.Items.Clear();
                errors.Items.Add("Not Running");
            }
        }

        /// <summary>
        /// Gets the run button
        /// </summary>
        /// <returns>
        /// the run button
        /// </returns>
        private ToolStripItem _RunButton()
        {
            if (strip.Items.Count > 0)
                return strip.Items[0];
            return null;
        }

        /// <summary>
        /// Gets the status bar
        /// </summary>
        /// <returns>
        /// the status bar
        /// </returns>
        private ToolStripItem _Status()
        {
            if (status.Items.Count > 0)
                return status.Items[0];
            return null;
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
                        _Manager = new CSVManager(d.FileName);
                        _Manager.Log();
                        _Manager.Listen();
                    }
                }
            } 
            else
            {
                _Manager.StopListening();
                _Manager.StopLogging();
                _Manager = null;
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
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Manager?.StopListening();
            Manager?.StopLogging();
        }
    }
}
