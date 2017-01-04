# This is a PowerShell Unit Test file.
# You need a unit test framework such as Pester to run PowerShell Unit tests. 
# You can download Pester from http://go.microsoft.com/fwlink/?LinkID=534084
#

Describe 'Convert-JsonToHashtable' {
	Context 'Basic' {
		It 'Checks a simple json conversion to a Hashtable' {
            $json = gc ./PesterTests/Test3.json -Raw
            $h = Convert-JsonToHashtable $json
            $h.GetType() -eq [Hashtable] | should be $true
            $h["definitions"]["name"] | should be 'john'
        }
    }
    Context 'Functional' {
        function Test-Splat {
            [CmdletBinding()]param (
            [Parameter()]$name,
            $foo
            )
            Write-Verbose "Test-Splat $name, $foo" -Verbose
            $name,$foo
        }
        $PSDefaultParameterValues.Clone();
        $PSDefaultParameterValues.Clear();

		It 'Splats json' {
            $json = gc ./PesterTests/Test4.json -raw
            $h = Convert-JsonToHashtable (Select-Json $json 'definitions') -FunctionName 'Test-Splat'
            Test-Splat @h
        }
		It 'Splats json throwing an error for extra parameter' {
            $json = gc ./PesterTests/Test4.json -raw
            $h = Convert-JsonToHashtable (Select-Json $json 'definitions')
            { Test-Splat @h } | should throw
        }
		It 'Creates PSDefaultParameterValues with Json' {
            $PSDefaultParameterValues.Count | should be 0
            $o = 'PesterTests\test4.json'
            Select-Json $o 'definitions' -AsPSDefaultParameterValues -Function 'Test-Splat' -Clone
            $PSDefaultParameterValues['Test-Splat:name'] | should be 'John'
            $PSDefaultParameterValues.Count | should be 2
            $result = Test-Splat -Verbose
            $result[0] | should be 'John'
            $result[1] | should be 'bar'
        }
		It 'Checks that global PSDefaultParameterValues have not been changed' {
            $PSDefaultParameterValues.Count | should be 0
        }
		It 'Creates PSDefaultParameterValues with Json' {
            $o = '{"Test-Splat":{"name":"John","foo":"{Get-Date -Format M/dd/yyyy}"}}'
            Convert-JsonToHashtable $o -AsPSDefaultParameterValues -KeySeparator ":" -Clone
            $PSDefaultParameterValues['Test-Splat:name'] | should be 'John'
            $result = Test-Splat -Verbose
            $result[0] | should be 'John'
            $result[1] | should be (Get-Date -Format M/dd/yyyy)
        }
		It 'Creates PSDefaultParameterValues with Json' {
            $PSDefaultParameterValues = $PSDefaultParameterValues.Clone();
            $PSDefaultParameterValues.Clear();
            function foofunc1 {
            [CmdletBinding()]param (
                [Parameter()]$name,
                $foo,
                $date,
                $array
                )
                Write-Verbose "foofunc1 $name, $foo, $date, $array" -Verbose
                $name,$foo,$date,$array
            }

            function foofunc2 {
            [CmdletBinding()]param (
                [Parameter()]$name,
                $foo,
                $date,
                $anull
                )
                Write-Verbose "foofunc2 $name, $foo, $date" -Verbose
                $name,$foo,$date,($anull -eq $null)
            }

            #Convert-JsonToHashtable 'PesterTests\test6.json' -AsVariables
            Convert-JsonToHashtable 'PesterTests\test6.json' -AsPSDefaultParameterValues -Clone
            $result1 = foofunc1
            $result1.count | should be 4
            $result1[3].count | should be 2
            $result2 = foofunc2
            $result2.count | should be 4
            $result2[3] | should be $true
        }
	}
}

Describe 'Select-Json' {
	Context 'Basic' {
		It 'Checks a simple json select' {
            $o =  @"
            {"r":{"foo":"bar"}}
"@
            $r = select-json -inputObject $o 'r.foo'
            $r | should match "bar"
        }
	}
}


Describe 'Merge-Json' {
	Context 'Basic' {
		It 'Check a simple json merge' {
            $o = '{"WebHookData":{"name":"foo","length":11264}}'
            $m = '{"WebHookData":{"name":"bar"}}'
            $r = merge-json -inputObject $o -mergeobject $m
            $r | should match 'bar'
            $r | should match '11264'
        }
	}
}
Describe 'Merge-Json' {
	Context 'From Files' {
		It 'Check a simple json merge' {
            $o = '{"WebHookData":{"name":"foo","length":11264}}'
            $m = '{"WebHookData":{"name":"bar"}}'
            $r = merge-json -inputObject $o -mergeobject $m
            $r | should match 'bar'
            $r | should match '11264'
        }
	}
}

Describe 'New-VariableFromJson' {
	Context 'Basic' {
		It 'Creates Variables with Json' {
            $o = '{"name":"foo","length":11264}'
            New-VariableFromJson $o
            $name | should be "foo"
        }
	}
	Context 'File' {
		It 'Creates Variables with Json' {
            New-VariableFromJson 'PesterTests\test4.json' -JsonPath 'definitions'
            $foo | should be "bar"
        }
	}
}
