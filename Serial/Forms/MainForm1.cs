using System;
using System.Windows;
using System.Threading;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO.Ports;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Web;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Timers;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;
using System.Net.Sockets;
using System.Configuration;

namespace Serial
{
    public partial class MainFormSerial : Form
    {
        static bool DO_NOT_GLOBAL_SAVE_CLIENT_SETTINGS_AGAIN = false;
        const int NUMBER_OF_PROFILES = 6;
        static int CurrentActiveProfile = 1; // default to PROFILE 1 on load
        SerialPort ComPort = new SerialPort();
        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);
        delegate void SetTextCallback(string text);
        public ConcurrentQueue<char> serialDataQueue;
        CreateLogFiles Log; //make global

        bool LoggingStarted = false;
        static string[] MacroDataString = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
        static ToolTip toolTip1 = new ToolTip(); 
        ChildForm FilterForm = new ChildForm();
      
        WarnForm TCPWarningForm = new WarnForm();
        WarnForm ComWarnForm = new WarnForm();

        public bool ASCII_ModeSettingsFlag = true;
        public bool ShowSingleLineSettingsFlag = false;
        public bool ExtendedASCIISettingsFlag = false;
        public bool CR_EQUAL_LFSettingsFlag = false;
        public bool ShowTimestampSettingsFlag = false;
        public bool RemovePrePendingCRLFSettingsFlag = false; 
        
        
        static int SERIALMODE       = 0;
        static int SOCKETMODESERVER = 1;
        static int SOCKETMODECLIENT = 2;
        static int SERIAL_SOCKET_MODE = SERIALMODE; 

        [DllImport("user32.dll")]
        public static extern int SendMessage(
              int hWnd,     // handle to destination window
              uint Msg,     // message
              long wParam,  // first message parameter
              long lParam   // second message parameter
              );

        private uint fPreviousExecutionState;

        AbortableBackgroundWorker SendATCommandsBackgroundWorker;

/*******************************************************************************************************************/
        void InitTCPSetting()
        { 
            object sender = null;
            EventArgs e = null;
            ServerClient_CheckedChanged( sender,  e);        
        }
/*******************************************************************************************************************/
        void CheckSettingsFileForCorruption()
        {
           try
           {
              ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
           }
           catch (ConfigurationErrorsException exception)
           {
              MessageBox.Show("Settings file corrupt, all settings will be lost");
              // handle error or tell user
              // to manually delete the file, you can do the following:
              File.Delete(exception.Filename); // this can throw an exception too, so be wary!
           }
        }
/*******************************************************************************************************************/
        public MainFormSerial()
        {            
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            CheckSettingsFileForCorruption();
            InitializeComponent();
            ComPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(COM_DataReceivedAsync);
            ScanCOMPorts();

           // this.DoubleBuffered = true;
            AddComboBoxBaudRates();
            LoadSavedSettings();
            
            WinApi.TimeBeginPeriod(1);
            


            addMacroButtonContexts();
            ProfileButton.AddContextMenuToButtonRN(31);
            addSendButtonContexts();            
            addToolTips();

            FilterForm.PassParentForm(this);
            TCPWarningForm.PassParentForm(this);
            ComWarnForm.PassParentForm(this);
            
            SearchUtil.PassFormHandle(this);
            MacroReAdjust();

            fPreviousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED | NativeMethods.ES_AWAYMODE_REQUIRED | NativeMethods.ES_DISPLAY_REQUIRED);

            if (fPreviousExecutionState == 0)
            {
                Console.WriteLine("SetThreadExecutionState failed. Do something here...");
                Close();
            }

            RestoreConnectionStates();             

            MainTextBox.AddContextMenuToTextBoxRN(1);   // copy and cut and paste
            OutgoingCharTextBox.AddContextMenuToTextBoxRN2(this, 1);   // copy and cut and paste

            SendATCommandsBackgroundWorker = new AbortableBackgroundWorker();
            SendATCommandsBackgroundWorker.DoWork += SendATCommandsBackgroundWorkerDoTheWork;
        }      
