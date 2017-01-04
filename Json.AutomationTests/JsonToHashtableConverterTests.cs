using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;

namespace Json.Automation.Tests {
    [TestClass()]
    public class JsonToHashtableConverterTests {
        [TestMethod()]
        public void JsonToHashtableConverterTest1() {
            var s = @"{""a"":""1"",""b"":""bee""}";
            var h = new JsonToHashtableConverter(null).Convert(s);
            Assert.AreEqual(2, h.Count);
        }

        private bool _filter(JProperty p) {
            return p.Name == "a";
        }

        [TestMethod()]
        public void JsonToHashtableConverterTest2() {

            var s = @"{""a"":""1"",""b"":""bee""}";
            var h = new JsonToHashtableConverter(null, _filter).Convert(s);
            Assert.AreEqual(1, h.Count);
        }
        [TestMethod()]
        public void JsonToHashtableConverterTest3() {
            Exception saveex = null;
            try {
                new JsonToHashtableConverter(null, _filter).Convert(new JProperty("key", "value"));
            } catch (Exception ex) {
                saveex = ex;
            }
            Assert.IsNotNull(saveex);
        }
        [TestMethod()]
        public void JsonToHashtableConverterTest4() {
            Hashtable o = new JsonToHashtableConverter(null, null, null, null, null, ":").Convert(JObject.Parse(@"{""r"":{""name"":""John""},""*"":{""foo"":""bar""}}")) as Hashtable;
            Assert.AreEqual("John", o["r:name"]);
            Assert.AreEqual("bar", o["*:foo"]);
        }
    }
}