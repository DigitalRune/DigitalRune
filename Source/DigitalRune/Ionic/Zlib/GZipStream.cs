// GZipStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.
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


namespace DigitalRune.Ionic.Zlib
{
    /// <summary>
    /// Not implemented.
    /// </summary>
    internal class GZipStream
    {
        internal static readonly System.DateTime _unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
#if !WINDOWS
        internal static readonly System.Text.Encoding iso8859dash1 = new Ionic.Encoding.Iso8859Dash1Encoding();
#else
        internal static readonly System.Text.Encoding iso8859dash1 = System.Text.Encoding.GetEncoding("iso-8859-1");
#endif
    }
}
