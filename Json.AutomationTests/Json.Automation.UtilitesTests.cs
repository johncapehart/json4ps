using Json.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Json.Automation.Tests {

    [TestClass()]
    public class JsonSourceTestClass {
        [TestMethod()]
        public void GetJsonTestFromFile1() {
            var o = JsonSource.GetJson(@"test1.json");
            Assert.IsNotNull(o["r"]);
        }
        [TestMethod()]
        public void GetJsonTestFromFile2() {
            var path = new System.IO.FileInfo(@"test1.json").FullName;
            var o = JsonSource.GetJson(path);
            Assert.IsNotNull(o["r"]);
        }
        [TestMethod()]
        public void GetJsonTestFromFile3() {
            var path = new System.IO.FileInfo(@".\test1.json").FullName;
            var o = JsonSource.GetJson(path);
            Assert.IsNotNull(o["r"]);
        }
        [TestMethod()]
        public void GetJsonTest2() {
            var o = JsonSource.GetJson(@"
{""r"":{""name"":""John"",""height"":6}}
");
            Assert.IsNotNull(o["r"]);
        }
        [TestMethod()]
        public void GetJsonTest3() {
            var o = JsonSource.GetJson(@"

{""r"":{""name"":""John"",""height"":6}}
");
            Assert.IsNotNull(o["r"]);
        }
    }
}