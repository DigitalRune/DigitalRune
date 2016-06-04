--------------------------------------------------------------------------------
The Samples Project
--------------------------------------------------------------------------------
This project contains many examples that show how to use the different libraries
of the DigitalRune Engine.

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

1. In Visual Studio open the sample solution "Samples-MonoGame-<Platform>.sln".
2. Build the project "Samples.Content.Pipeline-MonoGame".
   (This project is a content pipeline extension that is required to build the
   sample content.)
3. In Windows Explorer run
   "<DIGITALRUNE_FOLDER>\Samples\Build-Content-MonoGame-<Platform>.cmd".
   (This batch file calls the MonoGame Content Builder and builds the content
   project. The assets are packed into a ZIP archive.)
4. Now you can build and run the sample project in Visual Studio.

When the MonoGame content project or assets are modified, run the batch fail
again to build the new assets.


--------------------------------------------------------------------------------
Projects and Platforms
--------------------------------------------------------------------------------
The folder
  <DIGITALRUNE_FOLDER>\Samples\Samples\
contains many samples which can run on several platforms.
The subfolder
  <DIGITALRUNE_FOLDER>\Samples\Samples\Platforms\
contains a subfolder for the different platforms, e.g.
  <DIGITALRUNE_FOLDER>\Samples\Samples\Platforms\XNA-Windows.
  <DIGITALRUNE_FOLDER>\Samples\Samples\Platforms\XNA-WindowsPhone,
  <DIGITALRUNE_FOLDER>\Samples\Samples\Platforms\XNA-Xbox,
  <DIGITALRUNE_FOLDER>\Samples\Samples\Platforms\MonoGame-Windows,
  etc.
These subfolders contain the different Visual Studio projects and all platform-
specific files (e.g. manifest files for Windows Phone).
 
Here is a list of the current platforms and platform-specific remarks:

- XNA-Windows
  Samples built for Windows (desktop) using XNA.
  IMPORTANT: New users should look at this project first!
  
- XNA-WindowsPhone
  Samples built for Windows Phone using XNA.
  
- XNA-Xbox
  Samples built for Xbox 360.
  
- MonoGame-Windows
  Samples built for Windows (desktop) using MonoGame.

- MonoGame-Windows-Kinect
  Samples built for Kinect for Windows.
  Requires Visual Studio 2012 or higher and the Microsoft Kinect SDK 2.0.
  
- MonoGame-WindowsPhone
  Samples built for Windows Phone 8 using MonoGame.
  
- MonoGame-WindowsStore
  Samples built for Windows Store using MonoGame.
  
- MonoGame-WindowsUniversal
  Samples built for Universal Windows Platform (UWP) using MonoGame.
  
- MonoGame-Linux
  Samples built for Linux using MonoGame.

- MonoGame-MacOS
  Samples built for Mac OS X using using Mono and MonoGame.
  A Mac is required, Xamarin.Mac is NOT required to run these samples.

- MonoGame-Android
  Samples built for Android using MonoGame.
  Xamarin.Android is required to run these samples!

- MonoGame-iOS
  Samples built for iOS (iPad, iPhone) using MonoGame.
  A Mac and Xamarin.iOS are required to run these samples!


--------------------------------------------------------------------------------
Controls
--------------------------------------------------------------------------------
The controls depend on the currently selected sample. The general controls are:
- Using keyboard and mouse:
    Press <Esc> to show the sample menu.
    Press <F1> to show help.
    Press <F4> to show options.
    Press <Page Up>/<Page Down> to switch samples.
    Press <Alt>-<F4> to exit the application.
- Using game pad:
    Pess <Back> to show the sample menu.
    Press <Start> to select a game pad.
    Hold <Left Stick> to show Help screen.
    Use <DPad Left>/<DPad Right> to switch samples.

The Help window (<F1>) shows a description of the current sample and list of
additional controls. Please note: Most samples were designed for the desktop and
are best used with mouse and keyboard.


