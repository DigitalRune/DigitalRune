// ZipEntry.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2006-2010 Dino Chiesa.
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
    /// Represents a single entry in a ZipFile. Typically, applications get a ZipEntry
    /// by enumerating the entries within a ZipFile, or by adding an entry to a ZipFile.
    /// </summary>
    internal partial class ZipEntry
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Applications should never need to call this directly.  It is exposed to
        /// support COM Automation environments.
        /// </remarks>
        public ZipEntry()
        {
            _CompressionMethod = (Int16)CompressionMethod.Deflate;
            _Encryption = EncryptionAlgorithm.None;
            _Source = ZipEntrySource.None;
#if !WINDOWS
            // See https://dotnetzip.codeplex.com/workitem/14049
            AlternateEncoding = System.Text.Encoding.GetEncoding("UTF-8");
#else
            AlternateEncoding = System.Text.Encoding.GetEncoding("IBM437");
#endif
            AlternateEncodingUsage = ZipOption.Never;
        }

        /// <summary>
        ///   The time and date at which the file indicated by the <c>ZipEntry</c> was
        ///   last modified.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The DotNetZip library sets the LastModified value for an entry, equal to
        ///   the Last Modified time of the file in the filesystem.  If an entry is
        ///   added from a stream, the library uses <c>System.DateTime.Now</c> for this
        ///   value, for the given entry.
        /// </para>
        ///
        /// <para>
        ///   This property allows the application to retrieve and possibly set the
        ///   LastModified value on an entry, to an arbitrary value.  <see
        ///   cref="System.DateTime"/> values with a <see cref="System.DateTimeKind" />
        ///   setting of <c>DateTimeKind.Unspecified</c> are taken to be expressed as
        ///   <c>DateTimeKind.Local</c>.
        /// </para>
        ///
        /// <para>
        ///   Be aware that because of the way <see
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">PKWare's
        ///   Zip specification</see> describes how times are stored in the zip file,
        ///   the full precision of the <c>System.DateTime</c> datatype is not stored
        ///   for the last modified time when saving zip files.  For more information on
        ///   how times are formatted, see the PKZip specification.
        /// </para>
        ///
        /// <para>
        ///   The actual last modified time of a file can be stored in multiple ways in
        ///   the zip file, and they are not mutually exclusive:
        /// </para>
        ///
        /// <list type="bullet">
        ///   <item>
        ///     In the so-called "DOS" format, which has a 2-second precision. Values
        ///     are rounded to the nearest even second. For example, if the time on the
        ///     file is 12:34:43, then it will be stored as 12:34:44. This first value
        ///     is accessible via the <c>LastModified</c> property. This value is always
        ///     present in the metadata for each zip entry.  In some cases the value is
        ///     invalid, or zero.
        ///   </item>
        ///
        ///   <item>
        ///     In the so-called "Windows" or "NTFS" format, as an 8-byte integer
        ///     quantity expressed as the number of 1/10 milliseconds (in other words
        ///     the number of 100 nanosecond units) since January 1, 1601 (UTC).  This
        ///     format is how Windows represents file times.  This time is accessible
        ///     via the <c>ModifiedTime</c> property.
        ///   </item>
        ///
        ///   <item>
        ///     In the "Unix" format, a 4-byte quantity specifying the number of seconds since
        ///     January 1, 1970 UTC.
        ///   </item>
        ///
        ///   <item>
        ///     In an older format, now deprecated but still used by some current
        ///     tools. This format is also a 4-byte quantity specifying the number of
        ///     seconds since January 1, 1970 UTC.
        ///   </item>
        ///
        /// </list>
        ///
        /// <para>
        ///   Zip tools and libraries will always at least handle (read or write) the
        ///   DOS time, and may also handle the other time formats.  Keep in mind that
        ///   while the names refer to particular operating systems, there is nothing in
        ///   the time formats themselves that prevents their use on other operating
        ///   systems.
        /// </para>
        ///
        /// <para>
        ///   When reading ZIP files, the DotNetZip library reads the Windows-formatted
        ///   time, if it is stored in the entry, and sets both <c>LastModified</c> and
        ///   <c>ModifiedTime</c> to that value.
        /// </para>
        ///
        /// <para>
        ///   The last modified time of the file created upon a call to
        ///   <c>ZipEntry.Extract()</c> may be adjusted during extraction to compensate
        ///   for differences in how the .NET Base Class Library deals with daylight
        ///   saving time (DST) versus how the Windows filesystem deals with daylight
        ///   saving time.  Raymond Chen <see
        ///   href="http://blogs.msdn.com/oldnewthing/archive/2003/10/24/55413.aspx">provides
        ///   some good context</see>.
        /// </para>
        ///
        /// <para>
        ///   In a nutshell: Daylight savings time rules change regularly.  In 2007, for
        ///   example, the inception week of DST changed.  In 1977, DST was in place all
        ///   year round. In 1945, likewise.  And so on.  Win32 does not attempt to
        ///   guess which time zone rules were in effect at the time in question.  It
        ///   will render a time as "standard time" and allow the app to change to DST
        ///   as necessary.  .NET makes a different choice.
        /// </para>
        ///
        /// <para>
        ///   Compare the output of FileInfo.LastWriteTime.ToString("f") with what you
        ///   see in the Windows Explorer property sheet for a file that was last
        ///   written to on the other side of the DST transition. For example, suppose
        ///   the file was last modified on October 17, 2003, during DST but DST is not
        ///   currently in effect. Explorer's file properties reports Thursday, October
        ///   17, 2003, 8:45:38 AM, but .NETs FileInfo reports Thursday, October 17,
        ///   2003, 9:45 AM.
        /// </para>
        ///
        /// <para>
        ///   Win32 says, "Thursday, October 17, 2002 8:45:38 AM PST". Note: Pacific
        ///   STANDARD Time. Even though October 17 of that year occurred during Pacific
        ///   Daylight Time, Win32 displays the time as standard time because that's
        ///   what time it is NOW.
        /// </para>
        ///
        /// <para>
        ///   .NET BCL assumes that the current DST rules were in place at the time in
        ///   question.  So, .NET says, "Well, if the rules in effect now were also in
        ///   effect on October 17, 2003, then that would be daylight time" so it
        ///   displays "Thursday, October 17, 2003, 9:45 AM PDT" - daylight time.
        /// </para>
        ///
        /// <para>
        ///   So .NET gives a value which is more intuitively correct, but is also
        ///   potentially incorrect, and which is not invertible. Win32 gives a value
        ///   which is intuitively incorrect, but is strictly correct.
        /// </para>
        ///
        /// <para>
        ///   Because of this funkiness, this library adds one hour to the LastModified
        ///   time on the extracted file, if necessary.  That is to say, if the time in
        ///   question had occurred in what the .NET Base Class Library assumed to be
        ///   DST. This assumption may be wrong given the constantly changing DST rules,
        ///   but it is the best we can do.
        /// </para>
        ///
        /// </remarks>
        ///
        public DateTime LastModified
        {
            get { return _LastModified.ToLocalTime(); }
        }


        int BufferSize
        {
            get
            {
                return _container.BufferSize;
            }
        }

        /// <summary>
        /// Last Modified time for the file represented by the entry.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   This value corresponds to the "last modified" time in the NTFS file times
        ///   as described in <see
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">the Zip
        ///   specification</see>.  When getting this property, the value may be
        ///   different from <see cref="LastModified" />.  When setting the property,
        ///   the <see cref="LastModified"/> property also gets set, but with a lower
        ///   precision.
        /// </para>
        ///
        /// <para>
        ///   Let me explain. It's going to take a while, so get
        ///   comfortable. Originally, waaaaay back in 1989 when the ZIP specification
        ///   was originally described by the esteemed Mr. Phil Katz, the dominant
        ///   operating system of the time was MS-DOS. MSDOS stored file times with a
        ///   2-second precision, because, c'mon, <em>who is ever going to need better
        ///   resolution than THAT?</em> And so ZIP files, regardless of the platform on
        ///   which the zip file was created, store file times in exactly <see
        ///   href="http://www.vsft.com/hal/dostime.htm">the same format that DOS used
        ///   in 1989</see>.
        /// </para>
        ///
        /// <para>
        ///   Since then, the ZIP spec has evolved, but the internal format for file
        ///   timestamps remains the same.  Despite the fact that the way times are
        ///   stored in a zip file is rooted in DOS heritage, any program on any
        ///   operating system can format a time in this way, and most zip tools and
        ///   libraries DO - they round file times to the nearest even second and store
        ///   it just like DOS did 25+ years ago.
        /// </para>
        ///
        /// <para>
        ///   PKWare extended the ZIP specification to allow a zip file to store what
        ///   are called "NTFS Times" and "Unix(tm) times" for a file.  These are the
        ///   <em>last write</em>, <em>last access</em>, and <em>file creation</em>
        ///   times of a particular file. These metadata are not actually specific
        ///   to NTFS or Unix. They are tracked for each file by NTFS and by various
        ///   Unix filesystems, but they are also tracked by other filesystems, too.
        ///   The key point is that the times are <em>formatted in the zip file</em>
        ///   in the same way that NTFS formats the time (ticks since win32 epoch),
        ///   or in the same way that Unix formats the time (seconds since Unix
        ///   epoch). As with the DOS time, any tool or library running on any
        ///   operating system is capable of formatting a time in one of these ways
        ///   and embedding it into the zip file.
        /// </para>
        ///
        /// <para>
        ///   These extended times are higher precision quantities than the DOS time.
        ///   As described above, the (DOS) LastModified has a precision of 2 seconds.
        ///   The Unix time is stored with a precision of 1 second. The NTFS time is
        ///   stored with a precision of 0.0000001 seconds. The quantities are easily
        ///   convertible, except for the loss of precision you may incur.
        /// </para>
        ///
        /// <para>
        ///   A zip archive can store the {C,A,M} times in NTFS format, in Unix format,
        ///   or not at all.  Often a tool running on Unix or Mac will embed the times
        ///   in Unix format (1 second precision), while WinZip running on Windows might
        ///   embed the times in NTFS format (precision of of 0.0000001 seconds).  When
        ///   reading a zip file with these "extended" times, in either format,
        ///   DotNetZip represents the values with the
        ///   <c>ModifiedTime</c>, <c>AccessedTime</c> and <c>CreationTime</c>
        ///   properties on the <c>ZipEntry</c>.
        /// </para>
        ///
        /// <para>
        ///   While any zip application or library, regardless of the platform it
        ///   runs on, could use any of the time formats allowed by the ZIP
        ///   specification, not all zip tools or libraries do support all these
        ///   formats.  Storing the higher-precision times for each entry is
        ///   optional for zip files, and many tools and libraries don't use the
        ///   higher precision quantities at all. The old DOS time, represented by
        ///   <see cref="LastModified"/>, is guaranteed to be present, though it
        ///   sometimes unset.
        /// </para>
        ///
        /// <para>
        ///   Ok, getting back to the question about how the <c>LastModified</c>
        ///   property relates to this <c>ModifiedTime</c>
        ///   property... <c>LastModified</c> is always set, while
        ///   <c>ModifiedTime</c> is not. (The other times stored in the <em>NTFS
        ///   times extension</em>, <c>CreationTime</c> and <c>AccessedTime</c> also
        ///   may not be set on an entry that is read from an existing zip file.)
        ///   When reading a zip file, then <c>LastModified</c> takes the DOS time
        ///   that is stored with the file. If the DOS time has been stored as zero
        ///   in the zipfile, then this library will use <c>DateTime.Now</c> for the
        ///   <c>LastModified</c> value.  If the ZIP file was created by an evolved
        ///   tool, then there will also be higher precision NTFS or Unix times in
        ///   the zip file.  In that case, this library will read those times, and
        ///   set <c>LastModified</c> and <c>ModifiedTime</c> to the same value, the
        ///   one corresponding to the last write time of the file.  If there are no
        ///   higher precision times stored for the entry, then <c>ModifiedTime</c>
        ///   remains unset (likewise <c>AccessedTime</c> and <c>CreationTime</c>),
        ///   and <c>LastModified</c> keeps its DOS time.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <seealso cref="AccessedTime"/>
        /// <seealso cref="CreationTime"/>
        /// <seealso cref="Ionic.Zip.ZipEntry.LastModified"/>
        public DateTime ModifiedTime
        {
            get { return _Mtime; }
        }

        /// <summary>
        /// Last Access time for the file represented by the entry.
        /// </summary>
        /// <remarks>
        /// This value may or may not be meaningful.  If the <c>ZipEntry</c> was read from an existing
        /// Zip archive, this information may not be available. For an explanation of why, see
        /// <see cref="ModifiedTime"/>.
        /// </remarks>
        /// <seealso cref="ModifiedTime"/>
        /// <seealso cref="CreationTime"/>
        public DateTime AccessedTime
        {
            get { return _Atime; }
        }

        /// <summary>
        /// The file creation time for the file represented by the entry.
        /// </summary>
        ///
        /// <remarks>
        /// This value may or may not be meaningful.  If the <c>ZipEntry</c> was read
        /// from an existing zip archive, and the creation time was not set on the entry
        /// when the zip file was created, then this property may be meaningless. For an
        /// explanation of why, see <see cref="ModifiedTime"/>.
        /// </remarks>
        /// <seealso cref="ModifiedTime"/>
        /// <seealso cref="AccessedTime"/>
        public DateTime CreationTime
        {
            get { return _Ctime; }
        }


        /// <summary>
        /// The type of timestamp attached to the ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        /// This property is valid only for a ZipEntry that was read from a zip archive.
        /// It indicates the type of timestamp attached to the entry.
        /// </remarks>
        public ZipEntryTimestamp Timestamp
        {
            get
            {
                return _timestamp;
            }
        }

