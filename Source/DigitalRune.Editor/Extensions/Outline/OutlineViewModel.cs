// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Outlines
{
    /// <summary>
    /// Represents the Outline window.
    /// </summary>
    internal class OutlineViewModel : EditorDockTabItemViewModel
    {
        // TODO: Invalidate ICommands on navigation.
        // TODO: Disable timer if PropertySource supports INotifyPropertyChanged.


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string DockIdString = "Outline";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        //private readonly IEditorService _editor;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an instance of the <see cref="OutlineViewModel"/> that can be used at
        /// design-time.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="OutlineViewModel"/> that can be used at design-time.
        /// </value>
        internal static OutlineViewModel DesignInstance
        {
            get
            {
                var vm = new OutlineViewModel(null)
                {
                    Outline = new Outline
                    {
                        RootItems =
                        {
                            new OutlineItem
                            {
                                Text = "Root",
                                Children = new OutlineItemCollection
                                {
                                    new OutlineItem
                                    {
                                        Text = "Item 1",
                                        Icon = MultiColorGlyphs.Document,
                                    },
                                    new OutlineItem
                                    {
                                        Text = "Item 2",
                                        Children = new OutlineItemCollection
                                        {
                                            new OutlineItem
                                            {
                                                Text = "Item 2.1",
                                                Icon = MultiColorGlyphs.Image,
                                            },
                                            new OutlineItem
                                            {
                                                Text = "Item 2.2",
                                                Icon = MultiColorGlyphs.Image,
                                            },
                                            new OutlineItem
                                            {
                                                Text = "Item 2.3",
                                            },
                                        },
                                    },
                                    new OutlineItem
                                    {
                                        Text = "Item 3",
                                    }
                                }
                            }
                        }
                    }
                };
                return vm;
            }
        }


        /// <summary>
        /// Gets or sets the outline.
        /// </summary>
        /// <value>The outline.</value>
        public Outline Outline
        {
            get { return _outline; }
            set { SetProperty(ref _outline, value); }
        }
        private Outline _outline;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="OutlineViewModel"/> class.
        /// </summary>
        /// <param name="editor">The editor. Can be <see langword="null"/> at design-time.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public OutlineViewModel(IEditorService editor)
        {
            DisplayName = "Outline";
            DockId = DockIdString;
            //Icon = MultiColorGlyphs.Properties;
            IsPersistent = true;
            DockWidth = new GridLength(200);
            DockHeight = new GridLength(300);
            AutoHideWidth = 200;
            AutoHideHeight = 300;

            //if (!WindowsHelper.IsInDesignMode)
            //{
            //    if (editor == null)
            //        throw new ArgumentNullException(nameof(editor));

            //    _editor = editor;
            //}
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
