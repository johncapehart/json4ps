using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;

namespace Json.Automation {
    public class JsonConverter {
        protected PSCmdlet _context;
        public Nullable<int> Depth;
        public Nullable<int> KeyLength { get; set; }
        public string KeySeparator { get; set; }
        protected string DefaultPrefix { get; set; }
        public bool AsJson { get; set; }
        public Formatting Format { get; set; }
        public JPropertyFilter Filter { get; set; }
        public JsonConverter(PSCmdlet context, JPropertyFilter filter = null, string defaultPrefix = null, Nullable<int> depth = null, Nullable<int> keyLength = null, string keySeparator = null, Nullable<bool> asJson = null, Nullable<Formatting> formatting = null) {
            _context = context;
            Depth = depth;
            KeyLength = keyLength;
            KeySeparator = keySeparator;
            if (KeySeparator != null) {
                KeyLength = KeyLength ?? 2;
            }
            AsJson = asJson ?? false;
            if (AsJson) {
                Format = formatting ?? Formatting.None;
            }
            Filter = filter;
            DefaultPrefix = defaultPrefix;
            if (DefaultPrefix != null) {
                _keyStack.Push(DefaultPrefix);
            }
            if (KeyLength != null) {
                Depth = Math.Max((int)(Depth ?? 1), (int)KeyLength);
            }
        }
        public Stack<string> _keyStack = new Stack<string>();
        public string ComposeName(JProperty p) {
            var name = p.Name;
            if (KeyLength != null) {
                var ks = _keyStack.Reverse().ToArray();
                for (var i = 0; i < KeyLength - 1; i++) {
                    if (i < ks.Count()) {
                        name = ks[i] + KeySeparator + name;
                    }
                }
            }
            return name;
        }

        public void WriteDebug(string s) {
            if (_context != null) {
                _context.WriteDebug(s);
            }
        }
        public void WriteVerbose(string s) {
            if (_context != null) {
                _context.WriteVerbose(s);
            }
        }
    }
    public class JsonToHashtableConverter : JsonConverter {

        public JsonToHashtableConverter(PSCmdlet context, JPropertyFilter filter = null, string defaultPrefix = null, Nullable<int> depth = null, Nullable<int> keyLength = null, string keySeparator = null, Nullable<bool> asJson = null, Nullable<Formatting> formatting = null) :
            base(context, filter, defaultPrefix, depth, keyLength, keySeparator, asJson, formatting) {
        }

        public Hashtable Convert(string json) {
            return Convert(JToken.Parse(json)) as Hashtable;
        }

        public object Convert(JToken o) {
            object result = null;
            switch (o.Type) {
                case JTokenType.Object:
                    result = Convert((JObject)o);
                    break;
                case JTokenType.Array:
                    result = Convert((JArray)o);
                    break;
                case JTokenType.Property:
                    throw new System.Exception("Unexpected type");
                default:
                    result = Convert((JValue)o);
                    break;
            }
            return result;
        }

        private object Convert(JValue v) {
            if (v.Type == JTokenType.String) {
                var s = (string)v.Value;
                if (s.StartsWith("{") && s.EndsWith("}")) {
                    try {
                        WriteVerbose(string.Format("Creating ScriptBlock from {0}", s));
                        s = s.Substring(1, s.Length - 2);
                        return ScriptBlock.Create(s);
                    } catch (Exception ex) {
                        WriteDebug(string.Format("Expect Exception creating ScriptBlock", ex.ToString()));
                        throw ex;
                    }
                }
                return s;
            }
            return v.Value;
        }

        private Object Convert(JObject o) {
            if (AsJson && _keyStack.Count > (Depth ?? 0)) {
                return JsonConvert.SerializeObject(o, Format);
            } else {
                Hashtable result = new Hashtable();
                var c = o.Properties().Select(p => {
                    if (Filter == null || Filter(p)) {
                        _keyStack.Push(p.Name);
                        var v = Convert(p.Value);
                        _keyStack.Pop();
                        if (KeyLength == null || _keyStack.Count == KeyLength - 1) {
                            var name = ComposeName(p);
                            result.Add(name, v);
                        } else if (v is Hashtable) {
                            // float entries to top
                            foreach (DictionaryEntry item in v as Hashtable) {
                                result[item.Key] = item.Value;
                            }
                        } else {
                            WriteDebug(string.Format("Dropping {0} of type {1}", p.Name, p.Value.Type));
                            return 0;
                        }
                        return 1;
                    } else {
                        return 0;
                    }
                }).Count();
                WriteDebug(string.Format("Conversion count {0}", c));
                return result;
            }
        }

        private object[] Convert(JArray a) {
            var result = new List<object>();
            a.All(e => {
                result.Add(Convert(e));
                return true;
            });
            return result.ToArray();
        }
    }
}
