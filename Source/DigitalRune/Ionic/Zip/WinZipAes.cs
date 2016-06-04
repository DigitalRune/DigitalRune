// WinZipAes.cs
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

#if AESCRYPTO
using System;
using System.IO;
using System.Security.Cryptography;

namespace DigitalRune.Ionic.Zip
{
    /// <summary>
    ///   This is a helper class supporting WinZip AES encryption.
    ///   This class is intended for use only by the DotNetZip library.
    /// </summary>
    ///
    /// <remarks>
    ///   Most uses of the DotNetZip library will not involve direct calls into
    ///   the WinZipAesCrypto class.  Instead, the WinZipAesCrypto class is
    ///   instantiated and used by the ZipEntry() class when WinZip AES
    ///   encryption or decryption on an entry is employed.
    /// </remarks>
    internal class WinZipAesCrypto
    {
        private byte[] _Salt;
        private byte[] _providedPv;
        private byte[] _generatedPv;
        private int _KeyStrengthInBits;
        private byte[] _MacInitializationVector;
        private byte[] _StoredMac;
        private byte[] _keyBytes;
        private Int16 PasswordVerificationStored;
        private Int16 PasswordVerificationGenerated;
        private int Rfc2898KeygenIterations = 1000;
        private string _Password;
        private bool _cryptoGenerated ;

        private WinZipAesCrypto(string password, int KeyStrengthInBits)
        {
            _Password = password;
            _KeyStrengthInBits = KeyStrengthInBits;
        }

        public static WinZipAesCrypto ReadFromStream(string password, int KeyStrengthInBits, Stream s)
        {
            // from http://www.winzip.com/aes_info.htm
            //
            // Size(bytes)   Content
            // -----------------------------------
            // Variable      Salt value
            // 2             Password verification value
            // Variable      Encrypted file data
            // 10            Authentication code
            //
            // ZipEntry.CompressedSize represents the size of all of those elements.

            // salt size varies with key length:
            //    128 bit key => 8 bytes salt
            //    192 bits => 12 bytes salt
            //    256 bits => 16 bytes salt

            WinZipAesCrypto c = new WinZipAesCrypto(password, KeyStrengthInBits);

            int saltSizeInBytes = c._KeyStrengthInBytes / 2;
            c._Salt = new byte[saltSizeInBytes];
            c._providedPv = new byte[2];

            s.Read(c._Salt, 0, c._Salt.Length);
            s.Read(c._providedPv, 0, c._providedPv.Length);

            c.PasswordVerificationStored = (Int16)(c._providedPv[0] + c._providedPv[1] * 256);
            if (password != null)
            {
                c.PasswordVerificationGenerated = (Int16)(c.GeneratedPV[0] + c.GeneratedPV[1] * 256);
                if (c.PasswordVerificationGenerated != c.PasswordVerificationStored)
                    throw new BadPasswordException("bad password");
            }

            return c;
        }

        private byte[] GeneratedPV
        {
            get
            {
                if (!_cryptoGenerated) _GenerateCryptoBytes();
                return _generatedPv;
            }
        }


        private byte[] Salt
        {
            get
            {
                return _Salt;
            }
        }


        private int _KeyStrengthInBytes
        {
            get
            {
                return _KeyStrengthInBits / 8;

            }
        }

        public int SizeOfEncryptionMetadata
        {
            get
            {
                // 10 bytes after, (n-10) before the compressed data
                return _KeyStrengthInBytes / 2 + 10 + 2;
            }
        }

        public string Password
        {
            set
            {
                _Password = value;
                if (_Password != null)
                {
                    PasswordVerificationGenerated = (Int16)(GeneratedPV[0] + GeneratedPV[1] * 256);
                    if (PasswordVerificationGenerated != PasswordVerificationStored)
                        throw new Ionic.Zip.BadPasswordException();
                }
            }
            //private get { return _Password; }
        }


        private void _GenerateCryptoBytes()
        {
            //Console.WriteLine(" provided password: '{0}'", _Password);

            using (Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(_Password, Salt, Rfc2898KeygenIterations))
            {
                _keyBytes = rfc2898.GetBytes(_KeyStrengthInBytes); // 16 or 24 or 32 ???
                _MacInitializationVector = rfc2898.GetBytes(_KeyStrengthInBytes);
                _generatedPv = rfc2898.GetBytes(2);

                _cryptoGenerated = true;
            }
        }


        public byte[] KeyBytes
        {
            get
            {
                if (!_cryptoGenerated) _GenerateCryptoBytes();
                return _keyBytes;
            }
        }


