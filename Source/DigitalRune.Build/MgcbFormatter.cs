// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace DigitalRune.Build
{
    /// <summary>
    /// Sorts the assets in a MonoGame content project (.mgcb) files.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mgcb")]
    public static class MgcbFormatter
    {
        private class AssetComparer : IComparer<List<string>>
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
            public int Compare(List<string> x, List<string> y)
            {
                return string.Compare(x[0], y[0], StringComparison.OrdinalIgnoreCase);
            }
        }


#if DEBUG
        public static void Test()
        {
            ProcessFolder(@"X:\DigitalRune\Samples\Content", true);
        }
#endif


        /// <summary>
        /// Processes all MonoGame content project (.mgcb) in a directory.
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
                "*.mgcb",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var filename in filenames)
                ProcessFile(filename);
        }


        /// <summary>
        /// Processes a MonoGame content project (.mgcb) file.
        /// </summary>
        /// <param name="fileName">
        /// The file path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fileName"/> is <see langword="null"/>.
        /// </exception>
        public static void ProcessFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var oldLines = File.ReadAllLines(fileName);
            if (oldLines.Length == 0)
                return;

            // Copy the first lines until we find the first asset.
            var newLines = new List<string>();
            int index = 0;
            for (; index < oldLines.Length; index++)
            {
                var line = oldLines[index];
                if (IsAssetStart(line))
                    break;

                newLines.Add(line);
            }

            // Create one entry for each asset.
            var assets = new List<List<string>>();

            {
                List<string> asset = null;
                for (; index < oldLines.Length; index++)
                {
                    var line = oldLines[index];
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (IsAssetStart(line))
                    {
                        asset = new List<string>();
                        assets.Add(asset);
                    }

                    asset.Add(line);
                }
            }

            // Sort assets by the first line.
            assets.Sort(new AssetComparer());

            // Add asset lines.
            var addEmptyLine = false;
            foreach (var asset in assets)
            {
                if (addEmptyLine)
                    newLines.Add(string.Empty);

                foreach (var line in asset)
                    newLines.Add(line);

                addEmptyLine = true;
            }
            
            // Abort if file content has not changed.
            if (oldLines.SequenceEqual(newLines, StringComparer.Ordinal))
                return;

            File.WriteAllLines(fileName, newLines, Encoding.UTF8);
        }


        private static bool IsAssetStart(string line)
        {
            return line.StartsWith("#begin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
