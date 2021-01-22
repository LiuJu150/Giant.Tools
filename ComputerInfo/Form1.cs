using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var os = this.GetOS();

            this.LabName.Text = os.CSName;
            this.LabName.AutoSize = true;
            this.LabName.Font = new Font("Consolas", 18, FontStyle.Bold);
            this.LabName.ForeColor = Color.Green;
            this.LabName.Location = new Point(5, 5);
            this.panel1.Controls.Add(this.LabName);

            this.LabVersion.Text = os.Caption;
            this.LabVersion.AutoSize = true;
            this.LabVersion.Font = new Font("Consolas", 14, FontStyle.Bold);
            this.LabVersion.ForeColor = Color.Green;
            this.LabVersion.Location = new Point(5, 30);
            this.panel1.Controls.Add(this.LabVersion);

            this.LabUserName.Text = Environment.UserName;
            this.LabUserName.AutoSize = true;
            this.LabUserName.Font = new Font("Consolas", 14, FontStyle.Bold);
            this.LabUserName.ForeColor = Color.Green;
            this.LabUserName.Location = new Point(5, 50);
            this.panel1.Controls.Add(this.LabUserName);

            this.LabMemory.Text = $"RAM 0MB/0MB";
            this.LabMemory.AutoSize = true;
            this.LabMemory.Font = new Font("Consolas", 14, FontStyle.Bold);
            this.LabMemory.ForeColor = Color.Green;
            this.LabMemory.Location = new Point(5, 70);
            this.panel1.Controls.Add(this.LabMemory);

            this.LabIP.Text = this.GetIP();
            this.LabIP.AutoSize = true;
            this.LabIP.Font = new Font("Consolas", 14, FontStyle.Bold);
            this.LabIP.ForeColor = Color.Green;
            this.LabIP.Location = new Point(5, 90);
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
            var listIp = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                            .Select(p => p.GetIPProperties())
                            .SelectMany(p => p.UnicastAddresses)
                            .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address))
                            .ToList();
            var list = listIp.Select(s => s.Address.ToString()).Where(w => !w.StartsWith("169")).ToList();
            return String.Join(Environment.NewLine, list);
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
