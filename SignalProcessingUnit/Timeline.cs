using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundManipulation;

namespace SignalProcessingUnit {
    public partial class Timeline : UserControl {

        private const int TRACK_VIEWER_HEIGHT = 80;
        private static readonly TimeSpan DEFAULT_TIMELINE_LENGTH = TimeSpan.FromSeconds(10);
        private const int TIMELINE_LENGTH_PER_SECOND = 50;
        private const float TIMELINE_LENGTH_PER_MILLISECOND = TIMELINE_LENGTH_PER_SECOND / 1000f;
        private const int TIMELINE_POSITION_OFFSET_X = 25;
        private const int TIMELINE_POSITION_OFFSET_Y = -25;

        private BindingList<TrackViewer> _trackViewers;
        public BindingList<TrackViewer> TrackViewers { get => _trackViewers; set => _trackViewers = value; }

        private TimeSpan _timelineLength;
        public TimeSpan TimelineLength { get => _timelineLength; set => _timelineLength = value; }

        public Timeline() {
            InitializeComponent();
            this.TrackViewers = new BindingList<TrackViewer>() {
                AllowNew = true, AllowRemove = true, RaiseListChangedEvents = true, AllowEdit = true
            };
            this.TrackViewers.ListChanged += (obj, e) => {
                this.TimelineLength = this.TrackViewers.Max(track => track.File.Length);
                this.Invalidate();
            };
            this.InitDragDrop();
            this.Invalidate();
        }

        private void DrawInitialLines(PaintEventArgs e) {
            //drawing the Y axis //TODO: add drawing of the values: thresholds from 0 to 0.5, 0.5 to 1, 0 to -0.5 and -0.5 to -1
            Point yStart, yEnd;
            yStart = new Point(TIMELINE_POSITION_OFFSET_X, TIMELINE_POSITION_OFFSET_Y);
            yEnd = new Point(TIMELINE_POSITION_OFFSET_X, this.Height + TIMELINE_POSITION_OFFSET_Y);
            e.Graphics.DrawLine(
                Pens.Black,
                yStart,
                yEnd
            );
            float xAxisLength = this.GetXAxisLength();
            PointF start, end;
            start = new PointF(TIMELINE_POSITION_OFFSET_X, this.Height + TIMELINE_POSITION_OFFSET_Y);
            end = new PointF(xAxisLength + TIMELINE_POSITION_OFFSET_X, this.Height + TIMELINE_POSITION_OFFSET_Y);
            e.Graphics.DrawLine(
                Pens.Black,
                start,
                end
            );
            PointF _start, _end;
            _start = new PointF(TIMELINE_POSITION_OFFSET_X, TIMELINE_POSITION_OFFSET_Y);
            _end = new PointF(TIMELINE_POSITION_OFFSET_X + xAxisLength, TIMELINE_POSITION_OFFSET_Y);
            e.Graphics.DrawLine(Pens.Black, _start, _end);
        }

        private void DrawTimeline(PaintEventArgs e) {
            this.DrawInitialLines(e);
        }

        private float GetXAxisLength() {
            return TIMELINE_LENGTH_PER_SECOND * this.TimelineLength.Seconds + TIMELINE_LENGTH_PER_MILLISECOND * this.TimelineLength.Milliseconds;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            this.DrawTimeline(e);
        }


        private void Timeline_Load(object sender, EventArgs e) {
            this.Refresh();
        }

        private void InitDragDrop() {
            this.AllowDrop = true;
            this.DragEnter += (obj, e) => {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            };
            this.DragDrop += (obj, e) => {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files) {
                    if (files.Length > 0) {
                        if (System.IO.Path.GetExtension(files[0]).ToLower() != ".wav") {
                            MessageBox.Show("Only .wav files are supported!");
                            return;
                        }
                        var wavFile = SoundProcessing.ReadWavFile(files[0]);
                        
                        var time = SoundProcessing.GetSoundDuration(files[0]);
                        if (this.TimelineLength.CompareTo(time) < 0) {
                            this.TimelineLength = time;
                        }
                        var viewer = this.CreateNewTrackViewer(wavFile);
                        this.TrackViewers.Add(viewer);
                        this.Controls.Add(viewer);
                        viewer.InitializeComponent();
                        viewer.Invalidate();
                    }
                }
            };
        }

        private TrackViewer CreateNewTrackViewer(WaveFile file) {
            var viewer = new TrackViewer() { File = file, Height = TRACK_VIEWER_HEIGHT, Width = 1000 };
            return viewer;
        }
    }
}
