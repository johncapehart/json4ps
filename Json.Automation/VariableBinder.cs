using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Json.Automation {
    public class VariableBinder : HashtableBinder {
        public Nullable<ScopedItemOptions> Options { get; set; }
        public Nullable<int> Scope { get; set; }

        public VariableBinder(PSCmdlet context, Nullable<int> scope, Nullable<ScopedItemOptions> options) :
            base(context) {
            Scope = scope;
            Options = options;
        }

        public void Bind(Object o) {
            if (o is Hashtable) {
                var h = o as Hashtable;
                foreach (var k in h.Keys) {
                    var ks = k.ToString();
                    var v = h[k];
                    if (v != null) {
                        Bind(ks, v);
                    }
                }
            } else if (o is Array) {
                var a = o as Array;
                foreach (var e in a) {
                    Bind(e);
                }
            }
        }

        public void Bind(string name, object value) {
            if (Scope == null) {
                // if scope is null we can use the direct Set method
                PSVariable v = (Options == null) ?
                    new PSVariable(name, value) :
                    new PSVariable(name, value, (ScopedItemOptions)Options);
                _context.SessionState.PSVariable.Set(v);
            } else {
                // otherwise we use the New-Variable command
                var pipeline = Runspace.DefaultRunspace.CreateNestedPipeline();
                pipeline.Commands.Add("New-Variable");
                var c = pipeline.Commands[0];
                c.Parameters.Add("Name", name);
                c.Parameters.Add("Value", value);
                c.Parameters.Add("Scope", Scope);
                if (Options != null) {
                    c.Parameters.Add("Option", Options);
                }
                pipeline.Invoke();
            }
            var check = _context.GetVariableValue(name);
            _context.WriteVerbose(string.Format("Created variable {0} with value {1}", name, value.ToString()));
        }
    }
}
