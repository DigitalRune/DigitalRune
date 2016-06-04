// Zlib.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa and Microsoft Corporation.
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
//
// This module defines classes for ZLIB compression and
// decompression. This code is derived from the jzlib implementation of
// zlib, but significantly modified.  The object model is not the same,
// and many of the behaviors are new or different.  Nonetheless, in
// keeping with the license for jzlib, the copyright to that code is
// included below.
//
// ------------------------------------------------------------------
//
// The following notice applies to jzlib:
//
// Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in
// the documentation and/or other materials provided with the distribution.
//
// 3. The names of the authors may not be used to endorse or promote products
// derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
// INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// -----------------------------------------------------------------------
//
// jzlib is based on zlib-1.1.3.
//
// The following notice applies to zlib:
//
// -----------------------------------------------------------------------
//
// Copyright (C) 1995-2004 Jean-loup Gailly and Mark Adler
//
//   The ZLIB software is provided 'as-is', without any express or implied
//   warranty.  In no event will the authors be held liable for any damages
//   arising from the use of this software.
//
//   Permission is granted to anyone to use this software for any purpose,
//   including commercial applications, and to alter it and redistribute it
//   freely, subject to the following restrictions:
//
//   1. The origin of this software must not be misrepresented; you must not
//      claim that you wrote the original software. If you use this software
//      in a product, an acknowledgment in the product documentation would be
//      appreciated but is not required.
//   2. Altered source versions must be plainly marked as such, and must not be
//      misrepresented as being the original software.
//   3. This notice may not be removed or altered from any source distribution.
//
//   Jean-loup Gailly jloup@gzip.org
//   Mark Adler madler@alumni.caltech.edu
//
// -----------------------------------------------------------------------


namespace DigitalRune.Ionic.Zlib
{
    /// <summary>
    /// Describes how to flush the current deflate operation.
    /// </summary>
    /// <remarks>
    /// The different FlushType values are useful when using a Deflate in a streaming application.
    /// </remarks>
    internal enum FlushType
    {
        /// <summary>No flush at all.</summary>
        None = 0,

        /// <summary>Closes the current block, but doesn't flush it to
        /// the output. Used internally only in hypothetical
        /// scenarios.  This was supposed to be removed by Zlib, but it is
        /// still in use in some edge cases.
        /// </summary>
        Partial,

        /// <summary>
        /// Use this during compression to specify that all pending output should be
        /// flushed to the output buffer and the output should be aligned on a byte
        /// boundary.  You might use this in a streaming communication scenario, so that
        /// the decompressor can get all input data available so far.  When using this
        /// with a ZlibCodec, <c>AvailableBytesIn</c> will be zero after the call if
        /// enough output space has been provided before the call.  Flushing will
        /// degrade compression and so it should be used only when necessary.
        /// </summary>
        Sync,

        /// <summary>
        /// Use this during compression to specify that all output should be flushed, as
        /// with <c>FlushType.Sync</c>, but also, the compression state should be reset
        /// so that decompression can restart from this point if previous compressed
        /// data has been damaged or if random access is desired.  Using
        /// <c>FlushType.Full</c> too often can significantly degrade the compression.
        /// </summary>
        Full,

        /// <summary>Signals the end of the compression/decompression stream.</summary>
        Finish,
    }


    /// <summary>
    /// A general purpose exception class for exceptions in the Zlib library.
    /// </summary>
    /// <exclude />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ZlibException : System.Exception
    {
        /// <summary>
        /// The ZlibException class captures exception information generated
        /// by the Zlib library.
        /// </summary>
        public ZlibException()
            : base()
        {
        }

        /// <summary>
        /// This ctor collects a message attached to the exception.
        /// </summary>
        /// <param name="message">the message for the exception.</param>
        public ZlibException(System.String message)
            : base(message)
        {
        }
    }


    internal class SharedUtils
    {
        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, int bits)
        {
            return (int)((uint)number >> bits);
        }
    }


    /// <summary>
    /// Computes an Adler-32 checksum.
    /// </summary>
    /// <remarks>
    /// The Adler checksum is similar to a CRC checksum, but faster to compute, though less
    /// reliable.  It is used in producing RFC1950 compressed streams.  The Adler checksum
    /// is a required part of the "ZLIB" standard.  Applications will almost never need to
    /// use this class directly.
    /// </remarks>
    ///
    /// <exclude/>
    internal sealed class Adler
    {
        // largest prime smaller than 65536
        private const uint BASE = 65521;
        // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
        private const int NMAX = 5552;


#pragma warning disable 3001
#pragma warning disable 3002

        /// <summary>
        ///   Calculates the Adler32 checksum.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This is used within ZLIB.  You probably don't need to use this directly.
        ///   </para>
        /// </remarks>
        /// <example>
        ///    To compute an Adler32 checksum on a byte array:
        ///  <code>
        ///    var adler = Adler.Adler32(0, null, 0, 0);
        ///    adler = Adler.Adler32(adler, buffer, index, length);
        ///  </code>
        /// </example>
        public static uint Adler32(uint adler, byte[] buf, int index, int len)
        {
            if (buf == null)
                return 1;

            uint s1 = (uint) (adler & 0xffff);
            uint s2 = (uint) ((adler >> 16) & 0xffff);

            while (len > 0)
            {
                int k = len < NMAX ? len : NMAX;
                len -= k;
                while (k >= 16)
                {
                    //s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += buf[index++];
                        s2 += s1;
                    }
                    while (--k != 0);
                }
                s1 %= BASE;
                s2 %= BASE;
            }
            return (uint)((s2 << 16) | s1);
        }
#pragma warning restore 3001
#pragma warning restore 3002

    }

}