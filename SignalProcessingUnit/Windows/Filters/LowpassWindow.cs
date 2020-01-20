using SoundManipulation;
using StringUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalProcessingUnit.Windows.Filters {
    public partial class LowpassWindow : EffectParametresWindow {

        public LowpassWindow() : base() {
            InitializeComponent();
            this.InitWindowControls();
        }

        private void InitWindowControls() {
            this.LoadWindowFunctions();
            this.LoadDataFrameLengths();
            this.button1.Click += (obj, e) => {
                if (!this.ApplyEffect()) {
                    MessageBox.Show("Please check your input; Some of the values are incorrect.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
                //var t = new Thread(() => {
                //});
                //this.DialogResult = DialogResult.OK;
                //this.Close();
            };
        }

       


        private void LoadWindowFunctions() {
            var windowFuncs = typeof(WindowFunctions).
                GetMethods(BindingFlags.Public | BindingFlags.Static).
                Select(mi => String.Join(" ", mi.Name.SplitUppercase())).
                Where(name => !name.Contains("Gaussian")).
                ToArray();
            this.comboBox1.DataSource = windowFuncs;
            this.comboBox1.SelectedItem = windowFuncs[0];
        }

        private void LoadDataFrameLengths() {
            var lengths = Enumerable.
                Range(10, 4).
                Select(pow => (int)Math.Pow(2, pow)).
                ToArray();
            this.comboBox2.DataSource = lengths;
            this.comboBox2.SelectedItem = lengths[0];
        }

        protected override bool ApplyEffect() {
            if (String.IsNullOrWhiteSpace(this.textBox1.Text)) return false;
            bool parsingBoundarySuccessful = Double.TryParse(this.textBox2.Text, out double res);
            if (!parsingBoundarySuccessful) {
                return false;
            }
            double freqBoundary = res;
            int frameLength = (int)this.comboBox2.SelectedItem;
            bool parsingSuccessful = Int32.TryParse(this.textBox1.Text, out int windowLength);
            if (!parsingSuccessful) {
                return false;
            }
            string funcName = String.Join("", ((string)this.comboBox1.SelectedItem).Split(' '));
            MethodInfo windowFunction = typeof(WindowFunctions).GetMethod(funcName, BindingFlags.Static | BindingFlags.Public);
            if (windowFunction == null) {
                return false;
            }
            WindowFunc func = new WindowFunc() {
                Length = windowLength,
                Function = Delegate.CreateDelegate(typeof(Func<int, int, double>), windowFunction) as Func<int, int, double>
            };
            var result = SoundManipulation.Filters.Lowpass(
                this.BufferToModify, 
                frameLength, 
                freqBoundary, 
                func, 
                this.FileFormat.SampleRate
            );
            this.ModifiedBuffer = result;
            return true;
        }
    }
}
