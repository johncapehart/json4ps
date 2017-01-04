using Json.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;

namespace Json.Automation.Tests {
    [TestClass()]
    public class JsonSelectorTests {

        private bool _filter(JProperty p) {
            return p.Name == "a";
        }

        [TestMethod()]
        public void SelectTest1() {

            var s = @"{""a"":""1"",""b"":""bee""}";
            var s1 = new JsonSelector(_filter).Select(s);
            var o = JObject.Parse(s1);
            Assert.AreEqual("1", o["a"]);
            Assert.IsNull(o["b"]);
        }
        public void SelectTest2() {
            Exception saveex = null;
            try {
                var p = new JProperty("key", "value");
                new JsonSelector(_filter).Select(p);
            } catch (Exception ex) {
                saveex = ex;
            }
            Assert.IsNotNull(saveex);
        }

        [TestMethod()]
        public void SelectTestError() {
            try {
                var o = JObject.Parse(@"{""foo"":""bar""}");
                new JsonSelector(_filter).Select(o["foo"]);
                Assert.Fail("Expected exception was not thrown");
            } catch (Exception ex) {
                Assert.IsNotNull(ex);
            }
        }
    }
}