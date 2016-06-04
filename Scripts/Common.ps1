# Checks the exit code of a command (Windows EXE) and converts a failure exit
# code into a terminating error.
# See https://rkeithhill.wordpress.com/2009/08/03/effective-powershell-item-16-dealing-with-errors/
function DigitalRune:Test-LastExitCode([System.String] $commandName = "Command",
                                       [int[]] $successCodes = @(0), 
                                       [scriptblock] $cleanupScript = $null)
{
    if ($successCodes -contains $LastExitCode)
    {
        return;
    }

    if ($cleanupScript) 
    {
        "Executing cleanup script: $cleanupScript"
        &$cleanupScript
    }

    throw @"
$commandName failed with exit code $LastExitCode
CALLSTACK:$(Get-PSCallStack | Out-String)
"@
}


# Pauses the script and waits for user input.
# See https://adamstech.wordpress.com/2011/05/12/how-to-properly-pause-a-powershell-script/
function DigitalRune:Wait-Key
{
    If ($psISE) 
    {
        # We are in the PowerShell ISE.
        # The "ReadKey" functionality is not supported in Windows PowerShell ISE.
        # Show message box instead.
        $shell = New-Object -ComObject "WScript.Shell"
        $button = $shell.Popup("Click OK to continue.", 0, "Script Paused", 0)
        Return
    }
 
    Write-Host -NoNewline "Press any key to continue... "
 
    $ignore =
        16,  # Shift (left or right)
        17,  # Ctrl (left or right)
        18,  # Alt (left or right)
        20,  # Caps lock
        91,  # Windows key (left)
        92,  # Windows key (right)
        93,  # Menu key
        144, # Num lock
        145, # Scroll lock
        166, # Back
        167, # Forward
        168, # Refresh
        169, # Stop
        170, # Search
        171, # Favorites
        172, # Start/Home
        173, # Mute
        174, # Volume Down
        175, # Volume Up
        176, # Next Track
        177, # Previous Track
        178, # Stop Media
        179, # Play
        180, # Mail
        181, # Select Media
        182, # Application 1
        183  # Application 2
 
    # Wait for user key input. Ignore keys of the ignore list.
    While ($keyInfo.VirtualKeyCode -Eq $null -Or $ignore -Contains $keyInfo.VirtualKeyCode) {
        $keyInfo = $Host.UI.RawUI.ReadKey("NoEcho, IncludeKeyDown")
    }
 
    Write-Host
}


# Writes a message in red.
function DigitalRune:Write-Error([System.String] $message)
{
    Write-Host -ForegroundColor Red $message    
}


