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

namespace SignalProcessingUnit.Windows.Effects {
    public partial class TremoloWindow : EffectParametresWindow {
        public TremoloWindow() {
            InitializeComponent();

            this.button1.Click += (obj, e) => {
                if (!this.ApplyEffect()) {
                    MessageBox.Show("Error has occured. Please check parametres.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }

        protected override bool ApplyEffect() {
            try {
                double rate = Double.Parse(this.textBox2.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                double depth = Double.Parse(this.textBox1.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
                depth = MathUtils.MathUtilities.Clamp01(depth);
                var result = SoundManipulation.SoundEffects.Tremolo(this.BufferToModify, this.FileFormat, rate, depth);
                this.ModifiedBuffer = result;
            } catch (FormatException) {
                return false;
            }
            return true;
        }
    }
}