/****************************************************************************************************************************/
        private void RestoreConnectionStates()
        {
            if (global::Serial.Properties.Settings.Default.APP_RESTARTED)
            {
                //ComPortOpenClose(true);
                AutReCon_ChkBx.Checked = true;
                Socket_ClientConnect();
            }
            global::Serial.Properties.Settings.Default.APP_RESTARTED = false;
        }
/****************************************************************************************************************************/
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            // Restore previous state
            if (NativeMethods.SetThreadExecutionState(fPreviousExecutionState) == 0)
            {
                // No way to recover; already exiting
            }
        }
/****************************************************************************************************************************/
        private void cmdCloseClient_Click(object sender, EventArgs e)
        {
            Socket_ClientClose();
        }
/*******************************************************************************************************************/
        private void Clear_Button1_Click(object sender, EventArgs e)
        {
            MainTextBox.Clear();
        }
/*******************************************************************************************************************/
        public void Clear_Button2_Click(object sender, EventArgs e)
        {
           // SuspendCallBack = true;
            OutgoingCharTextBox.Clear();
        }
/*******************************************************************************************************************/
        public void InterceptSend(string input_string)
        {
            int FoundPos = input_string.IndexOf("HEX{");

            string replacedString = " ";

            bool HEX_Found = false;

            if (FoundPos != -1)
            {
                replacedString = input_string.Replace("HEX{", "");
                replacedString = replacedString.Replace("}", "");
                replacedString = replacedString.Replace("0x", "");
                replacedString = replacedString.Replace("0X", "");
                replacedString = replacedString.Replace(",", "");
                replacedString = replacedString.Replace(" ", "");
                HEX_Found = true;               
            }
            if (HEX_Found == false)
            {
                DateTime CurrentTime = DateTime.UtcNow;
                string UTC_String = CurrentTime.ToString("yyyy-MM-dd HH:mm:ss");
                input_string = input_string.Replace("<NOWTIME>", UTC_String);

                string LineEnding = AppendCR_CheckBox.Checked ? "\r" : "";
                LineEnding +=       AppendLF_CheckBox.Checked ? "\n" : "";

                byte[] byData;
                byData = System.Text.Encoding.ASCII.GetBytes(input_string + LineEnding);
                TransmitRawData(byData, 0, byData.Length);
                string DiagLineEnding = AppendLF_CheckBox.Checked ? "<CR>" : "";
                DiagLineEnding +=       AppendLF_CheckBox.Checked ? "<LF>" : "";

                AppendTextOutgoingCharTextBox(input_string + DiagLineEnding + LineEnding);                
            }
            else // we are going to send HEX which is represented in ASCII form
            {
               byte[] byData = StringToByteArrayFastest(replacedString);
               TransmitRawData(byData, 0, byData.Length);
               AppendTextOutgoingCharTextBox(replacedString);
               
            }           
        }
/***********************************************************************************************/
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }
            return arr;
        }
/***********************************************************************************************/
        private void AskForRestart()
        {
            DialogResult dialogResult = MessageBox.Show("This feature requires to run as administrator, re-run as administrator?", "Feature elevation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                DoRestarAsAdmin();
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }
        }
/***********************************************************************************************/
        private void DoRestarAsAdmin()
        {
            string ThisAppLocation = System.Reflection.Assembly.GetEntryAssembly().Location;

            ProcessStartInfo info = new ProcessStartInfo(ThisAppLocation);
            info.UseShellExecute = true;
            info.Verb = "runas";

            SaveSettings();
            Application.ExitThread();
            Process.Start(info);
        }
/***********************************************************************************************/
        static bool sg_PAUSED = false;
        private void FreezeButton_Click(object sender, EventArgs e)
        {
            //TextStyle infoStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
            sg_PAUSED ^= true;

            if (sg_PAUSED)
            {
                freezeToolStripMenuItem.Text = "Un Freeze";
                ButtonFlashTimer.Enabled = true;
                DISABLE_SCROLL_TO_BOTTOM_AGAIN = false;
                //OneShotHighlightButton.Enabled = true;
            }
            else
            {
                //OneShotHighlightButton.Enabled = false;
                if (!string.IsNullOrEmpty(LoggedSerialDataBAK_fctb.Text) && ButtonFlashTimer.Enabled == true)
                {
                    AppendTextBoxWithColor(MainTextBox, LoggedSerialDataBAK_fctb.Text, SerialTextColourStyle); // force immediate update of box text
                    LoggedSerialDataBAK_fctb.Clear();                    
                }               

                ButtonFlashTimer.Enabled = false;
                freezeToolStripMenuItem.ForeColor = Color.Black;
                freezeToolStripMenuItem.BackColor = Color.Gainsboro;
                freezeToolStripMenuItem.Text = " Freeze  ";
            }
        }
