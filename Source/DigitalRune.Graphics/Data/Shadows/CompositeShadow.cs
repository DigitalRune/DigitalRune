// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a shadow which combines several other <see cref="Shadow"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CompositeShadow"/> owns a collection of child shadows (see property
  /// <see cref="Shadows"/>). The <see cref="CompositeShadow"/> itself does not compute a
  /// shadow map (<see cref="Shadow.ShadowMap"/> is always <see langword="null"/>). A shadow map is
  /// computed for each child shadow. The shadows of the child shadows will be combined in the
  /// <see cref="Shadow.ShadowMask"/> of the <see cref="CompositeShadow"/>. The
  /// <see cref="Shadow.ShadowMask"/>s of the child shadows will be <see langword="null"/>.
  /// </para>
  /// <para>
  /// Here ares some applications for composite shadows:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// Combine two <see cref="CascadedShadow"/>s. One shadow covers a large distance and contains
  /// only static objects. The second shadow covers a short distance and contains only dynamic
  /// objects.
  /// </item>
  /// <item>
  /// Combine a <see cref="CascadedShadow"/> with a custom variance shadow map. Use the variance
  /// shadow map to create smooth shadows for distant hills. Use the <see cref="CascadedShadow"/>
  /// for detailed shadows of other objects.
  /// </item>
  /// </list>
  /// </remarks>
  public class CompositeShadow : Shadow
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the list of shadows.
    /// </summary>
    /// <value>The list of shadows. Empty by default.</value>
    public ShadowCollection Shadows { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeShadow"/> class.
    /// </summary>
    public CompositeShadow()
    {
      Shadows = new ShadowCollection();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shadow CreateInstanceCore()
    {
      return new CompositeShadow();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shadow source)
    {
      // Clone Shadow properties.
      base.CloneCore(source);

      // Clone CompositeShadow properties.
      var sourceTyped = (CompositeShadow)source;
      for (int i = 0; i < sourceTyped.Shadows.Count; i++)
        Shadows.Add(sourceTyped.Shadows[i].Clone());

      // ShadowMap is not cloned!
    }
    #endregion

    #endregion
  }
}