        public byte[] MacIv
        {
            get
            {
                if (!_cryptoGenerated) _GenerateCryptoBytes();
                return _MacInitializationVector;
            }
        }

        public byte[] CalculatedMac;


        public void ReadAndVerifyMac(System.IO.Stream s)
        {
            bool invalid = false;

            // read integrityCheckVector.
            // caller must ensure that the file pointer is in the right spot!
            _StoredMac = new byte[10];  // aka "authentication code"
            s.Read(_StoredMac, 0, _StoredMac.Length);

            if (_StoredMac.Length != CalculatedMac.Length)
                invalid = true;

            if (!invalid)
            {
                for (int i = 0; i < _StoredMac.Length; i++)
                {
                    if (_StoredMac[i] != CalculatedMac[i])
                        invalid = true;
                }
            }

            if (invalid)
                throw new Ionic.Zip.BadStateException("The MAC does not match.");
        }

    }


    /// <summary>
    ///   A stream that decrypts as it reads.  The
    ///   Crypto is AES in CTR (counter) mode, which is compatible with the AES
    ///   encryption employed by WinZip 12.0.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The AES/CTR encryption protocol used by WinZip works like this:
    ///
    ///       - start with a counter, initialized to zero.
    ///
    ///       - to encrypt, take the data by 16-byte blocks. For each block:
    ///         - apply the transform to the counter
    ///         - increement the counter
    ///         - XOR the result of the transform with the plaintext to
    ///           get the ciphertext.
    ///         - compute the mac on the encrypted bytes
    ///       - when finished with all blocks, store the computed MAC.
    ///
    ///       - to decrypt, take the data by 16-byte blocks. For each block:
    ///         - compute the mac on the encrypted bytes,
    ///         - apply the transform to the counter
    ///         - increement the counter
    ///         - XOR the result of the transform with the ciphertext to
    ///           get the plaintext.
    ///       - when finished with all blocks, compare the computed MAC against
    ///         the stored MAC
    ///
    ///   </para>
    /// </remarks>
    //
    internal class WinZipAesCipherStream : Stream
    {
        private WinZipAesCrypto _params;
        private System.IO.Stream _s;
        private int _nonce;
        private bool _finalBlock;

        private HMACSHA1 _mac;

        // Use RijndaelManaged from .NET 2.0.
        // AesManaged came in .NET 3.5, but we want to limit
        // dependency to .NET 2.0.  AES is just a restricted form
        // of Rijndael (fixed block size of 128, some crypto modes not supported).

        private RijndaelManaged _aesCipher;
        private ICryptoTransform _xform;

        private const int BLOCK_SIZE_IN_BYTES = 16;

        private byte[] counter = new byte[BLOCK_SIZE_IN_BYTES];
        private byte[] counterOut = new byte[BLOCK_SIZE_IN_BYTES];

        // I've had a problem when wrapping a WinZipAesCipherStream inside
        // a DeflateStream. Calling Read() on the DeflateStream results in
        // a Read() on the WinZipAesCipherStream, but the buffer is larger
        // than the total size of the encrypted data, and larger than the
        // initial Read() on the DeflateStream!  When the encrypted
        // bytestream is embedded within a larger stream (As in a zip
        // archive), the Read() doesn't fail with EOF.  This causes bad
        // data to be returned, and it messes up the MAC.

        // This field is used to provide a hard-stop to the size of
        // data that can be read from the stream.  In Read(), if the buffer or
        // read request goes beyond the stop, we truncate it.

        private long _length;
        private long _totalBytesXferred;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="s">The underlying stream</param>
        /// <param name="cryptoParams">The pre-initialized WinZipAesCrypto object.</param>
        /// <param name="length">The maximum number of bytes to read from the stream.</param>
        internal WinZipAesCipherStream(Stream s, WinZipAesCrypto cryptoParams, long length)
            : this(s, cryptoParams)
        {
            // don't read beyond this limit!
            _length = length;
            //Console.WriteLine("max length of AES stream: {0}", _length);
        }


