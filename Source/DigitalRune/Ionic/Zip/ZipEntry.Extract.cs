// ZipEntry.Extract.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa
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

#if PORTABLE
#pragma warning disable 1574  // Disable warning "reference not found in XML comments"
#endif


namespace DigitalRune.Ionic.Zip
{

    partial class ZipEntry
    {
        /// <summary>
        ///   Extracts the entry to the specified stream.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The caller can specify any write-able stream, for example a <see
        ///   cref="System.IO.FileStream"/>, a <see
        ///   cref="System.IO.MemoryStream"/>, or ASP.NET's
        ///   <c>Response.OutputStream</c>.  The content will be decrypted and
        ///   decompressed as necessary. If the entry is encrypted and no password
        ///   is provided, this method will throw.
        /// </para>
        /// <para>
        ///   The position on the stream is not reset by this method before it extracts.
        ///   You may want to call stream.Seek() before calling ZipEntry.Extract().
        /// </para>
        /// </remarks>
        ///
        /// <param name="stream">
        ///   the stream to which the entry should be extracted.
        /// </param>
        ///
        public void Extract(Stream stream)
        {
            InternalExtractToStream(stream, null, _container, _Source, FileName);
        }


        /// <summary>
        ///   Extracts the entry to the specified stream, using the specified
        ///   Password.  For example, the caller could extract to Console.Out, or
        ///   to a MemoryStream.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The caller can specify any write-able stream, for example a <see
        ///   cref="System.IO.FileStream"/>, a <see
        ///   cref="System.IO.MemoryStream"/>, or ASP.NET's
        ///   <c>Response.OutputStream</c>.  The content will be decrypted and
        ///   decompressed as necessary. If the entry is encrypted and no password
        ///   is provided, this method will throw.
        /// </para>
        /// <para>
        ///   The position on the stream is not reset by this method before it extracts.
        ///   You may want to call stream.Seek() before calling ZipEntry.Extract().
        /// </para>
        /// </remarks>
        ///
        ///
        /// <param name="stream">
        ///   the stream to which the entry should be extracted.
        /// </param>
        /// <param name="password">
        ///   The password to use for decrypting the entry.
        /// </param>
        public void ExtractWithPassword(Stream stream, string password)
        {
            InternalExtractToStream(stream, password, _container, _Source, FileName);
        }


