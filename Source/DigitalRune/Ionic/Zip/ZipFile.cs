// ZipFile.cs
//
// Copyright (c) 2006-2010 Dino Chiesa
// All rights reserved.
//
// This module is part of DotNetZip, a zipfile class library.
// The class library reads and writes zip files, according to the format
// described by PKware, at:
// http://www.pkware.com/business_and_developers/developer/popups/appnote.txt
//
//
// This code is released under the Microsoft Public License .
// See the License.txt for details.
//


using System;
using System.IO;
using System.Collections.Generic;
#if NETFX_CORE || PORTABLE
using System.Collections.ObjectModel;
#endif

namespace DigitalRune.Ionic.Zip
{
    /// <summary>
    ///   The ZipFile type represents a zip archive file.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///   This is the main type in the DotNetZip class library. This class reads and
    ///   writes zip files, as defined in the <see
    ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">specification
    ///   for zip files described by PKWare</see>.  The compression for this
    ///   implementation is provided by a managed-code version of Zlib, included with
    ///   DotNetZip in the classes in the Ionic.Zlib namespace.
    /// </para>
    ///
    /// <para>
    ///   This class provides a general purpose zip file capability.  Use it to read
    ///   zip files.
    /// </para>
    ///
    /// </remarks>
    internal partial class ZipFile :
    System.Collections.IEnumerable,
    System.Collections.Generic.IEnumerable<ZipEntry>,
    IDisposable
    {

        #region public properties

        /// <summary>
        ///   Size of the IO buffer used while saving.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   First, let me say that you really don't need to bother with this.  It is
        ///   here to allow for optimizations that you probably won't make! It will work
        ///   fine if you don't set or get this property at all. Ok?
        /// </para>
        ///
        /// <para>
        ///   Now that we have <em>that</em> out of the way, the fine print: This
        ///   property affects the size of the buffer that is used for I/O for each
        ///   entry contained in the zip file. When a file is read in to be compressed,
        ///   it uses a buffer given by the size here.  When you update a zip file, the
        ///   data for unmodified entries is copied from the first zip file to the
        ///   other, through a buffer given by the size here.
        /// </para>
        ///
        /// <para>
        ///   Changing the buffer size affects a few things: first, for larger buffer
        ///   sizes, the memory used by the <c>ZipFile</c>, obviously, will be larger
        ///   during I/O operations.  This may make operations faster for very much
        ///   larger files.  Last, for any given entry, when you use a larger buffer
        ///   there will be fewer progress events during I/O operations, because there's
        ///   one progress event generated for each time the buffer is filled and then
        ///   emptied.
        /// </para>
        ///
        /// <para>
        ///   The default buffer size is 8k.  Increasing the buffer size may speed
        ///   things up as you compress larger files.  But there are no hard-and-fast
        ///   rules here, eh?  You won't know til you test it.  And there will be a
        ///   limit where ever larger buffers actually slow things down.  So as I said
        ///   in the beginning, it's probably best if you don't set or get this property
        ///   at all.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        /// This example shows how you might set a large buffer size for efficiency when
        /// dealing with zip entries that are larger than 1gb.
        /// <code lang="C#">
        /// using (ZipFile zip = new ZipFile())
        /// {
        ///     zip.SaveProgress += this.zip1_SaveProgress;
        ///     zip.AddDirectory(directoryToZip, "");
        ///     zip.UseZip64WhenSaving = Zip64Option.Always;
        ///     zip.BufferSize = 65536*8; // 65536 * 8 = 512k
        ///     zip.Save(ZipFileToCreate);
        /// }
        /// </code>
        /// </example>
        public int BufferSize
        {
            get { return _BufferSize; }
            set { _BufferSize = value; }
        }


        /// <summary>
        ///   A comment attached to the zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   This property is read/write. It allows the application to specify a
        ///   comment for the <c>ZipFile</c>, or read the comment for the
        ///   <c>ZipFile</c>.  After setting this property, changes are only made
        ///   permanent when you call a <c>Save()</c> method.
        /// </para>
        ///
        /// <para>
        ///   According to <see
        ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">PKWARE's
        ///   zip specification</see>, the comment is not encrypted, even if there is a
        ///   password set on the zip file.
        /// </para>
        ///
        /// <para>
        ///   The specification does not describe how to indicate the encoding used
        ///   on a comment string. Many "compliant" zip tools and libraries use
        ///   IBM437 as the code page for comments; DotNetZip, too, follows that
        ///   practice.  On the other hand, there are situations where you want a
        ///   Comment to be encoded with something else, for example using code page
        ///   950 "Big-5 Chinese". To fill that need, DotNetZip will encode the
        ///   comment following the same procedure it follows for encoding
        ///   filenames: (a) if <see cref="AlternateEncodingUsage"/> is
        ///   <c>Never</c>, it uses the default encoding (IBM437). (b) if <see
        ///   cref="AlternateEncodingUsage"/> is <c>Always</c>, it always uses the
        ///   alternate encoding (<see cref="AlternateEncoding"/>). (c) if <see
        ///   cref="AlternateEncodingUsage"/> is <c>AsNecessary</c>, it uses the
        ///   alternate encoding only if the default encoding is not sufficient for
        ///   encoding the comment - in other words if decoding the result does not
        ///   produce the original string.  This decision is taken at the time of
        ///   the call to <c>ZipFile.Save()</c>.
        /// </para>
        ///
        /// <para>
        ///   When creating a zip archive using this library, it is possible to change
        ///   the value of <see cref="AlternateEncoding" /> between each
        ///   entry you add, and between adding entries and the call to
        ///   <c>Save()</c>. Don't do this.  It will likely result in a zip file that is
        ///   not readable by any tool or application.  For best interoperability, leave
        ///   <see cref="AlternateEncoding"/> alone, or specify it only
        ///   once, before adding any entries to the <c>ZipFile</c> instance.
        /// </para>
        ///
        /// </remarks>
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = value; }
        }


        /// <summary>
        ///   Returns true if an entry by the given name exists in the ZipFile.
        /// </summary>
        ///
        /// <param name='name'>the name of the entry to find</param>
        /// <returns>true if an entry with the given name exists; otherwise false.
        /// </returns>
        public bool ContainsEntry(string name)
        {
            // workitem 12534
            return _entries.ContainsKey(SharedUtilities.NormalizePathForUseInZipFile(name));
        }


        /// <summary>
        ///   Indicates whether to perform case-sensitive matching on the filename when
        ///   retrieving entries in the zipfile via the string-based indexer.
        /// </summary>
        ///
        /// <remarks>
        ///   The default value is <c>false</c>, which means don't do case-sensitive
        ///   matching. In other words, retrieving zip["ReadMe.Txt"] is the same as
        ///   zip["readme.txt"].  It really makes sense to set this to <c>true</c> only
        ///   if you are not running on Windows, which has case-insensitive
        ///   filenames. But since this library is not built for non-Windows platforms,
        ///   in most cases you should just leave this property alone.
        /// </remarks>
        public bool CaseSensitiveRetrieval
        {
            get
            {
                return _CaseSensitiveRetrieval;
            }

            set
            {
                // workitem 9868
                if (value != _CaseSensitiveRetrieval)
                {
                    _CaseSensitiveRetrieval = value;
                    _initEntriesDictionary();
                }
            }
        }


        /// <summary>
        ///   Indicates whether the most recent <c>Read()</c> operation read a zip file that uses
        ///   ZIP64 extensions.
        /// </summary>
        ///
        /// <remarks>
        ///   This property will return null (Nothing in VB) if you've added an entry after reading
        ///   the zip file.
        /// </remarks>
        public Nullable<bool> InputUsesZip64
        {
            get
            {
                if (_entries.Count > 65534)
                    return true;

                foreach (ZipEntry e in this)
                {
                    // if any entry was added after reading the zip file, then the result is null
                    if (e.Source != ZipEntrySource.ZipFile) return null;

                    // if any entry read from the zip used zip64, then the result is true
                    if (e._InputUsesZip64) return true;
                }
                return false;
            }
        }


        /// <summary>
        ///   A Text Encoding to use when encoding the filenames and comments for
        ///   all the ZipEntry items, during a ZipFile.Save() operation.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Whether the encoding specified here is used during the save depends
        ///     on <see cref="AlternateEncodingUsage"/>.
        ///   </para>
        /// </remarks>
        public System.Text.Encoding AlternateEncoding
        {
            get
            {
                return _alternateEncoding;
            }
            set
            {
                _alternateEncoding = value;
            }
        }


        /// <summary>
        ///   A flag that tells if and when this instance should apply
        ///   AlternateEncoding to encode the filenames and comments associated to
        ///   of ZipEntry objects contained within this instance.
        /// </summary>
        public ZipOption AlternateEncodingUsage
        {
            get
            {
                return _alternateEncodingUsage;
            }
            set
            {
                _alternateEncodingUsage = value;
            }
        }


        /// <summary>
        /// The default text encoding used in zip archives.  It is numeric 437, also
        /// known as IBM437.
        /// </summary>
        /// <seealso cref="Ionic.Zip.ZipFile.AlternateEncoding"/>
        public static System.Text.Encoding DefaultEncoding
        {
            get
            {
                return _defaultEncoding;
            }
        }


        /// <summary>
        /// Sets the password to be used on the <c>ZipFile</c> instance.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   To use this property when reading or an
        ///   existing ZipFile, do the following: set the Password property on the
        ///   <c>ZipFile</c>, then call one of the Extract() overloads on the <see
        ///   cref="ZipEntry" />. In this case, the entry is extracted using the
        ///   <c>Password</c> that is specified on the <c>ZipFile</c> instance. If you
        ///   have not set the <c>Password</c> property, then the password is
        ///   <c>null</c>, and the entry is extracted with no password.
        /// </para>
        ///
        /// <para>
        ///   If you set the Password property on the <c>ZipFile</c>, then call
        ///   <c>Extract()</c> an entry that has not been encrypted with a password, the
        ///   password is not used for that entry, and the <c>ZipEntry</c> is extracted
        ///   as normal. In other words, the password is used only if necessary.
        /// </para>
        ///
        /// <para>
        ///   The <see cref="ZipEntry"/> class also has a <see
        ///   cref="ZipEntry.Password">Password</see> property.  It takes precedence
        ///   over this property on the <c>ZipFile</c>.  Typically, you would use the
        ///   per-entry Password when most entries in the zip archive use one password,
        ///   and a few entries use a different password.  If all entries in the zip
        ///   file use the same password, then it is simpler to just set this property
        ///   on the <c>ZipFile</c> itself, whether creating a zip archive or extracting
        ///   a zip archive.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        /// 
        /// <code>
        /// // extract entries that use encryption
        /// using (ZipFile zip = ZipFile.Read("EncryptedArchive.zip"))
        /// {
        ///     zip.Password= "!Secret1";
        ///     zip.ExtractAll("extractDir");
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// ' extract entries that use encryption
        /// Using (zip as ZipFile = ZipFile.Read("EncryptedArchive.zip"))
        ///     zip.Password= "!Secret1"
        ///     zip.ExtractAll("extractDir")
        /// End Using
        /// </code>
        /// 
        /// </example>
        ///
        /// <seealso cref="Ionic.Zip.ZipEntry.Password">ZipEntry.Password</seealso>
        public String Password
        {
          //private get { return _Password; }
          set { _Password = value; }
        }


        internal Stream StreamForDiskNumber(uint diskNumber)
        {
            if (diskNumber + 1 == this._diskNumberWithCd ||
                (diskNumber == 0 && this._diskNumberWithCd == 0))
            {
                return this.ReadStream;
            }

            throw new NotSupportedException("ZIP files that span multiple disk files are not supported.");
        }
        #endregion

        #region Constructors

        /// <summary>
        ///   Create a zip file, without specifying a target filename or stream to save to.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Instances of the <c>ZipFile</c> class are not multi-thread safe.  You may
        ///   have multiple threads that each use a distinct <c>ZipFile</c> instance, or
        ///   you can synchronize multi-thread access to a single instance.  </para>
        ///
        /// </remarks>
        private ZipFile()
        {
            _InitInstance();
        }


        private void _initEntriesDictionary()
        {
            // workitem 9868
            StringComparer sc = (CaseSensitiveRetrieval) ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            _entries = (_entries == null)
                ? new Dictionary<String, ZipEntry>(sc)
                : new Dictionary<String, ZipEntry>(_entries, sc);
        }


        private void _InitInstance()
        {
            // workitem 7685, 9868
            _initEntriesDictionary();
        }
        #endregion



        #region Indexers and Collections

        private List<ZipEntry> ZipEntriesAsList
        {
            get
            {
                if (_zipEntriesAsList == null)
                    _zipEntriesAsList = new List<ZipEntry>(_entries.Values);
                return _zipEntriesAsList;
            }
        }

        /// <summary>
        ///   This is an integer indexer into the Zip archive.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This property is read-only.
        /// </para>
        ///
        /// <para>
        ///   Internally, the <c>ZipEntry</c> instances that belong to the
        ///   <c>ZipFile</c> are stored in a Dictionary.  When you use this
        ///   indexer the first time, it creates a read-only
        ///   <c>List&lt;ZipEntry&gt;</c> from the Dictionary.Values Collection.
        ///   If at any time you modify the set of entries in the <c>ZipFile</c>,
        ///   either by adding an entry, removing an entry, or renaming an
        ///   entry, a new List will be created, and the numeric indexes for the
        ///   remaining entries may be different.
        /// </para>
        ///
        /// <para>
        ///   This means you cannot rename any ZipEntry from
        ///   inside an enumeration of the zip file.
        /// </para>
        ///
        /// <param name="ix">
        ///   The index value.
        /// </param>
        ///
        /// </remarks>
        ///
        /// <returns>
        ///   The <c>ZipEntry</c> within the Zip archive at the specified index. If the
        ///   entry does not exist in the archive, this indexer throws.
        /// </returns>
        ///
        public ZipEntry this[int ix]
        {
            // workitem 6402
            get
            {
                return ZipEntriesAsList[ix];
            }
        }


        /// <summary>
        ///   This is a name-based indexer into the Zip archive.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This property is read-only.
        /// </para>
        ///
        /// <para>
        ///   The <see cref="CaseSensitiveRetrieval"/> property on the <c>ZipFile</c>
        ///   determines whether retrieval via this indexer is done via case-sensitive
        ///   comparisons. By default, retrieval is not case sensitive.  This makes
        ///   sense on Windows, in which filesystems are not case sensitive.
        /// </para>
        ///
        /// <para>
        ///   Regardless of case-sensitivity, it is not always the case that
        ///   <c>this[value].FileName == value</c>. In other words, the <c>FileName</c>
        ///   property of the <c>ZipEntry</c> retrieved with this indexer, may or may
        ///   not be equal to the index value.
        /// </para>
        ///
        /// <para>
        ///   This is because DotNetZip performs a normalization of filenames passed to
        ///   this indexer, before attempting to retrieve the item.  That normalization
        ///   includes: removal of a volume letter and colon, swapping backward slashes
        ///   for forward slashes.  So, <c>zip["dir1\\entry1.txt"].FileName ==
        ///   "dir1/entry.txt"</c>.
        /// </para>
        ///
        /// <para>
        ///   Directory entries in the zip file may be retrieved via this indexer only
        ///   with names that have a trailing slash. DotNetZip automatically appends a
        ///   trailing slash to the names of any directory entries added to a zip.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        /// This example extracts only the entries in a zip file that are .txt files.
        /// <code>
        /// using (ZipFile zip = ZipFile.Read("PackedDocuments.zip"))
        /// {
        ///   foreach (string s1 in zip.EntryFilenames)
        ///   {
        ///     if (s1.EndsWith(".txt"))
        ///       zip[s1].Extract("textfiles");
        ///   }
        /// }
        /// </code>
        /// <code lang="VB">
        ///   Using zip As ZipFile = ZipFile.Read("PackedDocuments.zip")
        ///       Dim s1 As String
        ///       For Each s1 In zip.EntryFilenames
        ///           If s1.EndsWith(".txt") Then
        ///               zip(s1).Extract("textfiles")
        ///           End If
        ///       Next
        ///   End Using
        /// </code>
        /// </example>
        ///
        /// <exception cref="System.ArgumentException">
        ///   Thrown if the caller attempts to assign a non-null value to the indexer.
        /// </exception>
        ///
        /// <param name="fileName">
        ///   The name of the file, including any directory path, to retrieve from the
        ///   zip.  The filename match is not case-sensitive by default; you can use the
        ///   <see cref="CaseSensitiveRetrieval"/> property to change this behavior. The
        ///   pathname can use forward-slashes or backward slashes.
        /// </param>
        ///
        /// <returns>
        ///   The <c>ZipEntry</c> within the Zip archive, given by the specified
        ///   filename. If the named entry does not exist in the archive, this indexer
        ///   returns <c>null</c> (<c>Nothing</c> in VB).
        /// </returns>
        ///
        public ZipEntry this[String fileName]
        {
            get
            {
                var key = SharedUtilities.NormalizePathForUseInZipFile(fileName);
                if (_entries.ContainsKey(key))
                    return _entries[key];
                // workitem 11056
                key = key.Replace("/", "\\");
                if (_entries.ContainsKey(key))
                    return _entries[key];
                return null;
            }
        }


        /// <summary>
        ///   The list of filenames for the entries contained within the zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///   According to the ZIP specification, the names of the entries use forward
        ///   slashes in pathnames.  If you are scanning through the list, you may have
        ///   to swap forward slashes for backslashes.
        /// </remarks>
        ///
        /// <seealso cref="Ionic.Zip.ZipFile.this[string]"/>
        ///
        /// <example>
        ///   This example shows one way to test if a filename is already contained
        ///   within a zip archive.
        /// <code>
        /// String zipFileToRead= "PackedDocuments.zip";
        /// string candidate = "DatedMaterial.xps";
        /// using (ZipFile zip = new ZipFile(zipFileToRead))
        /// {
        ///   if (zip.EntryFilenames.Contains(candidate))
        ///     Console.WriteLine("The file '{0}' exists in the zip archive '{1}'",
        ///                       candidate,
        ///                       zipFileName);
        ///   else
        ///     Console.WriteLine("The file, '{0}', does not exist in the zip archive '{1}'",
        ///                       candidate,
        ///                       zipFileName);
        ///   Console.WriteLine();
        /// }
        /// </code>
        /// <code lang="VB">
        ///   Dim zipFileToRead As String = "PackedDocuments.zip"
        ///   Dim candidate As String = "DatedMaterial.xps"
        ///   Using zip As ZipFile.Read(ZipFileToRead)
        ///       If zip.EntryFilenames.Contains(candidate) Then
        ///           Console.WriteLine("The file '{0}' exists in the zip archive '{1}'", _
        ///                       candidate, _
        ///                       zipFileName)
        ///       Else
        ///         Console.WriteLine("The file, '{0}', does not exist in the zip archive '{1}'", _
        ///                       candidate, _
        ///                       zipFileName)
        ///       End If
        ///       Console.WriteLine
        ///   End Using
        /// </code>
        /// </example>
        ///
        /// <returns>
        ///   The list of strings for the filenames contained within the Zip archive.
        /// </returns>
        ///
        public System.Collections.Generic.ICollection<String> EntryFileNames
        {
            get
            {
                return _entries.Keys;
            }
        }


        /// <summary>
        ///   Returns the readonly collection of entries in the Zip archive.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   If there are no entries in the current <c>ZipFile</c>, the value returned is a
        ///   non-null zero-element collection.  If there are entries in the zip file,
        ///   the elements are returned in no particular order.
        /// </para>
        /// <para>
        ///   This is the implied enumerator on the <c>ZipFile</c> class.  If you use a
        ///   <c>ZipFile</c> instance in a context that expects an enumerator, you will
        ///   get this collection.
        /// </para>
        /// </remarks>
        /// <seealso cref="EntriesSorted"/>
        public System.Collections.Generic.ICollection<ZipEntry> Entries
        {
            get
            {
                return _entries.Values;
            }
        }


        /// <summary>
        ///   Returns a readonly collection of entries in the Zip archive, sorted by FileName.
        /// </summary>
        ///
        /// <remarks>
        ///   If there are no entries in the current <c>ZipFile</c>, the value returned
        ///   is a non-null zero-element collection.  If there are entries in the zip
        ///   file, the elements are returned sorted by the name of the entry.
        /// </remarks>
        ///
        /// <example>
        ///
        ///   This example fills a Windows Forms ListView with the entries in a zip file.
        ///
        /// <code lang="C#">
        /// using (ZipFile zip = ZipFile.Read(zipFile))
        /// {
        ///     foreach (ZipEntry entry in zip.EntriesSorted)
        ///     {
        ///         ListViewItem item = new ListViewItem(n.ToString());
        ///         n++;
        ///         string[] subitems = new string[] {
        ///             entry.FileName.Replace("/","\\"),
        ///             entry.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
        ///             entry.UncompressedSize.ToString(),
        ///             String.Format("{0,5:F0}%", entry.CompressionRatio),
        ///             entry.CompressedSize.ToString(),
        ///             (entry.UsesEncryption) ? "Y" : "N",
        ///             String.Format("{0:X8}", entry.Crc)};
        ///
        ///         foreach (String s in subitems)
        ///         {
        ///             ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem();
        ///             subitem.Text = s;
        ///             item.SubItems.Add(subitem);
        ///         }
        ///
        ///         this.listView1.Items.Add(item);
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// <seealso cref="Entries"/>
        public System.Collections.Generic.ICollection<ZipEntry> EntriesSorted
        {
            get
            {
                var coll = new System.Collections.Generic.List<ZipEntry>();
                foreach (var e in this.Entries)
                {
                    coll.Add(e);
                }
                StringComparison sc = (CaseSensitiveRetrieval) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                coll.Sort((x, y) => { return String.Compare(x.FileName, y.FileName, sc); });
#if NETFX_CORE || PORTABLE
                return new ReadOnlyCollection<ZipEntry>(coll);
#else
                return coll.AsReadOnly();
#endif
            }
        }


        /// <summary>
        /// Returns the number of entries in the Zip archive.
        /// </summary>
        public int Count
        {
            get
            {
                return _entries.Count;
            }
        }

        #endregion

        #region Destructors and Disposers

        /// <summary>
        ///   Closes the read and write streams associated
        ///   to the <c>ZipFile</c>, if necessary.
        /// </summary>
        ///
        /// <remarks>
        ///   The Dispose() method is generally employed implicitly, via a <c>using(..) {..}</c>
        ///   statement. (<c>Using...End Using</c> in VB) If you do not employ a using
        ///   statement, insure that your application calls Dispose() explicitly.  For
        ///   example, in a Powershell application, or an application that uses the COM
        ///   interop interface, you must call Dispose() explicitly.
        /// </remarks>
        ///
        /// <example>
        /// This example extracts an entry selected by name, from the Zip file to the
        /// Console.
        /// <code>
        /// using (ZipFile zip = ZipFile.Read(zipfile))
        /// {
        ///   foreach (ZipEntry e in zip)
        ///   {
        ///     if (WantThisEntry(e.FileName))
        ///       zip.Extract(e.FileName, Console.OpenStandardOutput());
        ///   }
        /// } // Dispose() is called implicitly here.
        /// </code>
        ///
        /// <code lang="VB">
        /// Using zip As ZipFile = ZipFile.Read(zipfile)
        ///     Dim e As ZipEntry
        ///     For Each e In zip
        ///       If WantThisEntry(e.FileName) Then
        ///           zip.Extract(e.FileName, Console.OpenStandardOutput())
        ///       End If
        ///     Next
        /// End Using ' Dispose is implicity called here
        /// </code>
        /// </example>
        public void Dispose()
        {
            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Disposes any managed resources, if the flag is set, then marks the
        ///   instance disposed.  This method is typically not called explicitly from
        ///   application code.
        /// </summary>
        ///
        /// <remarks>
        ///   Applications should call <see cref="Dispose()">the no-arg Dispose method</see>.
        /// </remarks>
        ///
        /// <param name="disposeManagedResources">
        ///   indicates whether the method should dispose streams or not.
        /// </param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (!this._disposed)
            {
                if (disposeManagedResources)
                {
                    // dispose managed resources
                }

                this._disposed = true;
            }
        }
        #endregion


        #region private properties

        internal Stream ReadStream
        {
            get { return _readstream; }
        }


        #endregion

        #region private fields
        private bool _CaseSensitiveRetrieval;
        private Stream _readstream;
        private UInt32 _diskNumberWithCd;
        private bool _disposed;
        //private System.Collections.Generic.List<ZipEntry> _entries;
        private System.Collections.Generic.Dictionary<String, ZipEntry> _entries;
        private List<ZipEntry> _zipEntriesAsList;
        private string _Comment;
        internal string _Password;
        private long _locEndOfCDS = -1;
