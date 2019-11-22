using CollectionUtils;
using SignalProcessingUnit.Windows;
using SoundManipulation;
using StringUtils;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;
//TODO: create a custom attribute and mark the methods to use as processing methods
//TODO: create a config file with settings set from gui (with new window) and save them to the file
namespace SignalProcessingUnit {
    public partial class Form1 : Form {

        public Form1() {
            InitializeComponent();
            EffectWindows.FromEffectName("benin");
            this.SizeChanged += (obj, e) => {
                this.trackViewer1.ClearSelection();
                this.trackViewer1.FitToScreen();
            };
            this.LoadFilters();
            this.LoadEffects();
            this.LoadProcessingMethods();
            this.SetKeybinds();
        }

        private void SetKeybinds() {
            this.KeyPreview = true;
            this.KeyDown += (obj, e) => {
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C) {
                    TrackViewer.TrackViewers.ForEach(tv => tv.ClearSelection());
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.R) {
                    TrackViewer.TrackViewers.FirstOrDefault(tv => tv.IsSelected)?.Refresh();
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.P) {
                    playToolStripMenuItem_Click(obj, e);
                }
            };
            this.clearSelectionToolStripMenuItem.ShowShortcutKeys = true;
            this.clearSelectionToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + C";
            this.refreshToolStripMenuItem.ShowShortcutKeys = true;
            this.refreshToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + R";
            this.playToolStripMenuItem.ShowShortcutKeys = true;
            this.playToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + P";
        }

        private void Form1_Load(object sender, EventArgs e) {
            foreach (var obj in this.Controls) {
                Control control = (Control)obj;
                control.Refresh();
            }
            if (this.CanFocus) {
                this.Focus();
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
                SoundManipulation.SoundProcessing.Play(this.trackViewer1.Selection.Buffer.ToByteSamples(), this.trackViewer1.File.FileFormat);
            } else {
                SoundManipulation.SoundProcessing.Play(this.trackViewer1.File.SoundBuffer.ToByteSamples(), this.trackViewer1.File.FileFormat);
            }
        }

        //TODO: create a method to "fuse" effect buffer and sound buffer to avoid duplicate code
        private void effectsAndFiltersToolStripts_Click(object sender, EventArgs e) {
            if (sender is ToolStripMenuItem toolStrip) {
                string[] namespaces = { "Filters", "Effects"};
                foreach (var name in namespaces) {
                    try {
                        string windowClassName = String.Format("SignalProcessingUnit.Windows.{0}.{1}Window", name, toolStrip.Text);
                        var argsWindow = (EffectParametresWindow)Activator.CreateInstance(Type.GetType(windowClassName));
                        //set the buffer here, from the focused TrackViewer
                        #region Temporary effect buffer mapping
                        TrackViewer selectedTrackViewer = TrackViewer.TrackViewers.FirstOrDefault(tv => tv.IsSelected);
                        if (selectedTrackViewer == null) {
                            MessageBox.Show("Please select a sound!");
                            return;
                        }
                        argsWindow.BufferToModify = selectedTrackViewer.HasSelection ? selectedTrackViewer.Selection.Buffer : selectedTrackViewer.File.SoundBuffer;
                        argsWindow.FileFormat = selectedTrackViewer.File.FileFormat;
                        var dialogResult = argsWindow.ShowDialog();
                        if (dialogResult != DialogResult.OK) return;
                        var buffer = argsWindow.ModifiedBuffer;
                        argsWindow.Dispose();
                        //mapping
                        int start = 0, end = 0;
                        start = selectedTrackViewer.Selection.SelectionRectangle.Value.X * selectedTrackViewer.SamplesPerPixel;
                        end = (selectedTrackViewer.Selection.SelectionRectangle.Value.X + selectedTrackViewer.Selection.SelectionRectangle.Value.Width) * selectedTrackViewer.SamplesPerPixel;
                        int index = 0;
                        for (int i = start; i < end; ++i) {
                            selectedTrackViewer.File.SoundBuffer[i] = buffer[index++];
                        }
                        //applying the effect
                        if (selectedTrackViewer.HasSelection) {
                            var toLeftOfSelection = selectedTrackViewer.File.SoundBuffer.Slice(0, selectedTrackViewer.Selection.OriginStartPoint).ToList();
                            toLeftOfSelection.AddRange(buffer);
                            toLeftOfSelection.AddRange(selectedTrackViewer.File.SoundBuffer.Slice(selectedTrackViewer.Selection.OriginEndPoint, selectedTrackViewer.File.SoundBuffer.Length - selectedTrackViewer.Selection.OriginEndPoint));
                            selectedTrackViewer.File.SoundBuffer = toLeftOfSelection.ToArray();
                        } else {
                            selectedTrackViewer.File.SoundBuffer = argsWindow.ModifiedBuffer;
                        }
                        selectedTrackViewer.FitToScreen();
                        selectedTrackViewer.Refresh();
                        #endregion
                        break;
                    } catch (Exception) {}
                }
            }
        }

