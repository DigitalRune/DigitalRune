Push-Location

try
{
    # Change working directory.# 
    Set-Location .\Content

    # Build content with MonoGame Content Builder tool.
    ..\ThirdParty\DigitalRune\References\MonoGame\Windows\MGCB.exe /@:Content.mgcb
    if ($LASTEXITCODE -ne 0)
    {
      throw "ERROR - MonoGame could not build content."
    }

    # ZIP content.
    ..\ThirdParty\DigitalRune\Tools\Pack.exe --output bin\MonoGame\Windows\Content.zip --recursive --directory bin\MonoGame\Windows\Content *.*
    if ($LASTEXITCODE -ne 0)
    {
      throw "ERROR - Pack.exe could not create ZIP archive."
    }
}
catch
{
    Write-Host -NoNewLine 'Press any key to continue...';
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
}
finally
{
    Pop-Location
}