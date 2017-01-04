cd (Split-Path -Parent $MyInvocation.MyCommand.Path)
$DebugPreference = 'SilentlyContinue'
$VerbosePreference = 'SilentlyContinue'
$moduleName = (gi (get-location)).Name
$psISE.PowerShellTabs.Files | where IsSaved -ne $true | where DisplayName -match $moduleName | % { $_.Save() }
remove-module $moduleName -force -ErrorAction SilentlyContinue
./Build.ps1
Import-Module $moduleName -Verbose -ArgumentList $true -Force
Invoke-Pester
