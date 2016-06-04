// Shared.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2006-2011 Dino Chiesa.
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

using System;
using System.Globalization;
using System.IO;

namespace DigitalRune.Ionic.Zip
{
    /// <summary>
    /// Collects general purpose utility methods.
    /// </summary>
    internal static class SharedUtilities
    {
        // [DIGITALRUNE] Regular expression removed because it does not work.
        //private static System.Text.RegularExpressions.Regex doubleDotRegex1 =
        //    new System.Text.RegularExpressions.Regex(@"^(.*/)?([^/\\.]+/\\.\\./)(.+)$");

        private static string SimplifyFwdSlashPath(string path)
        {
            if (path.StartsWith("./", StringComparison.Ordinal)) path = path.Substring(2);
            path = path.Replace("/./", "/");

            // Replace foo/anything/../bar with foo/bar
            //path = doubleDotRegex1.Replace(path, "$1$3"); // [DIGITALRUNE] Does not work!
            return path;
        }


        /// <summary>
        /// Utility routine for transforming path names from filesystem format (on Windows that means backslashes) to
        /// a format suitable for use within zipfiles. This means trimming the volume letter and colon (if any) And
        /// swapping backslashes for forward slashes.
        /// </summary>
        /// <param name="pathName">source path.</param>
        /// <returns>transformed path</returns>
        public static string NormalizePathForUseInZipFile(string pathName)
        {
            // boundary case
            if (String.IsNullOrEmpty(pathName)) return pathName;

            // trim volume if necessary
            if ((pathName.Length >= 2)  && ((pathName[1] == ':') && (pathName[2] == '\\')))
                pathName =  pathName.Substring(3);

            // swap slashes
            pathName = pathName.Replace('\\', '/');

            // trim all leading slashes
            while (pathName.StartsWith("/", StringComparison.Ordinal)) pathName = pathName.Substring(1);

            return SimplifyFwdSlashPath(pathName);
        }

#if !WINDOWS
        // See https://dotnetzip.codeplex.com/workitem/14049
        static System.Text.Encoding ibm437 = System.Text.Encoding.GetEncoding("UTF-8");
#else
        static System.Text.Encoding ibm437 = System.Text.Encoding.GetEncoding("IBM437");
#endif

        static System.Text.Encoding utf8 = System.Text.Encoding.GetEncoding("UTF-8");

        internal static byte[] StringToByteArray(string value, System.Text.Encoding encoding)
        {
            byte[] a = encoding.GetBytes(value);
            return a;
        }
        internal static byte[] StringToByteArray(string value)
        {
            return StringToByteArray(value, ibm437);
        }

        //internal static byte[] Utf8StringToByteArray(string value)
        //{
        //    return StringToByteArray(value, utf8);
        //}

        //internal static string StringFromBuffer(byte[] buf, int maxlength)
        //{
        //    return StringFromBuffer(buf, maxlength, ibm437);
        //}

        internal static string Utf8StringFromBuffer(byte[] buf)
        {
            return StringFromBuffer(buf, utf8);
        }