        /// <summary>
        ///   Opens a readable stream corresponding to the zip entry in the
        ///   archive.  The stream decompresses and decrypts as necessary, as it
        ///   is read.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   DotNetZip offers a variety of ways to extract entries from a zip
        ///   file.  This method allows an application to extract an entry by
        ///   reading a <see cref="System.IO.Stream"/>.
        /// </para>
        ///
        /// <para>
        ///   The return value is of type <see
        ///   cref="Ionic.Crc.CrcCalculatorStream"/>.  Use it as you would any
        ///   stream for reading.  When an application calls <see
        ///   cref="Stream.Read(byte[], int, int)"/> on that stream, it will
        ///   receive data from the zip entry that is decrypted and decompressed
        ///   as necessary.
        /// </para>
        ///
        /// <para>
        ///   <c>CrcCalculatorStream</c> adds one additional feature: it keeps a
        ///   CRC32 checksum on the bytes of the stream as it is read.  The CRC
        ///   value is available in the <see
        ///   cref="Ionic.Crc.CrcCalculatorStream.Crc"/> property on the
        ///   <c>CrcCalculatorStream</c>.  When the read is complete, your
        ///   application
        ///   <em>should</em> check this CRC against the <see cref="ZipEntry.Crc"/>
        ///   property on the <c>ZipEntry</c> to validate the content of the
        ///   ZipEntry. You don't have to validate the entry using the CRC, but
        ///   you should, to verify integrity. Check the example for how to do
        ///   this.
        /// </para>
        ///
        /// <para>
        ///   If the entry is protected with a password, then you need to provide
        ///   a password prior to calling <see cref="OpenReader()"/>, either by
        ///   setting the <see cref="Password"/> property on the entry, or the
        ///   <see cref="ZipFile.Password"/> property on the <c>ZipFile</c>
        ///   itself. Or, you can use <see cref="OpenReader(String)" />, the
        ///   overload of OpenReader that accepts a password parameter.
        /// </para>
        ///
        /// <para>
        ///   If you want to extract entry data into a write-able stream that is
        ///   already opened, like a <see cref="System.IO.FileStream"/>, do not
        ///   use this method. Instead, use <see cref="Extract(Stream)"/>.
        /// </para>
        ///
        /// <para>
        ///   Your application may use only one stream created by OpenReader() at
        ///   a time, and you should not call other Extract methods before
        ///   completing your reads on a stream obtained from OpenReader().  This
        ///   is because there is really only one source stream for the compressed
        ///   content.  A call to OpenReader() seeks in the source stream, to the
        ///   beginning of the compressed content.  A subsequent call to
        ///   OpenReader() on a different entry will seek to a different position
        ///   in the source stream, as will a call to Extract() or one of its
        ///   overloads.  This will corrupt the state for the decompressing stream
        ///   from the original call to OpenReader().
        /// </para>
        /// </remarks>
        ///
        /// <example>
        ///   This example shows how to open a zip archive, then read in a named
        ///   entry via a stream. After the read loop is complete, the code
        ///   compares the calculated during the read loop with the expected CRC
        ///   on the <c>ZipEntry</c>, to verify the extraction.
        /// <code>
        /// using (ZipFile zip = new ZipFile(ZipFileToRead))
        /// {
        ///   ZipEntry e1= zip["Elevation.mp3"];
        ///   using (Ionic.Zlib.CrcCalculatorStream s = e1.OpenReader())
        ///   {
        ///     byte[] buffer = new byte[4096];
        ///     int n, totalBytesRead= 0;
        ///     do {
        ///       n = s.Read(buffer,0, buffer.Length);
        ///       totalBytesRead+=n;
        ///     } while (n&gt;0);
        ///      if (s.Crc32 != e1.Crc32)
        ///       throw new Exception(string.Format("The Zip Entry failed the CRC Check. (0x{0:X8}!=0x{1:X8})", s.Crc32, e1.Crc32));
        ///      if (totalBytesRead != e1.UncompressedSize)
        ///       throw new Exception(string.Format("We read an unexpected number of bytes. ({0}!={1})", totalBytesRead, e1.UncompressedSize));
        ///   }
        /// }
        /// </code>
        /// <code lang="VB">
        ///   Using zip As New ZipFile(ZipFileToRead)
        ///       Dim e1 As ZipEntry = zip.Item("Elevation.mp3")
        ///       Using s As Ionic.Zlib.CrcCalculatorStream = e1.OpenReader
        ///           Dim n As Integer
        ///           Dim buffer As Byte() = New Byte(4096) {}
        ///           Dim totalBytesRead As Integer = 0
        ///           Do
        ///               n = s.Read(buffer, 0, buffer.Length)
        ///               totalBytesRead = (totalBytesRead + n)
        ///           Loop While (n &gt; 0)
        ///           If (s.Crc32 &lt;&gt; e1.Crc32) Then
        ///               Throw New Exception(String.Format("The Zip Entry failed the CRC Check. (0x{0:X8}!=0x{1:X8})", s.Crc32, e1.Crc32))
        ///           End If
        ///           If (totalBytesRead &lt;&gt; e1.UncompressedSize) Then
        ///               Throw New Exception(String.Format("We read an unexpected number of bytes. ({0}!={1})", totalBytesRead, e1.UncompressedSize))
        ///           End If
        ///       End Using
        ///   End Using
        /// </code>
        /// </example>
        /// <seealso cref="Ionic.Zip.ZipEntry.Extract(System.IO.Stream)"/>
        /// <returns>The Stream for reading.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public Crc.CrcCalculatorStream OpenReader()
        {
            // workitem 10923
            if (_container.ZipFile == null)
                throw new InvalidOperationException("Use OpenReader() only with ZipFile.");

            // use the entry password if it is non-null,
            // else use the zipfile password, which is possibly null
            return InternalOpenReader(_Password ?? _container.Password);
        }

