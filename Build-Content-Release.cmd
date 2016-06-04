@ECHO OFF
SETLOCAL

ECHO ----- Building DigitalRune.Graphics.Content...

:: Change working directory.
cd Source\DigitalRune.Graphics.Content

:: Build content with MonoGame Content Builder tool.
..\MonoGame\Tools\MGCB\bin\Windows\AnyCPU\Release\MGCB.exe /@:DigitalRune.Graphics.mgcb || GOTO error

:: ZIP content.
..\..\Tools\Pack\bin\x64\Release\Pack.exe --output bin\DigitalRune.zip --recursive --directory bin\Windows *.* || GOTO error

cd ..\..

ECHO.
ECHO ----- Building DigitalRune.Editor.Game.Content...

:: Change working directory.
cd Source\DigitalRune.Editor.Game.Content

:: Build content with MonoGame Content Builder tool.
..\MonoGame\Tools\MGCB\bin\Windows\AnyCPU\Release\MGCB.exe /@:DigitalRune.Editor.Game-Release.mgcb || GOTO error

:: ZIP content.
..\..\Tools\Pack\bin\x64\Release\Pack.exe --output bin\DigitalRune.Editor.Game.zip --recursive --directory bin *.* || GOTO error

cd ..\..


ECHO.
ECHO SUCCESS - Content build successful.
PAUSE
EXIT

:error
ECHO.
ECHO ERROR - Content build failed.
PAUSE
EXIT /B 1
