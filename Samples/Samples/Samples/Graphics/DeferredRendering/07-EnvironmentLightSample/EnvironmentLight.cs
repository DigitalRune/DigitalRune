#if !WP7 && !WP8
using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // A light which uses an environment cube map to light objects and make them reflect
  // the environment.
  public class EnvironmentLight : Light
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public new Shape Shape
    {
      get { return base.Shape; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        base.Shape = value;
      }
    }

    public Vector3F Color { get; set; }
    public float DiffuseIntensity { get; set; }
    public float SpecularIntensity { get; set; }
    public float HdrScale { get; set; }
    public TextureCube EnvironmentMap { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public EnvironmentLight()
    {
      Color = new Vector3F(1);
      DiffuseIntensity = 1;
      SpecularIntensity = 1;
      HdrScale = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Light CreateInstanceCore()
    {
      return new EnvironmentLight();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Light source)
    {
      // Clone Light properties.
      base.CloneCore(source);
      Shape = source.Shape.Clone();

      // Clone EnvironmentLight properties.
      var sourceTyped = (EnvironmentLight)source;
      Color = sourceTyped.Color;
      DiffuseIntensity = sourceTyped.DiffuseIntensity;
      SpecularIntensity = sourceTyped.SpecularIntensity;
      HdrScale = sourceTyped.HdrScale;
      EnvironmentMap = sourceTyped.EnvironmentMap;
    }
    #endregion


    /// <inheritdoc/>
    public override Vector3F GetIntensity(float distance)
    {
      return Color * Math.Max(DiffuseIntensity, SpecularIntensity) * HdrScale;
    }
    #endregion
  }
}
#endif