        /// <summary>
        ///   Opens a readable stream for an encrypted zip entry in the archive.
        ///   The stream decompresses and decrypts as necessary, as it is read.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   See the documentation on the <see cref="OpenReader()"/> method for
        ///   full details. This overload allows the application to specify a
        ///   password for the <c>ZipEntry</c> to be read.
        /// </para>
        /// </remarks>
        ///
        /// <param name="password">The password to use for decrypting the entry.</param>
        /// <returns>The Stream for reading.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public Crc.CrcCalculatorStream OpenReader(string password)
        {
            // workitem 10923
            if (_container.ZipFile == null)
                throw new InvalidOperationException("Use OpenReader() only with ZipFile.");

            return InternalOpenReader(password);
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        internal Crc.CrcCalculatorStream InternalOpenReader(string password)
        {
            ValidateCompression(_CompressionMethod_FromZipFile, FileName, GetUnsupportedCompressionMethod(_CompressionMethod));
            ValidateEncryption(Encryption, FileName, _UnsupportedAlgorithmId);
            SetupCryptoForExtract(password);

            // workitem 7958
            if (this._Source != ZipEntrySource.ZipFile)
                throw new BadStateException("You must call ZipFile.Save before calling OpenReader");

            // LeftToRead is a count of bytes remaining to be read (out)
            // from the stream AFTER decompression and decryption.
            // It is the uncompressed size, unless ... there is no compression in which
            // case ...?  :< I'm not sure why it's not always UncompressedSize
            var leftToRead = (_CompressionMethod_FromZipFile == (short)CompressionMethod.None)
                ? _CompressedFileDataSize
                : UncompressedSize;

            this.ArchiveStream.Seek(this.FileDataPosition, SeekOrigin.Begin);

            _inputDecryptorStream = GetExtractDecryptor(ArchiveStream);
            var input3 = GetExtractDecompressor(_inputDecryptorStream);

            return new Crc.CrcCalculatorStream(input3, leftToRead);
        }


        /// <summary>
        /// Extract to a stream
        /// In other words, you can extract to a stream or to a directory (filesystem), but not both!
        /// The Password param is required for encrypted entries.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void InternalExtractToStream(Stream outStream, string password, ZipContainer zipContainer, ZipEntrySource zipEntrySource, string fileName)
        {
            // workitem 7958
            if (zipContainer == null)
                throw new BadStateException("This entry is an orphan");

            // workitem 10355
            if (zipContainer.ZipFile == null)
                throw new InvalidOperationException("Use Extract() only with ZipFile.");

            if (zipEntrySource != ZipEntrySource.ZipFile)
                throw new BadStateException("You must call ZipFile.Save before calling any Extract method");

            _ioOperationCanceled = false;

            try
            {
                ValidateCompression(_CompressionMethod_FromZipFile, fileName, GetUnsupportedCompressionMethod(_CompressionMethod));
                ValidateEncryption(Encryption, fileName, _UnsupportedAlgorithmId);

                if (IsDoneWithOutputToStream())
                {
                    return;
                }

                // If no password explicitly specified, use the password on the entry itself,
                // or on the zipfile itself.
                if (_Encryption_FromZipFile != EncryptionAlgorithm.None)
                    EnsurePassword(password);

                if (ExtractToStream(ArchiveStream, outStream, Encryption, _Crc32))
                    goto ExitTry;

                ExitTry: ;
            }
            catch (Exception)
            {
                _ioOperationCanceled = true;
                throw;
            }
        }


        bool ExtractToStream(Stream archiveStream, Stream output, EncryptionAlgorithm encryptionAlgorithm, int expectedCrc32)
        {
            if (_ioOperationCanceled)
                return true;

            var calculatedCrc32 = ExtractAndCrc(archiveStream, output,
                _CompressionMethod_FromZipFile, _CompressedFileDataSize,
                UncompressedSize);

            if (_ioOperationCanceled)
                return true;

            VerifyCrcAfterExtract(calculatedCrc32, encryptionAlgorithm, expectedCrc32, archiveStream, UncompressedSize);
            return false;
        }


        void EnsurePassword(string password)
        {
            var p = password ?? _Password ?? _container.Password;
            if (p == null) throw new BadPasswordException();
            SetupCryptoForExtract(p);
        }



        internal void VerifyCrcAfterExtract(Int32 calculatedCrc32, EncryptionAlgorithm encryptionAlgorithm, int expectedCrc32, Stream archiveStream, long uncompressedSize)
        {
#if AESCRYPTO
            // After extracting, Validate the CRC32
            if (calculatedCrc32 != expectedCrc32)
            {
                // CRC is not meaningful with WinZipAES and AES method 2 (AE-2)
                if ((encryptionAlgorithm != EncryptionAlgorithm.WinZipAes128 &&
                     encryptionAlgorithm != EncryptionAlgorithm.WinZipAes256)
                    || _WinZipAesMethod != 0x02)
                    throw new BadCrcException("CRC error: the file being extracted appears to be corrupted. " +
                                              String.Format(CultureInfo.InvariantCulture, "Expected 0x{0:X8}, Actual 0x{1:X8}", expectedCrc32, calculatedCrc32));
            }

            // ignore MAC if the size of the file is zero
            if (uncompressedSize == 0)
                return;

            // calculate the MAC
            if (encryptionAlgorithm == EncryptionAlgorithm.WinZipAes128 ||
                encryptionAlgorithm == EncryptionAlgorithm.WinZipAes256)
            {
                var wzs = _inputDecryptorStream as WinZipAesCipherStream;
                _aesCrypto_forExtract.CalculatedMac = wzs.FinalAuthentication;

                _aesCrypto_forExtract.ReadAndVerifyMac(archiveStream); // throws if MAC is bad
                // side effect: advances file position.
            }
#else
            if (calculatedCrc32 != expectedCrc32)
                throw new BadCrcException("CRC error: the file being extracted appears to be corrupted. " +
                                          String.Format(CultureInfo.InvariantCulture, "Expected 0x{0:X8}, Actual 0x{1:X8}", expectedCrc32, calculatedCrc32));
#endif
        }


        private void _CheckRead(int nbytes)
        {
            if (nbytes == 0)
                throw new BadReadException(String.Format(CultureInfo.InvariantCulture,
                                           "bad read of entry {0} from compressed archive.",
                                           FileName));
        }


        private Stream _inputDecryptorStream;

        int ExtractAndCrc(Stream archiveStream, Stream targetOutput,
            short compressionMethod,
            long compressedFileDataSize,
            long uncompressedSize)
        {
            int crcResult;
            var input = archiveStream;

            // change for workitem 8098
            input.Seek(FileDataPosition, SeekOrigin.Begin);

            var bytes = new byte[BufferSize];

            // The extraction process varies depending on how the entry was
            // stored.  It could have been encrypted, and it coould have
            // been compressed, or both, or neither. So we need to check
            // both the encryption flag and the compression flag, and take
            // the proper action in all cases.

            var leftToRead = (compressionMethod != (short)CompressionMethod.None)
                ? uncompressedSize
                : compressedFileDataSize;

            // Get a stream that either decrypts or not.
            _inputDecryptorStream = GetExtractDecryptor(input);

            var input3 = GetExtractDecompressor( _inputDecryptorStream );

            var bytesWritten = 0L;
            // As we read, we maybe decrypt, and then we maybe decompress. Then we write.
            using (var s1 = new Crc.CrcCalculatorStream(input3))
            {
                while (leftToRead > 0)
                {
                    //Console.WriteLine("ExtractOne: LeftToRead {0}", LeftToRead);

                    // Casting LeftToRead down to an int is ok here in the else clause,
                    // because that only happens when it is less than bytes.Length,
                    // which is much less than MAX_INT.
                    int len = (leftToRead > bytes.Length) ? bytes.Length : (int)leftToRead;
                    int n = s1.Read(bytes, 0, len);

                    // must check data read - essential for detecting corrupt zip files
                    _CheckRead(n);

                    targetOutput.Write(bytes, 0, n);
                    leftToRead -= n;
                    bytesWritten += n;

                    if (_ioOperationCanceled)
                        break;
                }

                crcResult = s1.Crc;
            }

            return crcResult;
        }


        internal Stream GetExtractDecompressor(Stream input2)
        {
            // get a stream that either decompresses or not.
            switch (_CompressionMethod_FromZipFile)
            {
                case (short)CompressionMethod.None:
                    return input2;
                case (short)CompressionMethod.Deflate:
                    return new Zlib.DeflateStream(input2, true);
#if BZIP
                case (short)CompressionMethod.BZip2:
                    return new BZip2.BZip2InputStream(input2, true);
#endif
            }

            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                                            "Failed to find decompressor matching {0}",
                                            _CompressionMethod_FromZipFile));
        }


