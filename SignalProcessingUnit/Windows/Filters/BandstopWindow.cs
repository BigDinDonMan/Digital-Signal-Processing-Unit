﻿using GeneralUtils;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalProcessingUnit.Windows.Filters {
    public partial class BandstopWindow : EffectParametresWindow {
        public BandstopWindow() {
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
            if (!Int32.TryParse(this.textBox1.Text, out int windowLength)) {
                return false;
            }
            if (!Double.TryParse(this.textBox3.Text, out double lowerFreqBoundary)) {
                return false;
            }
            if (!Double.TryParse(this.textBox2.Text, out double upperFreqBoundary)) {
                return false;
            }
            if (upperFreqBoundary < lowerFreqBoundary) {
                GeneralUtilities.Swap(ref upperFreqBoundary, ref lowerFreqBoundary);
            }
            int frameLength = (int)this.comboBox2.SelectedItem;
            string funcName = String.Join("", (this.comboBox1.SelectedItem as string).Split(' '));
            MethodInfo methodInfo = typeof(WindowFunctions).GetMethod(funcName, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null) return false;
            WindowFunc windowFunction = new WindowFunc() {
                Length = windowLength,
                Function = Delegate.CreateDelegate(typeof(Func<int, int, double>), methodInfo) as Func<int, int, double>
            };
            var result = SoundManipulation.Filters.Bandstop(
                this.BufferToModify, 
                frameLength, 
                lowerFreqBoundary, 
                upperFreqBoundary, 
                windowFunction, 
                this.FileFormat.SampleRate
            );
            this.ModifiedBuffer = result;
            return true;
        }

        private void BandstopWindow_Load(object sender, EventArgs e) {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void label5_Click(object sender, EventArgs e) {

        }
    }
}
