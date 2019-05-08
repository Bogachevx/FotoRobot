using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FotoRobot
{
    public partial class CTrackBar : UserControl
    {
        public CTrackBar()
        {
            InitializeComponent();
        }

        public int _Value;
        public int Value
        {
            get { return _Value; }
            set
            {
                if (value > MaximumValue)
                    _Value = MaximumValue;
                else if (value < MinimumValue)
                    _Value = MinimumValue;
                else
                    _Value = value;
                
                textValue.Text = _Value.ToString();
                trackValue.Value = _Value;
            }
        }

        public int _MinValue;
        public int MinimumValue
        {
            get { return trackValue.Minimum; }
            set { trackValue.Minimum = value; _MinValue = value; }
        }

        public String TrackbarLabel
        {
            get { return trackLabel.Text; }
            set { trackLabel.Text = value; }
        }

        public int _MaxValue;
        public int MaximumValue
        {
            get { return trackValue.Maximum; }
            set { trackValue.Maximum = value; _MaxValue = value; }
        }


        private void trackValue_ValueChanged(object sender, EventArgs e)
        {
            textValue.Text = trackValue.Value.ToString();
            _Value = trackValue.Value;
        }

        private void textValue_TextChanged(object sender, EventArgs e)
        {

        }

        private void textValue_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                textValue.Text = ((Convert.ToInt32(textValue.Text) < _MinValue) || (Convert.ToInt32(textValue.Text) > _MaxValue)) ? trackValue.Value.ToString() : textValue.Text;
                trackValue.Value = Convert.ToInt32(textValue.Text);
            }
        }

        private void textValue_MouseLeave(object sender, EventArgs e)
        {
            textValue.Text = trackValue.Value.ToString();
        }

        private void textValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;

            if (!Char.IsDigit(number) && number != 8)
            {
                e.Handled = true;
            }
        }

    }
}
