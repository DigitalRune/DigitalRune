--------------------------------------------------------------------------------
Project Samples.Content.Pipeline
--------------------------------------------------------------------------------
This project provides several classes that extend the XNA/MonoGame content
pipeline. To use the content pipeline extension, add the assembly as a reference
to the content project.

XNA Content Projects
--------------------
- Open the Solution Explorer in Visual Studio.
- Right-click the content project in the Solution Explorer.
- Select "Add Reference...".
- In the Projects tab select "Samples.Content.Pipeline".
- Click "OK".

The content project also needs to reference the DigitalRune content pipeline DLLs:
- Right-click the content project in the Solution Explorer.
- Select "Add Reference...".
- In the Browse tab browse to "<DIGITALRUNE_FOLDER>\References\XNA\Windows".
- Select the content pipeline DLLs:
    DigitalRune.Mathematics.Content.Pipeline.dll
    DigitalRune.Geometry.Content.Pipeline.dll
    DigitalRune.Animation.Content.Pipeline.dll
    DigitalRune.Graphics.Content.Pipeline.dll
    DigitalRune.Game.UI.Content.Pipeline.dll
- Click "OK".

MonoGame Content Projects
-------------------------
- Open the MonoGame Pipeline Tools by starting
  "<DIGITALRUNE_FOLDER>\References\MonoGame\Windows\Pipeline.exe"
  (or the shortcut "<DIGITALRUNE_FOLDER>\Tools\Pipeline.cmd").
- Open the MonoGame content project (.mgcb file).
- In the project tree view select the project (root node).
- In the property grid below select "References" and click the "..." button.
- Add the following references in the popup window:

  ..\..\References\MonoGame\Windows\DigitalRune.Animation.Content.Pipeline.dll
  ..\..\References\MonoGame\Windows\DigitalRune.Game.UI.Content.Pipeline.dll
  ..\..\References\MonoGame\Windows\DigitalRune.Geometry.Content.Pipeline.dll
  ..\..\References\MonoGame\Windows\DigitalRune.Graphics.Content.Pipeline.dll
  ..\..\References\MonoGame\Windows\DigitalRune.Mathematics.Content.Pipeline.dll
  ..\Samples.Content.Pipeline\bin\MonoGame\Samples.Content.Pipeline.dll

(You may need to adjust the path if the content project or the content pipeline
extensions lie in a different folder.)
