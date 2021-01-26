using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerInfo
{
    public partial class Form1 : Form
    {
        private Label LabName { get; set; }
        private Label LabVersion { get; set; }
        private Label LabUserName { get; set; }
        private Label LabMemory { get; set; }
        private long TotalMemory { get; set; }
        private Label LabCPU { get; set; }
        private Label LabDisk { get; set; }
        private Label LabIP { get; set; }

        private PerformanceCounter CpuPC { get; set; }
        private PerformanceCounter RamPC { get; set; }
        private PerformanceCounter DiskPC { get; set; }
        public Form1()
        {
            InitializeComponent();
            this.LabName = new Label();
            this.LabVersion = new Label();
            this.LabUserName = new Label();
            this.LabMemory = new Label();
            this.LabCPU = new Label();
            this.LabIP = new Label();
            this.LabDisk = new Label();

            this.CpuPC = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            this.RamPC = new PerformanceCounter("Memory", "Available KBytes");
            this.DiskPC = new PerformanceCounter("PhysicalDisk", "% Idle Time", "_Total");
            //this.RamPC = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var os = this.GetOS();
            this.TotalMemory = (long)os.TotalVisibleMemorySize;

            this.LabName.Text = os.CSName;
            this.LabName.AutoSize = true;
            this.LabName.Font = new Font("Consolas", 32, FontStyle.Bold);
            this.LabName.ForeColor = Color.GreenYellow;
            this.LabName.Location = new Point(5, 0);
            this.panel1.Controls.Add(this.LabName);

            this.LabVersion.Text = os.Caption.Replace("Microsoft", "").Replace(" ", "");
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

            this.LabCPU.Text = $"CPU 0%";
            this.LabCPU.AutoSize = true;
            this.LabCPU.Font = new Font("Consolas", 16, FontStyle.Bold);
            this.LabCPU.ForeColor = Color.GreenYellow;
            this.LabCPU.Location = new Point(5, 135);
            this.panel1.Controls.Add(this.LabCPU);

            this.LabDisk.Text = $"DISK 0%";
            this.LabDisk.AutoSize = true;
            this.LabDisk.Font = new Font("Consolas", 16, FontStyle.Bold);
            this.LabDisk.ForeColor = Color.GreenYellow;
            this.LabDisk.Location = new Point(5, 160);
            this.panel1.Controls.Add(this.LabDisk);

            this.LabIP.Text = this.GetIP();
            this.LabIP.AutoSize = true;
            this.LabIP.Font = new Font("Consolas", 16, FontStyle.Bold);
            this.LabIP.ForeColor = Color.GreenYellow;
            this.LabIP.Location = new Point(5, 185);
            this.panel1.Controls.Add(this.LabIP);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.LabCPU.Text = $"CPU {this.CpuPC.NextValue().ToString("F1")}%";
            var useMemory = long.Parse(this.RamPC.NextValue().ToString("F0"));
            var displayString = $"RAM {MemoryToString(this.TotalMemory - useMemory)}/{MemoryToString(this.TotalMemory)}";
            this.LabMemory.Text = displayString;
            this.LabDisk.Text = $"Disk {(100.0F - this.DiskPC.NextValue()).ToString("F1")}%";

            try
            {
                var activeWin = SystemHelper.GetForegroundWindow();
                var title = new StringBuilder(256);
                var from = SystemHelper.GetWindowText(activeWin, title, title.Capacity);

                if (title.ToString().Contains("远程桌面"))
                    this.Hide();
                else
                    this.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.Show();
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

        private OSInfoModel GetOS()
        {
            string strQuery = "select Caption,CSName,TotalVisibleMemorySize from win32_OperatingSystem";
            SelectQuery queryOS = new SelectQuery(strQuery);
            string Caption = "";
            string CSName = "";
            ulong TotalVisibleMemorySize = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(queryOS))
            {
                using (var queryResult = searcher.Get())
                {
                    foreach (var os in queryResult)
                    {
                        Caption = (string)os["Caption"];
                        CSName = (string)os["CSName"];
                        TotalVisibleMemorySize = (ulong)os["TotalVisibleMemorySize"];
                    }
                }
            }
            return new OSInfoModel() { Caption = Caption, CSName = CSName, TotalVisibleMemorySize = TotalVisibleMemorySize };
        }
    }
}
