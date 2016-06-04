// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents an oriented, textured quad used for drawing impostors, particles, text, and other 
  /// effects.
  /// </summary>
  /// <remarks>
  /// <para>
  /// DigitalRune Graphics supports two types of billboards:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <description><see cref="ImageBillboard"/> ... static or animated bitmap</description>
  /// </item>
  /// <item>
  /// <description><see cref="TextBillboard"/> ... text using a bitmap font</description>
  /// </item>
  /// </list>
  /// <para>
  /// A billboard can be positioned by adding a <see cref="BillboardNode"/> to a 3D scene. The 
  /// billboard orientation can be set using the <see cref="Orientation"/> property. See class
  /// <see cref="BillboardOrientation"/> for more information.
  /// </para>
  /// <para>
  /// <strong>Billboard Size:</strong>
  /// To resize an <see cref="ImageBillboard"/> change the <see cref="ImageBillboard.Size"/> 
  /// property or change the <see cref="SceneNode.ScaleLocal"/> of the associated scene node.
  /// To resize a <see cref="TextBillboard"/> change the <see cref="TextBillboard.Font"/> or change 
  /// the <see cref="SceneNode.ScaleLocal"/> of the associated scene node. (The font size is 
  /// measured directly in world space unit, i.e. a 12 pt font is 12 units in world space.)
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// <see cref="Billboard"/>s are cloneable. <see cref="Clone"/> creates a copy of the current 
  /// sprite. The <see cref="ImageBillboard.Texture"/> of a <see cref="ImageBillboard"/> or the 
  /// <see cref="TextBillboard.Text"/> of a <see cref="TextBillboard"/> is copied by reference (no 
  /// deep copy).
  /// </para>
  /// </remarks>
  /// <seealso cref="ImageBillboard"/>
  /// <seealso cref="TextBillboard"/>
  /// <seealso cref="BillboardNode"/>
  /// <seealso cref="BillboardNormal"/>
  /// <seealso cref="BillboardOrientation"/>
  public abstract class Billboard : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the bounding shape.
    /// </summary>
    /// <value>The bounding shape.</value>
    internal SphereShape Shape { get; private set; }


    /// <summary>
    /// Gets or sets the name of this billboard.
    /// </summary>
    /// <value>The name of this billboard. The default value <see langword="null"/>.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the orientation of the billboard.
    /// </summary>
    /// <value>
    /// The billboard orientation. The default value is 
    /// <see cref="BillboardOrientation.ViewPlaneAligned"/>.
    /// </value>
    public BillboardOrientation Orientation { get; set; }


    /// <summary>
    /// Gets or sets the tint color of the billboard.
    /// </summary>
    /// <value>The tint color (non-premultiplied). The default value is white (1, 1, 1).</value>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the opacity of the billboard.
    /// </summary>
    /// <value>The opacity. The default value is 1 (opaque).</value>
    public float Alpha { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Billboard" /> class.
    /// </summary>
    protected Billboard()
    {
      Shape = new SphereShape(0);
      Orientation = BillboardOrientation.ViewPlaneAligned;
      Color = new Vector3F(1, 1, 1);
      Alpha = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Billboard"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Billboard"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="Billboard"/> (Section "Cloning") for more information 
    /// about cloning.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Billboard"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Billboard"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public Billboard Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Billboard"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method,
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Billboard"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Billboard CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Billboard. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the <see cref="Billboard"/>
    /// derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Billboard"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Billboard"/> derived class must 
    /// implement this method. A typical implementation is to simply call the default constructor 
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected abstract Billboard CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified 
    /// <see cref="Billboard"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Billboard"/> derived class must 
    /// implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> to 
    /// copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(Billboard source)
    {
      Name = source.Name;
      Orientation = source.Orientation;
      Color = source.Color;
      Alpha = source.Alpha;
    }
    #endregion

    #endregion
  }
}