/****************************************************************************************************************************/
        static int oldLineindex = -1;
        public void LineIndexCallBack(int lineIndex)
        {
            if (FilterForm.FormAvailable == true)
            {
                string text_toPassToOtherForm2 = MainTextBox.Lines[lineIndex];
                FilterForm.AddTextForm3(text_toPassToOtherForm2 + "\n");

                if (lineIndex != -1 && oldLineindex != lineIndex)
                {
                    string text_toPassToOtherForm = MainTextBox.Lines[lineIndex];
                    FilterForm.AddTextForm3(text_toPassToOtherForm + "\n");
                }
            }
            oldLineindex = lineIndex;
        }
/***********************************************************************************************/
        private void Help_button_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Macro not fully implemented \n" +
                           "*******************************\n\n" +
                           "To send raw binary use '#' followed by a 3 digit decimal number ranging from 000-255 \n" +
                           "eg. \"this is ascii followed by #098#105#110#097#114#121 ASCII\n" +
                           "will result in \"this is ascii followed by binary ASCII\"\n\n" +
                           "*************************************************\n" +
                           "Muptiple search terms supported, separated by'|' symbol\n" +
                           "Log files are stores in same location as the .exe", "Quick Help");
        }
/***********************************************************************************************/
        static bool sg_TOGGLE = false;
        private void ButtonFlashTimerTimeoutEventHandler(object sender, EventArgs e)
        {
            sg_TOGGLE ^= true;
            if (sg_TOGGLE)
            {
               freezeToolStripMenuItem.ForeColor = Color.White;
               freezeToolStripMenuItem.BackColor = Color.Blue; //InactiveCaption
            }
            else
            {
               freezeToolStripMenuItem.ForeColor = Color.Black;
               freezeToolStripMenuItem.BackColor = Color.Gainsboro; // restore freeze button colour
            }
        }
/***********************************************************************************************/
        private void SndFlbutton_Click(object sender, EventArgs e)
        {
            TextBox tbResult;
            tbResult = tbBoot1;

            if (IsFileStreamingInProcess())
            {
                WaitingForOk = false;
                if (SendATCommandsBackgroundWorker.IsBusy == true)
                {
                    SendATCommandsBackgroundWorker.Abort();
                    SendATCommandsBackgroundWorker.Dispose();
                }
                SndFlbutton.Text = "Send File";
                return; // the previose file sending in progress should catch this
            }
            try
            {
                ofdInputFile.InitialDirectory = Path.GetDirectoryName(tbResult.Text);
            }
            catch (Exception) {  }

            try
            {
                DialogResult result = ofdInputFile.ShowDialog();
                if (result == DialogResult.OK)
                {
                    tbResult.Text = ofdInputFile.FileName;

                    if (!IsFileModemATFile() && !IsFileScriptFile())
                    {
                        TransmitBinaryFile();
                    }
                    else if (IsFileModemATFile())
                    {
                        SendATCommandsBackgroundWorker.RunWorkerAsync();
                    }
                    else
                    {
                        SendATCommandsBackgroundWorker.RunWorkerAsync();
                    }
                }
            }
            catch (Exception) { } // catches to file selected exception
        }

