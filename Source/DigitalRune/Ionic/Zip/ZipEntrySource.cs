// ZipEntrySource.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------


namespace DigitalRune.Ionic.Zip
{
    /// <summary>
    /// An enum that specifies the source of the ZipEntry. 
    /// </summary>
    internal enum ZipEntrySource
    {
        /// <summary>
        /// Default value.  Invalid on a bonafide ZipEntry.
        /// </summary>
        None = 0,

        /// <summary>
        /// The entry was instantiated from a stream.
        /// </summary>
        Stream,

        /// <summary>
        /// The ZipEntry was instantiated by reading a zipfile.
        /// </summary>
        ZipFile,
    }
}