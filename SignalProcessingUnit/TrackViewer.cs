﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundManipulation;
using System.Timers;
using CollectionUtils;
using GeneralUtils;

namespace SignalProcessingUnit {

    public class Selection {
        private Rectangle? _selectionRectangle;
        private double[] _buffer;
        private int _originStartPoint;
        private int _originEndPoint;

        public Rectangle? SelectionRectangle { get => _selectionRectangle; set => _selectionRectangle = value; }
        public double[] Buffer { get => _buffer; set => _buffer = value; }
        public int OriginStartPoint { get => _originStartPoint; set => _originStartPoint = value; }
        public int OriginEndPoint { get => _originEndPoint; set => _originEndPoint = value; }
        public bool IsEmpty { get => this._selectionRectangle?.IsEmpty ?? true; }

        public void Clear() {
            this._selectionRectangle = new Rectangle(0, 0, 0, 0);
            this._originEndPoint = this._originStartPoint = 0;
            this._buffer = null;
        }
    }

    //TODO: RYSUJ KURWA W KONTROLCE, NIE CONTROLPAINT CIULU
     public partial class TrackViewer : UserControl {

        private WaveFile _file;
        private int _samplesPerPixel = 128;
        private Selection _selection;

        #region Properties

        public WaveFile File {
            get => _file;
            set {
                _file = value;
                if (_file != null) {
                    this.Width = _file.SoundBuffer.Length / _samplesPerPixel + 1;
                }
                this.Refresh();
            }
        }

        public int SamplesPerPixel {
            get => _samplesPerPixel;
            set {
                _samplesPerPixel = value;
                this.Invalidate();
            }
        }

        public bool HasSelection {
            get => !this.Selection.IsEmpty;
        }

        public Selection Selection {
            get => _selection;
            set => _selection = value;
        }
        
        #endregion

        public TrackViewer() {
            InitializeComponent();
            this.DoubleBuffered = true;
            this._selection = new Selection();
        }

        #region Events
        private void TrackViewer_SizeChanged(object sender, EventArgs e) {
            this.ClearSelection();
            this.FitToScreen();
        }

        private void TrackViewer_Load(object sender, EventArgs e) {
            this.Refresh();
        }
        #endregion

        #region Painting events
        private void DrawVerticalLine(PaintEventArgs e, int x) {
            e.Graphics.DrawLine(Pens.Black, x, 0, x, this.Height);
        }

        //private void PaintVerticalLines(PaintEventArgs e) {
        //    this._verticalLineCoords.ForEach(x => this.DrawVerticalLine(e, x));
        //}

        private void FillSelectionRect(PaintEventArgs e) {
            if (this.Selection.IsEmpty) return;
            e.Graphics.FillRectangle(Brushes.DarkGray, this.Selection.SelectionRectangle.Value);
        }

        protected override void OnPaint(PaintEventArgs e) {
            this.FillSelectionRect(e);
            this.PaintTrackWaveForm(e);
            base.OnPaint(e);
        }

        #region mouse handler functions and fields

        private bool _mouseDragging = false;
        private Point _mousePosition, _startPosition;

        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                _startPosition = e.Location;
                _mouseDragging = true;
                _mousePosition = new Point(-1, -1);
                this.Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (_mouseDragging && e.Button == MouseButtons.Left) {
                _mouseDragging = false;
                if (_mousePosition.X == -1) return;
                int x1, x2;
                x1 = _mousePosition.X;
                x2 = _startPosition.X;
                if (x2 > x1) {
                    GeneralUtilities.Swap(ref x2, ref x1);
                }
                this.Selection.SelectionRectangle = new Rectangle(x2, 0, x1 - x2, this.Height);
                this.MapSelectionToSound();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (_mouseDragging) {
               // this._verticalLineCoords.Add(_startPosition.X);
              //  this._verticalLineCoords.Add(e.X);
                if (_mousePosition.X != -1) {
                 //   this._verticalLineCoords.Add(_mousePosition.X);
                    int x1, x2;
                    x1 = _mousePosition.X;
                    x2 = _startPosition.X;
                    if (x2 > x1) {
                        GeneralUtilities.Swap(ref x2, ref x1);
                    }

                    this.Selection.SelectionRectangle = new Rectangle(x2, 0, x1 - x2, this.Height);
                }
                _mousePosition = e.Location;
                this.Invalidate();
            }
            base.OnMouseMove(e);
        }

        #endregion

        private void ClearSelection() {
            this.Invalidate();
        }


        public void FitToScreen() {
            if (_file == null || _file.SoundBuffer == null) return;
            if (this.Width == 0) return;
            this.SamplesPerPixel = _file.SoundBuffer.Length / this.Width;
        }

        private void MapSelectionToSound() {
            if (this.Selection.IsEmpty) return;
            int start = 0, end = 0;
            start = this.Selection.SelectionRectangle.Value.X * this._samplesPerPixel;
            end = (this.Selection.SelectionRectangle.Value.Width + this.Selection.SelectionRectangle.Value.X) * this._samplesPerPixel;
            if (start > end) {
                GeneralUtilities.Swap(ref start, ref end);
            }
            this._selection.Buffer = this._file.SoundBuffer.Slice(start, end - start);
            this._selection.OriginEndPoint = end;
            this._selection.OriginStartPoint = start;
        }

        private void PaintTrackWaveForm(PaintEventArgs e) {
            if (this._file == null) return;
            double[] buffer = this._file.SoundBuffer;
            if (buffer == null) return;
            short[] paintBuffer = new short[this._samplesPerPixel];
            int offset = 0;
            short max = 0, min = 0;

            void FindMinAndMax(int len) {
                for (int i = 0; i < len; ++i) {
                    paintBuffer[i] = (short)(buffer[i + offset] * Int16.MaxValue);
                    if (paintBuffer[i] > max) max = paintBuffer[i];
                    if (paintBuffer[i] < min) min = paintBuffer[i];
                }
            }

            for (float x = e.ClipRectangle.X; x < e.ClipRectangle.Right; x += 1f, offset += this._samplesPerPixel) {
                try {
                    FindMinAndMax(this._samplesPerPixel);
                } catch (IndexOutOfRangeException) {
                    FindMinAndMax(buffer.Length - offset);
                }

                float lowPercent, highPercent;
                lowPercent = ((float)min - short.MinValue) / ushort.MaxValue;
                highPercent = ((float)max - short.MinValue) / ushort.MaxValue;
                e.Graphics.DrawLine(Pens.Black, x, this.Height * lowPercent, x, this.Height * highPercent);
                min = max = 0;
            }
        }
        #endregion
    }
}
