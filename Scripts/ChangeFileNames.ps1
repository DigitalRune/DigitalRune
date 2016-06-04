 # This script replaces _ with - in file names: xx_xxx_xx to xx-xxx-xx.

function RenameFile
{
    param ([System.IO.FileInfo]$fileInfo)
    
    $fileInfo.Name
    $newName = $fileInfo.Name.Replace('_', '-')
    #Write-Output $newName

    Rename-Item -Path $fileInfo.FullName -NewName $newName
}
 
 Get-ChildItem -File | ForEach-Object { RenameFile $_ }
