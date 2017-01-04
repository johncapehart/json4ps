[CmdletBinding()]param(
    [switch]$RegisterAliases
)
$dllpath = "$PSScriptRoot\lib\Json.Automation.dll"
write-verbose "Importing $dllpath"
import-module $dllpath

if ($RegisterAliases) {
    #region ConvertFrom-Json proxy
    function ConvertFrom-Json {
     [CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=217031', RemotingCapability='None')]
         param(
            [Parameter(Mandatory=$true, Position=1, ValueFromPipeline=$true)]
            [AllowEmptyString()]
            [string]
            $InputObject,
            [Parameter(Mandatory=$false)]$FunctionName,
            [switch]$AsHashtable
        )
        process {
            if ($AsHashtable) {
                Convert-JsonToHashtable $InputObject
            } else {
                Microsoft.PowerShell.Utility\ConvertFrom-Json @PSBoundParameters
            }
        }
    }
    #endregion

    # Copied from log4ps.psm1
    function Register-Aliases {
        [CmdletBinding()]
        Param(
            [string]
            $ModuleName = $PSCmdlet.MyInvocation.MyCommand.Module.Name,
            [string]
            $Prefix = $PSCmdlet.MyInvocation.MyCommand.Module.Prefix,
            [string[]]
            [ValidateSet('ConvertFrom-Json')] 
            $Streams = @('ConvertFrom-Json')
        )

        Process {
            foreach($aliasName in $streams) {
                Remove-Item "alias:$aliasName" -Force -ErrorAction SilentlyContinue
                $verb = ($aliasName -split '-')[0]
                $noun = ($aliasName -split '-')[1] 
                Set-Alias -Scope Global -Name $aliasName -Value "$ModuleName\$verb-${prefix}$noun"
            }
        }
    }

    Register-Aliases

    $MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
        Remove-Item alias:'ConvertFrom-Json' -ErrorAction SilentlyContinue
    }
}
