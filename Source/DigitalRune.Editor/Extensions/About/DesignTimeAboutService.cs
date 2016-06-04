// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.About
{
    internal class DesignTimeAboutService : IAboutService
    {
        public string ApplicationName
        {
            get { return "MyApplication"; }
            set { }
        }


        public string Copyright
        {
            get { return "Copyright 2011-2016 DigitalRune GmbH"; }
            set { }
        }


        public string Version
        {
            get { return "1.0.0.123"; }
            set { }
        }


        public object Information
        {
            get { return "Mock information about my application."; }
            set { }
        }


        public string InformationAsString
        {
            get { return "Mock information about my application.\nWith 2 lines."; }
            set { }
        }


        public object Icon
        {
            get { return MultiColorGlyphs.Plugin; }
            set { }
        }


        public ICollection<EditorExtensionDescription> ExtensionDescriptions
        {
            get
            {
                return new List<EditorExtensionDescription>
                {
                    new EditorExtensionDescription
                    {
                        Description = "Description of the first extension.\nWith 2 lines.",
                        Icon = Icon,
                        Name = "My First Extension",
                        Version = "1.2.3.4",
                    },
                    new EditorExtensionDescription
                    {
                        Description = "Description of the second extension.\nWith 2 lines.",
                        Icon = Icon,
                        Name = "My Second Extension",
                        Version = "1.2.3.4",
                    },
                };
            }
        }


        public void Show()
        {
        }


        public string CopyInformationToClipboard()
        {
            return string.Empty;
        }
    }
}
