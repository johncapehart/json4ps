using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Text;

namespace Json.Automation {

    public class JsonCmdlet : PSCmdlet {
        [Parameter(Mandatory = true,
           Position = 0,
           ValueFromPipeline = true)]
        public string[] InputObject { get; set; }
        [Parameter(Mandatory = false)]
        [Alias("Format")]
        public Formatting Formatting { get; set; } = Formatting.None;
        public virtual string JsonPath { get; set; }

        [Parameter(Mandatory = false)]
        [Alias("FunctionName")]
        public FunctionParameterFilter Function { get; set; }
        [Parameter()]
        public Nullable<int> Depth { get; set; }
        [Parameter(Mandatory = false)]
        public Nullable<int> KeyLength { get; set; }
        [Parameter(Mandatory = false)]
        public string KeySeparator { get; set; }
        [Parameter()]
        public SwitchParameter AsHashtable { get; set; }
        [Parameter()]
        public SwitchParameter AsJson { get; set; }
        [Parameter()]
        public SwitchParameter AsPSDefaultParameterValues { get; set; }
        [Parameter()]
        public SwitchParameter Clone { get; set; }
        [Parameter()]
        public SwitchParameter Force { get; set; }
        [Parameter()]
        public SwitchParameter AsVariables { get; set; }
        [Parameter(Mandatory = false)]
        public Nullable<int> Scope { get; set; }
        [Parameter(Mandatory = false)]
        public Nullable<ScopedItemOptions> Option { get; set; }

        protected bool _emitHashtable = false;
        protected void WriteResult(JValue o) {
            WriteObject(o.Value);
        }

        public object ToHashtable(JToken o, string defaultPrefix, Nullable<int> keyLength, string keySeparator) {
            if (Function == null) {
                Function = new FunctionParameterFilter(null);
            }
            var jc = new JsonToHashtableConverter(this, Function.Filter, defaultPrefix, Depth, keyLength, keySeparator, AsJson, Formatting);
            return jc.Convert(o);
        }

        protected void WriteResult(JToken o) {
            if (o != null) {
                switch (o.Type) {
                    case JTokenType.Object:
                    case JTokenType.Array:
                    case JTokenType.Property:
                        if (AsPSDefaultParameterValues) {
                            if (this.ShouldProcess(_baseObject.ToString(), "Set Defaults")) {
                                string defaultPrefix = null;
                                if (Function != null) {
                                    defaultPrefix = Function.FunctionName;
                                }
                                new PSDefaultParameterValuesBinder(this, Clone, Force).Bind((Hashtable)ToHashtable(o, defaultPrefix, 2, ":"));
                            }
                        }
                        if (AsVariables) {
                            if (ShouldProcess(_baseObject.ToString(), "New-Variable")) {
                                string keySeparator = null;
                                if (KeyLength != null) {
                                    keySeparator = KeySeparator ?? "_";
                                };
                                new VariableBinder(this, Scope, Option).Bind(ToHashtable(o, null, KeyLength, keySeparator));
                            }
                        }
                        if (!AsVariables && !AsPSDefaultParameterValues) {
                            if (_emitHashtable || AsHashtable) {
                                WriteObject(ToHashtable(o, null, KeyLength, KeySeparator));
                            } else {
                                WriteObject(JsonConvert.SerializeObject(o, Formatting));
                            }
                        }
                        break;
                    default:
                        WriteResult(o as JValue);
                        break;
                }
            }
        }

        protected JToken _baseObject = null;

        protected virtual void MergePipeline() {
            foreach (var i in InputObject) {
                var jt = JsonSource.GetJson(i, SessionState.Path.CurrentFileSystemLocation);
                if (_baseObject == null) {
                    _baseObject = jt;
                } else {
                    if (_baseObject.Type == JTokenType.Array) {
                        throw new JsonMergeException(string.Format("Merging top level Json arrays is not supported by {0}", MyInvocation.InvocationName));
                    }
                    if (_baseObject.Type != JTokenType.Object) {
                        throw new JsonMergeException(string.Format("Merging Json primitives is not supported by {0}", MyInvocation.InvocationName));
                    }
                    var o = _baseObject as JObject;
                    o.Merge(jt);
                }
            }
        }

        protected override void EndProcessing() {
            base.EndProcessing();
            if (JsonPath == null) {
                JsonPath = string.Empty;
            }
            var result = _baseObject.SelectToken(JsonPath);
            if (Function != null) {
                new JsonSelector(Function.Filter).Select(result);
            }
            WriteResult(result);
        }


    }

}
