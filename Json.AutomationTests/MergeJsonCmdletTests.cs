using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Management.Automation;

namespace Json.Automation.Tests {
    [TestClass()]
    public class MergeJsonCmdletTestClass : TestBase {
        [TestMethod()]
        public void MergeJsonCmdletWithMergeObject() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = '{""r"":{""name"":""John"",""height"":6,""age"":30}}'
$s2 = '{""r"":{""name"":""Mary"",""height"":5.5}}'
Merge-Json $s1 -MergeObject $s2
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("Mary", o["r"]["name"]);
            Assert.AreEqual(5.5, o["r"]["height"]);
            Assert.AreEqual(30, o["r"]["age"]);
        }
        [TestMethod()]
        public void MergeJsonCmdletTest2() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = '{""r"":{""name"":""John"",""height"":6,""age"":30}}'
$s2 = '{""r"":{""name"":""Mary"",""height"":5.5}}'
@($s1, $s2) | Merge-Json
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("Mary", o["r"]["name"]);
            Assert.AreEqual(5.5, o["r"]["height"]);
            Assert.AreEqual(30, o["r"]["age"]);
        }
        [TestMethod()]
        public void MergeJsonCmdletTest3() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":""Mary"",""height"":5.5}}'
Merge-Json @($s1, $s2)
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("Mary", o["r"]["name"]);
            Assert.AreEqual(5.5, o["r"]["height"]);
        }
        [TestMethod()]
        public void MergeJsonCmdletTest4() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":""Mary""}}'
Merge-Json @('test1.json', $s2)
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("Mary", o["r"]["name"]);
            Assert.AreEqual(6, o["r"]["height"]);
        }
        [TestMethod()]
        public void MergeJsonCmdletRegressionTest1() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":""Mary""}}'
Merge-Json $s1 $s2
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("Mary", o["r"]["name"]);
            Assert.AreEqual(6, o["r"]["height"]);
        }
        [TestMethod()]
        public void MergeJsonCmdletRegressionTest2() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            Exception ex0 = null;
            try {
                result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":""Mary""}}'
Merge-Json $s1 $s2 -JsonPath 'r'
");
            } catch (Exception ex) {
                ex0 = ex;
            }
            Assert.IsNotNull(ex0);
        }
        [TestMethod()]
        public void MergeJsonCmdletTestWithNull() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":null,""foo"":""bar""}}'
Merge-Json @('test1.json', $s2)
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("John", o["r"]["name"]);
            Assert.AreEqual(6, o["r"]["height"]);
            Assert.AreEqual("bar", o["r"]["foo"]);
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":null,""foo"":""bar""}}'
Merge-Json @('test1.json', $s2) -AsHashTable
");
            var h = (Hashtable)result[0].BaseObject;
            h = (Hashtable)h["r"];
            Assert.AreEqual("bar", h["foo"]);
}
        [TestMethod()]
        public void MergeJsonCmdletTestWithBoolean() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":null,""foo"":false,""bar"":true}}'
Merge-Json @('test1.json', $s2)
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("John", o["r"]["name"]);
            Assert.AreEqual(6, o["r"]["height"]);
            Assert.AreEqual(false, o["r"]["foo"]);
            Assert.AreEqual(true, o["r"]["bar"]);
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":null,""foo"":false,""bar"":true}}'
Merge-Json 'test1.json', $s2 -AsHashtable
");
            Hashtable h = (Hashtable)((Hashtable)result[0].BaseObject)["r"];
            Assert.AreEqual(false, h["foo"]);
            Assert.AreEqual(true, h["bar"]);
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":null,""foo"":false,""bar"":true}}'
Merge-Json 'test1.json', $s2 -AsPSDefaultParameterValues
$PSDefaultParameterValues;
");
            h = (Hashtable)result[0].BaseObject;
        }
        [TestMethod()]
        public void MergeJsonCmdletTestWithArray() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
$s1 = gc 'test1.json' -raw
$s2 = '{""r"":{""name"":null,""foo"":[""bar"",""blech""]}}'
Merge-Json @('test1.json', $s2)
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("John", o["r"]["name"]);
            Assert.AreEqual(6, o["r"]["height"]);
            Assert.IsInstanceOfType(o["r"]["foo"], typeof(JArray));
        }
        [TestMethod()]
        public void MergeJsonCmdletTestFromFile() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
Merge-Json '.\test1.json','..\test2.json'
");
            JObject o = JObject.Parse(result[0].ToString());
            Assert.AreEqual("Mary", o["r"]["name"]);
            Assert.AreEqual(6, o["r"]["height"]);
        }
        [TestMethod()]
        public void MergeJsonCmdletTestFromFile2() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
Merge-Json '..\..\..\build.json','..\..\..\build.secrets.json' -AsPSDefaultParameterValues -Clone
$PSDefaultParameterValues;
");
            var d = (DefaultParameterDictionary)result[0].BaseObject;
            Assert.AreEqual("Json4PS", d[@"Publish-Module:ModuleName"].ToString());
        }
        [TestMethod()]
        public void MergeJsonCmdletTestFromFile3() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
Merge-Json '..\..\..\build.json','..\..\..\build.secrets.json' -AsHashtable
");
            var h = (Hashtable)result[0].BaseObject;
            var h2 = (Hashtable)h["Publish-Module"];
            Assert.AreEqual("Json4PS", h2["ModuleName"]);
        }
    }
}
