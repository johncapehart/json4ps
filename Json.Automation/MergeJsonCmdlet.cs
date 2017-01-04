using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;

namespace Json.Automation {
    [Cmdlet(VerbsData.Merge, "Json")]
    [OutputType(typeof(String))]
    public class MergeJsonCmdlet : JsonCmdlet {

        [Parameter(Mandatory = false, Position = 1)]
        public string MergeObject { get; set; }

        JToken _mergeObject;

        protected override void BeginProcessing() {
            base.BeginProcessing();
            if (!string.IsNullOrEmpty(MergeObject)) {
                _mergeObject = JToken.Parse(MergeObject);
            }
        }

        protected override void MergePipeline() {
            if (_mergeObject != null) {
                foreach (var i in InputObject) {
                    var o = JsonSource.GetJson(i, SessionState.Path.CurrentFileSystemLocation) as JObject;
                    o.Merge(_mergeObject);
                    WriteResult(o);
                }
            } else {
               base.MergePipeline();
            }
        }
        protected override void ProcessRecord() {
            base.ProcessRecord();
            MergePipeline();
        }
        protected override void EndProcessing() {
            if (_baseObject != null && _mergeObject == null) {
                WriteResult(_baseObject);
            }
        }
    }
}
