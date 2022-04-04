using System;
using System.IO.Ports;
using System.Reflection;
using System.Windows.Forms;
using UCNLDrivers;
using UCNLNMEA;

namespace RedPhoneDXConfig
{
    public partial class MainForm : Form
    {
        #region Custom items

        enum LocalErrorID
        {
            LOC_ERR_NO_ERROR = 0,
            LOC_ERR_INVALID_SYNTAX = 1,
            LOC_ERR_UNSUPPORTED = 2,
            LOC_ERR_TRANSMITTER_BUSY = 3,
            LOC_ERR_ARGUMENT_OUT_OF_RANGE = 4,
            LOC_ERR_INVALID_OPERATION = 5,
            LOC_ERR_UNKNOWN_FIELD_ID = 6,
            LOC_ERR_VALUE_UNAVAILIBLE = 7,
            LOC_ERR_CHKSUM_ERROR = 8,
            LOC_ERR_UNKNOWN
        }

        #endregion


        #region Properties

        double[] freqs = new double[] { 32768.0, 32768.0, 31250.0, 31250.0, 28500.0, 28500.0, 25000.0, 25000.0, 32000.0, 32000.0 };
        string[] bands = new string[] { "LOW", "HIGH", "LOW", "HIGH", "LOW", "HIGH", "LOW", "HIGH" };

        string portName
        {
            get
            {
                if (portNameCbx.SelectedIndex >= 0)
                    return portNameCbx.SelectedItem.ToString();
                else
                    return string.Empty;
            }
            set
            {
                var idx = portNameCbx.Items.IndexOf(value);
                if (idx >= 0)
                    portNameCbx.SelectedIndex = idx;
            }
        }

        NMEASerialPort port;
        PrecisionTimer timer;

        readonly int timeoutCntMax = 5;
        int timeoutCnt = 0;

        string lastMsg = string.Empty;
        string cActionDescription = string.Empty;

        bool deviceInfoUpdated = false;

        int ssbChannelID
        {
            get { return Convert.ToInt32(channelIDEdit.Value) - 1; }
            set 
            {
                if (channelIDEdit.InvokeRequired)
                    channelIDEdit.Invoke((MethodInvoker)delegate { channelIDEdit.Value = value + 1; });
                else
                    channelIDEdit.Value = value + 1;
            }
        }

        bool isRWLTEnabled
        {
            get { return rwltEnabledChb.Checked; }
            set 
            {
                if (rwltEnabledChb.InvokeRequired)
                    rwltEnabledChb.Invoke((MethodInvoker)delegate { rwltEnabledChb.Checked = value; });
                else
                    rwltEnabledChb.Checked = value;
            }
        }

        int rwltDiverID
        {
            get { return Convert.ToInt32(rwltDiverIDEdit.Value); }
            set 
            {
                if (rwltDiverIDEdit.InvokeRequired)
                    rwltDiverIDEdit.Invoke((MethodInvoker)delegate { rwltDiverIDEdit.Value = value; });
                else
                    rwltDiverIDEdit.Value = value; 
            }
        }

        int rwltChannelID
        {
            get { return Convert.ToInt32(rwltChannelIDEdit.Value); }
            set 
            {
                if (rwltChannelIDEdit.InvokeRequired)
                    rwltChannelIDEdit.Invoke((MethodInvoker)delegate { rwltChannelIDEdit.Value = value; });
                else
                    rwltChannelIDEdit.Value = value; 
            }
        }

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();

            this.Text = string.Format("{0} v{1}", Application.ProductName, Assembly.GetExecutingAssembly().GetName().Version);

            NMEAParser.AddManufacturerToProprietarySentencesBase(ManufacturerCodes.RPH);

            NMEAParser.AddProprietarySentenceDescription(ManufacturerCodes.RPH, "0", "c--c,x");
            NMEAParser.AddProprietarySentenceDescription(ManufacturerCodes.RPH, "1", "x,x,x,x");
            NMEAParser.AddProprietarySentenceDescription(ManufacturerCodes.RPH, "?", "x");
            NMEAParser.AddProprietarySentenceDescription(ManufacturerCodes.RPH, "!", "c--c,c--c,x,c--c,x,x,x,x,x");

            refreshPortsBtn_Click(null, null);

