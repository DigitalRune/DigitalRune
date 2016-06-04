--------------------------------------------------------------------------------
Project InteropSample
--------------------------------------------------------------------------------
This sample application shows how to use the DigitalRune Graphics library to
display 3D graphics in Windows Forms and WPF windows.
(If you want to create a "pure" WPF application which displays 3D graphics,
please look at the project "WpfInteropSample2" instead.)

--------------------------------------------------------------------------------
BUILD INSTRUCTIONS
--------------------------------------------------------------------------------

XNA
---
The sample projects for XNA can be run like any other XNA game.
(No special build steps required.)

MonoGame
--------
The MonoGame content pipeline is not yet integrated into the Visual Studio. The
MonoGame content projects are built using a batch file. Please follow these
steps to build the MonoGame projects:

1. In Visual Studio open the solution "InteropSamples-MonoGame-Windows".
2. Build the project "Samples.Content.Pipeline-MonoGame".
   (This project is a content pipeline extension that is required to build the
   sample content.)
3. In Windows Explorer run
   "<DIGITALRUNE_FOLDER>\Samples\Build-Content-MonoGame-Windows.cmd".
   (This batch file calls the MonoGame Content Builder and builds the content
   project. The assets are packed into a ZIP archive.)
4. In Visual Studio build the solution.

When the MonoGame content project or assets are modified, run the batch fail
again to build the new assets.

--------------------------------------------------------------------------------
Description
--------------------------------------------------------------------------------
This sample demonstrates how to open Windows Forms or WPF windows from an 
existing XNA/MonoGame game. These windows could, for example, be used to implement
a game editor. Both windows contain presentation targets into which 3D graphics is
rendered.

Press <1> - <4> to toggle between: windowed mode, Windows Forms window, 
WPF window, and full screen mode.

The XNA and the MonoGame version are almost identical, except that the WPF windows
is using the ElementPresentationTarget in XNA and the D3DImagePresentationTarget
in MonoGame.

The Content folder in the MonoGame version contains links to packaged graphics
assets. Some of these assets are pre-built assets of DigitalRune Graphics. Other
assets are created by the content build script (see BUILD INSTRUCTIONS).
The "Build action" of these items is set to "Content". "Copy to output directory"
is set to "Copy if newer".
Regular assets (XNB files) can be loaded in code using the XNA/MonoGame
ContentManager. Packaged assets (ZIP files) can be loaded using the DigitalRune
StorageContentManager.
