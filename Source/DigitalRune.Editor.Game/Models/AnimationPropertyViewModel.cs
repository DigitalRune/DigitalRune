// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Character;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Models
{
    /// <summary>
    /// Describes a single animation value in a property grid.
    /// </summary>
    internal class AnimationPropertyViewModel : ObservableObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly ModelDocument _document;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name;


        public SkeletonKeyFrameAnimation Animation
        {
            get { return _animation; }
            set { SetProperty(ref _animation, value); }
        }
        private SkeletonKeyFrameAnimation _animation;


        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (SetProperty(ref _isPlaying, value))
                {
                    if (value)
                        _document.PlayAnimation(Name);
                    else
                        _document.StopAnimation();
                }
            }
        }
        private bool _isPlaying;


        public DelegateCommand ToggleIsPlayingCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public AnimationPropertyViewModel(ModelDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _document = document;
            ToggleIsPlayingCommand = new DelegateCommand(OnToggleIsPlaying);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnToggleIsPlaying()
        {
            IsPlaying = !IsPlaying;
        }
        #endregion
    }
}