        private void processingMethodsToolStrips_Click(object sender, EventArgs e) {
            if (sender is ToolStripMenuItem toolStrip) {
                string methodName = String.Join("", toolStrip.Text.Split());
                string windowClassName = String.Format("SignalProcessingUnit.Windows.Processing.{0}Window", methodName);
                try {
                    var argsWindow = (EffectParametresWindow)Activator.CreateInstance(Type.GetType(windowClassName));
                    //fill buffer here
                    var result = argsWindow.ShowDialog();
                    if (result != DialogResult.OK) return;
                    double[] buffer = argsWindow.ModifiedBuffer;
                    argsWindow.Dispose();
                } catch (Exception) {
                    //method does not need parametres, like reversing the signal
                    MethodInfo method = typeof(SoundProcessing).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                    if (method == null) return;
                    TrackViewer selectedTrackViewer = TrackViewer.TrackViewers.FirstOrDefault(tv => tv.IsSelected);
                    if (selectedTrackViewer == null) {
                        MessageBox.Show("Please select a sound!");
                        return;
                    }
                    double[] buffer = selectedTrackViewer.HasSelection ? selectedTrackViewer.Selection.Buffer : selectedTrackViewer.File.SoundBuffer;
                    method.Invoke(null, new object[] { buffer });
                    if (selectedTrackViewer.HasSelection) {
                        int index = 0;
                        for (int i = selectedTrackViewer.Selection.OriginStartPoint; i < selectedTrackViewer.Selection.OriginEndPoint; ++i) {
                            selectedTrackViewer.File.SoundBuffer[i] = buffer[index++];
                        }
                    } else {
                        selectedTrackViewer.File.SoundBuffer = buffer;
                    }
                    selectedTrackViewer.Invalidate();
                }
            }
        }

        private void LoadFilters() {
            MethodInfo[] filterMethods = typeof(Filters).
                GetMethods(BindingFlags.Static | BindingFlags.Public).
                OrderBy(m => m.Name).
                ToArray();
            foreach (var filter in filterMethods) {
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Text = filter.Name
                };
                item.Click += this.effectsAndFiltersToolStripts_Click;
                this.filtersToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void LoadEffects() {
            MethodInfo[] effects = typeof(SoundEffects).
                GetMethods(BindingFlags.Static | BindingFlags.Public).
                OrderBy(m => m.Name).
                ToArray();
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
                OrderBy(m => m.Name).
                ToArray();
            foreach (var processingMethod in processingMethods) {
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Text = String.Join(" ", processingMethod.Name.SplitUppercase())
                };
                item.Click += this.processingMethodsToolStrips_Click;
                this.processingToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            //clear selection from all track viewers/focused track viewers
            TrackViewer.TrackViewers.ForEach(tv => tv.ClearSelection());
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e) {
            var viewer = TrackViewer.TrackViewers.FirstOrDefault(tv => tv.IsSelected);
            if (viewer != null) {
                if (viewer.HasSelection) {
                    SoundProcessing.Play(viewer.Selection.Buffer.ToByteSamples(), viewer.File.FileFormat);
                } else {
                    viewer.File.Play();
                }
            }
        }
    }
}
