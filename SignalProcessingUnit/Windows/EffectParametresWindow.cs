using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalProcessingUnit.Windows {
    public class EffectParametresWindow : Form {
        public double[] BufferToModify { get; set; }

        public EffectParametresWindow() : base() {}


        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // EffectParametresWindow
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "EffectParametresWindow";
            this.Load += new System.EventHandler(this.EffectParametresWindow_Load);
            this.ResumeLayout(false);

        }

        private void EffectParametresWindow_Load(object sender, EventArgs e) {

        }
    }
}