#if !WINDOWS
        // See https://dotnetzip.codeplex.com/workitem/14049
        private System.Text.Encoding _alternateEncoding = System.Text.Encoding.GetEncoding("UTF-8");
#else
        private System.Text.Encoding _alternateEncoding = System.Text.Encoding.GetEncoding("IBM437"); // UTF-8
#endif
        private ZipOption _alternateEncodingUsage = ZipOption.Never;
#if !WINDOWS
        // See https://dotnetzip.codeplex.com/workitem/14049
        private static System.Text.Encoding _defaultEncoding = System.Text.Encoding.GetEncoding("UTF-8");
#else
        private static System.Text.Encoding _defaultEncoding = System.Text.Encoding.GetEncoding("IBM437");
#endif

        private int _BufferSize = BufferSizeDefault;

        internal Zip64Option _zip64 = Zip64Option.Default;

        /// <summary>
        ///   Default size of the buffer used for IO.
        /// </summary>
        public static readonly int BufferSizeDefault = 32768;

        #endregion
    }

    /// <summary>
    ///   Options for using ZIP64 extensions when saving zip archives.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
    ///   Designed many years ago, the <see
    ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">original zip
    ///   specification from PKWARE</see> allowed for 32-bit quantities for the
    ///   compressed and uncompressed sizes of zip entries, as well as a 32-bit quantity
    ///   for specifying the length of the zip archive itself, and a maximum of 65535
    ///   entries.  These limits are now regularly exceeded in many backup and archival
    ///   scenarios.  Recently, PKWare added extensions to the original zip spec, called
    ///   "ZIP64 extensions", to raise those limitations.  This property governs whether
    ///   DotNetZip will use those extensions when writing zip archives. The use of
    ///   these extensions is optional and explicit in DotNetZip because, despite the
    ///   status of ZIP64 as a bona fide standard, many other zip tools and libraries do
    ///   not support ZIP64, and therefore a zip file with ZIP64 extensions may be
    ///   unreadable by some of those other tools.
    /// </para>
    ///
    /// <para>
    ///   Set this property to <see cref="Zip64Option.Always"/> to always use ZIP64
    ///   extensions when saving, regardless of whether your zip archive needs it.
    ///   Suppose you add 5 files, each under 100k, to a ZipFile. If you specify Always
    ///   for this flag, you will get a ZIP64 archive, though the archive does not need
    ///   to use ZIP64 because none of the original zip limits had been exceeded.
    /// </para>
    ///
    /// <para>
    ///   Set this property to <see cref="Zip64Option.Never"/> to tell the DotNetZip
    ///   library to never use ZIP64 extensions.  This is useful for maximum
    ///   compatibility and interoperability, at the expense of the capability of
    ///   handling large files or large archives.  NB: Windows Explorer in Windows XP
    ///   and Windows Vista cannot currently extract files from a zip64 archive, so if
    ///   you want to guarantee that a zip archive produced by this library will work in
    ///   Windows Explorer, use <c>Never</c>. If you set this property to <see
    ///   cref="Zip64Option.Never"/>, and your application creates a zip that would
    ///   exceed one of the Zip limits, the library will throw an exception while saving
    ///   the zip file.
    /// </para>
    ///
    /// <para>
    ///   Set this property to <see cref="Zip64Option.AsNecessary"/> to tell the
    ///   DotNetZip library to use the ZIP64 extensions when required by the
    ///   entry. After the file is compressed, the original and compressed sizes are
    ///   checked, and if they exceed the limits described above, then zip64 can be
    ///   used. That is the general idea, but there is an additional wrinkle when saving
    ///   to a non-seekable device, like the ASP.NET <c>Response.OutputStream</c>, or
    ///   <c>Console.Out</c>.  When using non-seekable streams for output, the entry
    ///   header - which indicates whether zip64 is in use - is emitted before it is
    ///   known if zip64 is necessary.  It is only after all entries have been saved
    ///   that it can be known if ZIP64 will be required.  On seekable output streams,
    ///   after saving all entries, the library can seek backward and re-emit the zip
    ///   file header to be consistent with the actual ZIP64 requirement.  But using a
    ///   non-seekable output stream, the library cannot seek backward, so the header
    ///   can never be changed. In other words, the archive's use of ZIP64 extensions is
    ///   not alterable after the header is emitted.  Therefore, when saving to
    ///   non-seekable streams, using <see cref="Zip64Option.AsNecessary"/> is the same
    ///   as using <see cref="Zip64Option.Always"/>: it will always produce a zip
    ///   archive that uses ZIP64 extensions.
    /// </para>
    ///
    /// </remarks>
    internal enum Zip64Option
    {
        /// <summary>
        /// The default behavior, which is "Never".
        /// (For COM clients, this is a 0 (zero).)
        /// </summary>
        Default = 0,
        /// <summary>
        /// Do not use ZIP64 extensions when writing zip archives.
        /// (For COM clients, this is a 0 (zero).)
        /// </summary>
        Never = 0,
        /// <summary>
        /// Use ZIP64 extensions when writing zip archives, as necessary.
        /// For example, when a single entry exceeds 0xFFFFFFFF in size, or when the archive as a whole
        /// exceeds 0xFFFFFFFF in size, or when there are more than 65535 entries in an archive.
        /// (For COM clients, this is a 1.)
        /// </summary>
        AsNecessary = 1,
        /// <summary>
        /// Always use ZIP64 extensions when writing zip archives, even when unnecessary.
        /// (For COM clients, this is a 2.)
        /// </summary>
        Always
    }


    /// <summary>
    ///  An enum representing the values on a three-way toggle switch
    ///  for various options in the library. This might be used to
    ///  specify whether to employ a particular text encoding, or to use
    ///  ZIP64 extensions, or some other option.
    /// </summary>
    internal enum ZipOption
    {
        /// <summary>
        /// The default behavior. This is the same as "Never".
        /// (For COM clients, this is a 0 (zero).)
        /// </summary>
        Default = 0,
        /// <summary>
        /// Never use the associated option.
        /// (For COM clients, this is a 0 (zero).)
        /// </summary>
        Never = 0,
        /// <summary>
        /// Use the associated behavior "as necessary."
        /// (For COM clients, this is a 1.)
        /// </summary>
        AsNecessary = 1,
        /// <summary>
        /// Use the associated behavior Always, whether necessary or not.
        /// (For COM clients, this is a 2.)
        /// </summary>
        Always
    }


}



