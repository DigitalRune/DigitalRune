// ZipFile.Read.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa.
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
using System.Collections.Generic;

namespace DigitalRune.Ionic.Zip
{
    partial class ZipFile
    {
        /// <summary>
        ///   Reads a zip archive from a stream.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   Using this overload, the stream is read using the default
        ///   <c>System.Text.Encoding</c>, which is the <c>IBM437</c>
        ///   codepage. If you want to specify the encoding to use when
        ///   reading the zipfile content, see
        ///   <see cref="ZipFile.Read(Stream,
        ///   System.Text.Encoding)">ZipFile.Read(Stream, Encoding)</see>.
        /// </para>
        ///
        /// <para>
        ///   Reading of zip content begins at the current position in the
        ///   stream.  This means if you have a stream that concatenates
        ///   regular data and zip data, if you position the open, readable
        ///   stream at the start of the zip data, you will be able to read
        ///   the zip archive using this constructor, or any of the ZipFile
        ///   constructors that accept a <see cref="System.IO.Stream" /> as
        ///   input. Some examples of where this might be useful: the zip
        ///   content is concatenated at the end of a regular EXE file, as
        ///   some self-extracting archives do.  (Note: SFX files produced
        ///   by DotNetZip do not work this way; they can be read as normal
        ///   ZIP files). Another example might be a stream being read from
        ///   a database, where the zip content is embedded within an
        ///   aggregate stream of data.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        /// <para>
        ///   This example shows how to Read zip content from a stream, and
        ///   extract one entry into a different stream. In this example,
        ///   the filename "NameOfEntryInArchive.doc", refers only to the
        ///   name of the entry within the zip archive.  A file by that
        ///   name is not created in the filesystem.  The I/O is done
        ///   strictly with the given streams.
        /// </para>
        ///
        /// <code>
        /// using (ZipFile zip = ZipFile.Read(InputStream))
        /// {
        ///    zip.Extract("NameOfEntryInArchive.doc", OutputStream);
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using zip as ZipFile = ZipFile.Read(InputStream)
        ///    zip.Extract("NameOfEntryInArchive.doc", OutputStream)
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <param name="zipStream">the stream containing the zip data.</param>
        ///
        /// <returns>The ZipFile instance read from the stream</returns>
        ///
        public static ZipFile Read(Stream zipStream)
        {
            return Read(zipStream, null);
        }