--------------------------------------------------------------------------------
Content for XNA projects (.contentproj)
--------------------------------------------------------------------------------
Each Samples project references its own content projects and the content project
which contains the default DigitalRune Graphics content. The DigtalRune Graphics
content project provides pre-built resources (shaders and textures) required by
DigitalRune Graphics. It is located in the folder
  <DIGITALRUNE_FOLDER>\Content\XNA\Windows\  (for Windows games)
or
  <DIGITALRUNE_FOLDER>\Content\XNA\Xbox360\  (for Xbox games).


--------------------------------------------------------------------------------
Content for MonoGame projects (.mgcb)
--------------------------------------------------------------------------------
The MonoGame content projects (.mgcb files) can be edited using the MonoGame
Pipeline Tool. To open the MonoGame Pipeline Tool run
  "<DIGITALRUNE_FOLDER>\References\MonoGame\Windows\Pipeline.exe"
  or the shortcut "<DIGITALRUNE_FOLDER>\Tools\Pipeline.cmd".

Each MonoGame sample project contains a folder "Content". This folder contains
links to packaged graphics assets. Some of these assets are pre-built assets of
DigitalRune Graphics. Other assets are created by the content build script (see
BUILD INSTRUCTIONS).
The "Build action" of these items is set to "Content". "Copy to output directory"
is set to "Copy if newer".


--------------------------------------------------------------------------------
Special Remarks for Windows Store/Windows Phone/iOS/Android
--------------------------------------------------------------------------------
Most samples were designed and optimized for the desktop and do not have touch
controls!


--------------------------------------------------------------------------------
Source Code Overview
--------------------------------------------------------------------------------
Sample Framework
----------------
The types in the folder "Sample Framework" implement the infrastructure of the
application (menu, help, etc.). The class SampleFramework automatically 
discovers samples using reflection and provides means to switch between samples 
at runtime. It also creates the GUI (e.g. menu, help, options).

Samples
-------
Samples are derived from the XNA class GameComponent and need to be marked with
the SampleAttribute. Example:
  [Sample(...)]
  class MySample : GameComponent { ... }
The solution also provides several abstract base classes (Sample, BasicSample,
PhysicsSample, etc.) that samples can inherit. These base classes implement
common tasks such as retrieving the game services, setting up a basic graphics
screen for rendering, and more.
When a sample is disposed, it has to undo all changes made to game services
(e.g. remove all game objects from the game object service, remove all rigid
bodies from the physics simulation, dispose scene nodes, etc.) to avoid side
effects for other samples.
At any time only a single sample instance is active.

Game Objects
------------
Classes derived from DigitalRune.Game.GameObject implement game logic.
Examples: Camera logic, loading and updating model and rigid body, etc.
Game objects are created and added to the game object service by samples and
removed when the sample is disposed. The project contains several GameObjects
that are reused in different samples.
Game objects load resources (graphics, physics objects, particle systems and
animations) in GameObject.OnLoad() and release the resources in
GameObject.OnUnload(). Game logic is implemented in GameObject.OnUpdate() which
is executed in every frame.

Graphics Screens
----------------
The project contains several GraphicsScreen implementations that are reused in
different samples. DigitalRune GraphicsScreens are explained in the DigitalRune
Graphics documentation and samples.

(Multithreaded) Game Loop
-------------------------
The class SampleGame implements the game loop. The game loop executes the
different sub-systems of the game (input, physics, animation, graphics, etc.).
Optionally, the game loop can run services in parallel to better utilize
multi-core CPUs. Further, each subsystem (especially geometry, physics,
animation, particles) uses multi-threading internally.

For more information, please check out the detailed comments in the source code.


--------------------------------------------------------------------------------
Misc
--------------------------------------------------------------------------------
This project contains a lot of samples built for many platforms. The samples
were primarily designed to be run on the Windows desktop. Some samples might not
perform well on all platforms, e.g. mobile phones.

