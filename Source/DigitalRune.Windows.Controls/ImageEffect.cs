// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using static System.FormattableString;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Provides miscellaneous options for processing an image.
    /// </summary>
    public class ImageEffect : ShaderEffect
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly PixelShader ImageEffectShader = new PixelShader { UriSource = MakePackUri("Resources/ImageEffect.ps") };
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //-------------------------------------------------------------- 

        /// <summary>
        /// Identifies the <see cref="Input"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(ImageEffect), 0);


        /// <summary>
        /// Gets or sets the input that is being processed.
        /// This is a dependency property.
        /// </summary>
        /// <value>The input that is being processed.</value>
        [Description("Gets or sets the input that is being processed.")]
        [Category(Categories.Brushes)]
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(ImageEffect),
            new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        /// <summary>
        /// Gets or sets the tint color.
        /// This is a dependency property.
        /// </summary>
        /// <value>The tint color. The default value is <see cref="Colors.White"/>.</value>
        [Description("Gets or sets the tint color.")]
        [Category(Categories.Appearance)]
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Opacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
            "Opacity",
            typeof(double),
            typeof(ImageEffect),
            new UIPropertyMetadata(Boxed.DoubleOne, PixelShaderConstantCallback(1)));

        /// <summary>
        /// Gets or sets the opacity (alpha value).
        /// This is a dependency property.
        /// </summary>
        /// <value>The opacity (alpha value). The default value is 1.</value>
        [Description("Gets or sets the opacity (alpha value).")]
        [Category(Categories.Appearance)]
        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Saturation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            "Saturation",
            typeof(double),
            typeof(ImageEffect),
            new FrameworkPropertyMetadata(Boxed.DoubleOne, PixelShaderConstantCallback(2)));

        /// <summary>
        /// Gets or sets the color saturation.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The color saturation where 0 = desaturated colors and 1 = saturated colors. The default
        /// value is 1.
        /// </value>
        [Description("Gets or sets the color saturation.")]
        [Category(Categories.Appearance)]
        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffect"/> class.
        /// </summary>
        public ImageEffect()
        {
            PixelShader = ImageEffectShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(ColorProperty);
            UpdateShaderValue(OpacityProperty);
            UpdateShaderValue(SaturationProperty);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        // MakePackUri is a utility method for computing a pack URI for the given resource.
        private static Uri MakePackUri(string relativeFile)
        {
            var assembly = typeof(ImageEffect).Assembly;

            // Extract the short name.
            string assemblyShortName = assembly.ToString().Split(',')[0];
            string uriString = Invariant($"pack://application:,,,/{assemblyShortName};component/{relativeFile}");
            return new Uri(uriString);
        }
        #endregion
    }
}
