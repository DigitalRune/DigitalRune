// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor.Documents
{
    internal sealed class DesignTimeDocument : Document
    {
        public DesignTimeDocument()
            : base(null, new DocumentType("Sample Type", null))
        {
            Uri = new Uri("file:///C:/Temp/document.txt");
        }


        protected override DocumentViewModel OnCreateViewModel()
        {
            throw new NotImplementedException();
        }


        protected override void OnLoad()
        {
            throw new NotImplementedException();
        }


        protected override void OnSave()
        {
            throw new NotImplementedException();
        }
    }
}
