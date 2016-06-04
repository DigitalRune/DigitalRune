// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace DigitalRune.Build
{
    /// <summary>
    /// Sorts the XML elements in C# project (.csproj) files .
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Proj")]
    public static class CSProjFormatter
    {
        private class ElementComparer : IComparer<XElement>
        {
            private static List<string> StickyItems = new List<string>
            {
                "BootstrapperPackage",
            };

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
            public int Compare(XElement x, XElement y)
            {
                if (x == null)
                    throw new ArgumentNullException(nameof(x));
                if (y == null)
                    throw new ArgumentNullException(nameof(y));

                // Keep "sticky" nodes on top and do not mix them with other node types.
                var xLocalName = x.Name.LocalName;
                var yLocalName = y.Name.LocalName;
                var xStickyIndex = StickyItems.IndexOf(xLocalName);
                var yStickyIndex = StickyItems.IndexOf(yLocalName);

                if (xStickyIndex >= 0 || yStickyIndex >= 0)
                {
                    if (xStickyIndex < 0)
                        xStickyIndex = int.MaxValue;
                    if (yStickyIndex < 0)
                        yStickyIndex = int.MaxValue;

                    return xStickyIndex - yStickyIndex;
                }


                // First sort by Include path.
                // For symbolic links, the Include path can be convoluted (e.g. "..\..\..\bla").
                // They have a Link child element with a better name.
                var xLinkElement = x.Element(x.Name.Namespace + "Link"); 
                var xIncludeAttribute = xLinkElement?.Value ?? ((string)x.Attribute("Include") ?? string.Empty);

                var yLinkElement = y.Element(y.Name.Namespace + "Link");
                var yIncludeAttribute = yLinkElement?.Value ?? ((string)y.Attribute("Include") ?? string.Empty);

                var result = string.Compare(xIncludeAttribute, yIncludeAttribute, StringComparison.Ordinal);
                if (result != 0)
                    return result;

                // Then sort by node name.
                return string.Compare(xLocalName, yLocalName, StringComparison.Ordinal);
            }
        }


#if DEBUG
        public static void Test()
        {
            //ProcessFile(@"X:\DigitalRune\Samples\Samples\Platforms\MonoGame-WindowsPhone\Samples-MonoGame-WindowsPhone.csproj");
            ProcessFolder(@"X:\DigitalRune\Samples\Samples", true);
        }
#endif


        /// <summary>
        /// Processes all C# project (.csproj) files in a directory.
        /// </summary>
        /// <param name="directory">
        /// The directory. Can be <see langword="null"/> or an empty string, in which case the 
        /// current directory is used.
        /// </param>
        /// <param name="recursive">
        /// If set to <see langword="true"/>, the .csproj files in all subdirectories are processed
        /// too. If set to <see langword="false"/>, only the .csproj files in the top directory are
        /// processed.
        /// </param>
        public static void ProcessFolder(string directory, bool recursive)
        {
            if (!string.IsNullOrEmpty(directory))
            {
                if (!Path.IsPathRooted(directory))
                    directory = Path.Combine(Environment.CurrentDirectory, directory);
            }
            else
            {
                directory = Environment.CurrentDirectory;
            }

            var filenames = Directory.GetFiles(
                directory,
                "*.csproj",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var filename in filenames)
                ProcessFile(filename);
        }


        /// <summary>
        /// Processes a C# project (.csproj) file.
        /// </summary>
        /// <param name="fileName">
        /// The file path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fileName"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void ProcessFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var xDocument = XDocument.Load(fileName);
            if (xDocument.Root == null)
                return;

            // XML namespace.
            var ns = xDocument.Root.Name.Namespace;

            // XML node names.
            var itemGroupName = ns + "ItemGroup";
            var referenceName = ns + "Reference";
            var projectReferenceName = ns + "ProjectReference";

            var comparer = new ElementComparer();

            // Remove all ItemGroups.
            var itemGroups = xDocument.Root.Elements(itemGroupName).ToArray();
            foreach (var itemGroup in itemGroups)
                itemGroup.Remove();

            // ItemGroups can have a Condition. Such ItemGroups must not be merged with other
            // ItemGroups, but we sort the child Items.
            var itemGroupsWithAttributes = itemGroups.Where(e => e.HasAttributes).ToArray();
            foreach (var itemGroup in itemGroupsWithAttributes)
            {
                var sortedItems = itemGroup.Elements().OrderBy(e => e, comparer).ToArray();
                // ReSharper disable once CoVariantArrayConversion
                itemGroup.ReplaceNodes(sortedItems);
                xDocument.Root.Add(itemGroup);
            }
            
            // Get all items. Delete all comments!
            itemGroups = itemGroups.ToArray();
            var items = itemGroups.Where(e => !e.HasAttributes)
                                  .Elements()
                                  .Where(e => e.NodeType != XmlNodeType.Comment)
                                  .ToArray();

            // Create one ItemGroup with sorted Reference elements.
            var referenceItemGroup = new XElement(
                itemGroupName,
                items.Where(e => e.Name == referenceName)
                     .OrderBy(e => e, comparer));

            if (referenceItemGroup.HasElements)
                xDocument.Root.Add(referenceItemGroup);

            // Create one ItemGroup with sorted ProjectReference elements.
            var projectReferenceItemGroup = new XElement(
                itemGroupName,
                items.Where(e => e.Name == projectReferenceName)
                     .OrderBy(e => e, comparer));

            if (projectReferenceItemGroup.HasElements)
                xDocument.Root.Add(projectReferenceItemGroup);

            // Add all other items to another group.
            var otherItemGroup = new XElement(
                itemGroupName,
                items.Where(e => e.Name != referenceName && e.Name != projectReferenceName)
                     .OrderBy(e => e, comparer));

            if (otherItemGroup.HasElements)
                xDocument.Root.Add(otherItemGroup);

            // Save new file but only if the content has really changed.
            var oldContent = File.ReadAllBytes(fileName);
            byte[] newContent;
            using (var memoryStream = new MemoryStream())
            using (var textWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                xDocument.Save(textWriter);
                newContent = memoryStream.ToArray();
            }

            if (oldContent.SequenceEqual(newContent))
                return;

            File.WriteAllBytes(fileName, newContent);
        }
    }
}
