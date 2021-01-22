using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MsgAlert
{
    public partial class Form1 : Form
    {
        public Form1(string msg = "NONE")
        {
            InitializeComponent();
            this.LabMsg.Text = msg;
        }
    }
}
