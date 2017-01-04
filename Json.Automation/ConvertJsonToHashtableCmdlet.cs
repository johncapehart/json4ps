using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Json.Automation {
    [Cmdlet(VerbsData.Convert, "JsonToHashtable")]
    [OutputType(typeof(Hashtable))]
    public class ConvertJsonToHashtableCmdlet : SelectJsonCmdletBase {
        protected override void BeginProcessing() {
            _emitHashtable = true;
            base.BeginProcessing();
        }
        protected override void ProcessRecord() {
            base.ProcessRecord();
            MergePipeline();
        }

        protected override void EndProcessing() {
            base.EndProcessing();
        }
    }
}
