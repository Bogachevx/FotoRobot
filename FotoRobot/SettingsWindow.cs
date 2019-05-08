using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FotoRobot
{
    public partial class SettingsWindow : Form
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            cameraHeight.Text = SettingsTransfer.cameraResolution.Height.ToString();
            cameraWidth.Text = SettingsTransfer.cameraResolution.Width.ToString();
            paperHeight.Text = SettingsTransfer.paperSize.Height.ToString();
            paperWidth.Text = SettingsTransfer.paperSize.Width.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Size camRes = new Size(Convert.ToInt32(cameraWidth.Text),
                Convert.ToInt32(cameraHeight.Text));
            Size papWid = new Size(Convert.ToInt32(paperWidth.Text),
                Convert.ToInt32(paperHeight.Text));
            SettingsTransfer.cameraResolution = camRes;
            SettingsTransfer.paperSize = papWid;
            //SettingsTransfer.cameraResolution = new Size()
        }

        private void digitsHandler(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
            (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }
    }
}
