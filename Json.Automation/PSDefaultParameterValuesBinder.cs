using Newtonsoft.Json.Linq;
using System.Collections;
using System.Management.Automation;

namespace Json.Automation {
    public class PSDefaultParameterValuesBinder : HashtableBinder {
        bool _force;
        public PSDefaultParameterValuesBinder(PSCmdlet context, bool clone, bool force) :
            base(context) {
            if (clone) {
                var v = _context.SessionState.PSVariable.Get("PSdefaultParameterValues");
                var d = (Hashtable)v.Value;
                var d2 = new DefaultParameterDictionary((Hashtable)d.Clone());
                var v2 = new PSVariable("PSdefaultParameterValues", d2);
                _context.SessionState.PSVariable.Set(v2);
            }
            _force = force;
        }

        public void Bind(Hashtable h) {
            foreach (var k in h.Keys) {
                var ks = k.ToString();
                var v = h[k];
                if (v != null) {
                    Bind(ks, v);
                }
            }
        }

        public void Bind(string key, object value) {
            var v = _context.SessionState.PSVariable.Get("PSDefaultParameterValues");
            var d = (Hashtable)v.Value;
            if (_force) {
                if (d.ContainsKey(key)) {
                    d.Remove(key);
                }
            }
            d.Add(key, value);
            _context.WriteVerbose(string.Format("Added entry to PSdefaultParameterValues {0} with value {1}", key, value.ToString()));
        }
    }
}
