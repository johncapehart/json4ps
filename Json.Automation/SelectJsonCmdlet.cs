using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;

namespace Json.Automation {

    public class SelectJsonCmdletBase : JsonCmdlet {
        [Parameter(Mandatory = false,
            Position = 1)]
        [Alias("Path")]
        public override string JsonPath { get; set; }
    }

    [Cmdlet(VerbsCommon.Select, "Json")]
    [OutputType(typeof(object))]
    public class SelectJson : SelectJsonCmdletBase {
        protected override void ProcessRecord() {
            base.ProcessRecord();
            MergePipeline();
        }
        protected override void EndProcessing() {
            base.EndProcessing();
        }
    }

}
