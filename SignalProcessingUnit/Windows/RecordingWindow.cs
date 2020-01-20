using NAudio.Wave;
using SoundManipulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace SignalProcessingUnit.Windows {
    public partial class RecordingWindow : Form {

        public enum ChannelsType {
            Mono = 1,
            Stereo = 2
        }

        private static readonly Dictionary<string, int> SamplingRates;

        static RecordingWindow() {
            SamplingRates = new Dictionary<string, int>();
            var displayStrings = new[] { "44.1 kHz", "16 kHz" };
            var sampleRatesInHZ = new[] { 44100, 16000 };
            foreach (var (key, value) in displayStrings.Zip(sampleRatesInHZ, (o1, o2) => (key: o1, value: o2))) {
                SamplingRates.Add(key, value);
            }
        }

        private SoundRecorder _recorder = null;
        private System.Windows.Forms.Timer _recordingTimer = null;
        private Stopwatch _stopwatch = null;

        public RecordingWindow() {
            InitializeComponent();
            this.InitControls();
            this.FormClosing += (obj, e) => this._recorder?.Dispose();
        }

        private void InitControls() {
            this.button2.Enabled = false;
            this.button1.Click += (obj, e) => this.StartRecording();
            this.button2.Click += (obj, e) => this.StopRecording();
            this.button3.Click += (obj, e) => {
                using (var folderDialog = new FolderBrowserDialog()) {
                    if (folderDialog.ShowDialog() == DialogResult.OK) {
                        this.textBox1.Text = folderDialog.SelectedPath;
                    }
                }
            };

            var keys = SamplingRates.Keys.ToArray();
            var values = Enum.GetValues(typeof(ChannelsType)).Cast<ChannelsType>().Cast<object>().ToArray();
            this.comboBox1.Items.AddRange(keys);
            this.comboBox2.Items.AddRange(values);
            this.comboBox1.SelectedItem = keys[0];
            this.comboBox2.SelectedItem = values[0];
        }

        public void StartRecording() {
            this.label4.Text = "00:00.00";
            var channels = (int)((ChannelsType)this.comboBox2.SelectedItem);
            var sampleRate = (int)(SamplingRates[this.comboBox1.SelectedItem as string]);
            var fileFormat = new WaveFormat(sampleRate, channels);
            this._recorder = new SoundRecorder(fileFormat, this.textBox1.Text);
            this._recorder.Start();
            this._recordingTimer = this.CreateTimer();
            this._stopwatch = Stopwatch.StartNew();
            this.button1.Enabled = false;
            this.button2.Enabled = true;
        }

        private System.Windows.Forms.Timer CreateTimer() {
            var timer = new System.Windows.Forms.Timer {
                Interval = 1000,
                Enabled = true
            };
            timer.Tick += (obj, e) => {
                this.label4.Text = (this._stopwatch == null ? "00:00.00" : String.Format("{0:hh\\:mm\\:ss}", this._stopwatch.Elapsed));
            };
            return timer;
        }

        public void StopRecording() {
            this._recorder.Stop();
            this._recorder.Dispose();
            this.button1.Enabled = true;
            this.button2.Enabled = false;
            this._recordingTimer.Stop();
            this._stopwatch.Stop();
        }
    }
}