        internal static string StringFromBuffer(byte[] buf, System.Text.Encoding encoding)
        {
            // this form of the GetString() method is required for .NET CF compatibility
            string s = encoding.GetString(buf, 0, buf.Length);
            return s;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
        internal static int ReadSignature(System.IO.Stream s)
        {
            int x = 0;
            try { x = _ReadFourBytes(s, "n/a"); }
            catch (BadReadException) { }
            return x;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
        internal static int ReadEntrySignature(System.IO.Stream s)
        {
            // handle the case of ill-formatted zip archives - includes a data descriptor
            // when none is expected.
            int x = 0;
            try
            {
                x = _ReadFourBytes(s, "n/a");
                if (x == ZipConstants.ZipEntryDataDescriptorSignature)
                {
                    // advance past data descriptor - 12 bytes if not zip64
                    s.Seek(12, SeekOrigin.Current);
                    x = _ReadFourBytes(s, "n/a");
                    if (x != ZipConstants.ZipEntrySignature)
                    {
                        // Maybe zip64 was in use for the prior entry.
                        // Therefore, skip another 8 bytes.
                        s.Seek(8, SeekOrigin.Current);
                        x = _ReadFourBytes(s, "n/a");
                        if (x != ZipConstants.ZipEntrySignature)
                        {
                            // seek back to the first spot
                            s.Seek(-24, SeekOrigin.Current);
                            x = _ReadFourBytes(s, "n/a");
                        }
                    }
                }
            }
            catch (BadReadException) { }
            return x;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
        internal static int ReadInt(System.IO.Stream s)
        {
            return _ReadFourBytes(s, "Could not read block - no data!  (position 0x{0:X8})");
        }

        private static int _ReadFourBytes(System.IO.Stream s, string message)
        {
            int n = 0;
            byte[] block = new byte[4];
            n = s.Read(block, 0, block.Length);
            if (n != block.Length) throw new BadReadException(String.Format(CultureInfo.InvariantCulture, message, s.Position));
            int data = unchecked((((block[3] * 256 + block[2]) * 256) + block[1]) * 256 + block[0]);
            return data;
        }



        /// <summary>
        ///   Finds a signature in the zip stream. This is useful for finding
        ///   the end of a zip entry, for example, or the beginning of the next ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     Scans through 64k at a time.
        ///   </para>
        ///
        ///   <para>
        ///     If the method fails to find the requested signature, the stream Position
        ///     after completion of this method is unchanged. If the method succeeds in
        ///     finding the requested signature, the stream position after completion is
        ///     direct AFTER the signature found in the stream.
        ///   </para>
        /// </remarks>
        ///
        /// <param name="stream">The stream to search</param>
        /// <param name="SignatureToFind">The 4-byte signature to find</param>
        /// <returns>The number of bytes read</returns>
        internal static long FindSignature(System.IO.Stream stream, int SignatureToFind)
        {
            long startingPosition = stream.Position;

            int BATCH_SIZE = 65536; //  8192;
            byte[] targetBytes = new byte[4];
            targetBytes[0] = (byte)(SignatureToFind >> 24);
            targetBytes[1] = (byte)((SignatureToFind & 0x00FF0000) >> 16);
            targetBytes[2] = (byte)((SignatureToFind & 0x0000FF00) >> 8);
            targetBytes[3] = (byte)(SignatureToFind & 0x000000FF);
            byte[] batch = new byte[BATCH_SIZE];
            int n = 0;
            bool success = false;
            do
            {
                n = stream.Read(batch, 0, batch.Length);
                if (n != 0)
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (batch[i] == targetBytes[3])
                        {
                            long curPosition = stream.Position;
                            stream.Seek(i - n, System.IO.SeekOrigin.Current);

                            // workitem 7711
                            int sig = ReadSignature(stream);

                            success = (sig == SignatureToFind);
                            if (!success)
                            {
                                stream.Seek(curPosition, System.IO.SeekOrigin.Begin);
                            }
                            else
                                break; // out of for loop
                        }
                    }
                }
                else break;
                if (success) break;

            } while (true);

            if (!success)
            {
                stream.Seek(startingPosition, System.IO.SeekOrigin.Begin);
                return -1;  // or throw?
            }

            // subtract 4 for the signature.
            long bytesRead = (stream.Position - startingPosition) - 4;

            return bytesRead;
        }


        internal static DateTime PackedToDateTime(Int32 packedDateTime)
        {
            // workitem 7074 & workitem 7170
            if (packedDateTime == 0xFFFF || packedDateTime == 0)
                return new System.DateTime(1995, 1, 1, 0, 0, 0, 0);  // return a fixed date when none is supplied.

            Int16 packedTime = unchecked((Int16)(packedDateTime & 0x0000ffff));
            Int16 packedDate = unchecked((Int16)((packedDateTime & 0xffff0000) >> 16));

            int year = 1980 + ((packedDate & 0xFE00) >> 9);
            int month = (packedDate & 0x01E0) >> 5;
            int day = packedDate & 0x001F;

            int hour = (packedTime & 0xF800) >> 11;
            int minute = (packedTime & 0x07E0) >> 5;
            //int second = packedTime & 0x001F;
            int second = (packedTime & 0x001F) * 2;

            // validation and error checking.
            // this is not foolproof but will catch most errors.
            if (second >= 60) { minute++; second = 0; }
            if (minute >= 60) { hour++; minute = 0; }
            if (hour >= 24) { day++; hour = 0; }

            DateTime d = System.DateTime.Now;
            bool success= false;
            try
            {
                d = new System.DateTime(year, month, day, hour, minute, second, 0);
                success= true;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                if (year == 1980 && (month == 0 || day == 0))
                {
                    try
                    {
                        d = new System.DateTime(1980, 1, 1, hour, minute, second, 0);
                success= true;
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                        try
                        {
                            d = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
                success= true;
                        }
                        catch (System.ArgumentOutOfRangeException) { }

                    }
                }
                // workitem 8814
                // my god, I can't believe how many different ways applications
                // can mess up a simple date format.
                else
                {
                    try
                    {
                        while (year < 1980) year++;
                        while (year > 2030) year--;
                        while (month < 1) month++;
                        while (month > 12) month--;
                        while (day < 1) day++;
                        while (day > 28) day--;
                        while (minute < 0) minute++;
                        while (minute > 59) minute--;
                        while (second < 0) second++;
                        while (second > 59) second--;
                        d = new System.DateTime(year, month, day, hour, minute, second, 0);
                        success= true;
                    }
                    catch (System.ArgumentOutOfRangeException) { }
                }
            }
            if (!success)
            {
                string msg = String.Format(CultureInfo.InvariantCulture, "y({0}) m({1}) d({2}) h({3}) m({4}) s({5})", year, month, day, hour, minute, second);
                throw new ZipException(String.Format(CultureInfo.InvariantCulture, "Bad date/time format in the zip file. ({0})", msg));

            }
            // workitem 6191
            //d = AdjustTime_Reverse(d);
            d = DateTime.SpecifyKind(d, DateTimeKind.Local);
            return d;
        }

    }
}
