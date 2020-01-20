using StringUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalProcessingUnit.Windows.Processing {
    public partial class ChangeVolumeWindow : EffectParametresWindow {
        public ChangeVolumeWindow() {
            InitializeComponent();
            this.InitWindowControls();
        }

        private void InitWindowControls() {
            this.InitSlider();
            this.button1.Click += (obj, e) => {
                if (!this.ApplyEffect()) {
                    MessageBox.Show("Please check your input; Some of the values are incorrect.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }

        protected override bool ApplyEffect() {
            float multiplier = this.trackBar1.Value / 100f;
            var result = SoundManipulation.SoundProcessing.ChangeVolume(
                this.BufferToModify, 
                multiplier, 
                this.FileFormat
            );
            this.ModifiedBuffer = result;
            return true;
        }

        private void InitSlider() {
            this.trackBar1.Minimum = 0;
            this.trackBar1.Maximum = Convert.ToInt32(
                SoundManipulation.SoundProcessing.Constants.MAXIMUM_VOLUME_MULTIPLIER * 100
            );
            this.trackBar1.TickFrequency = 5;
            this.trackBar1.ValueChanged += (obj, e) => {
                this.textBox1.Text = (this.trackBar1.Value / 100.0).ToString();
            };
            this.trackBar1.Scroll += (obj, e) => {
                double pos = (double)this.trackBar1.Value / this.trackBar1.TickFrequency;
                int index = Convert.ToInt32(Math.Round(pos));
                this.trackBar1.Value = index * this.trackBar1.TickFrequency;
            };
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            if (sender is TextBox box) {
                try {
                    if (String.IsNullOrWhiteSpace(box.Text)) {
                        this.trackBar1.Value = 0;
                    }
                    var textboxValue = Double.Parse(box.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                    this.trackBar1.Value = Convert.ToInt32(textboxValue * 100);
                    box.SelectionStart = box.Text.Length;
                } catch (Exception) {
                    return;
                } 
            }
        }
    }
}
