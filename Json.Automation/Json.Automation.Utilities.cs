using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Json.Automation {

    public class JsonSource {
        public static Regex JsonMatch = new Regex(@"^[\s]*([\{]|[\[])", RegexOptions.Multiline);
        public static Regex FileMatch = new Regex(@"^[\.\w]");

        public static JToken GetJson(string json, PathInfo currentDirectory = null) {
            if (!JsonMatch.IsMatch(json) && FileMatch.IsMatch(json)) {
                var path = json;
                if (currentDirectory != null && !Path.IsPathRooted(path)) {
                    // Path.Combine fails when path contains '..' so we concatentate instead
                    var file = new FileInfo(string.Concat(currentDirectory.Path, @"\", path));
                    path = file.FullName;
                }
                using (StreamReader r = new StreamReader(path)) {
                    json = r.ReadToEnd();
                }
            }
            return JToken.Parse(json);
        }
    }

    public class FunctionParameterFilter {
        public static List<string> GetFunctionParameters(string function, string parameterset = null) {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(function)) {
                var rs = Runspace.DefaultRunspace;
                var pipeline = rs.CreateNestedPipeline();
                var command = new Command("Get-Command");
                command.Parameters.Add("Name", function);
                pipeline.Commands.Add(command);
                var commandResult = pipeline.Invoke();
                if (commandResult.Count == 1) {
                    var c = commandResult[0].BaseObject as CommandInfo;
                    if (!string.IsNullOrEmpty(parameterset)) {
                        result = c.ParameterSets
                            .Where(i => i.Name == parameterset)
                            .Select(i => i.Parameters
                                .Select(p => p.Name)
                                .ToList()
                            )
                            .First();
                    } else {
                        result = c.Parameters
                            .Select(p => p.Key).ToList();
                    }
                }
            }
            return result;
        }

        public static explicit operator FunctionParameterFilter(string function) {
            return new FunctionParameterFilter(function);
        }

        public string FunctionName { get; set; }

        public ISet<string> _parameters;
        public FunctionParameterFilter(string functionName, string parameterset = null) {
            FunctionName = functionName;
            if (!string.IsNullOrEmpty(FunctionName)) {
                _parameters = new HashSet<string>(GetFunctionParameters(FunctionName, parameterset));
            }
        }
        public bool Filter(JProperty p) {
            return _parameters == null || _parameters.Contains(p.Name);
        }
        public bool Filter(DictionaryEntry p) {
            return _parameters == null || _parameters.Contains(p.Key);
        }
    }

    public delegate bool JPropertyFilter(JProperty p);

    public class JsonSelector {

        JPropertyFilter _filter;

        public JsonSelector(JPropertyFilter filter = null) {
            _filter = filter;
        }

        public string Select(string json, Formatting formatting = Formatting.Indented) {
            var o = JToken.Parse(json);
            Select(o);
            return JsonConvert.SerializeObject(o, formatting);
        }
        public void Select(JToken o) {
            switch (o.Type) {
                case JTokenType.Object:
                    Select((JObject)o);
                    break;
                case JTokenType.Array:
                    Select((JArray)o);
                    break;
                default:
                    throw new System.Exception("Unexpected type");
            }
        }
        private void Select(JObject o) {
            List<string> l = o.Properties()
                .Where(p => !_filter(p))
                .Select(p => p.Name)
                .ToList();
            l.ForEach(n => o.Remove(n));
        }
        private void Select(JArray a) {
            a.All(e => {
                Select(e);
                return true;
            });
        }
    }

    public class JsonMergeException : System.Exception {
        public JsonMergeException(string message) : base(message) {
        }
    }
}
