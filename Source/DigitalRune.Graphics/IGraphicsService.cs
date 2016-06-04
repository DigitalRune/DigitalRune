// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if !NETFX_CORE && !WP7 && !WP8 && !XBOX && !PORTABLE
using System.Windows.Forms;
#endif
using DigitalRune.Graphics.Interop;
using Microsoft.Xna.Framework;

#if PORTABLE || WINDOWS_UWP
#pragma warning disable 1574  // Disable warning "XML comment has cref attribute that could not be resolved."
#endif


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages graphics-related objects, like graphics screens and presentation targets, and graphics
  /// resources.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="IGraphicsService"/> is the main interface for graphics-related tasks. It
  /// provides access to
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// the <see cref="GraphicsDevice"/>,
  /// </item>
  /// <item>
  /// the game main window (see <see cref="GameForm"/>, Windows only),
  /// </item>
  /// <item>
  /// the graphics screens (see <see cref="Screens"/> collection)
  /// </item>
  /// <item>
  /// the presentation targets (see <see cref="PresentationTargets"/> collection)
  /// </item>
  /// <item>
  /// and other properties.
  /// </item>
  /// </list>
  /// <para>
  /// <strong>Graphics screens: </strong>
  /// A <see cref="GraphicsScreen"/> renders game content, like the 3D scene or the HUD. The
  /// graphics service manages a collection of graphics screens, which are rendered back to front.
  /// </para>
  /// <para>
  /// <strong>Presentation targets:</strong>
  /// By default, the output is written into the back buffer. Optionally, the output can be written
  /// into a <i>presentation target</i>. A presentation target (see interface
  /// <see cref="IPresentationTarget"/>) is either a Windows Forms control
  /// (<see cref="FormsPresentationTarget"/>) or a WPF control (<see cref="ElementPresentationTarget"/>,
  /// <see cref="D3DImagePresentationTarget"/>) where the graphics can be displayed.
  /// </para>
  /// </remarks>
  public interface IGraphicsService
  {
    /// <summary>
    /// Gets the content manager that can be used to load predefined DigitalRune Graphics content
    /// (e.g. predefined shaders, post-processing effects, lookup textures, etc.).
    /// </summary>
    /// <value>The content manager.</value>
    ContentManager Content { get; }


    /// <summary>
    /// Gets or sets the render target pool.
    /// </summary>
    /// <value>The render target pool.</value>
    RenderTargetPool RenderTargetPool { get; }


    /// <summary>
    /// Gets the graphics device.
    /// </summary>
    /// <value>The graphics device.</value>
    GraphicsDevice GraphicsDevice { get; }


    /// <summary>
    /// Gets the main form (main window) of the 
    /// <see cref="Game"/>. 
    /// </summary>
    /// <value>
    /// The game window form (<strong>System.Windows.Forms.Form</strong>). This property is set on 
    /// Windows (desktop) and is <see langword="null"/> on all other platforms. 
    /// </value>
    object GameForm { get; }


    /// <summary>
    /// A collection of all presentation targets.
    /// </summary>
    /// <value>The presentation targets.</value>
    /// <remarks>
    /// <para>
    /// This property is not available on the following platforms: Silverlight, Windows Phone 7, 
    /// Xbox 360.
    /// </para>
    /// <para>
    /// A presentation target (see interface <see cref="IPresentationTarget"/>) is either a Windows
    /// Forms control or a WPF control where the graphics can be displayed.
    /// </para>
    /// </remarks>
    PresentationTargetCollection PresentationTargets { get; }


    /// <summary>
    /// Gets or sets the effect interpreters.
    /// </summary>
    /// <value>The effect interpreters.</value>
    /// <remarks>
    /// <para>
    /// User-defined <see cref="IEffectInterpreter"/> can be added to the collection to support new 
    /// effect techniques and parameters.
    /// </para>
    /// <para>
    /// Any changes to <see cref="EffectInterpreters"/> or <see cref="EffectBinders"/> need to be 
    /// made before the actual <see cref="EffectBinding"/>s are created. The 
    /// <see cref="IEffectInterpreter"/>s and <see cref="IEffectBinder"/>s are automatically applied
    /// when an new <see cref="EffectBinding"/> is created.
    /// </para>
    /// <para>
    /// Effect interpreters at the start of the collection have higher priority.
    /// </para>
    /// </remarks>
    EffectInterpreterCollection EffectInterpreters { get; }


    /// <summary>
    /// Gets or sets the effect binders.
    /// </summary>
    /// <value>The effect binders.</value>
    /// <remarks>
    /// <para>
    /// User-defined <see cref="IEffectBinder"/> can be added to the collection to support new 
    /// effect techniques and parameter bindings.
    /// </para>
    /// <para>
    /// Any changes to <see cref="EffectInterpreters"/> or <see cref="EffectBinders"/> need to be 
    /// made before the actual <see cref="EffectBinding"/>s are created. The 
    /// <see cref="IEffectInterpreter"/>s and <see cref="IEffectBinder"/>s are automatically applied
    /// when an new <see cref="EffectBinding"/> is created.
    /// </para>
    /// <para>
    /// Effect binders at the start of the collection have higher priority.
    /// </para>
    /// </remarks>
    EffectBinderCollection EffectBinders { get; } 


    /// <summary>
    /// Gets or sets the graphics screens.
    /// </summary>
    /// <value>The collection of <see cref="GraphicsScreen"/>s.</value>
    /// <remarks>
    /// <para>
    /// This <see cref="GraphicsScreenCollection"/> manages the graphics screens to be rendered.
    /// Graphics screens are rendered back (index 0) to front (index Count - 1).
    /// </para>
    /// <para>
    /// The graphics service only renders fully or partially visible screens. The property
    /// <see cref="GraphicsScreen.Coverage"/> of a <see cref="GraphicsScreen"/> indicates whether
    /// its content covers the entire screen and occludes screens with lower index, or whether it is
    /// partially transparent. The graphics service reads the <see cref="GraphicsScreen.Coverage"/> 
    /// property to determine which screens need to be rendered.
    /// </para>
    /// </remarks>
    GraphicsScreenCollection Screens { get; }


    /// <summary>
    /// Gets or sets the total elapsed time.
    /// </summary>
    /// <value>The total elapsed time.</value>
    /// <remarks>
    /// <para>
    /// This value can be used to control time-based animations. In effects it can be used with the
    /// <see cref="DefaultEffectParameterSemantics.Time"/> semantic.
    /// </para>
    /// <para>
    /// This value is automatically set by the graphics service. It is computed by continuously
    /// summing up the <see cref="DeltaTime"/> values.
    /// </para>
    /// <para>
    /// <strong>Important:</strong><br/>
    /// If the time in seconds is cast to a single precision floating point number
    /// (<see cref="float"/>), then it will become unusable after several hours or days - depending
    /// on the required resolution. Therefore, this value must be reset regularly in long-running
    /// applications. To reset the time just set this property back to <code>TimeSpan.Zero</code>.
    /// It is best to reset this value when the user does not notice the change, i.e. when entering
    /// a new level or a full-screen menu.
    /// </para>
    /// </remarks>
    TimeSpan Time { get; set; }


    /// <summary>
    /// Gets the elapsed time since the last frame.
    /// </summary>
    /// <value>The elapsed time since the last frame.</value>
    /// <remarks>
    /// This value is equal to the delta time parameter which was passed to the most recent call of
    /// <see cref="GraphicsManager.Update"/>.
    /// </remarks>
    TimeSpan DeltaTime { get; }


    /// <summary>
    /// Gets the number of the current frame.
    /// </summary>
    /// <value>The number of the current frame.</value>
    /// <remarks>
    /// <para>
    /// This number is incremented by the graphics service at the start of each frame. The initial
    /// value is -1, which indicates that nothing has been rendered so far.
    /// </para>
    /// <para>
    /// When the frame number exceeds the max integer value <see cref="int.MaxValue"/>, then it
    /// restarts at 0.
    /// </para>
    /// </remarks>
    int Frame { get; }


    /// <summary>
    /// Gets custom data associated with this graphics service or the graphics device.
    /// </summary>
    /// <value>The dictionary with user-defined data.</value>
    /// <remarks>
    /// Some resources, like default textures, should exist only once per graphics device. Such 
    /// resources can be stored in this dictionary. The disposable objects (i.e. objects that 
    /// implement <see cref="IDisposable"/>) in this dictionary will be automatically disposed when 
    /// the graphics device is disposed.
    /// </remarks>
    Dictionary<string, object> Data { get; }
  }
}
