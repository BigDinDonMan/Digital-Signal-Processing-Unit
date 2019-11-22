using SignalProcessingUnit.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalProcessingUnit {
    public static class EffectWindows {
        public static EffectParametresWindow FromEffectName(string name) {
            var className = String.Format("SignalProcessingUnit.Windows.Effects.{0}Window", name);
            var type = Type.GetType(className);
            return FromType(type);
        } 

        public static EffectParametresWindow FromFilterName(string name) {
            var className = String.Format("SignalProcessingUnit.Windows.Filters.{0}Window", name);
            var type = Type.GetType(className);
            return FromType(type);
        }

        public static EffectParametresWindow FromProcessingMethodName(string name) {
            var className = String.Format("SignalProcessingUnit.Windows.Processing.{0}Window", name);
            try {
                var type = Type.GetType(className);
                return FromType(type);
            } catch (InvalidCastException) {
                return null;
            }
        }

        public static EffectParametresWindow FromType(Type t) {
            return (EffectParametresWindow)Activator.CreateInstance(t);
        }
    }
}
