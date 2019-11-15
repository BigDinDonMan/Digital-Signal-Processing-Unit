using NAudio.Wave;
using SignalProcessingUnit.Windows;
using SignalProcessingUnit.Windows.Effects;
using SoundManipulation;
using StringUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//TODO: create a custom attribute and mark the methods to use as processing methods
namespace SignalProcessingUnit {
    public partial class Form1 : Form {

        public Form1() {
            InitializeComponent();
            this.SizeChanged += (obj, e) => {
                this.trackViewer1.FitToScreen();
            };
            this.LoadFilters();
            this.LoadEffects();
            this.LoadProcessingMethods();
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
            return SoundManipulation.SoundProcessing.ReadWavFile(path);
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
                SoundManipulation.SoundProcessing.Play(this.trackViewer1.SelectionBuffer.ToByteSamples(), this.trackViewer1.File.FileFormat);
            } else {
                SoundManipulation.SoundProcessing.Play(this.trackViewer1.File.SoundBuffer.ToByteSamples(), this.trackViewer1.File.FileFormat);
            }
        }

        private void effectsAndFiltersToolStripts_Click(object sender, EventArgs e) {
            if (sender is ToolStripMenuItem toolStrip) {
                string[] namespaces = { "Filters", "Effects"};
                foreach (var name in namespaces) {
                    try {
                        string windowClassName = String.Format("SignalProcessingUnit.Windows.{0}.{1}Window", name, toolStrip.Text);
                        var argsWindow = (EffectParametresWindow)Activator.CreateInstance(Type.GetType(windowClassName));
                        //set the buffer here, from the focused TrackViewer
                        var dialogResult = argsWindow.ShowDialog();
                        if (dialogResult != DialogResult.OK) return;
                        double[] buffer = argsWindow.BufferToModify;
                        argsWindow.Dispose();
                        break;
                    } catch (Exception) { }
                }
            }
        }

        private void processingMethodsToolStrips_Click(object sender, EventArgs e) {
            if (sender is ToolStripMenuItem toolStrip) {
                string methodName = String.Join("", toolStrip.Text.Split());
                string windowClassName = "SignalProcessingUnit.Windows.Processing." + methodName + "Window";
                try {
                    var argsWindow = (EffectParametresWindow)Activator.CreateInstance(Type.GetType(windowClassName));
                    //fill buffer here
                    var result = argsWindow.ShowDialog();
                    if (result != DialogResult.OK) return;
                    double[] buffer = argsWindow.BufferToModify;
                    argsWindow.Dispose();
                } catch (Exception) {
                    //method does not need parametres, like reversing the signal
                }
            }
        }

        private void LoadFilters() {
            MethodInfo[] filterMethods = typeof(Filters).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var filter in filterMethods) {
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Text = filter.Name
                };
                item.Click += this.effectsAndFiltersToolStripts_Click;
                this.filtersToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void LoadEffects() {
            MethodInfo[] effects = typeof(SoundEffects).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var effect in effects) {
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Text = effect.Name
                };
                item.Click += this.effectsAndFiltersToolStripts_Click;
                this.effectsToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void LoadProcessingMethods() {
            MethodInfo[] processingMethods = typeof(SoundProcessing).
                GetMethods(BindingFlags.Static | BindingFlags.Public).
                Where(info => info.GetCustomAttribute(typeof(ProcessingMethodAttribute)) != null).
                ToArray();
            foreach (var processingMethod in processingMethods) {
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Text = String.Join(" ", processingMethod.Name.SplitUppercase())
                };
                item.Click += this.processingMethodsToolStrips_Click;
                this.processingToolStripMenuItem.DropDownItems.Add(item);
            }
        }
    }
}