/**************************************************************************************************************************/
        void TransmitBinaryFile()
        {
            byte[] fileBytes = File.ReadAllBytes(ofdInputFile.FileName);
            StringBuilder sb = new StringBuilder();
            double percent;
            int CHUNK_SIZE = 1000;
            int remainder;
            int loop;

            if (fileBytes.Length > CHUNK_SIZE)
            {
                int chunk_count = fileBytes.Length / CHUNK_SIZE;
                remainder = fileBytes.Length - (CHUNK_SIZE * chunk_count);

                for (loop = 0; loop < chunk_count; loop++)
                {
                    ComPortByteArraySend(fileBytes, loop * CHUNK_SIZE, CHUNK_SIZE);
                    percent = (loop / ((double)chunk_count / 100));
                    SndFlbutton.Text = (int)percent + "%";
                }
                ComPortByteArraySend(fileBytes, CHUNK_SIZE * chunk_count, remainder); // send the remainder
                SndFlbutton.Text = "Send File";
            }
            else
            {
                ComPortByteArraySend(fileBytes, 0, fileBytes.Length); // write the whole lot
            }
        }
/***********************************************************************************************/
        private void FilterCheckChanged(object sender, EventArgs e)
        {
            //if (FiltercheckBox.Checked == true)
            //{
            //    ReceivedDataMainTextBox.WordWrap = false;
            //   // WrapCheckBox.Checked = false;
            //    FilterForm.Show();
            //    FilterForm.FormAvailable = true;
            //}
            //else
            //{
            //    FilterForm.HideForm();
            //    FilterForm.FormAvailable = false;
            //}
        }
/***********************************************************************************************/
        public void FilterFormHasClosed_CallBack()
        {
            FilterForm.FormAvailable = false;
            //FiltercheckBox.Checked = false;
        }
/***********************************************************************************************/
        public void WarnFormHasClosed_CallBack()
        {
            if(InvokeRequired)
                this.Invoke(new Action(WarnFormHasClosed_CallBack), new object[] {});
           TCPWarningForm.FormAvailable = false;         
        }
/***********************************************************************************************/
        public void ComWarnFormHasClosed_CallBack()
        {
            if (InvokeRequired)
                this.Invoke(new Action(ComWarnFormHasClosed_CallBack), new object[] { });
            ComWarnForm.FormAvailable = false;
        }
/***********************************************************************************************/
        private void btnDonate_Click(object sender, System.EventArgs e)
        {
            string url = "";

            string business = "nibbly78@googlemail.com";  // your paypal email
            string description = "Donation";            // '%20' represents a space. remember HTML!
            string country = "GB";                  // AU, US, etc.
            string currency = "GBP";                 // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }
/***********************************************************************************************/
        private void TipsBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("No Help or tips yet!", "Help n Tips");
        }
/***********************************************************************************************/   
        private void ShowLineNumbers_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowSingleLineSettingsFlag)
            {
                MainTextBox.ShowLineNumbers = true;
            }
            else
            {
                MainTextBox.ShowLineNumbers = false;
            }
        }
/***********************************************************************************************/
        private void HighlightEnableCheckChanged(object sender, EventArgs e)
        {
            if (HighlightGroup1EnableTickBox.Checked || HighlightGroup2EnableTickBox.Checked || HighlightGroup3EnableTickBox.Checked)
            {
                MainTextBox.AppendText(" \b", SerialTextColourStyle); // force a change in the box by print a scace then a backspace
            }
        }
/***********************************************************************************************/
        static int MaxValue = 0;
        static bool DISABLE_SCROLL_TO_BOTTOM_AGAIN = false;
        private void ReceivedDataMainTextBox_Scroll(object sender, ScrollEventArgs e)
        {
            if (MaxValue < e.NewValue || MaxValue < e.NewValue )
            {
                if (e.NewValue > e.OldValue)
                MaxValue = e.NewValue; // go to hold max scroll value to later determin if user has scrolled to this position
                else
                    MaxValue = e.OldValue;
            }
                                       // i.e. scrolled to the bottom
            if ((e.OldValue > e.NewValue) && MaxValue != e.NewValue)
            {
                HighlighVisibleRangeSingleCall();
                // user has scolled up at this poing so pause textbox
                if (sg_PAUSED == false)
                {

                    FreezeButton_Click(sender, e);
                    DISABLE_SCROLL_TO_BOTTOM_AGAIN = false;
                }
            }
            else if (MaxValue == e.NewValue && DISABLE_SCROLL_TO_BOTTOM_AGAIN == false) // RN This is the only way I could detect scolling to the bottom of the screen        
            {
                do
                {
                    Thread thread = new Thread(new ThreadStart(SetMouseButtonStateTHREAD));
                    thread.Start();                
                } while (sgLEFT_MOUSE_DOWN); // had to be done in separate thread because this locked up originally, dont know why...

                FreezeButton_Click(sender, e);
                DISABLE_SCROLL_TO_BOTTOM_AGAIN = true;               
             }                
        }