#if !PORTABLE
        /// <summary>
        ///   The file attributes for the entry.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   The <see cref="System.IO.FileAttributes">attributes</see> in NTFS include
        ///   ReadOnly, Archive, Hidden, System, and Indexed.  When adding a
        ///   <c>ZipEntry</c> to a ZipFile, these attributes are set implicitly when
        ///   adding an entry from the filesystem.  When adding an entry from a stream
        ///   or string, the Attributes are not set implicitly.  Regardless of the way
        ///   an entry was added to a <c>ZipFile</c>, you can set the attributes
        ///   explicitly if you like.
        /// </para>
        ///
        /// <para>
        ///   When reading a <c>ZipEntry</c> from a <c>ZipFile</c>, the attributes are
        ///   set according to the data stored in the <c>ZipFile</c>. If you extract the
        ///   entry from the archive to a filesystem file, DotNetZip will set the
        ///   attributes on the resulting file accordingly.
        /// </para>
        ///
        /// <para>
        ///   The attributes can be set explicitly by the application.  For example the
        ///   application may wish to set the <c>FileAttributes.ReadOnly</c> bit for all
        ///   entries added to an archive, so that on unpack, this attribute will be set
        ///   on the extracted file.  Any changes you make to this property are made
        ///   permanent only when you call a <c>Save()</c> method on the <c>ZipFile</c>
        ///   instance that contains the ZipEntry.
        /// </para>
        ///
        /// <para>
        ///   For example, an application may wish to zip up a directory and set the
        ///   ReadOnly bit on every file in the archive, so that upon later extraction,
        ///   the resulting files will be marked as ReadOnly.  Not every extraction tool
        ///   respects these attributes, but if you unpack with DotNetZip, as for
        ///   example in a self-extracting archive, then the attributes will be set as
        ///   they are stored in the <c>ZipFile</c>.
        /// </para>
        ///
        /// <para>
        ///   These attributes may not be interesting or useful if the resulting archive
        ///   is extracted on a non-Windows platform.  How these attributes get used
        ///   upon extraction depends on the platform and tool used.
        /// </para>
        ///
        /// <para>
        ///   This property is only partially supported in the Silverlight version
        ///   of the library: applications can read attributes on entries within
        ///   ZipFiles. But extracting entries within Silverlight will not set the
        ///   attributes on the extracted files.
        /// </para>
        ///
        /// </remarks>
