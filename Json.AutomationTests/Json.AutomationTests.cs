using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Management.Automation;

namespace Json.Automation.Tests {

    public class TestBase {
        public RunspaceInvoke getRunspace() {
            var rs = new RunspaceInvoke();
            var result = rs.Invoke(@"import-module '.\Json.Automation.dll'");
            return rs;
        }
    }

    [TestClass()]
    public class SelectHashtableCmdletTestClass : TestBase {
        [TestMethod()]
        public void SelectHashtableCmdletTest1() {
            var rs = getRunspace();
            var result = rs.Invoke(@"
function Test-Splat {
    [CmdLetBinding()]param (
    $name,
    $foo
    )
    $name
}
$t = @{'name'='John';'foo'='bar';'extra'='bad'}
$p = Select-Hashtable $t -FunctionName 'Test-Splat';
Test-Splat @p
");
            Assert.AreEqual("John", result[0].ToString());
        }
    }
}