        /// <summary>
        /// Reads a zip archive from a stream, using the specified text Encoding.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// Reading of zip content begins at the current position in the stream.  This
        /// means if you have a stream that concatenates regular data and zip data, if
        /// you position the open, readable stream at the start of the zip data, you
        /// will be able to read the zip archive using this constructor, or any of the
        /// ZipFile constructors that accept a <see cref="System.IO.Stream" /> as
        /// input. Some examples of where this might be useful: the zip content is
        /// concatenated at the end of a regular EXE file, as some self-extracting
        /// archives do.  (Note: SFX files produced by DotNetZip do not work this
        /// way). Another example might be a stream being read from a database, where
        /// the zip content is embedded within an aggregate stream of data.
        /// </para>
        /// </remarks>
        ///
        /// <param name="zipStream">the stream containing the zip data.</param>
        ///
        /// <param name="encoding">
        /// The text encoding to use when reading entries that do not have the UTF-8
        /// encoding bit set.  Be careful specifying the encoding.  If the value you use
        /// here is not the same as the Encoding used when the zip archive was created
        /// (possibly by a different archiver) you will get unexpected results and
        /// possibly exceptions.  See the <see cref="AlternateEncoding"/>
        /// property for more information.
        /// </param>
        ///
        /// <returns>an instance of ZipFile</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ZipFile Read(Stream zipStream, System.Text.Encoding encoding)
        {
            if (zipStream == null)
                throw new ArgumentNullException("zipStream");

            ZipFile zf = new ZipFile();
            zf._alternateEncoding = encoding ?? ZipFile.DefaultEncoding;
            zf._alternateEncodingUsage = ZipOption.Always;
            zf._readstream = (zipStream.Position == 0L)
                ? zipStream
                : new OffsetStream(zipStream);

            ReadIntoInstance(zf);
            return zf;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ZipFile")]
        private static void ReadIntoInstance(ZipFile zf)
        {
            Stream s = zf.ReadStream;
            try
            {
                if (!s.CanSeek)
                {
                    ReadIntoInstance_Orig(zf);
                    return;
                }

                // change for workitem 8098
                //zf._originPosition = s.Position;

                // Try reading the central directory, rather than scanning the file.

                uint datum = ReadFirstFourBytes(s);

                if (datum == ZipConstants.EndOfCentralDirectorySignature)
                    return;


                // start at the end of the file...
                // seek backwards a bit, then look for the EoCD signature.
                int nTries = 0;
                bool success = false;

                // The size of the end-of-central-directory-footer plus 2 bytes is 18.
                // This implies an archive comment length of 0.  We'll add a margin of
                // safety and start "in front" of that, when looking for the
                // EndOfCentralDirectorySignature
                long posn = s.Length - 64;
                long maxSeekback = Math.Max(s.Length - 0x4000, 10);
                do
                {
                    if (posn < 0) posn = 0;  // BOF
                    s.Seek(posn, SeekOrigin.Begin);
                    long bytesRead = SharedUtilities.FindSignature(s, (int)ZipConstants.EndOfCentralDirectorySignature);
                    if (bytesRead != -1)
                        success = true;
                    else
                    {
                        if (posn==0) break; // started at the BOF and found nothing
                        nTries++;
                        // Weird: with NETCF, negative offsets from SeekOrigin.End DO
                        // NOT WORK. So rather than seek a negative offset, we seek
                        // from SeekOrigin.Begin using a smaller number.
                        posn -= (32 * (nTries + 1) * nTries);
                    }
                }
                while (!success && posn > maxSeekback);

                if (success)
                {
                    // workitem 8299
                    zf._locEndOfCDS = s.Position - 4;

                    byte[] block = new byte[16];
                    s.Read(block, 0, block.Length);

                    zf._diskNumberWithCd = BitConverter.ToUInt16(block, 2);

                    if (zf._diskNumberWithCd == 0xFFFF)
                        throw new ZipException("Spanned archives with more than 65534 segments are not supported at this time.");

                    zf._diskNumberWithCd++; // I think the number in the file differs from reality by 1

                    int i = 12;

                    uint offset32 = (uint) BitConverter.ToUInt32(block, i);
                    if (offset32 == 0xFFFFFFFF)
                    {
                        Zip64SeekToCentralDirectory(zf);
                    }
                    else
                    {
                        // change for workitem 8098
                        s.Seek(offset32, SeekOrigin.Begin);
                    }

                    ReadCentralDirectory(zf);
                }
                else
                {
                    // Could not find the central directory.
                    // Fallback to the old method.
                    // workitem 8098: ok
                    //s.Seek(zf._originPosition, SeekOrigin.Begin);
                    s.Seek(0L, SeekOrigin.Begin);
                    ReadIntoInstance_Orig(zf);
                }
            }
            catch (Exception ex1)
            {
                throw new ZipException("Cannot read that as a ZipFile", ex1);
            }
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EoCD")]
        private static void Zip64SeekToCentralDirectory(ZipFile zf)
        {
            Stream s = zf.ReadStream;
            byte[] block = new byte[16];

            // seek back to find the ZIP64 EoCD.
            // I think this might not work for .NET CF ?
            s.Seek(-40, SeekOrigin.Current);
            s.Read(block, 0, 16);

            Int64 offset64 = BitConverter.ToInt64(block, 8);
            // change for workitem 8098
            s.Seek(offset64, SeekOrigin.Begin);
            //zf.SeekFromOrigin(Offset64);

            uint datum = (uint)Ionic.Zip.SharedUtilities.ReadInt(s);
            if (datum != ZipConstants.Zip64EndOfCentralDirectoryRecordSignature)
              throw new BadReadException(String.Format(CultureInfo.InvariantCulture, "  Bad signature (0x{0:X8}) looking for ZIP64 EoCD Record at position 0x{1:X8}", datum, s.Position));

            s.Read(block, 0, 8);
            Int64 Size = BitConverter.ToInt64(block, 0);

            block = new byte[Size];
            s.Read(block, 0, block.Length);

            offset64 = BitConverter.ToInt64(block, 36);
            // change for workitem 8098
            s.Seek(offset64, SeekOrigin.Begin);
            //zf.SeekFromOrigin(Offset64);
        }


        private static uint ReadFirstFourBytes(Stream s)
        {
            uint datum = (uint)Ionic.Zip.SharedUtilities.ReadInt(s);
            return datum;
        }



        private static void ReadCentralDirectory(ZipFile zf)
        {
            // We must have the central directory footer record, in order to properly
            // read zip dir entries from the central directory.  This because the logic
            // knows when to open a spanned file when the volume number for the central
            // directory differs from the volume number for the zip entry.  The
            // _diskNumberWithCd was set when originally finding the offset for the
            // start of the Central Directory.

            ZipEntry de;
            // in lieu of hashset, use a dictionary
            var previouslySeen = new Dictionary<String,object>();
            while ((de = ZipEntry.ReadDirEntry(zf, previouslySeen)) != null)
            {
                de.ResetDirEntry();

                zf._entries.Add(de.FileName,de);

                previouslySeen.Add(de.FileName, null); // to prevent dupes
            }

            // workitem 8299
            if (zf._locEndOfCDS > 0)
                zf.ReadStream.Seek(zf._locEndOfCDS, SeekOrigin.Begin);

            ReadCentralDirectoryFooter(zf);

            // We keep the read stream open after reading.
        }




        // build the TOC by reading each entry in the file.
        private static void ReadIntoInstance_Orig(ZipFile zf)
        {
            //zf._entries = new System.Collections.Generic.List<ZipEntry>();
            zf._entries = new System.Collections.Generic.Dictionary<String,ZipEntry>();

            ZipEntry e;

            // work item 6647:  PK00 (packed to removable disk)
            bool firstEntry = true;
            ZipContainer zc = new ZipContainer(zf);
            while ((e = ZipEntry.ReadEntry(zc, firstEntry)) != null)
            {
                zf._entries.Add(e.FileName,e);
                firstEntry = false;
            }

            // read the zipfile's central directory structure here.
            // workitem 9912
            // But, because it may be corrupted, ignore errors.
            try
            {
                ZipEntry de;
                // in lieu of hashset, use a dictionary
                var previouslySeen = new Dictionary<String,Object>();
                while ((de = ZipEntry.ReadDirEntry(zf, previouslySeen)) != null)
                {
                    // Housekeeping: Since ZipFile exposes ZipEntry elements in the enumerator,
                    // we need to copy the comment that we grab from the ZipDirEntry
                    // into the ZipEntry, so the application can access the comment.
                    // Also since ZipEntry is used to Write zip files, we need to copy the
                    // file attributes to the ZipEntry as appropriate.
                    ZipEntry e1 = zf._entries[de.FileName];
                    if (e1 != null)
                    {
                        e1._Comment = de.Comment;
                        if (de.IsDirectory) e1.MarkAsDirectory();
                    }
                    previouslySeen.Add(de.FileName,null); // to prevent dupes
                }

                // workitem 8299
                if (zf._locEndOfCDS > 0)
                    zf.ReadStream.Seek(zf._locEndOfCDS, SeekOrigin.Begin);

                ReadCentralDirectoryFooter(zf);
            }
            catch (ZipException) { }
            catch (IOException) { }
        }




        private static void ReadCentralDirectoryFooter(ZipFile zf)
        {
            Stream s = zf.ReadStream;
            int signature = Ionic.Zip.SharedUtilities.ReadSignature(s);

            byte[] block = null;
            int j = 0;
            if (signature == ZipConstants.Zip64EndOfCentralDirectoryRecordSignature)
            {
                // We have a ZIP64 EOCD
                // This data block is 4 bytes sig, 8 bytes size, 44 bytes fixed data,
                // followed by a variable-sized extension block.  We have read the sig already.
                // 8 - datasize (64 bits)
                // 2 - version made by
                // 2 - version needed to extract
                // 4 - number of this disk
                // 4 - number of the disk with the start of the CD
                // 8 - total number of entries in the CD on this disk
                // 8 - total number of entries in the CD
                // 8 - size of the CD
                // 8 - offset of the CD
                // -----------------------
                // 52 bytes

                block = new byte[8 + 44];
                s.Read(block, 0, block.Length);

                Int64 DataSize = BitConverter.ToInt64(block, 0);  // == 44 + the variable length

                if (DataSize < 44)
                    throw new ZipException("Bad size in the ZIP64 Central Directory.");

                //zf._versionMadeBy = BitConverter.ToUInt16(block, j);
                j += 2;
                //zf._versionNeededToExtract = BitConverter.ToUInt16(block, j);
                j += 2;
                zf._diskNumberWithCd = BitConverter.ToUInt32(block, j);
                j += 2;

                //zf._diskNumberWithCd++; // hack!!

                // read the extended block
                block = new byte[DataSize - 44];
                s.Read(block, 0, block.Length);
                // discard the result

                signature = Ionic.Zip.SharedUtilities.ReadSignature(s);
                if (signature != ZipConstants.Zip64EndOfCentralDirectoryLocatorSignature)
                    throw new ZipException("Inconsistent metadata in the ZIP64 Central Directory.");

                block = new byte[16];
                s.Read(block, 0, block.Length);
                // discard the result

                signature = Ionic.Zip.SharedUtilities.ReadSignature(s);
            }

            // Throw if this is not a signature for "end of central directory record"
            // This is a sanity check.
            if (signature != ZipConstants.EndOfCentralDirectorySignature)
            {
                s.Seek(-4, SeekOrigin.Current);
                throw new BadReadException(String.Format(CultureInfo.InvariantCulture, 
                                                         "Bad signature ({0:X8}) at position 0x{1:X8}",
                                                         signature, s.Position));
            }

            // read the End-of-Central-Directory-Record
            block = new byte[16];
            zf.ReadStream.Read(block, 0, block.Length);

            // off sz  data
            // -------------------------------------------------------
            //  0   4  end of central dir signature (0x06054b50)
            //  4   2  number of this disk
            //  6   2  number of the disk with start of the central directory
            //  8   2  total number of entries in the  central directory on this disk
            // 10   2  total number of entries in  the central directory
            // 12   4  size of the central directory
            // 16   4  offset of start of central directory with respect to the starting disk number
            // 20   2  ZIP file comment length
            // 22  ??  ZIP file comment

            if (zf._diskNumberWithCd == 0)
            {
                zf._diskNumberWithCd = BitConverter.ToUInt16(block, 2);
                //zf._diskNumberWithCd++; // hack!!
            }

            // read the comment here
            ReadZipFileComment(zf);
        }



        private static void ReadZipFileComment(ZipFile zf)
        {
            // read the comment here
            byte[] block = new byte[2];
            zf.ReadStream.Read(block, 0, block.Length);

            Int16 commentLength = (short)(block[0] + block[1] * 256);
            if (commentLength > 0)
            {
                block = new byte[commentLength];
                zf.ReadStream.Read(block, 0, block.Length);

                // workitem 10392 - prefer ProvisionalAlternateEncoding,
                // first.  The fix for workitem 6513 tried to use UTF8
                // only as necessary, but that is impossible to test
                // for, in this direction. There's no way to know what
                // characters the already-encoded bytes refer
                // to. Therefore, must do what the user tells us.

                string s1 = zf.AlternateEncoding.GetString(block, 0, block.Length);
                zf.Comment = s1;
            }
        }


        /// <summary>
        /// Checks a stream to see if it contains a valid zip archive.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// This method reads the zip archive contained in the specified stream, verifying
        /// the ZIP metadata as it reads.  If testExtract is true, this method also extracts
        /// each entry in the archive, dumping all the bits into <see cref="Stream.Null"/>.
        /// </para>
        ///
        /// <para>
        /// If everything succeeds, then the method returns true.  If anything fails -
        /// for example if an incorrect signature or CRC is found, indicating a corrupt
        /// file, the the method returns false.  This method also returns false for a
        /// file that does not exist.
        /// </para>
        ///
        /// <para>
        /// If <c>testExtract</c> is true, this method reads in the content for each
        /// entry, expands it, and checks CRCs.  This provides an additional check
        /// beyond verifying the zip header data.
        /// </para>
        ///
        /// <para>
        /// If <c>testExtract</c> is true, and if any of the zip entries are protected
        /// with a password, this method will return false.  If you want to verify a
        /// ZipFile that has entries which are protected with a password, you will need
        /// to do that manually.
        /// </para>
        /// </remarks>
        ///
        /// <param name="stream">The stream to check.</param>
        /// <param name="testExtract">true if the caller wants to extract each entry.</param>
        /// <returns>true if the stream contains a valid zip archive.</returns>
        public static bool IsZipFile(Stream stream, bool testExtract)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            bool result = false;
            try
            {
                if (!stream.CanRead) return false;

                var bitBucket = Stream.Null;

                using (ZipFile zip1 = ZipFile.Read(stream, null))
                {
                    if (testExtract)
                    {
                        foreach (var e in zip1)
                        {
                            if (!e.IsDirectory)
                            {
                                e.Extract(bitBucket);
                            }
                        }
                    }
                }
                result = true;
            }
            catch (IOException) { }
            catch (ZipException) { }
            return result;
        }
    }
}