            timer = new PrecisionTimer();
            timer.Mode = Mode.Periodic;
            timer.Period = 3000;
            timer.Tick += (o, e) =>
                {
                    if (++timeoutCnt > timeoutCntMax)
                    {
                        timer.Stop();
                        InvokeSetStatusText(string.Format("{0} failed", cActionDescription));
                    }
                    else
                    {
                        TrySend(lastMsg);                        
                    }
                };
            timer.Started += (o, e) =>
                {
                    timeoutCnt = 1;
                    InvokeSetEnabledState(connectionBtn, false);
                    InvokeSetEnabledState(querySettingsBtn, false);
                    InvokeSetEnabledState(applySettingsBtn, false);

                    InvokeSetEnabledState(deviceInfoGroup, false);
                };
            timer.Stopped += (o, e) =>
                {
                    InvokeSetEnabledState(connectionBtn, true);
                    InvokeSetEnabledState(querySettingsBtn, port.IsOpen);
                    InvokeSetEnabledState(applySettingsBtn, port.IsOpen && deviceInfoUpdated);

                    InvokeSetEnabledState(deviceInfoGroup, port.IsOpen);
                };

            isRWLTEnabled = false;
            ssbChannelID = 0;
            
        }

        #endregion
        
        #region Methods

        private void InvokeSetEnabledState(Control ctrl, bool enabled)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke((MethodInvoker)delegate { ctrl.Enabled = enabled; });
            else
                ctrl.Enabled = enabled;
        }

        private void InvokeSetText(Control ctrl, string text)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke((MethodInvoker)delegate { ctrl.Text = text; });
            else
                ctrl.Text = text;
        }

        private void InvokeSetStatusText(string text)
        {
            if (statusStrip.InvokeRequired)
                statusStrip.Invoke((MethodInvoker)delegate { statusLbl.Text = text; });
            else
                statusLbl.Text = text;
        }

        private void StartSend(string msg, string actionDescription)
        {
            cActionDescription = actionDescription;
            lastMsg = msg;
            timer.Start();
            TrySend(msg);
            
        }

        private void TrySend(string msg)
        {
            InvokeSetStatusText(string.Format("{0} (try {1} of {2})...", cActionDescription, timeoutCnt, timeoutCntMax));

            if ((port != null) && (port.IsOpen))
            {
                try
                {
                    port.SendData(msg);                                  
                }
                catch (Exception ex)
                {
                    //
                }
            }            
        }


        private void Parse_ACK(object[] parameters)
        {
            bool isOK = false;

            string sntID = string.Empty;
            LocalErrorID errorID = LocalErrorID.LOC_ERR_UNKNOWN;

            try
            {
                sntID = (string)parameters[0];
                errorID = (LocalErrorID)(int)parameters[1];

                isOK = true;
            }
            catch
            {

            }

            if (isOK)
            {
                timer.Stop();
                InvokeSetStatusText("ACK received");

                if (sntID == "1")
                {
                    if (errorID == LocalErrorID.LOC_ERR_NO_ERROR)
                    {
                        InvokeSetStatusText("Settings successfully updated");
                        MessageBox.Show("Settings successfully updated", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);                         
                    }
                }
            }            
        }

        private void Parse_DINFO(object[] parameters)
        {                        
            //serialNumber,sys_moniker,sys_version,core_moniker [release],core_version,ssbCH,isRWLT,RWLT_diverID,RWLT_channelID
            string serialNumber = string.Empty;
            string sysMoniker = string.Empty;
            int sysVersion = -1;
            string coreMoniker = string.Empty;
            int coreVersion = -1;
            int ssbCh = -1;
            bool isRWLT = false;
            int rwltDiver = 0;
            int rwltCh = 0;

            bool isOK = false;

            try
            {
                serialNumber = (string)parameters[0];
                sysMoniker = (string)parameters[1];
                sysVersion = (int)parameters[2];
                coreMoniker = (string)parameters[3];
                coreVersion = (int)parameters[4];
                ssbCh = (int)parameters[5];
                isRWLT = Convert.ToBoolean((int)parameters[6]);
                rwltDiver = (int)parameters[7];
                rwltCh = (int)parameters[8];

                isOK = true;
            }
            catch 
            {

            }

            if (isOK)
            {
                timer.Stop();
                InvokeSetStatusText("Device info received");

                ssbChannelID = ssbCh;
                isRWLTEnabled = isRWLT;
                rwltDiverID = rwltDiver;
                rwltChannelID = rwltCh;

                InvokeSetText(deviceInfoTxb, string.Format("S/N: {0}\r\nSYSTEM: {1} v{2}\r\nCORE: {3} v{4}",
                    serialNumber, sysMoniker, BCDVersionToStr(sysVersion), coreMoniker, BCDVersionToStr(coreVersion)));

                deviceInfoUpdated = true;

                InvokeSetEnabledState(applySettingsBtn, true);
                InvokeSetEnabledState(deviceInfoGroup, true);
                InvokeSetEnabledState(settingsGroup, true);
            }
            else
            {
                InvokeSetStatusText("Device info incorrect");
            }
        }

        private string BCDVersionToStr(int versionData)
        {
            return string.Format("{0}.{1}", (versionData >> 0x08).ToString(), (versionData & 0xff).ToString("X2"));
        }


        #endregion

        #region Handlers

        private void refreshPortsBtn_Click(object sender, EventArgs e)
        {
            var portNames = SerialPort.GetPortNames();
            portNameCbx.Items.Clear();

            if ((portNames != null) && (portNames.Length > 0))
            {
                portNameCbx.Items.AddRange(portNames);
                portNameCbx.SelectedIndex = 0;

                connectionBtn.Enabled = true;
            }
        }

        private void connectionBtn_Click(object sender, System.EventArgs e)
        {
            if ((port == null) || (!port.IsOpen))
            {
                port = new NMEASerialPort(new SerialPortSettings(portName, BaudRate.baudRate9600, Parity.None, DataBits.dataBits8, StopBits.One, Handshake.None));
                port.NewNMEAMessage += new EventHandler<NewNMEAMessageEventArgs>(port_NewNMEAMessageReceived);

                try
                {
                    port.Open();

                    deviceInfoTxb.Clear();

                    ssbChannelID = 0;
                    isRWLTEnabled = false;

                    deviceInfoUpdated = false;
                    connectionBtn.Text = "CONNECTION ✓";
                    portNameCbx.Enabled = false;
                    refreshPortsBtn.Enabled = false;

                    querySettingsBtn.Enabled = true;
                    applySettingsBtn.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                querySettingsBtn_Click(null, null);
            }
            else
            {
                if (timer.IsRunning)
                    timer.Stop();

                try
                {
                    port.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                connectionBtn.Text = "CONNECTION";
                portNameCbx.Enabled = true;
                refreshPortsBtn.Enabled = true;
                querySettingsBtn.Enabled = false;
                applySettingsBtn.Enabled = false;
                settingsGroup.Enabled = false;

            }
        }

        private void port_NewNMEAMessageReceived(object sender, NewNMEAMessageEventArgs e)
        {
            NMEASentence _result = null;
            bool parsed = false;
            try
            {
                _result = NMEAParser.Parse(e.Message);
                parsed = true;
            }
            catch
            {

            }

            if ((parsed) && (_result is NMEAProprietarySentence))
            {
                NMEAProprietarySentence pSentence = (NMEAProprietarySentence)_result;

                if (pSentence.Manufacturer == ManufacturerCodes.RPH)
                {
                    if (pSentence.SentenceIDString == "0")
                        Parse_ACK(pSentence.parameters);
                    else if (pSentence.SentenceIDString == "!")
                        Parse_DINFO(pSentence.parameters);
                }                
            }
        }

        private void querySettingsBtn_Click(object sender, EventArgs e)
        {
            if (port.IsOpen)
            {
                StartSend(
                    NMEAParser.BuildProprietarySentence(ManufacturerCodes.RPH, "?", new object[] { 0 }),
                    "Querying device info");
            }
        }

        private void applySettingsBtn_Click(object sender, EventArgs e)
        {
            if (port.IsOpen)
            {
                StartSend(NMEAParser.BuildProprietarySentence(ManufacturerCodes.RPH, "1", new object[]
                {
                    ssbChannelID,
                    Convert.ToInt32(isRWLTEnabled),
                    rwltDiverID,
                    rwltChannelID
                }),
                "Applying new settings");
            }
        }

        private void rwltEnabledChb_CheckedChanged(object sender, EventArgs e)
        {
            rwltChannelIDEdit.Enabled = isRWLTEnabled;
            rwltChannelIDLbl.Enabled = isRWLTEnabled;
            rwltDiverIDEdit.Enabled = isRWLTEnabled;
            rwltDiverIDLbl.Enabled = isRWLTEnabled;
        }

        private void channelIDEdit_ValueChanged(object sender, EventArgs e)
        {
            channelDescriptionLbl.Text = string.Format("{0:F00} Hz, {1} SIDE BAND", freqs[ssbChannelID], bands[ssbChannelID]);
        }

        #endregion
    }
}
