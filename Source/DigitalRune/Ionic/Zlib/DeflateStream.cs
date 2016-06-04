// DeflateStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2010 Dino Chiesa.
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

namespace DigitalRune.Ionic.Zlib
{
    /// <summary>
    /// A class for decompressing streams using the Deflate algorithm.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
    ///   The DeflateStream is a <see
    ///   href="http://en.wikipedia.org/wiki/Decorator_pattern">Decorator</see> on a <see
    ///   cref="System.IO.Stream"/>.  It adds DEFLATE decompression to any stream.
    /// </para>
    ///
    /// <para>
    ///   Using this stream, applications can decompress data via stream
    ///   <c>Read</c> operations. The compression format used is
    ///   DEFLATE, which is documented in <see
    ///   href="http://www.ietf.org/rfc/rfc1951.txt">IETF RFC 1951</see>, "DEFLATE
    ///   Compressed Data Format Specification version 1.3.".
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="GZipStream" />
    internal class DeflateStream : System.IO.Stream
    {
        private ZlibBaseStream _baseStream;
        private bool _disposed;

        /// <summary>
        ///   Create a DeflateStream for decompression.
        /// </summary>
        ///
        /// <param name="stream">The stream which will be read.</param>
        public DeflateStream(System.IO.Stream stream)
            : this(stream, false)
        {
        }

        /// <summary>
        ///   Create a <c>DeflateStream</c> and explicitly specify whether the
        ///   stream should be left open after Inflation.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   This constructor allows the application to request that the captive stream
        ///   remain open after the inflation occurs.  By default, after
        ///   <c>Close()</c> is called on the stream, the captive stream is also
        ///   closed. In some cases this is not desired, for example if the stream is a
        ///   memory stream that will be re-read after compression.  Specify true for
        ///   the <paramref name="leaveOpen"/> parameter to leave the stream open.
        /// </para>
        ///
        /// <para>
        ///   The <c>DeflateStream</c> will use the default compression level.
        /// </para>
        ///
        /// <para>
        ///   See the other overloads of this constructor for example code.
        /// </para>
        /// </remarks>
        ///
        /// <param name="stream">
        ///   The stream which will be read. This is called the
        ///   "captive" stream in other places in this documentation.
        /// </param>
        ///
        /// <param name="leaveOpen">true if the application would like the stream to
        /// remain open after inflation.</param>
        public DeflateStream(System.IO.Stream stream, bool leaveOpen)
        {
            _baseStream = new ZlibBaseStream(stream, ZlibStreamFlavor.DEFLATE, leaveOpen);
        }

        #region System.IO.Stream methods
        /// <summary>
        ///   Dispose the stream.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This may or may not result in a <c>Close()</c> call on the captive
        ///     stream.  See the constructors that have a <c>leaveOpen</c> parameter
        ///     for more information.
        ///   </para>
        ///   <para>
        ///     Application code won't call this code directly.  This method may be
        ///     invoked in two distinct scenarios.  If disposing == true, the method
        ///     has been called directly or indirectly by a user's code, for example
        ///     via the public Dispose() method. In this case, both managed and
        ///     unmanaged resources can be referenced and disposed.  If disposing ==
        ///     false, the method has been called by the runtime from inside the
        ///     object finalizer and this method should not reference other objects;
        ///     in that case only unmanaged resources must be referenced or
        ///     disposed.
        ///   </para>
        /// </remarks>
        /// <param name="disposing">
        ///   true if the Dispose method was invoked by user code.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    if (disposing && (this._baseStream != null))
                        this._baseStream.Dispose();
                    _disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


        /// <summary>
        /// Indicates whether the stream can be read.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports reading.
        /// </remarks>
        public override bool CanRead
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                return _baseStream._stream.CanRead;
            }
        }

        /// <summary>
        /// Indicates whether the stream supports Seek operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanSeek
        {
            get { return false; }
        }


        /// <summary>
        /// Indicates whether the stream can be written.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports writing.
        /// </remarks>
        public override bool CanWrite
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                return _baseStream._stream.CanWrite;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
            _baseStream.Flush();
        }

        /// <summary>
        /// Reading this property always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The position of the stream pointer.
        /// </summary>
        ///
        /// <remarks>
        ///   Setting this property always throws a <see
        ///   cref="NotImplementedException"/>. Reading will return the total bytes
        ///   read in. The count may refer to compressed bytes or uncompressed bytes,
        ///   depending on how you've used the stream.
        /// </remarks>
        public override long Position
        {
            get
            {
                if (this._baseStream._streamMode == Ionic.Zlib.ZlibBaseStream.StreamMode.Reader)
                    return this._baseStream._z.TotalBytesIn;
                return 0;
            }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Read data from the stream.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   If you wish to use the <c>DeflateStream</c> to compress data while
        ///   reading, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Compress</c>, providing an uncompressed data stream.
        ///   Then call Read() on that <c>DeflateStream</c>, and the data read will be
        ///   compressed as you read.  If you wish to use the <c>DeflateStream</c> to
        ///   decompress data while reading, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Decompress</c>, providing a readable compressed data
        ///   stream.  Then call Read() on that <c>DeflateStream</c>, and the data read
        ///   will be decompressed as you read.
        /// </para>
        ///
        /// <para>
        ///   A <c>DeflateStream</c> can be used for <c>Read()</c> or <c>Write()</c>, but not both.
        /// </para>
        ///
        /// </remarks>
        /// <param name="buffer">The buffer into which the read data should be placed.</param>
        /// <param name="offset">the offset within that data array to put the first byte read.</param>
        /// <param name="count">the number of bytes to read.</param>
        /// <returns>the number of bytes actually read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
            return _baseStream.Read(buffer, offset, count);
        }


        /// <summary>
        /// Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="offset">this is irrelevant, since it will always throw!</param>
        /// <param name="origin">this is irrelevant, since it will always throw!</param>
        /// <returns>irrelevant!</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="value">this is irrelevant, since it will always throw!</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        ///
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

