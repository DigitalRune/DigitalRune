. .\Scripts\Common.ps1

# This script finds all AssemblyInfo.template and generates/updates the
# AssemblyInfo.cs using TortoiseSVN SubWCRev.
$sourceVersionFile = "AssemblyInfo.template"  # Name of source version file.
$targetVersionFile = "AssemblyInfo.cs"        # Name of target version file.
$workingCopyPath = ".."                       # Path to working copy relative to version file.


function Invoke-SubWCRev([System.IO.FileInfo]$sourceFileInfo, 
                         [System.String]$targetVersionFile, 
                         [System.String]$workingCopyPath)
{
    $versionPath = $sourceFileInfo.DirectoryName
    $workingCopyPath = Join-Path $versionPath $workingCopyPath
    $sourceVersionFile = $sourceFileInfo.FullName
    $targetVersionFile = Join-Path $versionPath $targetVersionFile
    SubWCRev $workingCopyPath $sourceVersionFile $targetVersionFile -f
    DigitalRune:Test-LastExitCode "SubWCRev"
}


Get-ChildItem -File -Include $sourceVersionFile -Recurse | ForEach-Object { Invoke-SubWCRev $_ $targetVersionFile $workingCopyPath }
