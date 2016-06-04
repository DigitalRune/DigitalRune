// OffsetStream.cs
// ------------------------------------------------------------------
//
// Copyright (c)  2009 Dino Chiesa
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
using System.IO;

namespace DigitalRune.Ionic.Zip
{
    internal class OffsetStream : System.IO.Stream
    {
        private Int64 _originalPosition;
        private Stream _innerStream;

        public OffsetStream(Stream s)
            : base()
        {
            _originalPosition = s.Position;
            _innerStream = s;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Length
        {
            get
            {
                return _innerStream.Length;
            }
        }

        public override long Position
        {
            get { return _innerStream.Position - _originalPosition; }
            set { _innerStream.Position = _originalPosition + value; }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            return _innerStream.Seek(_originalPosition + offset, origin) - _originalPosition;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}