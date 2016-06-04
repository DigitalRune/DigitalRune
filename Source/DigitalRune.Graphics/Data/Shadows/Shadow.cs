// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the shadow of a specific <see cref="LightNode"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class defines the desired shadow map format. During rendering it stores a reference to
  /// the <see cref="ShadowMap"/> and <see cref="ShadowMask"/>.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="Shadow"/>s are cloneable. <see cref="Clone"/> creates a deep copy of the current 
  /// shadow - unless documented otherwise (see derived classes). The shadow settings, like 
  /// <see cref="Prefer16Bit"/>, <see cref="PreferredSize"/>, etc. are duplicated - but the actual 
  /// shadow maps or shadow masks are not copied, since these resources cannot be shared between
  /// different shadow casting light nodes and they are usually updated in each frame anyways.
  /// </para>
  /// </remarks>
  public abstract class Shadow 
  {
    // Note:
    // We could add an Enabled flag to disable the shadow of a light nodes without
    // throwing away the settings.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the shadow map.
    /// </summary>
    /// <value>The shadow map. The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// This property is set by the shadow map renderer.
    /// </remarks>
    public Texture ShadowMap { get; set; }


    /// <summary>
    /// Gets or sets the shadow mask.
    /// </summary>
    /// <value>The shadow mask. The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// <para>
    /// This property is set by the shadow mask renderer.
    /// </para>
    /// <para>
    /// The shadow mask is a render target with the same size as the scene. It contains the filtered
    /// shadow values (0 = shadow, 1 = no shadow) as viewed from the camera.
    /// </para>
    /// </remarks>
    /// <seealso cref="ShadowMaskChannel"/>
    public RenderTarget2D ShadowMask { get; set; }


    /// <summary>
    /// Gets or sets the shadow mask channel.
    /// </summary>
    /// <value>The shadow mask channel. The default value is 0.</value>
    /// <remarks>
    /// Each <see cref="ShadowMask"/> can contain the shadow terms of several shadows. Each shadow
    /// uses one channel (R, G, B, or A) of the shadow mask. The <see cref="ShadowMaskChannel"/> is
    /// the index of the used channel (0 = R, 1 = B, 2 = G, 3 = A).
    /// </remarks>
    /// <seealso cref="ShadowMask"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 0 or greater than 3.
    /// </exception>
    public int ShadowMaskChannel
    {
      get { return _shadowMaskChannel; }
      set
      {
        if (value < 0 || value > 3)
          throw new ArgumentOutOfRangeException("value", "ShadowMaskChannel must be 0, 1, 2 or 3.");

        _shadowMaskChannel = value;
      }
    }
    private int _shadowMaskChannel;


    /// <summary>
    /// Gets or sets the size of the desired size of the shadow map in texels.
    /// </summary>
    /// <value>The preferred size of the shadow map in texels.</value>
    public int PreferredSize { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the shadow map should use a 16-bit format to store
    /// depth.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a 16 bit shadow map format should be used; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool Prefer16Bit { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Shadow"/> class.
    /// </summary>
    protected Shadow()
    {
      PreferredSize = 512;
      Prefer16Bit = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Shadow"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Shadow"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="Shadow"/> (Section "Cloning") for more information 
    /// about cloning.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Shadow"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Shadow"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public Shadow Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Shadow"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="Shadow"/> method, which this 
    /// method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Shadow"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Shadow CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Shadow. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="Shadow"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Shadow"/> derived class must implement this method. A typical implementation is to
    /// simply call the default constructor and return the result. 
    /// </para>
    /// </remarks>
    protected abstract Shadow CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="Shadow"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Shadow"/> derived class must implement
    /// this method. A typical implementation is to call <c>base.CloneCore(this)</c> to copy all 
    /// properties of the base class and then copy all properties of the derived class.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(Shadow source)
    {
      PreferredSize = source.PreferredSize;
      Prefer16Bit = source.Prefer16Bit;

      // ShadowMap, ShadowMask, ShadowMaskChannel are not cloned!
    }
    #endregion

    #endregion
  }
}