// ==================================================================
//
// Information on the ZIP format:
//
// From
// http://www.pkware.com/documents/casestudies/APPNOTE.TXT
//
//  Overall .ZIP file format:
//
//     [local file header 1]
//     [file data 1]
//     [data descriptor 1]  ** sometimes
//     .
//     .
//     .
//     [local file header n]
//     [file data n]
//     [data descriptor n]   ** sometimes
//     [archive decryption header]
//     [archive extra data record]
//     [central directory]
//     [zip64 end of central directory record]
//     [zip64 end of central directory locator]
//     [end of central directory record]
//
// Local File Header format:
//         local file header signature ... 4 bytes  (0x04034b50)
//         version needed to extract ..... 2 bytes
//         general purpose bit field ..... 2 bytes
//         compression method ............ 2 bytes
//         last mod file time ............ 2 bytes
//         last mod file date............. 2 bytes
//         crc-32 ........................ 4 bytes
//         compressed size................ 4 bytes
//         uncompressed size.............. 4 bytes
//         file name length............... 2 bytes
//         extra field length ............ 2 bytes
//         file name                       varies
//         extra field                     varies
//
//
// Data descriptor:  (used only when bit 3 of the general purpose bitfield is set)
//         (although, I have found zip files where bit 3 is not set, yet this descriptor is present!)
//         local file header signature     4 bytes  (0x08074b50)  ** sometimes!!! Not always
//         crc-32                          4 bytes
//         compressed size                 4 bytes
//         uncompressed size               4 bytes
//
//
//   Central directory structure:
//
//       [file header 1]
//       .
//       .
//       .
//       [file header n]
//       [digital signature]
//
//
//       File header:  (This is a ZipDirEntry)
//         central file header signature   4 bytes  (0x02014b50)
//         version made by                 2 bytes
//         version needed to extract       2 bytes
//         general purpose bit flag        2 bytes
//         compression method              2 bytes
//         last mod file time              2 bytes
//         last mod file date              2 bytes
//         crc-32                          4 bytes
//         compressed size                 4 bytes
//         uncompressed size               4 bytes
//         file name length                2 bytes
//         extra field length              2 bytes
//         file comment length             2 bytes
//         disk number start               2 bytes
//         internal file attributes **     2 bytes
//         external file attributes ***    4 bytes
//         relative offset of local header 4 bytes
//         file name (variable size)
//         extra field (variable size)
//         file comment (variable size)
//
// ** The internal file attributes, near as I can tell,
// uses 0x01 for a file and a 0x00 for a directory.
//
// ***The external file attributes follows the MS-DOS file attribute byte, described here:
// at http://support.microsoft.com/kb/q125019/
// 0x0010 => directory
// 0x0020 => file
//
//
// End of central directory record:
//
//         end of central dir signature    4 bytes  (0x06054b50)
//         number of this disk             2 bytes
//         number of the disk with the
//         start of the central directory  2 bytes
//         total number of entries in the
//         central directory on this disk  2 bytes
//         total number of entries in
//         the central directory           2 bytes
//         size of the central directory   4 bytes
//         offset of start of central
//         directory with respect to
//         the starting disk number        4 bytes
//         .ZIP file comment length        2 bytes
//         .ZIP file comment       (variable size)
//
// date and time are packed values, as MSDOS did them
// time: bits 0-4 : seconds (divided by 2)
//            5-10: minute
//            11-15: hour
// date  bits 0-4 : day
//            5-8: month
//            9-15 year (since 1980)
//
// see http://msdn.microsoft.com/en-us/library/ms724274(VS.85).aspx

