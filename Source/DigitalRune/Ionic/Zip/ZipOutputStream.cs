// ZipOutputStream.cs
//
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa.
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


using System.IO;

namespace DigitalRune.Ionic.Zip
{
    internal class ZipContainer
    {
        private ZipFile _zf;

        public ZipContainer(ZipFile zf)
        {
            _zf = zf;
        }

        public ZipFile ZipFile
        {
            get { return _zf; }
        }

        public string Password
        {
            get
            {
                return _zf._Password;
            }
        }

        public int BufferSize
        {
            get
            {
                return _zf.BufferSize;
            }
        }

        public System.Text.Encoding AlternateEncoding
        {
            get
            {
                return _zf.AlternateEncoding;
            }
        }

        public Stream ReadStream
        {
            get
            {
                return _zf.ReadStream;
            }
        }
    }
}