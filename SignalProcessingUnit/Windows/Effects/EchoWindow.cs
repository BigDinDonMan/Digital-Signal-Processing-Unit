using SoundManipulation;
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
    public partial class EchoWindow : EffectParametresWindow {
        public EchoWindow() {
            InitializeComponent();
            this.InitControls();
        }

        private void InitControls() {
            this.InitCombobox();
            this.InitRepeats();
            this.button1.Click += (obj, e) => {
                if (!this.ApplyEffect()) {
                    MessageBox.Show("Error while applying the effect. Please check parametres.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
        }

        private void InitCombobox() {
            var enumValues = Enum.GetValues(typeof(DelayUnit)).Cast<DelayUnit>().ToArray();
            this.comboBox1.DataSource = enumValues;
            this.comboBox1.SelectedItem = enumValues[0];
        }

        private void InitRepeats() {
            var items = Enumerable.Range(1, 15).Select(i => i.ToString()).ToArray();
            Array.Reverse(items);
            this.domainUpDown1.Sorted = true;
            this.domainUpDown1.Items.AddRange(items);
            this.domainUpDown1.SelectedItem = items[items.Length - 1];
        }

        private void EchoWindow_Load(object sender, EventArgs e) {

        }

        protected override bool ApplyEffect() {
            var delayUnit = (DelayUnit)this.comboBox1.SelectedItem;
            var success = Int32.TryParse(this.domainUpDown1.SelectedItem as string, out int repeats);
            if (!success) return false;
            var delaySuccess = Int32.TryParse(this.textBox1.Text, out int delayBetweenRepeats);
            if (!delaySuccess || delayBetweenRepeats < 0) return false;
            var result = SoundEffects.Echo(this.BufferToModify, this.FileFormat, delayBetweenRepeats, repeats, delayUnit);
            this.ModifiedBuffer = result;
            return true;
        }
    }
}