/***********************************************************************************************/
        private void ShowMessageBox(string text, string caption)
        {
            Thread t = new Thread(() => MyMessageBox(text, caption));
            t.Start();
        }
/***********************************************************************************************/
        private void MyMessageBox(object text, object caption)
        {
            MessageBox.Show((string)text, (string)caption);
        }
/***********************************************************************************************/
        static bool sgLEFT_MOUSE_DOWN = false;
        private void SetMouseButtonStateTHREAD()
        {
            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
                sgLEFT_MOUSE_DOWN = true; 
            else
                sgLEFT_MOUSE_DOWN = false;               
        }
/***********************************************************************************************/
        private void ClientConnect_Button(object sender, EventArgs e)
        {
            Socket_ClientConnect();
        }
/***********************************************************************************************/
        private void AutReCon_ChkBx_CheckedChanged(object sender, EventArgs e)
        {
            if (AutReCon_ChkBx.Checked == true)
                StartPollTimer();
            else
                StopClientConnectPollTimer();
        }
/****************************************************************************************************************************/
        void UpadateLabelVisiblity(Label label, bool option)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<Label, bool>(UpadateLabelVisiblity), new object[] { label, option });
                return;
            }
            label.Visible = option;
        }
/****************************************************************************************************************************/
        void UpdateCheckBox(CheckBox checkbox, bool status)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<CheckBox, bool>(UpdateCheckBox), new object[] { checkbox, status });
                return;
            }
            checkbox.Checked = status;
        }
/****************************************************************************************************************************/
        void UpadateRadio_button(RadioButton radio_button, bool option)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<RadioButton, bool>(UpadateRadio_button), new object[] { radio_button, option });
                return;
            }
            radio_button.Enabled = option;
        }
/****************************************************************************************************************************/
        public void UpdateStatusLabel(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateStatusLabel), new object[] { text });
                return;
            }
            ClientConnection_label.Text = text;
            ClientConnection_label.Visible = true; 
        }
/****************************************************************************************************************************/
        void UpadateButtonText(Button button, string LabelString)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<Button, string>(UpadateButtonText), new object[] { button, LabelString });
                return;
            }
            button.Text = LabelString;
        }
/****************************************************************************************************************************/
        private void ActiveSequcheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(sg_IPCONNECTED_SERVER)
            {
                if (ActiveSequcheckBox1.Checked == true)
                {
                byte[] toBytes = Encoding.ASCII.GetBytes("IPCOMMAND_MAKE_HARD_TO_MATCH,RESQ_ON");
                Socket_ServerToClientSend(toBytes);
                }
                else
                {
                byte[] toBytes = Encoding.ASCII.GetBytes("IPCOMMAND_MAKE_HARD_TO_MATCH,RESQ_OFF");
                Socket_ServerToClientSend(toBytes);
                }         
            }             
        }
/****************************************************************************************************************************/
        private void COMPortOpenClose_Click(object sender, EventArgs e)
        {
            ComPortOpenClose(true);
        }

/****************************************************************************************************************************/
        void RestartApp()
        {
            global::Serial.Properties.Settings.Default.APP_RESTARTED = true;
     
            SaveSettings();
            Application.Restart();
          //  Application.ExitThread();
            Environment.Exit(0);        
        }
/****************************************************************************************************************************/    
        private void Form1_HelpButtonClicked(object sender, EventArgs e)
        {
            MessageBox.Show("No Help or tips yet!", "Help n Tips");
        }
/****************************************************************************************************************************/
        public static void UserHexDecode(bool empty)
        {
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.Arguments = Clipboard.GetText();
            string HexString = startinfo.Arguments;

            HexString = HexString.Replace("0x", "");
            HexString = HexString.Replace("0X", "");
            HexString = HexString.Replace(",", "");
            HexString = HexString.Replace(" ", "");
            HexString = HexString.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

            string ConvertedHex = ConvertHex(HexString);

            if (ConvertedHex != "")
               MessageBox.Show(ConvertHex(HexString),"Hex To Ascii");
            else
               MessageBox.Show("Unable to convert. (did you highlight HEX data?)", "Hex To Ascii");
        }
