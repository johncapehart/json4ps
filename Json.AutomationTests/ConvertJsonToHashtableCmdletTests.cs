using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Management.Automation;

namespace Json.Automation.Tests {
    [TestClass()]
    public class ConvertJsonToHashtableCmdletTestClass : TestBase {

        [TestMethod()]
        public void ConvertJsonToHashtableCmdletTest1() {
            var rs = getRunspace();
            var result = rs.Invoke(@"Convert-JsonToHashtable 'test3.json'");
            Hashtable h = result[0].BaseObject as Hashtable;
            var h2 = (h["definitions"] as Hashtable);
            Assert.AreEqual("John", h2["name"]);
            var h3 = h2["data0"] as Hashtable;
            var h4 = h3["data1"] as Hashtable;
            Assert.AreEqual((Int64)1, h4["one"]);
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletKeySeparatorTest1() {
            var rs = getRunspace();
            var result = rs.Invoke(@"Convert-JsonToHashtable 'test3.json' -KeySeparator '-'");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(Hashtable));
            var h = result[0].BaseObject as Hashtable;
            Assert.AreEqual("John", h["definitions-name"]);
        }

        [TestMethod()]
        public void ConvertJsonToHashtableCmdletFunctionNameTest1() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name
}
$t = gc 'test3.json' -Raw
$t = Select-Json $t 'definitions' 
$p = Convert-JsonToHashtable $t -FunctionName 'Test-Splat';
Test-Splat @p
");
            Assert.AreEqual("John", result[0].ToString());
            result = rs.Invoke(@"
$p = Convert-JsonToHashtable $t;
try {Test-Splat @p } catch { $_ }
");
            Assert.AreEqual("ErrorRecord", result[0].BaseObject.GetType().Name);
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesTest1() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name
}
$t = gc 'test3.json' -Raw
$t = Select-Json $t 'definitions' 
$p = Convert-JsonToHashtable $t -FunctionName 'Test-Splat' -AsPSDefaultParameterValues;
Test-Splat
");
            Assert.AreEqual("John", result[0].ToString());
            result = rs.Invoke(@"$PSDefaultParameterValues");
            var d = result[0].BaseObject as DefaultParameterDictionary;
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual("John", d["Test-Splat:name"]);
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesTest2() {
            var rs = getRunspace();
            //var result = rs.Invoke(@"import-module 'Json4PS'");
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name
}
$t = Select-Json 'test1.json' 'r' 
$p = Convert-JsonToHashtable $t -AsPSDefaultParameterValues -FunctionName 'Test-Splat' -Clone;
Test-Splat
");
            Assert.AreEqual("John", result[0].ToString());
            result = rs.Invoke(@"$PSDefaultParameterValues ");
            var d = result[0].BaseObject as DefaultParameterDictionary;
            Assert.AreEqual("John", d["Test-Splat:name"]);
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesTest3() {
            var rs = getRunspace();
            //var result = rs.Invoke(@"import-module 'Json4PS'");
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name
    $foo
}
$PSDefaultParameterValues = $PSDefaultParameterValues.Clone();
$t = '{""Test-Splat"":{""name"":""John""},""*"":{""foo"":""bar""}}'
$p = Convert-JsonToHashtable $t -AsPSDefaultParameterValues
Test-Splat
");
            Assert.AreEqual("John", result[0].ToString());
            Assert.AreEqual("bar", result[1].ToString());
            result = rs.Invoke(@"$PSDefaultParameterValues | out-string ");
            Assert.IsTrue(result[0].ToString().Contains("John"));
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesTest4() {
            var rs = getRunspace();
            //var result = rs.Invoke(@"import-module 'Json4PS'");
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $date,
    $foo
    )
    $date
    $foo
}
$PSDefaultParameterValues = $PSDefaultParameterValues.Clone();
$t = '{""Test-Splat"":{""name"":""John""},""*"":{""date"":""{ dir c:\\Windows* | select -f 1 -expand Name }""}}'
$p = Convert-JsonToHashtable $t -AsPSDefaultParameterValues
");
            result = rs.Invoke(@"$PSDefaultParameterValues | out-string");
            Assert.IsTrue(result[0].ToString().Contains("Windows"));
            result = rs.Invoke(@"Test-Splat");
            Assert.IsTrue(result[0].ToString().Contains("Windows"));
        }

        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesCloneDepthTest1() {
            var rs = getRunspace();
            //var result = rs.Invoke(@"import-module 'Json4PS'");
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $e
    )
    $name
    $e
}
$t = '{""d"":{""Test-Splat"":{""name"":""John"",""e"":""{ dir c:\\Windows* | select -f 1 -expand Name }""}}}'
$p = Convert-JsonToHashtable $t -AsPSDefaultParameterValues -Path 'd' -Clone
");
            result = rs.Invoke(@"$PSDefaultParameterValues");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(DefaultParameterDictionary));
            Assert.AreEqual("John", ((DefaultParameterDictionary)result[0].BaseObject)["Test-Splat:name"]);
            result = rs.Invoke(@"Test-Splat");
            Assert.AreEqual("Windows", result[1].ToString());
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesBadScriptBlockTest1() {
            var rs = getRunspace();
            try {
                var result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $e
    )
    $name
    $e
}
$t = '{""Test-Splat"":{""name"":""John"",""e"":""{{ c:\\Windows* | select -f 1 -expand Name }""}}'
$p = Convert-JsonToHashtable $t -AsPSDefaultParameterValues -Clone
");
                Assert.Fail("Expected exception was not thrown");
            } catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains(@"Missing closing '}' in statement block or type definition"));
            }
        }

        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesScriptBlockTest3() {
            var rs = getRunspace();
            var result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name
    $foo
}
$o = '{""Test-Splat"":{""name"":""John"",""foo"":""{Get-Date -Format M/dd/yyyy}""}}'
Convert-JsonToHashtable $o -AsPSDefaultParameterValues -KeySeparator "":"" -Clone
$PSDefaultParameterValues
Test-Splat -Verbose
");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(DefaultParameterDictionary));
            Assert.AreEqual(DateTime.Today, DateTime.Parse(result[2].BaseObject.ToString()));
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesForceTest1() {
            var rs = getRunspace();
            var result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $number
    )
    $name
    $number
}
$o = '{""Test-Splat"":{""name"":""John"",""number"":99}}'
Convert-JsonToHashtable $o -AsPSDefaultParameterValues -KeySeparator "":"" -Clone
$PSDefaultParameterValues
Test-Splat -Verbose
");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(DefaultParameterDictionary));
            Assert.AreEqual("John", result[1].BaseObject.ToString());
            Assert.AreEqual((long)99, result[2].BaseObject);
            result = rs.Invoke(@"
$o = '{""Test-Splat"":{""name"":""Dan""}}'
Convert-JsonToHashtable $o -AsPSDefaultParameterValues -Force
$PSDefaultParameterValues
Test-Splat -Verbose
");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(DefaultParameterDictionary));
            Assert.AreEqual("Dan", result[1].BaseObject.ToString());
            Assert.AreEqual((long)99, result[2].BaseObject);

            try {
                result = rs.Invoke(@"
$o = '{""Test-Splat"":{""name"":""Dan""}}'
Convert-JsonToHashtable $o -AsPSDefaultParameterValues
$PSDefaultParameterValues
");
                Assert.Fail("Expected exception was not thrown");
            } catch (Exception ex) {
                Assert.IsNotNull(ex);
            }
        }
        [TestMethod()]
        public void ConvertJsonToHashtableCmdletDroppedValuesTest1() {
            var rs = getRunspace();
            var result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $number
    )
    $name
    $number
}
$o = '{""e"":{""d"":{""Test-Splat"":{""name"":""John""},""*"":{""number"":99}},""dropme"":101}}'
Convert-JsonToHashtable $o -AsPSDefaultParameterValues -Path 'e.d' -Clone
$PSDefaultParameterValues
Test-Splat -Verbose
");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(DefaultParameterDictionary));
            var d = result[0].BaseObject as DefaultParameterDictionary;
            Assert.AreEqual(2, d.Count);
            Assert.AreEqual("John", result[1].BaseObject.ToString());
            Assert.AreEqual((long)99, result[2].BaseObject);

        }
        [TestMethod]
        public void ConvertJsonToHashtableCmdletAsPSDefaultParameterValuesTest6() {
            var rs = getRunspace();
            var result = rs.Invoke(@"
$PSDefaultParameterValues = $PSDefaultParameterValues.clone();
$PSDefaultParameterValues.Clear();
Convert-JsonToHashtable 'test6.json' -AsPSDefaultParameterValues
$PSDefaultParameterValues
");
            Assert.IsInstanceOfType(result[0].BaseObject, typeof(DefaultParameterDictionary));
        }
    }
}
