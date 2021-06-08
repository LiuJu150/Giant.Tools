using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace QRCode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var code = this.txtCode.Text;
            var width = this.numWidth.Value;
            var height = this.numHeight.Value;

            var writer = new BarcodeWriter();
            writer.Format = this.rdoBarcode.Checked ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE;
            var options = new ZXing.Common.EncodingOptions()
            {
                Width = (int)width,
                Height = (int)height,
                Margin = 2,
                PureBarcode = false
            };
            writer.Options = options;
            var img = writer.Write(code);
            Clipboard.SetImage(img);
            this.imgBox.Image = img;
        }
    }
}