        internal Stream GetExtractDecryptor(Stream input)
        {
            Stream input2;
            if (_Encryption_FromZipFile == EncryptionAlgorithm.PkzipWeak)
                input2 = new ZipCipherStream(input, _zipCrypto_forExtract);

#if AESCRYPTO
            else if (_Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes128 ||
                _Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes256)
                input2 = new WinZipAesCipherStream(input, _aesCrypto_forExtract, _CompressedFileDataSize);
#endif

            else
                input2 = input;

            return input2;
        }




        #region Support methods


        // workitem 7968
        static string GetUnsupportedAlgorithm(uint unsupportedAlgorithmId)
        {
            string alg;
            switch (unsupportedAlgorithmId)
            {
                case 0:
                    alg = "--";
                    break;
                case 0x6601:
                    alg = "DES";
                    break;
                case 0x6602: // - RC2 (version needed to extract < 5.2)
                    alg = "RC2";
                    break;
                case 0x6603: // - 3DES 168
                    alg = "3DES-168";
                    break;
                case 0x6609: // - 3DES 112
                    alg = "3DES-112";
                    break;
                case 0x660E: // - AES 128
                    alg = "PKWare AES128";
                    break;
                case 0x660F: // - AES 192
                    alg = "PKWare AES192";
                    break;
                case 0x6610: // - AES 256
                    alg = "PKWare AES256";
                    break;
                case 0x6702: // - RC2 (version needed to extract >= 5.2)
                    alg = "RC2";
                    break;
                case 0x6720: // - Blowfish
                    alg = "Blowfish";
                    break;
                case 0x6721: // - Twofish
                    alg = "Twofish";
                    break;
                case 0x6801: // - RC4
                    alg = "RC4";
                    break;
                case 0xFFFF: // - Unknown algorithm
                default:
                    alg = String.Format(CultureInfo.InvariantCulture, "Unknown (0x{0:X4})", unsupportedAlgorithmId);
                    break;
            }
            return alg;
        }


