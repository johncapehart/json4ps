using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;


namespace Json.Automation {
    [Cmdlet(VerbsCommon.New, "VariableFromJson", SupportsShouldProcess = true)]
    [OutputType(typeof(void))]
    public class NewVariableFromJsonCmdlet : SelectJsonCmdletBase {

        protected override void BeginProcessing() {
            this.AsVariables = true;
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
