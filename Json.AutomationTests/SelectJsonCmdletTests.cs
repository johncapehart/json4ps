using Json.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Management.Automation;

namespace Json.AutomationTests {

    [TestClass()]
    public class GetFunctionParameterNamesCmdletTestClass {
        [TestMethod()]
        public void GetFunctionParameterNamesCmdletTest1() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"$p = Get-FunctionParameterNames 'Select-Object'");
            result = rs.Invoke(@"$p.Count");
            Assert.IsTrue((int)result[0].BaseObject > 0);
        }
        [TestMethod()]
        public void GetFunctionParameterNamesCmdletTest2() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"$p = Get-FunctionParameterNames 'Select-Object' 'DefaultParameter'");
            result = rs.Invoke(@"$p.Count");
            Assert.IsTrue((int)result[0].BaseObject > 0);
        }
    }

    [TestClass()]
    public class SelectJsonCmdletTestClass {
        [TestMethod()]
        public void SelectJsonCmdletTest1() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"Get-Module Json.Automation");
            Assert.IsTrue(result.Count == 1);
            result = rs.Invoke(@"$testString='{""r"":{ ""foo"":""bar""}}'");
            result = rs.Invoke(@"Select-Json $testString 'r.foo'");
            Assert.AreEqual("bar", result[0].ToString());
        }
        [TestMethod()]
        public void SelectJsonCmdletRegressionTest1() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"Get-Module Json.Automation");
            Assert.IsTrue(result.Count == 1);
            result = rs.Invoke(@"$testString='{""r"":{ ""foo"":""bar""}}'");
            result = rs.Invoke(@"Select-Json $testString -Path 'r.foo'");
            Assert.AreEqual("bar", result[0].ToString());
        }
        [TestMethod()]
        public void SelectJsonCmdletTest2() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"Get-Module Json.Automation");
            Assert.IsTrue(result.Count == 1);
            result = rs.Invoke(@"$testString='{""r"":{ ""foo"":8}}'");
            result = rs.Invoke(@"Select-Json $testString 'r.foo'");
            Assert.AreEqual("8", result[0].ToString());
        }
        [TestMethod()]
        public void SelectJsonCmdletTest3() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"$t = gc 'test3.json' -Raw; 
$t = Select-Json $t -Format 'Indented';
$o = $t | convertfrom-json;
$o.definitions.data0.data1.one.getType();
$t
");
            Assert.AreEqual("System.RuntimeType", result[0].BaseObject.GetType().ToString());
        }
        [TestMethod()]
        public void SelectJsonCmdletTest4() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"
import-module '.\Json.Automation.dll'
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name
}
$t = '{""name"":""John"",""foo"":""bar"",""extra"":""bad""}'
$p = Select-Json $t -FunctionName 'Test-Splat' -AsHashtable
Test-Splat @p
");
            Assert.AreEqual("John", result[0].ToString());
        }
        [TestMethod()]
        public void SelectJsonCmdletTest5() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"
import-module '.\Json.Automation.dll'
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name,$foo
}
$t = '[{""name"":""John"",""foo"":""bar"",""extra"":""bad""}]'
$p = Select-Json $t -FunctionName 'Test-Splat' -AsHashtable
$p
");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(Hashtable));
            result = rs.Invoke(@"$p1 = $p[0];Test-Splat @p1");
            Assert.AreEqual("John", result[0].ToString());
        }
        [TestMethod()]
        public void SelectJsonCmdletAsDefaultParameterValues() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"
import-module '.\Json.Automation.dll'
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name,$foo
}
    $o = '..\..\..\PesterTests\test4.json'
    Select-Json $o 'definitions' -AsPSDefaultParameterValues -Function 'Test-Splat'
$PSDefaultParameterValues
");
            Assert.AreEqual("John", ((Hashtable)result[0].BaseObject)["Test-Splat:name"]);
            result = rs.Invoke(@"Test-Splat");
            Assert.AreEqual("John", result[0].ToString());
        }
        [TestMethod()]
        public void SelectJsonCmdletTestException() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"
import-module '.\Json.Automation.dll'
$t = '[{""name"":""John"",""foo"":""bar"",""extra"":""bad""}]'
try {
    $p = Select-Json $t,'{}' -ErrorAction Stop
} catch {
    $_
}
");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(ErrorRecord));
        }
    }

}
