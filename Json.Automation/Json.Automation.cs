using System.Collections.Generic;
using System.Management.Automation;

namespace Json.Automation {

    [Cmdlet(VerbsCommon.Get, "FunctionParameterNames")]
    [OutputType(typeof(List<string>))]
    public class GetFunctionParameterNamesCmdlet : PSCmdlet {
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        [Parameter(Mandatory = false, Position = 1)]
        public string ParameterSet { get; set; }

        protected override void ProcessRecord() {
            base.ProcessRecord();
            WriteObject(FunctionParameterFilter.GetFunctionParameters(Name, ParameterSet).ToArray());
        }
    }
}