        internal WinZipAesCipherStream(System.IO.Stream s, WinZipAesCrypto cryptoParams)
            : base()
        {
            _params = cryptoParams;
            _s = s;
            _nonce = 1;

            if (_params == null)
                throw new BadPasswordException("Supply a password to use AES encryption.");

            int keySizeInBits = _params.KeyBytes.Length * 8;
            if (keySizeInBits != 256 && keySizeInBits != 128 && keySizeInBits != 192)
                throw new ArgumentOutOfRangeException("cryptoParams", "size of key must be 128, 192, or 256");

            _mac = new HMACSHA1(_params.MacIv);

            _aesCipher = new System.Security.Cryptography.RijndaelManaged();
            _aesCipher.BlockSize = 128;
            _aesCipher.KeySize = keySizeInBits;  // 128, 192, 256
            _aesCipher.Mode = CipherMode.ECB;
            _aesCipher.Padding = PaddingMode.None;

            byte[] iv = new byte[BLOCK_SIZE_IN_BYTES]; // all zeroes

            // Create an ENCRYPTOR, regardless whether doing decryption or encryption.
            // It is reflexive.
            _xform = _aesCipher.CreateEncryptor(_params.KeyBytes, iv);
        }

        private void XorInPlace(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = (byte)(counterOut[i] ^ buffer[offset + i]);
            }
        }

        private int ReadTransformOneBlock(byte[] buffer, int offset, int last)
        {
            if (_finalBlock)
                throw new NotSupportedException();

            int bytesRemaining = last - offset;
            int bytesToRead = (bytesRemaining > BLOCK_SIZE_IN_BYTES)
                ? BLOCK_SIZE_IN_BYTES
                : bytesRemaining;

            // update the counter
            System.Array.Copy(BitConverter.GetBytes(_nonce++), 0, counter, 0, 4);

            // Determine if this is the final block
            if ((bytesToRead == bytesRemaining) &&
                (_length > 0) &&
                (_totalBytesXferred + last == _length))
            {
                _mac.TransformFinalBlock(buffer, offset, bytesToRead);
                counterOut = _xform.TransformFinalBlock(counter,
                                                        0,
                                                        BLOCK_SIZE_IN_BYTES);
                _finalBlock = true;
            }
            else
            {
                _mac.TransformBlock(buffer, offset, bytesToRead, null, 0);
                _xform.TransformBlock(counter,
                                      0, // offset
                                      BLOCK_SIZE_IN_BYTES,
                                      counterOut,
                                      0);  // offset
            }

            XorInPlace(buffer, offset, bytesToRead);
            return bytesToRead;
        }


        private void ReadTransformBlocks(byte[] buffer, int offset, int count)
        {
            int posn = offset;
            int last = count + offset;

            while (posn < buffer.Length && posn < last )
            {
                int n = ReadTransformOneBlock (buffer, posn, last);
                posn += n;
            }
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset",
                                                      "Must not be less than zero.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                                                      "Must not be less than zero.");

            if (buffer.Length < offset + count)
                throw new ArgumentException("The buffer is too small");

            // When I wrap a WinZipAesStream in a DeflateStream, the
            // DeflateStream asks its captive to read 4k blocks, even if the
            // encrypted bytestream is smaller than that.  This is a way to
            // limit the number of bytes read.

            int bytesToRead = count;

            if (_totalBytesXferred >= _length)
            {
                return 0; // EOF
            }

            long bytesRemaining = _length - _totalBytesXferred;
            if (bytesRemaining < count) bytesToRead = (int)bytesRemaining;

            int n = _s.Read(buffer, offset, bytesToRead);


            ReadTransformBlocks(buffer, offset, bytesToRead);

            _totalBytesXferred += n;
            return n;
        }



        /// <summary>
        /// Returns the final HMAC-SHA1-80 for the data that was encrypted.
        /// </summary>
        public byte[] FinalAuthentication
        {
            get
            {
                if (!_finalBlock)
                {
                    // special-case zero-byte files
                    if ( _totalBytesXferred != 0)
                        throw new BadStateException("The final hash has not been computed.");

                    // Must call ComputeHash on an empty byte array when no data
                    // has run through the MAC.

                    byte[] b = {  };
                    _mac.ComputeHash(b);
                    // fall through
                }
                byte[] macBytes10 = new byte[10];
                System.Array.Copy(_mac.Hash, 0, macBytes10, 0, 10);
                return macBytes10;
            }
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }



        /// <summary>
        ///   Close the stream.
        /// </summary>
        public override void Close()
        {
            _s.Close();
        }


        /// <summary>
        /// Returns true if the stream can be read.
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }


        /// <summary>
        /// Always returns false.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Returns true if the CryptoMode is Encrypt.
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// Flush the content in the stream.
        /// </summary>
        public override void Flush()
        {
            _s.Flush();
        }

        /// <summary>
        /// Getting this property throws a NotImplementedException.
        /// </summary>
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Getting or Setting this property throws a NotImplementedException.
        /// </summary>
        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// This method throws a NotImplementedException.
        /// </summary>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method throws a NotImplementedException.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
