using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComputerInfo
{
    public partial class Form1 : Form
    {
        public Label LabName { get; set; }
        public Label LabVersion { get; set; }
        public Label LabUserName { get; set; }
        public Label LabMemory { get; set; }
        public Label LabIP { get; set; }
        public Form1()
        {
            InitializeComponent();
            this.LabName = new Label();
            this.LabVersion = new Label();
            this.LabUserName = new Label();
            this.LabMemory = new Label();
            this.LabIP = new Label();

            Console.WriteLine("abcdefsalkdfjsalfjsklafj;sldajflskajkl");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var os = this.GetOS();

            this.LabName.Text = os.CSName;
            this.LabName.AutoSize = true;
            this.LabName.Font = new Font("Consolas", 32, FontStyle.Bold);
            this.LabName.ForeColor = Color.GreenYellow;
            this.LabName.Location = new Point(5, 0);
            this.panel1.Controls.Add(this.LabName);

            this.LabVersion.Text = os.Caption.Replace("Microsoft ", "");
            this.LabVersion.AutoSize = true;
            this.LabVersion.Font = new Font("Consolas", 16, FontStyle.Bold);
            this.LabVersion.ForeColor = Color.GreenYellow;
            this.LabVersion.Location = new Point(5, 60);
            this.panel1.Controls.Add(this.LabVersion);

            this.LabUserName.Text = Environment.UserName;
            this.LabUserName.AutoSize = true;
            this.LabUserName.Font = new Font("Consolas", 16, FontStyle.Bold);
            this.LabUserName.ForeColor = Color.GreenYellow;
            this.LabUserName.Location = new Point(5, 85);
            this.panel1.Controls.Add(this.LabUserName);

            this.LabMemory.Text = $"RAM 0MB/0MB";
            this.LabMemory.AutoSize = true;
            this.LabMemory.Font = new Font("Consolas", 16, FontStyle.Bold);
            this.LabMemory.ForeColor = Color.GreenYellow;
            this.LabMemory.Location = new Point(5, 110);
            this.panel1.Controls.Add(this.LabMemory);

            this.LabIP.Text = this.GetIP();
            this.LabIP.AutoSize = true;
            this.LabIP.Font = new Font("Consolas", 16, FontStyle.Bold);
            this.LabIP.ForeColor = Color.GreenYellow;
            this.LabIP.Location = new Point(5, 135);
            this.panel1.Controls.Add(this.LabIP);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string strQuery = "select TotalVisibleMemorySize,FreePhysicalMemory from win32_OperatingSystem";
            SelectQuery queryOS = new SelectQuery(strQuery);
            ulong TotalVisibleMemorySize = 0;
            ulong FreePhysicalMemory = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(queryOS))
            {
                using (var queryResult = searcher.Get())
                {
                    foreach (var os in queryResult)
                    {
                        TotalVisibleMemorySize = (ulong)os["TotalVisibleMemorySize"];
                        FreePhysicalMemory = (ulong)os["FreePhysicalMemory"];
                    }
                }
            }
            var displayString = $"RAM {MemoryToString((long)FreePhysicalMemory)}/{MemoryToString((long)TotalVisibleMemorySize)}";
            this.LabMemory.Text = displayString;

            try
            {
                var activeWin = SystemHelper.GetForegroundWindow();
                var title = new StringBuilder(256);
                var from = SystemHelper.GetWindowText(activeWin, title, title.Capacity);

                if (title.ToString().Contains("远程桌面"))
                {
                    this.LabName.Visible = false;
                    this.LabVersion.Visible = false;
                    this.LabUserName.Visible = false;
                    this.LabMemory.Visible = false;
                    this.LabIP.Visible = false;
                }
                else
                {
                    this.LabName.Visible = true;
                    this.LabVersion.Visible = true;
                    this.LabUserName.Visible = true;
                    this.LabMemory.Visible = true;
                    this.LabIP.Visible = true;
                }
            }
            catch (Exception ex)
            {
                this.LabName.Visible = true;
                this.LabVersion.Visible = true;
                this.LabUserName.Visible = true;
                this.LabMemory.Visible = true;
                this.LabIP.Visible = true;
            }
        }

        static String MemoryToString(long byteCount)
        {
            string[] suf = { "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private string GetIP()
        {
            var listIP = new List<String>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            var ipStr = ip.Address.ToString();
                            if (!(ipStr.StartsWith("169") || ipStr.EndsWith(".1") || ipStr.EndsWith(".255")))
                                listIP.Add(ipStr);
                        }
                    }
                }
            }
            return String.Join(Environment.NewLine, listIP);
        }

        private (string Caption, string CSName) GetOS()
        {
            string strQuery = "select Caption,CSName from win32_OperatingSystem";
            SelectQuery queryOS = new SelectQuery(strQuery);
            string Caption = "";
            string CSName = "";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(queryOS))
            {
                using (var queryResult = searcher.Get())
                {
                    foreach (var os in queryResult)
                    {
                        Caption = (string)os["Caption"];
                        CSName = (string)os["CSName"];
                    }
                }
            }
            return (Caption, CSName);
        }
    }
}
