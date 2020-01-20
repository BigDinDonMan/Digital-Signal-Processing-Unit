using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalProcessingUnit.Windows.Effects {
    public partial class FlangerWindow : EffectParametresWindow {
        public FlangerWindow() {
            InitializeComponent();
            this.InitControls();
        }

        private void InitControls() {
            this.button1.Click += (obj, e) => {
                if (!this.ApplyEffect()) {
                    MessageBox.Show("Error while applying effect. Please check your parametres.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }

        protected override bool ApplyEffect() {
            var freqSuccess = Single.TryParse(this.textBox1.Text, out float sweepFrequency);
            var rangeSuccess = Int32.TryParse(this.textBox3.Text, out int sweepRange);
            var delaySuccess = Int32.TryParse(this.textBox2.Text, out int delay);
            if (!freqSuccess || !rangeSuccess || !delaySuccess) {
                return false;
            }
            var result = SoundManipulation.SoundEffects.Flanger(this.BufferToModify, this.FileFormat, sweepFrequency, sweepRange, delay);
            this.ModifiedBuffer = result;
            return true;
        }
    }
}