/****************************************************************************************************************************/
        public void PasteSend()
        {
           AppendTextOutgoingCharTextBox( Clipboard.GetText() );
        }
/****************************************************************************************************************************/
    public static string ConvertHex(String hexString)
    {
       try
       {
          string ascii = string.Empty;
     
          for (int i = 0; i < hexString.Length; i += 2)
          {
              String hs = string.Empty;
     
              hs   = hexString.Substring(i,2);
              uint decval =   System.Convert.ToUInt32(hs, 16);
              char character = System.Convert.ToChar(decval);
              ascii += character;
          }
          return ascii;
       }
       catch (Exception ex) { Console.WriteLine(ex.Message); }
       return string.Empty;
}
 /***********************************************************************************************/
    private void SaveCurrentToFile_Click(object sender, EventArgs e)
    {
        SaveFileDialog savefile = new SaveFileDialog();
        // set a default file name
        savefile.FileName = "unknown.txt";
        // set filters - this can be done in properties as well
        savefile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

        if (savefile.ShowDialog() == DialogResult.OK)
        {
            using (StreamWriter sw = new StreamWriter(savefile.FileName))
            {
                sw.WriteLine(MainTextBox.Text);
                sw.Flush();
                sw.Close();
            }
        }
    }
/**********************************************************************************************************/
    void TransmitRawData(byte[] Rawdata, int offset, int length)
    {
        if ((SERIAL_SOCKET_MODE == SERIALMODE) || (SERIAL_SOCKET_MODE == SERIALMODE + SOCKETMODECLIENT))
        {
            ComPortByteArraySend(Rawdata, offset, length); // send as serial only
        }
        else
        if (SERIAL_SOCKET_MODE == SOCKETMODESERVER)
        {
           byte[] target = new byte[length - offset];
           Array.Copy(Rawdata, offset, target, 0, length);
           Socket_TCPTransmit(Rawdata, length);
        }
    }
/***********************************************************************************/  
    private void startLoggingToolStripMenuItem_Click(object sender, EventArgs e)
    {
       SaveFileDialog savefile = new SaveFileDialog();
       // set a default file name
       savefile.FileName = "unknown.txt";
       // set filters - this can be done in properties as well
       savefile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

       if (savefile.ShowDialog() == DialogResult.OK)
       {
          //StopLogButton.Enabled = true;
          //StartLogButton.Enabled = false;
          LoggingStarted = true;

          Log = new CreateLogFiles();
          Log.CreateLog((savefile.FileName), "");
       }
    }
/***********************************************************************************/  
    private void stopLoggingToolStripMenuItem_Click(object sender, EventArgs e)
    {
       //StopLogButton.Enabled = false;
       //StartLogButton.Enabled = true;
       LoggingStarted = false;
       try
       {
          Log.CloseLog();
       }
       catch (Exception) { }
    }
/***********************************************************************************/  
    private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
       SaveFileDialog savefile = new SaveFileDialog();
       // set a default file name
       savefile.FileName = "unknown.txt";
       // set filters - this can be done in properties as well
       savefile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

       if (savefile.ShowDialog() == DialogResult.OK)
       {
          using (StreamWriter sw = new StreamWriter(savefile.FileName))
          {
             sw.WriteLine(MainTextBox.Text);
             sw.Flush();
             sw.Close();
          }
       }
    }
