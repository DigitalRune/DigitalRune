# This script formats C# projects (*.csproj) and MonoGame content builder projects (*.mgcb).
# The included code/asset files are sorted alphabetically, which makes it easy to compare 
# and merge project files for different platforms with a merge tool like WinMerge.

. .\Scripts\Common.ps1

try
{
    Add-Type -Path .\Source\DigitalRune.Build\bin\Release\DigitalRune.Build.dll
}
catch
{
    DigitalRune:Write-Error "Could not find DigitalRune.Build.dll. Please build the Release version of DigitalRune.Build before calling this script."
    throw 
}

[DigitalRune.Build.CSProjFormatter]::ProcessFolder("Source", $true)
[DigitalRune.Build.CSProjFormatter]::ProcessFolder("Samples", $true)
[DigitalRune.Build.CSProjFormatter]::ProcessFolder("Tests", $true)
[DigitalRune.Build.CSProjFormatter]::ProcessFolder("Tools", $true)
#[DigitalRune.Build.MgcbFormatter]::ProcessFolder("Distribution\Samples\Content", $true)
