using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace _HostedNetwork
{
    public partial class MainForm : Form
    {
        Process proc = new Process();
        string SSIDName = "";
        string Password = "";
        string Mode = "";

        public MainForm()
        {
            InitializeComponent();
            if (checkHostedNetworkSupport())
            {
                checkHostedNetworkInfo();
            }
        }

        public bool checkHostedNetworkSupport()
        {
            bool support = false;

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/c netsh wlan show drivers";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if(line.Contains("Hosted network supported") && line.Contains("Yes"))
                {
                    support = true;
                }
                if (line.Contains("Hosted network supported") && line.Contains("No"))
                {
                    toolStripStatusLabel2.Text = "Network card does not support network hosting";
                    groupBoxInfo.Enabled = false;
                    groupBoxStatus.Enabled = false;
                }
            }
            proc.WaitForExit();
            
            return support;
        }

        public void checkHostedNetworkInfo()
        {
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/c netsh wlan show hostednetwork";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("SSID name"))
                {
                    textBoxSSID.Text = line.Substring(line.LastIndexOf(':') + 2).Replace("\"", "");
                    SSIDName = textBoxSSID.Text;
                }
                if (line.Contains("Mode"))
                {
                    string mode = line.Substring(line.LastIndexOf(':') + 2);
                    if (mode == "Disallowed")
                    {
                        comboBoxMode.SelectedIndex = 1;
                        Mode = "disallow";
                    }
                    else
                    {
                        comboBoxMode.SelectedIndex = 0;
                        Mode = "allow";
                    }
                }
                if (line.Contains("Status"))
                {
                    if (line.Substring(line.LastIndexOf(':') + 2) == "Not started")
                    {
                        buttonStop.Enabled = false;
                    }
                    else if (line.Substring(line.LastIndexOf(':') + 2) == "Not available")
                    {
                        MessageBox.Show("The hosted network status is not available, you might have the driver disabled in Device Manager or your network card does not support network hosting\n\nCheck Google for Microsoft Hosted Network Virtual Adapter", "Hosted network unavailable");
                        toolStripStatusLabel2.Text = "Hosted network is not available";
                        buttonStart.Enabled = false;
                        buttonStop.Enabled = false;
                    }
                    else
                        buttonStart.Enabled = false;
                }
            }
            proc.WaitForExit();
            

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/c netsh wlan show hostednetwork setting=security";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("User security key") && !line.Contains("usage"))
                {
                    textBoxPassword.Text = line.Substring(line.LastIndexOf(':') + 2).Replace("\"", "");
                    Password = textBoxPassword.Text;
                }
            }
            proc.WaitForExit();
            
        }

        private void textBoxPassword_MouseHover(object sender, EventArgs e)
        {
            textBoxPassword.PasswordChar = '\0';
        }

        private void textBoxPassword_MouseLeave(object sender, EventArgs e)
        {
            textBoxPassword.PasswordChar = '*';
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/c netsh wlan start hostednetwork";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("hosted network started"))
                {
                    toolStripStatusLabel2.Text = "Hosted Network is started";
                    toolStripStatusLabel2.ForeColor = System.Drawing.Color.Green;
                    groupBoxInfo.Enabled = false;
                    buttonStart.Enabled = false;
                    buttonStop.Enabled = true;
                }
                else
                {
                    toolStripStatusLabel2.Text = "Hosted Network could not be started";
                    toolStripStatusLabel2.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show("Hosted Network could not be started", "Error");
                }
            }
            proc.WaitForExit();
            
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/c netsh wlan stop hostednetwork";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("hosted network stopped"))
                {
                    toolStripStatusLabel2.Text = "Hosted Network is stopped";
                    toolStripStatusLabel2.ForeColor = System.Drawing.Color.Red;
                    groupBoxInfo.Enabled = false;
                    buttonStop.Enabled = false;
                    buttonStart.Enabled = true;
                }
                else
                {
                    toolStripStatusLabel2.Text = "Hosted Network could not be stoped";
                    toolStripStatusLabel2.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show("Hosted Network could not be stopped", "Error");
                }
            }
            proc.WaitForExit();
            
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (comboBoxMode.SelectedIndex == 0)
                Mode = "allow";
            else
                Mode = "disallow";

            SSIDName = textBoxSSID.Text;
            Password = textBoxPassword.Text;

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/c netsh wlan set hostednetwork mode=" + Mode + " ssid=" + SSIDName + " key=" + Password;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            MessageBox.Show("Network info saved");
            proc.WaitForExit();
            
       }
    }
}