#if NETFX_CORE
        public global::Windows.Storage.FileAttributes Attributes
        {
          // workitem 7071
          get { return (global::Windows.Storage.FileAttributes)_ExternalFileAttrs; }
        }
#else
        public System.IO.FileAttributes Attributes
        {
          // workitem 7071
          get { return (System.IO.FileAttributes)_ExternalFileAttrs; }
        }
#endif
#endif


        /// <summary>
        ///   The name of the file contained in the ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// <para>
        ///   When reading a zip file, this property takes the value of the entry name
        ///   as stored in the zip file. If you extract such an entry, the extracted
        ///   file will take the name given by this property.
        /// </para>
        ///
        /// <para>
        ///   Applications can set this property when creating new zip archives or when
        ///   reading existing archives. When setting this property, the actual value
        ///   that is set will replace backslashes with forward slashes, in accordance
        ///   with <see
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">the Zip
        ///   specification</see>, for compatibility with Unix(tm) and ... get
        ///   this.... Amiga!
        /// </para>
        ///
        /// </remarks>
        public string FileName
        {
            get { return _FileNameInArchive; }
        }


        /// <summary>
        /// An enum indicating the source of the ZipEntry.
        /// </summary>
        public ZipEntrySource Source
        {
            get { return _Source; }
        }


        /// <summary>
        /// The version of the zip engine needed to read the ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This is a readonly property, indicating the version of <a
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">the Zip
        ///   specification</a> that the extracting tool or library must support to
        ///   extract the given entry.  Generally higher versions indicate newer
        ///   features.  Older zip engines obviously won't know about new features, and
        ///   won't be able to extract entries that depend on those newer features.
        /// </para>
        ///
        /// <list type="table">
        /// <listheader>
        /// <term>value</term>
        /// <description>Features</description>
        /// </listheader>
        ///
        /// <item>
        /// <term>20</term>
        /// <description>a basic Zip Entry, potentially using PKZIP encryption.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>45</term>
        /// <description>The ZIP64 extension is used on the entry.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>46</term>
        /// <description> File is compressed using BZIP2 compression*</description>
        /// </item>
        ///
        /// <item>
        /// <term>50</term>
        /// <description> File is encrypted using PkWare's DES, 3DES, (broken) RC2 or RC4</description>
        /// </item>
        ///
        /// <item>
        /// <term>51</term>
        /// <description> File is encrypted using PKWare's AES encryption or corrected RC2 encryption.</description>
        /// </item>
        ///
        /// <item>
        /// <term>52</term>
        /// <description> File is encrypted using corrected RC2-64 encryption**</description>
        /// </item>
        ///
        /// <item>
        /// <term>61</term>
        /// <description> File is encrypted using non-OAEP key wrapping***</description>
        /// </item>
        ///
        /// <item>
        /// <term>63</term>
        /// <description> File is compressed using LZMA, PPMd+, Blowfish, or Twofish</description>
        /// </item>
        ///
        /// </list>
        ///
        /// <para>
        ///   There are other values possible, not listed here. DotNetZip supports
        ///   regular PKZip encryption, and ZIP64 extensions.  DotNetZip cannot extract
        ///   entries that require a zip engine higher than 45.
        /// </para>
        ///
        /// <para>
        ///   This value is set upon reading an existing zip file, or after saving a zip
        ///   archive.
        /// </para>
        /// </remarks>
        public Int16 VersionNeeded
        {
            get { return _VersionNeeded; }
        }

        /// <summary>
        /// The comment attached to the ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Each entry in a zip file can optionally have a comment associated to
        ///   it. The comment might be displayed by a zip tool during extraction, for
        ///   example.
        /// </para>
        ///
        /// <para>
        ///   By default, the <c>Comment</c> is encoded in IBM437 code page. You can
        ///   specify an alternative with <see cref="AlternateEncoding"/> and
        ///  <see cref="AlternateEncodingUsage"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="AlternateEncoding"/>
        /// <seealso cref="AlternateEncodingUsage"/>
        public string Comment
        {
            get { return _Comment; }
        }


        /// <summary>
        ///   The bitfield for the entry as defined in the zip spec. You probably
        ///   never need to look at this.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   You probably do not need to concern yourself with the contents of this
        ///   property, but in case you do:
        /// </para>
        ///
        /// <list type="table">
        /// <listheader>
        /// <term>bit</term>
        /// <description>meaning</description>
        /// </listheader>
        ///
        /// <item>
        /// <term>0</term>
        /// <description>set if encryption is used.</description>
        /// </item>
        ///
        /// <item>
        /// <term>1-2</term>
        /// <description>
        /// set to determine whether normal, max, fast deflation.  DotNetZip library
        /// always leaves these bits unset when writing (indicating "normal"
        /// deflation"), but can read an entry with any value here.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>3</term>
        /// <description>
        /// Indicates that the Crc32, Compressed and Uncompressed sizes are zero in the
        /// local header.  This bit gets set on an entry during writing a zip file, when
        /// it is saved to a non-seekable output stream.
        /// </description>
        /// </item>
        ///
        ///
        /// <item>
        /// <term>4</term>
        /// <description>reserved for "enhanced deflating". This library doesn't do enhanced deflating.</description>
        /// </item>
        ///
        /// <item>
        /// <term>5</term>
        /// <description>set to indicate the zip is compressed patched data.  This library doesn't do that.</description>
        /// </item>
        ///
        /// <item>
        /// <term>6</term>
        /// <description>
        /// set if PKWare's strong encryption is used (must also set bit 1 if bit 6 is
        /// set). This bit is not set if WinZip's AES encryption is set.</description>
        /// </item>
        ///
        /// <item>
        /// <term>7</term>
        /// <description>not used</description>
        /// </item>
        ///
        /// <item>
        /// <term>8</term>
        /// <description>not used</description>
        /// </item>
        ///
        /// <item>
        /// <term>9</term>
        /// <description>not used</description>
        /// </item>
        ///
        /// <item>
        /// <term>10</term>
        /// <description>not used</description>
        /// </item>
        ///
        /// <item>
        /// <term>11</term>
        /// <description>
        /// Language encoding flag (EFS).  If this bit is set, the filename and comment
        /// fields for this file must be encoded using UTF-8. This library currently
        /// does not support UTF-8.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>12</term>
        /// <description>Reserved by PKWARE for enhanced compression.</description>
        /// </item>
        ///
        /// <item>
        /// <term>13</term>
        /// <description>
        ///   Used when encrypting the Central Directory to indicate selected data
        ///   values in the Local Header are masked to hide their actual values.  See
        ///   the section in <a
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">the Zip
        ///   specification</a> describing the Strong Encryption Specification for
        ///   details.
        /// </description>
        /// </item>
        ///
        /// <item>
        /// <term>14</term>
        /// <description>Reserved by PKWARE.</description>
        /// </item>
        ///
        /// <item>
        /// <term>15</term>
        /// <description>Reserved by PKWARE.</description>
        /// </item>
        ///
        /// </list>
        ///
        /// </remarks>
        public Int16 BitField
        {
            get { return _BitField; }
        }


        /// <summary>
        ///   The compressed size of the file, in bytes, within the zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///   When reading a <c>ZipFile</c>, this value is read in from the existing
        ///   zip file. When creating or updating a <c>ZipFile</c>, the compressed
        ///   size is computed during compression.  Therefore the value on a
        ///   <c>ZipEntry</c> is valid after a call to <c>Save()</c> (or one of its
        ///   overloads) in that case.
        /// </remarks>
        ///
        /// <seealso cref="Ionic.Zip.ZipEntry.UncompressedSize"/>
        public Int64 CompressedSize
        {
            get { return _CompressedSize; }
        }

        /// <summary>
        ///   The size of the file, in bytes, before compression, or after extraction.
        /// </summary>
        ///
        /// <remarks>
        ///   When reading a <c>ZipFile</c>, this value is read in from the existing
        ///   zip file. When creating or updating a <c>ZipFile</c>, the uncompressed
        ///   size is computed during compression.  Therefore the value on a
        ///   <c>ZipEntry</c> is valid after a call to <c>Save()</c> (or one of its
        ///   overloads) in that case.
        /// </remarks>
        ///
        /// <seealso cref="Ionic.Zip.ZipEntry.CompressedSize"/>
        public Int64 UncompressedSize
        {
            get { return _UncompressedSize; }
        }

        /// <summary>
        /// The ratio of compressed size to uncompressed size of the ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This is a ratio of the compressed size to the uncompressed size of the
        ///   entry, expressed as a double in the range of 0 to 100+. A value of 100
        ///   indicates no compression at all.  It could be higher than 100 when the
        ///   compression algorithm actually inflates the data, as may occur for small
        ///   files, or uncompressible data that is encrypted.
        /// </para>
        ///
        /// <para>
        ///   You could format it for presentation to a user via a format string of
        ///   "{3,5:F0}%" to see it as a percentage.
        /// </para>
        ///
        /// <para>
        ///   If the size of the original uncompressed file is 0, implying a
        ///   denominator of 0, the return value will be zero.
        /// </para>
        ///
        /// <para>
        ///   This property is valid after reading in an existing zip file, or after
        ///   saving the <c>ZipFile</c> that contains the ZipEntry. You cannot know the
        ///   effect of a compression transform until you try it.
        /// </para>
        ///
        /// </remarks>
        public Double CompressionRatio
        {
            get
            {
                if (UncompressedSize == 0) return 0;
                return 100 * (1.0 - (1.0 * CompressedSize) / (1.0 * UncompressedSize));
            }
        }

        /// <summary>
        /// The 32-bit CRC (Cyclic Redundancy Check) on the contents of the ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para> You probably don't need to concern yourself with this. It is used
        /// internally by DotNetZip to verify files or streams upon extraction.  </para>
        ///
        /// <para> The value is a <see href="http://en.wikipedia.org/wiki/CRC32">32-bit
        /// CRC</see> using 0xEDB88320 for the polynomial. This is the same CRC-32 used in
        /// PNG, MPEG-2, and other protocols and formats.  It is a read-only property; when
        /// creating a Zip archive, the CRC for each entry is set only after a call to
        /// <c>Save()</c> on the containing ZipFile. When reading an existing zip file, the value
        /// of this property reflects the stored CRC for the entry.  </para>
        ///
        /// </remarks>
        public Int32 Crc
        {
            get { return _Crc32; }
        }

        /// <summary>
        /// True if the entry is a directory (not a file).
        /// This is a readonly property on the entry.
        /// </summary>
        public bool IsDirectory
        {
            get { return _IsDirectory; }
        }

        /// <summary>
        /// A derived property that is <c>true</c> if the entry uses encryption.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This is a readonly property on the entry.  When reading a zip file,
        ///   the value for the <c>ZipEntry</c> is determined by the data read
        ///   from the zip file.  After saving a ZipFile, the value of this
        ///   property for each <c>ZipEntry</c> indicates whether encryption was
        ///   actually used (which will have been true if the <see
        ///   cref="Password"/> was set and the <see cref="Encryption"/> property
        ///   was something other than <see cref="EncryptionAlgorithm.None"/>.
        /// </para>
        /// </remarks>
        public bool UsesEncryption
        {
            get { return (_Encryption_FromZipFile != EncryptionAlgorithm.None); }
        }


        /// <summary>
        ///   Gets the encryption algorithm used for the entry.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   The Zip specification from PKWare defines a set of encryption algorithms,
        ///   and the data formats for the zip archive that support them, and PKWare
        ///   supports those algorithms in the tools it produces. Other vendors of tools
        ///   and libraries, such as WinZip or Xceed, typically support <em>a
        ///   subset</em> of the algorithms specified by PKWare. These tools can
        ///   sometimes support additional different encryption algorithms and data
        ///   formats, not specified by PKWare. The AES Encryption specified and
        ///   supported by WinZip is the most popular example. This library supports a
        ///   subset of the complete set of algorithms specified by PKWare and other
        ///   vendors.
        /// </para>
        ///
        /// <para>
        ///   There is no common, ubiquitous multi-vendor standard for strong encryption
        ///   within zip files. There is broad support for so-called "traditional" Zip
        ///   encryption, sometimes called Zip 2.0 encryption, as <see
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">specified
        ///   by PKWare</see>, but this encryption is considered weak and
        ///   breakable. This library currently supports the Zip 2.0 "weak" encryption,
        ///   and also a stronger WinZip-compatible AES encryption, using either 128-bit
        ///   or 256-bit key strength. If you want DotNetZip to support an algorithm
        ///   that is not currently supported, call the author of this library and maybe
        ///   we can talk business.
        /// </para>
        ///
        /// <para>
        ///   The WinZip AES encryption algorithms are not supported on the .NET Compact
        ///   Framework.
        /// </para>
        /// </remarks>
        ///
        /// <seealso cref="Ionic.Zip.ZipEntry.Password">ZipEntry.Password</seealso>
        public EncryptionAlgorithm Encryption
        {
            get { return _Encryption; }
        }


        /// <summary>
        /// The Password to be used when decrypting an entry upon Extract().
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This is a write-only property on the entry. Set it to specify the
        ///   password to be used when extracting an existing entry that is encrypted.
        /// </para>
        ///
        /// <para>
        ///   The password set here is implicitly used to to decrypt during the <see
        ///   cref="Extract"/> or <see cref="OpenReader()"/> operation.
        /// </para>
        ///
        /// <para>
        ///   Consider setting the <see cref="Encryption"/> property when using a
        ///   password. Answering concerns that the standard password protection
        ///   supported by all zip tools is weak, WinZip has extended the ZIP
        ///   specification with a way to use AES Encryption to protect entries in the
        ///   Zip file. Unlike the "PKZIP 2.0" encryption specified in the PKZIP
        ///   specification, <see href=
        ///   "http://en.wikipedia.org/wiki/Advanced_Encryption_Standard">AES
        ///   Encryption</see> uses a standard, strong, tested, encryption
        ///   algorithm. DotNetZip can create zip archives that use WinZip-compatible
        ///   AES encryption, if you set the <see cref="Encryption"/> property. But,
        ///   archives created that use AES encryption may not be readable by all other
        ///   tools and libraries. For example, Windows Explorer cannot read a
        ///   "compressed folder" (a zip file) that uses AES encryption, though it can
        ///   read a zip file that uses "PKZIP encryption."
        /// </para>
        ///
        /// </remarks>
        ///
        /// <seealso cref="Ionic.Zip.ZipEntry.Encryption"/>
        public string Password
        {
            set
            {
                _Password = value;
                if (_Password == null)
                {
                    _Encryption = EncryptionAlgorithm.None;
                }
                else
                {
                    // We're setting a non-null password.

                    // For entries obtained from a zip file that are encrypted, we cannot
                    // simply restream (recompress, re-encrypt) the file data, because we
                    // need the old password in order to decrypt the data, and then we
                    // need the new password to encrypt.  So, setting the password is
                    // never going to work on an entry that is stored encrypted in a zipfile.

                    // But it is not en error to set the password, obviously: callers will
                    // set the password in order to Extract encrypted archives.

                    // If the source is a zip archive and there was previously no encryption
                    // on the entry, then we must re-stream the entry in order to encrypt it.
                    if (this._Source == ZipEntrySource.ZipFile && !_sourceIsEncrypted)
                        _restreamRequiredOnSave = true;

                    if (Encryption == EncryptionAlgorithm.None)
                    {
                        _Encryption = EncryptionAlgorithm.PkzipWeak;
                    }
                }
            }
            //private get { return _Password; }
        }



        internal bool IsChanged
        {
            get
            {
                return _restreamRequiredOnSave | _metadataChanged;
            }
        }


        /// <summary>
        ///   Specifies the alternate text encoding used by this ZipEntry
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The default text encoding used in Zip files for encoding filenames and
        ///     comments is IBM437, which is something like a superset of ASCII.  In
        ///     cases where this is insufficient, applications can specify an
        ///     alternate encoding.
        ///   </para>
        ///   <para>
        ///     When creating a zip file, the usage of the alternate encoding is
        ///     governed by the <see cref="AlternateEncodingUsage"/> property.
        ///     Typically you would set both properties to tell DotNetZip to employ an
        ///     encoding that is not IBM437 in the zipfile you are creating.
        ///   </para>
        ///   <para>
        ///     Keep in mind that because the ZIP specification states that the only
        ///     valid encodings to use are IBM437 and UTF-8, if you use something
        ///     other than that, then zip tools and libraries may not be able to
        ///     successfully read the zip archive you generate.
        ///   </para>
        ///   <para>
        ///     The zip specification states that applications should presume that
        ///     IBM437 is in use, except when a special bit is set, which indicates
        ///     UTF-8. There is no way to specify an arbitrary code page, within the
        ///     zip file itself. When you create a zip file encoded with gb2312 or
        ///     ibm861 or anything other than IBM437 or UTF-8, then the application
        ///     that reads the zip file needs to "know" which code page to use. In
        ///     some cases, the code page used when reading is chosen implicitly. For
        ///     example, WinRar uses the ambient code page for the host desktop
        ///     operating system. The pitfall here is that if you create a zip in
        ///     Copenhagen and send it to Tokyo, the reader of the zipfile may not be
        ///     able to decode successfully.
        ///   </para>
        /// </remarks>
        /// <example>
        ///   This example shows how to create a zipfile encoded with a
        ///   language-specific encoding:
        /// <code>
        ///   using (var zip = new ZipFile())
        ///   {
        ///      zip.AlternateEnoding = System.Text.Encoding.GetEncoding("ibm861");
        ///      zip.AlternateEnodingUsage = ZipOption.Always;
        ///      zip.AddFileS(arrayOfFiles);
        ///      zip.Save("Myarchive-Encoded-in-IBM861.zip");
        ///   }
        /// </code>
        /// </example>
        /// <seealso cref="ZipFile.AlternateEncodingUsage" />
        public System.Text.Encoding AlternateEncoding
        {
            get; set;
        }


        /// <summary>
        ///   Describes if and when this instance should apply
        ///   AlternateEncoding to encode the FileName and Comment, when
        ///   saving.
        /// </summary>
        /// <seealso cref="ZipFile.AlternateEncoding" />
        public ZipOption AlternateEncodingUsage
        {
            get; set;
        }


        // /// <summary>
        // /// The text encoding actually used for this ZipEntry.
        // /// </summary>
        // ///
        // /// <remarks>
        // ///
        // /// <para>
        // ///   This read-only property describes the encoding used by the
        // ///   <c>ZipEntry</c>.  If the entry has been read in from an existing ZipFile,
        // ///   then it may take the value UTF-8, if the entry is coded to specify UTF-8.
        // ///   If the entry does not specify UTF-8, the typical case, then the encoding
        // ///   used is whatever the application specified in the call to
        // ///   <c>ZipFile.Read()</c>. If the application has used one of the overloads of
        // ///   <c>ZipFile.Read()</c> that does not accept an encoding parameter, then the
        // ///   encoding used is IBM437, which is the default encoding described in the
        // ///   ZIP specification.  </para>
        // ///
        // /// <para>
        // ///   If the entry is being created, then the value of ActualEncoding is taken
        // ///   according to the logic described in the documentation for <see
        // ///   cref="ZipFile.ProvisionalAlternateEncoding" />.  </para>
        // ///
        // /// <para>
        // ///   An application might be interested in retrieving this property to see if
        // ///   an entry read in from a file has used Unicode (UTF-8).  </para>
        // ///
        // /// </remarks>
        // ///
        // /// <seealso cref="ZipFile.ProvisionalAlternateEncoding" />
        // public System.Text.Encoding ActualEncoding
        // {
        //     get
        //     {
        //         return _actualEncoding;
        //     }
        // }


        internal void MarkAsDirectory()
        {
            _IsDirectory = true;
            // workitem 6279
            if (!_FileNameInArchive.EndsWith("/", StringComparison.Ordinal))
                _FileNameInArchive += "/";
        }


        /// <summary>
        ///   Indicates whether an entry is marked as a text file. Be careful when
        ///   using on this property. Unless you have a good reason, you should
        ///   probably ignore this property.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The ZIP format includes a provision for specifying whether an entry in
        ///   the zip archive is a text or binary file.  This property exposes that
        ///   metadata item. Be careful when using this property: It's not clear
        ///   that this property as a firm meaning, across tools and libraries.
        /// </para>
        ///
        /// <para>
        ///   To be clear, when reading a zip file, the property value may or may
        ///   not be set, and its value may or may not be valid.  Not all entries
        ///   that you may think of as "text" entries will be so marked, and entries
        ///   marked as "text" are not guaranteed in any way to be text entries.
        ///   Whether the value is set and set correctly depends entirely on the
        ///   application that produced the zip file.
        /// </para>
        ///
        /// <para>
        ///   There are many zip tools available, and when creating zip files, some
        ///   of them "respect" the IsText metadata field, and some of them do not.
        ///   Unfortunately, even when an application tries to do "the right thing",
        ///   it's not always clear what "the right thing" is.
        /// </para>
        ///
        /// <para>
        ///   There's no firm definition of just what it means to be "a text file",
        ///   and the zip specification does not help in this regard. Twenty years
        ///   ago, text was ASCII, each byte was less than 127. IsText meant, all
        ///   bytes in the file were less than 127.  These days, it is not the case
        ///   that all text files have all bytes less than 127.  Any unicode file
        ///   may have bytes that are above 0x7f.  The zip specification has nothing
        ///   to say on this topic. Therefore, it's not clear what IsText really
        ///   means.
        /// </para>
        ///
        /// <para>
        ///   This property merely tells a reading application what is stored in the
        ///   metadata for an entry, without guaranteeing its validity or its
        ///   meaning.
        /// </para>
        ///
        /// <para>
        ///   When DotNetZip is used to create a zipfile, it attempts to set this
        ///   field "correctly." For example, if a file ends in ".txt", this field
        ///   will be set. Your application may override that default setting.  When
        ///   writing a zip file, you must set the property before calling
        ///   <c>Save()</c> on the ZipFile.
        /// </para>
        ///
        /// <para>
        ///   When reading a zip file, a more general way to decide just what kind
        ///   of file is contained in a particular entry is to use the file type
        ///   database stored in the operating system.  The operating system stores
        ///   a table that says, a file with .jpg extension is a JPG image file, a
        ///   file with a .xml extension is an XML document, a file with a .txt is a
        ///   pure ASCII text document, and so on.  To get this information on
        ///   Windows, <see
        ///   href="http://www.codeproject.com/KB/cs/GetFileTypeAndIcon.aspx"> you
        ///   need to read and parse the registry.</see> </para>
        /// </remarks>
        ///
        /// <example>
        /// <code>
        /// using (var zip = new ZipFile())
        /// {
        ///     var e = zip.UpdateFile("Descriptions.mme", "");
        ///     e.IsText = true;
        ///     zip.Save(zipPath);
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using zip As New ZipFile
        ///     Dim e2 as ZipEntry = zip.AddFile("Descriptions.mme", "")
        ///     e.IsText= True
        ///     zip.Save(zipPath)
        /// End Using
        /// </code>
        /// </example>
        public bool IsText
        {
            // workitem 7801
            get { return _IsText; }
            set { _IsText = value; }
        }


        /// <summary>Provides a string representation of the instance.</summary>
        /// <returns>a string representation of the instance.</returns>
        public override String ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "ZipEntry::{0}", FileName);
        }


        internal Stream ArchiveStream
        {
            get
            {
                if (_archiveStream == null)
                {
                    if (_container.ZipFile != null)
                    {
                        var zf = _container.ZipFile;
                        _archiveStream = zf.StreamForDiskNumber(_diskNumber);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                return _archiveStream;
            }
        }


        private void SetFdpLoh()
        {
            // The value for FileDataPosition has not yet been set.
            // Therefore, seek to the local header, and figure the start of file data.
            // workitem 8098: ok (restore)
            long origPosition = this.ArchiveStream.Position;
            try
            {
                this.ArchiveStream.Seek(this._RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
            }
            catch (System.IO.IOException exc1)
            {
                string description = String.Format(CultureInfo.InvariantCulture, 
                                                   "Exception seeking  entry({0}) offset(0x{1:X8}) len(0x{2:X8})",
                                                   this.FileName, this._RelativeOffsetOfLocalHeader,
                                                   this.ArchiveStream.Length);
                throw new BadStateException(description, exc1);
            }

            byte[] block = new byte[30];
            this.ArchiveStream.Read(block, 0, block.Length);

            // At this point we could verify the contents read from the local header
            // with the contents read from the central header.  We could, but don't need to.
            // So we won't.

            Int16 filenameLength = (short)(block[26] + block[27] * 256);
            Int16 extraFieldLength = (short)(block[28] + block[29] * 256);

            // Console.WriteLine("  pos  0x{0:X8} ({0})", this.ArchiveStream.Position);
            // Console.WriteLine("  seek 0x{0:X8} ({0})", filenameLength + extraFieldLength);

            this.ArchiveStream.Seek(filenameLength + extraFieldLength, SeekOrigin.Current);

            this._LengthOfHeader = 30 + extraFieldLength + filenameLength +
                GetLengthOfCryptoHeaderBytes(_Encryption_FromZipFile);

            // Console.WriteLine("  ROLH  0x{0:X8} ({0})", _RelativeOffsetOfLocalHeader);
            // Console.WriteLine("  LOH   0x{0:X8} ({0})", _LengthOfHeader);
            // workitem 8098: ok (arithmetic)
            this.__FileDataPosition = _RelativeOffsetOfLocalHeader + _LengthOfHeader;
            // Console.WriteLine("  FDP   0x{0:X8} ({0})", __FileDataPosition);

            // restore file position:
            // workitem 8098: ok (restore)
            this.ArchiveStream.Seek(origPosition, SeekOrigin.Begin);
        }


#if AESCRYPTO
        private static int GetKeyStrengthInBits(EncryptionAlgorithm a)
        {
            if (a == EncryptionAlgorithm.WinZipAes256) return 256;
            else if (a == EncryptionAlgorithm.WinZipAes128) return 128;
            return -1;
        }
#endif

        internal static int GetLengthOfCryptoHeaderBytes(EncryptionAlgorithm a)
        {
            //if ((_BitField & 0x01) != 0x01) return 0;
            if (a == EncryptionAlgorithm.None) return 0;

#if AESCRYPTO
            if (a == EncryptionAlgorithm.WinZipAes128 ||
                a == EncryptionAlgorithm.WinZipAes256)
            {
                int KeyStrengthInBits = GetKeyStrengthInBits(a);
                int sizeOfSaltAndPv = ((KeyStrengthInBits / 8 / 2) + 2);
                return sizeOfSaltAndPv;
            }
#endif

            if (a == EncryptionAlgorithm.PkzipWeak)
                return 12;
            throw new ZipException("internal error");
        }


        internal long FileDataPosition
        {
            get
            {
                if (__FileDataPosition == -1)
                    SetFdpLoh();

                return __FileDataPosition;
            }
        }


        private ZipCrypto _zipCrypto_forExtract;
#if AESCRYPTO
        private WinZipAesCrypto _aesCrypto_forExtract;
        private Int16 _WinZipAesMethod;
#endif

        internal DateTime _LastModified;
        private DateTime _Mtime, _Atime, _Ctime;  // workitem 6878: NTFS quantities
        private string _FileNameInArchive;
        internal Int16 _VersionNeeded;
        internal Int16 _BitField;
        internal Int16 _CompressionMethod;
        private Int16 _CompressionMethod_FromZipFile;
        internal string _Comment;
        private bool _IsDirectory;
        internal Int64 _CompressedSize;
        internal Int64 _CompressedFileDataSize; // CompressedSize less 12 bytes for the encryption header, if any
        internal Int64 _UncompressedSize;
        internal Int32 _TimeBlob;
        internal Int32 _Crc32;
        internal byte[] _Extra;
        private bool _metadataChanged;
        private bool _restreamRequiredOnSave;
        private bool _sourceIsEncrypted;
        private UInt32 _diskNumber;

        internal ZipContainer _container;

        private long __FileDataPosition = -1;
        internal Int64 _RelativeOffsetOfLocalHeader;
        private int _LengthOfHeader;
        private int _LengthOfTrailer;
        internal bool _InputUsesZip64;
        private UInt32 _UnsupportedAlgorithmId;

        internal string _Password;
        internal ZipEntrySource _Source;
        internal EncryptionAlgorithm _Encryption;
        internal EncryptionAlgorithm _Encryption_FromZipFile;
        internal byte[] _WeakEncryptionHeader;
        internal Stream _archiveStream;
        private bool _ioOperationCanceled;
        private bool _IsText; // workitem 7801
        private ZipEntryTimestamp _timestamp;

        private static System.DateTime _unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        // summary
        // The default size of the IO buffer for ZipEntry instances. Currently it is 8192 bytes.
        // summary
        //public const int IO_BUFFER_SIZE_DEFAULT = 8192; // 0x8000; // 0x4400

    }



    /// <summary>
    ///   An enum that specifies the type of timestamp available on the ZipEntry.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
    ///   The last modified time of a file can be stored in multiple ways in
    ///   a zip file, and they are not mutually exclusive:
    /// </para>
    ///
    /// <list type="bullet">
    ///   <item>
    ///     In the so-called "DOS" format, which has a 2-second precision. Values
    ///     are rounded to the nearest even second. For example, if the time on the
    ///     file is 12:34:43, then it will be stored as 12:34:44. This first value
    ///     is accessible via the <c>LastModified</c> property. This value is always
    ///     present in the metadata for each zip entry.  In some cases the value is
    ///     invalid, or zero.
    ///   </item>
    ///
    ///   <item>
    ///     In the so-called "Windows" or "NTFS" format, as an 8-byte integer
    ///     quantity expressed as the number of 1/10 milliseconds (in other words
    ///     the number of 100 nanosecond units) since January 1, 1601 (UTC).  This
    ///     format is how Windows represents file times.  This time is accessible
    ///     via the <c>ModifiedTime</c> property.
    ///   </item>
    ///
    ///   <item>
    ///     In the "Unix" format, a 4-byte quantity specifying the number of seconds since
    ///     January 1, 1970 UTC.
    ///   </item>
    ///
    ///   <item>
    ///     In an older format, now deprecated but still used by some current
    ///     tools. This format is also a 4-byte quantity specifying the number of
    ///     seconds since January 1, 1970 UTC.
    ///   </item>
    ///
    /// </list>
    ///
    /// <para>
    ///   This bit field describes which of the formats were found in a <c>ZipEntry</c> that was read.
    /// </para>
    ///
    /// </remarks>
    [Flags]
    internal enum ZipEntryTimestamp
    {
        /// <summary>
        /// Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// A DOS timestamp with 2-second precision.
        /// </summary>
        DOS = 1,

        /// <summary>
        /// A Windows timestamp with 100-ns precision.
        /// </summary>
        Windows = 2,

        /// <summary>
        /// A Unix timestamp with 1-second precision.
        /// </summary>
        Unix = 4,

        /// <summary>
        /// A Unix timestamp with 1-second precision, stored in InfoZip v1 format.  This
        /// format is outdated and is supported for reading archives only.
        /// </summary>
        InfoZip1 = 8,
    }



    /// <summary>
    ///   The method of compression to use for a particular ZipEntry.
    /// </summary>
    ///
    /// <remarks>
    ///   <see
    ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">PKWare's
    ///   ZIP Specification</see> describes a number of distinct
    ///   cmopression methods that can be used within a zip
    ///   file. DotNetZip supports a subset of them.
    /// </remarks>
    internal enum CompressionMethod
    {
        /// <summary>
        /// No compression at all. For COM environments, the value is 0 (zero).
        /// </summary>
        None = 0,

        /// <summary>
        ///   DEFLATE compression, as described in <see
        ///   href="http://www.ietf.org/rfc/rfc1951.txt">IETF RFC
        ///   1951</see>.  This is the "normal" compression used in zip
        ///   files. For COM environments, the value is 8.
        /// </summary>
        Deflate = 8,

#if BZIP
        /// <summary>
        ///   BZip2 compression, a compression algorithm developed by Julian Seward.
        ///   For COM environments, the value is 12.
        /// </summary>
        BZip2 = 12,
#endif
    }
}
