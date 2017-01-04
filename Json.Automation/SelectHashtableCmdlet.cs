using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;

namespace Json.Automation {

    [Cmdlet(VerbsCommon.Select, "Hashtable")]
    [OutputType(typeof(Hashtable))]
    public class SelectHashtableCmdlet : PSCmdlet {
        [Parameter(Mandatory = true, Position = 0,
            HelpMessage = "Hashtable from which to select items"
        )]
        public Hashtable InputObject { get; set; }

        [Parameter(Mandatory = false, Position = 1)]
        [Alias("Function")]
        public string FunctionName { get; set; }

        FunctionParameterFilter _filter;
        Hashtable _result;
        protected override void BeginProcessing() {
            base.BeginProcessing();
            _filter = new FunctionParameterFilter(FunctionName);
            _result = new Hashtable();
        }

        protected override void ProcessRecord() {
            base.ProcessRecord();
            InputObject.Cast<DictionaryEntry>()
                .Where(e => _filter.Filter(e))
                .Select(e => { _result.Add(e.Key, e.Value); return 1; })
                .Count();
        }
        protected override void EndProcessing() {
            base.EndProcessing();
            WriteObject(_result);
        }
    }
}
