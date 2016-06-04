// IMPORTANT: Do not change AssemblyInfo.cs. The file is generated automatically. 
// Apply any changes to AssemblyInfo.template instead.

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;


// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DigitalRune.Windows.Docking")]
[assembly: AssemblyDescription("Docking windows library for the Windows Presentation Foundation (WPF) and Silverlight.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DigitalRune")]
[assembly: AssemblyProduct("DigitalRune")]
[assembly: AssemblyCopyright("Copyright © 2009-2014 DigitalRune GmbH. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

[assembly: NeutralResourcesLanguage("en-US")]


#if !SILVERLIGHT
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
  //(used if a resource is not found in the page, 
  // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
  //(used if a resource is not found in the page, 
  // app, or any theme specific resource dictionaries)
)]
#endif


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.3.0.0")]
[assembly: AssemblyFileVersion("1.3.0.12830")]
[assembly: CLSCompliant(true)]

// Define xmlns for use in XAML.
[assembly: XmlnsPrefix("http://schemas.digitalrune.com/windows", "dr")]
[assembly: XmlnsDefinition("http://schemas.digitalrune.com/windows", "DigitalRune.Windows.Docking")]

// Internals are visible to these assemblies:
// Using our real strong name certificate:
[assembly: InternalsVisibleTo("DigitalRune.Windows.Docking.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100615fc42c6443b01aa1cb1c87b1c452c35ed3019ab1cf15cb424d94cb427548c527ad08e079d71067f52795343aec489eeb4c2fd3b1b02aab848f4ef9501c99434943fb95156218061968f143245f8a7263551acca6ea9dde29065bb6528871cabf90e354d6d75dc52214b72cfb7afec5be2e320faf50278d1f23b953eebb9695")]

// Using the fake developer certificate:
[assembly: InternalsVisibleTo("DigitalRune.Windows.Docking.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100134c437761f8388de5869bf9e0c59190163bfd6b17582c33e19552b09457a2be711f523cbb8ef5ad661213cab882621c594b77d5733b693fa4078fe839e15250c50c90d442b3bd301030219848e489559ee5553757e2c24b900fe90736f0fdd4af2406c33414aee29be634f71d92326a4fa246e6cc167768df0bcd64fbe42cc0")]
