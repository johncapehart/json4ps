using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Management.Automation;

namespace Json.Automation.Tests {
    [TestClass()]
    public class NewVariableFromJsonCmdletTestClass : TestBase {
        [TestMethod()]
        public void NewVariableFromJsonTestSimple() {
            var rs = getRunspace();
            var result = rs.Invoke(@"$testString='{ ""foo"":""bar""}'");
            result = rs.Invoke(@"New-VariableFromJson $testString");
            result = rs.Invoke(@"$foo");
            Assert.AreEqual("bar", result[0].ToString());
        }

        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithSelect() {
            var rs = getRunspace();
            var result = rs.Invoke(@"$testString='{""r"":{ ""foo"":""bar""}}'");
            result = rs.Invoke(@"New-VariableFromJson (Select-Json $testString 'r')");
            result = rs.Invoke(@"$foo");
            Assert.AreEqual("bar", result[0].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithMerge() {
            var rs = getRunspace();
            var result = rs.Invoke(@"$testString1='{""r"":{ ""foo"":""bar""}}'");
            result = rs.Invoke(@"$testString2='{""r"":{ ""foo"":""blech""}}'");
            result = rs.Invoke(@"$testString1, $testString2 | New-VariableFromJson -Path 'r'");
            result = rs.Invoke(@"$foo");
            Assert.AreEqual("blech", result[0].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithMerge2() {
            var rs = getRunspace();
            var result = rs.Invoke(@"$testString1='{""r"":{ ""foo"":""bar""}}'");
            result = rs.Invoke(@"$testString2='{""r"":{ ""foo"":""blech""}}'");
            result = rs.Invoke(@"New-VariableFromJson $testString1,$testString2 -Path 'r'");
            result = rs.Invoke(@"$foo");
            Assert.AreEqual("blech", result[0].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithMerge3() {
            var rs = getRunspace();
            var result = rs.Invoke(@"Get-Module Json.Automation");
            result = rs.Invoke(@"New-VariableFromJson 'test1.json','test2.json' -Path 'r'");
            result = rs.Invoke(@"$name");
            Assert.AreEqual("Mary", result[0].ToString());
            result = rs.Invoke(@"$height");
            Assert.AreEqual("6", result[0].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithScope() {
            var rs = getRunspace();
            var result = rs.Invoke(@"$testString='{""r"":{ ""foo"":""bar"", ""blech"":""zork""}}'");
            result = rs.Invoke(@"function foofunc { new-variable -name 'scopetest' -value 99 -scope 1; New-VariableFromJson (Select-Json $testString 'r') -scope 1 }");
            result = rs.Invoke(@"foofunc;$foo");
            result = rs.Invoke(@"$foo");
            Assert.AreEqual("bar", result[0].ToString());
            result = rs.Invoke(@"$blech");
            Assert.AreEqual("zork", result[0].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletScopeDepthTest() {
            var rs = getRunspace();
            var result = rs.Invoke(@"$testString='[{""r"":{ ""foo"":""bar"", ""blech"":""zork""}}]'");
            result = rs.Invoke(@"function foofunc { New-VariableFromJson $testString -scope 1 -Path '$[0].r' }");
            result = rs.Invoke(@"foofunc");
            result = rs.Invoke(@"$foo");
            Assert.AreEqual("bar", result[0].ToString());
            result = rs.Invoke(@"$blech");
            Assert.AreEqual("zork", result[0].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletOptionTest() {
            var rs = getRunspace();
            var result = rs.Invoke(@"$testString='[{""foo"":""bar""}]'");
            result = rs.Invoke(@"function foofunc { New-VariableFromJson $testString -option 'Private'; $foo }");
            result = rs.Invoke(@"foofunc;$foo");
            Assert.AreEqual("bar", result[0].ToString());
            Assert.IsNull(result[1]);
            result = rs.Invoke(@"function foofunc { New-VariableFromJson $testString -option 'ReadOnly' -scope 1; $foo }");
            result = rs.Invoke(@"foofunc;$foo");
            Assert.AreEqual("bar", result[0].ToString());
            Assert.AreEqual("bar", result[1].ToString());
            try {
                result = rs.Invoke(@"$foo = 1");
                Assert.Fail("Expected exception was not thrown");
            } catch (Exception ex) {
                Assert.AreEqual("Cannot overwrite variable foo because it is read-only or constant.", ex.Message);
            }
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithSpecialCharacters() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
New-VariableFromJson(gc test4.json -raw);
$special
");
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("foo %", result[0].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithComposeKey() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
New-VariableFromJson test1.json -KeyLength 2;
$r_name
$r_array
");
            Assert.IsTrue(result.Count == 3);
            Assert.AreEqual("John", result[0].ToString());
            Assert.AreEqual("one", result[1].ToString());
        }
        [TestMethod()]
        public void NewVariableFromJsonCmdletTestWithArray() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module json4ps");
            result = rs.Invoke(@"
New-VariableFromJson test5.json -AsJson;
$UnlockRequest
");
            Assert.IsTrue(result.Count == 1);
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(string));
            Assert.IsTrue(result[0].ToString().Contains(@"""Hostname"":""someserver"""));
            result = rs.Invoke(@"
New-VariableFromJson test5.json;
$UnlockRequest
");
            Assert.IsTrue(result.Count == 1);
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(Hashtable));
            var h = result[0].BaseObject as Hashtable;
            Assert.AreEqual("someserver", h["Hostname"]);
        }
    }

}