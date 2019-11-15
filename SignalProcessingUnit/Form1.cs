using NAudio.Wave;
using SoundManipulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalProcessingUnit {
    public partial class Form1 : Form {

        public Form1() {
            InitializeComponent();
            // this.WindowState = FormWindowState.Maximized;
            this.SizeChanged += (obj, e) => {
                this.trackViewer1.FitToScreen();
            };
            this.KeyPreview = true;
            this.KeyDown += (obj, e) => {
                if (e.Modifiers == Keys.LControlKey && e.KeyCode == Keys.R) {
                    this.Refresh();
                }
            };
        }

        private void Form1_Load(object sender, EventArgs e) {
            foreach (var obj in this.Controls) {
                Control control = (Control)obj;
                control.Refresh();
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            this.trackViewer1.Refresh();
        }

        private WaveFile LoadFile(string path) {
            return SoundProcessing.ReadWavFile(path);
        }

        private void waveViewer1_Load(object sender, EventArgs e) {
        }

        private void trackViewer1_Load(object sender, EventArgs e) {
            this.trackViewer1.File = LoadFile("00_otusznje.wav");
            this.trackViewer1.FitToScreen();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e) {
            this.trackViewer1.FitToScreen();
        }

        private void button1_Click_1(object sender, EventArgs e) {
            //temporary
            if (this.trackViewer1.HasSelection) {
                SoundProcessing.Play(this.trackViewer1.SelectionBuffer.ToByteSamples(), this.trackViewer1.File.FileFormat);
            } else {
                SoundProcessing.Play(this.trackViewer1.File.SoundBuffer.ToByteSamples(), this.trackViewer1.File.FileFormat);
            }
        }
    }
}
