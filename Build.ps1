cd (Split-Path -Parent $MyInvocation.MyCommand.Path)
. ../Gists/Publish-Module.ps1
# the following is needed to copy the .dlls before they are loaded by Merge-Json
Copy-Module -ComputerName 'localhost' -ModuleName Json4PS -BinarySourceDirectory lib -Verbose -Force
import-module Json4PS
{
    Merge-Json 'build.json','build.secrets.json' -AsPSDefaultParameterValues -Verbose -clone 
    Publish-Module -Verbose
}.Invoke()
