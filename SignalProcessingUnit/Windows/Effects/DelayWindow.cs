using SoundManipulation;
using StringUtils;
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
    public partial class DelayWindow : EffectParametresWindow {

        public DelayWindow() {
            InitializeComponent();
            this.FillComboBox();
            this.textBox1.Text = "0";
            this.textBox1.KeyPress += this.TextBox1_KeyPress;
            this.button1.Click += this.applyEffectButton_Click;
        }

        private void FillComboBox() {
            DelayUnit[] values = Enum.GetValues(typeof(DelayUnit)).Cast<DelayUnit>().ToArray();
            this.comboBox1.DataSource = values;
            this.comboBox1.SelectedItem = values[0];
        }

        protected override void ApplyEffect() {
            var delayUnit = (DelayUnit)this.comboBox1.SelectedItem;
            var delayValue = (int)Int32.Parse(this.textBox1.Text);
            var result = SoundEffects.Delay(this.BufferToModify, this.FileFormat, delayValue, delayUnit);
            this.ModifiedBuffer = result;
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (sender is TextBox box) {
                if (box.Text.Length >= 4 && !Char.IsControl(e.KeyChar)) {
                    e.Handled = true;
                    return;
                }
                if (box.Text == "0") {
                    box.Clear();
                }
                char c = e.KeyChar;
                e.Handled = !(box.Text.Length > 0 ? Char.IsDigit(c) || Char.IsControl(c) : (Char.IsDigit(c) && c != '0') || Char.IsControl(c));
            }
        }

        private void DelayWindow_Load(object sender, EventArgs e) {

        }

        private void applyEffectButton_Click(object sender, EventArgs e) {
            //Call a processing function with params from GUI
            this.ApplyEffect();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        
    }
}