        // workitem 7968
        static string GetUnsupportedCompressionMethod(short compressionMethod)
        {
            string meth;
            switch ((int)compressionMethod)
            {
                case 0:
                    meth = "Store";
                    break;
                case 1:
                    meth = "Shrink";
                    break;
                case 8:
                    meth = "DEFLATE";
                    break;
                case 9:
                    meth = "Deflate64";
                    break;
                case 12:
                    meth = "BZIP2"; // only if BZIP not compiled in
                    break;
                case 14:
                    meth = "LZMA";
                    break;
                case 19:
                    meth = "LZ77";
                    break;
                case 98:
                    meth = "PPMd";
                    break;
                default:
                    meth = String.Format(CultureInfo.InvariantCulture, "Unknown (0x{0:X4})", compressionMethod);
                    break;
            }
            return meth;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DotNetZip")]
        private static void ValidateEncryption(EncryptionAlgorithm encryptionAlgorithm, string fileName, uint unsupportedAlgorithmId)
        {
            if (encryptionAlgorithm != EncryptionAlgorithm.PkzipWeak &&
#if AESCRYPTO
                encryptionAlgorithm != EncryptionAlgorithm.WinZipAes128 &&
                encryptionAlgorithm != EncryptionAlgorithm.WinZipAes256 &&
#endif
                encryptionAlgorithm != EncryptionAlgorithm.None)
            {
                // workitem 7968
                if (unsupportedAlgorithmId != 0)
                    throw new ZipException(string.Format(CultureInfo.InvariantCulture, 
                                                         "Cannot extract: Entry {0} is encrypted with an algorithm not supported by DotNetZip: {1}",
                                                         fileName, GetUnsupportedAlgorithm(unsupportedAlgorithmId)));
                throw new ZipException(string.Format(CultureInfo.InvariantCulture, 
                                                     "Cannot extract: Entry {0} uses an unsupported encryption algorithm ({1:X2})",
                                                     fileName, (int)encryptionAlgorithm));
            }
        }

        static void ValidateCompression(short compressionMethod, string fileName, string compressionMethodName)
        {
            if ((compressionMethod != (short)CompressionMethod.None) &&
                (compressionMethod != (short)CompressionMethod.Deflate)
#if BZIP
                && (compressionMethod != (short)CompressionMethod.BZip2)
#endif
                )
                throw new ZipException(String.Format(CultureInfo.InvariantCulture,
                                                     "Entry {0} uses an unsupported compression method (0x{1:X2}, {2})",
                                                     fileName, compressionMethod, compressionMethodName));
        }


        private void SetupCryptoForExtract(string password)
        {
            //if (password == null) return;
            if (_Encryption_FromZipFile == EncryptionAlgorithm.None) return;

            if (_Encryption_FromZipFile == EncryptionAlgorithm.PkzipWeak)
            {
                if (password == null)
                    throw new ZipException("Missing password.");

                this.ArchiveStream.Seek(this.FileDataPosition - 12, SeekOrigin.Begin);
                _zipCrypto_forExtract = ZipCrypto.ForRead(password, this);
            }

#if AESCRYPTO
            else if (_Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes128 ||
                 _Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes256)
            {
                if (password == null)
                    throw new ZipException("Missing password.");

                // If we already have a WinZipAesCrypto object in place, use it.
                // It can be set up in the ReadDirEntry(), or during a previous Extract.
                if (_aesCrypto_forExtract != null)
                {
                    _aesCrypto_forExtract.Password = password;
                }
                else
                {
                    int sizeOfSaltAndPv = GetLengthOfCryptoHeaderBytes(_Encryption_FromZipFile);
                    this.ArchiveStream.Seek(this.FileDataPosition - sizeOfSaltAndPv, SeekOrigin.Begin);
                    int keystrength = GetKeyStrengthInBits(_Encryption_FromZipFile);
                    _aesCrypto_forExtract = WinZipAesCrypto.ReadFromStream(password, keystrength, this.ArchiveStream);
                }
            }
#endif
        }



        /// <summary>
        /// Validates that the args are consistent; returning whether the caller can return
        /// because it's done, or not (caller should continue)
        /// </summary>
        bool IsDoneWithOutputToStream()
        {
            return IsDirectory || FileName.EndsWith("/", StringComparison.Ordinal);
        }

        #endregion

    }
}