/***********************************************************************************/  
    //int split_distance = 397;
    bool mouse_down = false;

    private void MouseDown(object sender, MouseEventArgs e)
    {
   //    mouse_down = true;
    }

    private void MouseUp(object sender, MouseEventArgs e)
    {
    //   mouse_down = false;
    }

    private void SplitterMoving(object sender, SplitterCancelEventArgs e)
    {
       if (this.mouse_down)
       {
       //   split_distance = MainRealEstate_SplitC.SplitterDistance;
       //   AppendTextOutgoingCharTextBox(MainRealEstate_SplitC.SplitterDistance.ToString() + " "); 
       }
    }

    private void Layout(object sender, LayoutEventArgs e)
    {
     //  MainRealEstate_SplitC.SplitterDistance = split_distance;
    }

    private void MRE_Resize(object sender, EventArgs e)
    {
       //MainRealEstate_SplitC.SplitterDistance = split_distance;
     //  AppendTextOutgoingCharTextBox(MainRealEstate_SplitC.SplitterDistance.ToString() + " "); 
    }

    private void TextChangingCallback(object sender, TextChangingEventArgs e)
    {
        if (OnChangeHighlightRadioButton.Checked)
        {
           Do_On_The_Fly_Highlighting();
        }
    }

    private void VisibleRangeChangedCallback(object sender, EventArgs e)
    {
        if (VisibleRangeHighlightRadioButton.Checked)
        {
            Do_On_The_Fly_Highlighting();
        }
    }

    private void TextChangedDelayedCallback(object sender, TextChangedEventArgs e)
    {
        if (OnChangeDelayedHighlightRadioButton.Checked)
        {
            Do_On_The_Fly_Highlighting();
        }
    }

    private void HighlightClearButtonClick(object sender, EventArgs e)
    {
        MainTextBox.Range.ClearStyle(Highlight_1_RegularStyle);
        MainTextBox.Range.ClearStyle(Highlight_2_RegularStyle);
        MainTextBox.Range.ClearStyle(Highlight_3_RegularStyle);
        MainTextBox.Range.ClearStyle(Highlight_4_RegularStyle);
    }

    private void OneShotHighlightClick(object sender, EventArgs e)
    {
        string searchstring1 = HighlightTextBox1String.Text;
        string searchstring2 = HighlightTextBox2String.Text;
        string searchstring3 = HighlightTextBox3String.Text;
        string searchstring4 = HighlightTextBox4String.Text;

        //MainTextBox.Range.ClearStyle(Highlight_1_RegularStyle);
        //MainTextBox.Range.ClearStyle(Highlight_2_RegularStyle);
        //MainTextBox.Range.ClearStyle(Highlight_3_RegularStyle);
        if (HighlightGroup1EnableTickBox.Checked)
        {
            MainTextBox.Range.SetStyle(Highlight_1_RegularStyle, searchstring1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        if (HighlightGroup2EnableTickBox.Checked)
        {
            MainTextBox.Range.SetStyle(Highlight_2_RegularStyle, searchstring2, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        if (HighlightGroup3EnableTickBox.Checked)
        {
            MainTextBox.Range.SetStyle(Highlight_3_RegularStyle, searchstring3, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        if (HighlightGroup4EnableTickBox.Checked)
        {
            MainTextBox.Range.SetStyle(Highlight_4_RegularStyle, searchstring4, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }

        /***********************************************************************************/
    } // closing brace of public partial class Form1 : Form
    /***********************************************************************************/
    /***********************************************************************************/
    /***********************************************************************************/
    /***********************************************************************************/
    /***********************************************************************************/
    public static class WinApi
    {   /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]

        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]

        public static extern uint TimeEndPeriod(uint uMilliseconds);
    }
    /***********************************************************************************/
    public class Win32
    {
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string s_File);

        public static IntPtr LoadLibrary(string s_File)
        {
            IntPtr h_Module = LoadLibraryW(s_File);
            if (h_Module != IntPtr.Zero)
                return h_Module;

            int s32_Error = Marshal.GetLastWin32Error();
            throw new Win32Exception(s32_Error);
        }
    }
    /***********************************************************************************/
    internal static class NativeMethods
    {
        // Import SetThreadExecutionState Win32 API and necessary flags
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
    }
    /***********************************************************************************/
    public class AbortableBackgroundWorker : BackgroundWorker
    {
        private Thread workerThread;

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            workerThread = Thread.CurrentThread;
            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true; //We must set Cancel property to true!
                Thread.ResetAbort(); //Prevents ThreadAbortException propagation
            }
        }

        public void Abort()
        {
            if (workerThread != null)
            {
                workerThread.Abort();
                workerThread = null;
            }
        }
    }
}// closing brace of namesapce Serial


