// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if XNA && (WINDOWS || XBOX)
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Represents the skeletal pose and facial expression of an Xbox LIVE Avatar. 
  /// (Only available in the XNA-compatible build for Windows and Xbox 360.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type is available only in the XNA-compatible builds of the DigitalRune.Animation.dll for
  /// Windows and Xbox 360.)
  /// </para>
  /// <para>
  /// The <see cref="AvatarPose"/> stores the facial expression (see property 
  /// <see cref="Expression"/>) and the skeleton pose (see property <see cref="SkeletonPose"/>) of
  /// an Xbox LIVE Avatar.
  /// </para>
  /// <para>
  /// The skeleton and the skeleton pose is automatically created from an XNA 
  /// <see cref="AvatarRenderer"/> instance. The avatar renderer must already be in its
  /// "ready" state (<c>AvatarRender.State == AvatarRendererState.Ready</c>) when the 
  /// <see cref="AvatarPose"/> is created. 
  /// </para>
  /// <para>
  /// <strong>IAnimatableObject:</strong><br/>
  /// The <see cref="AvatarPose"/> implements the interface <see cref="IAnimatableObject"/>,
  /// which means that it can be animated using the animation system. The animatable properties are
  /// <see cref="SkeletonPose"/> and <see cref="Expression"/>. (When calling the method
  /// <see cref="IAnimatableObject.GetAnimatableProperty{T}"/> directly the properties are 
  /// identified using the strings <c>"SkeletonPose"</c> and <c>"Expression"</c>.)
  /// </para>
  /// <para>
  /// <strong>IAvatarAnimation:</strong><br/>
  /// The class implements the interface <see cref="IAvatarAnimation"/>, which means the object
  /// can be passed to an <see cref="AvatarRenderer"/> for rendering.
  /// </para>
  /// </remarks>
  public class AvatarPose : IAnimatableObject, IAvatarAnimation
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the bone names of the Xbox LIVE Avatar.
    /// </summary>
    /// <value>The bone names of the Xbox LIVE Avatar.</value>
    private static string[] BoneNames
    {
      get
      {
        if (_boneNames == null)
        {
          _boneNames = new string[AvatarRenderer.BoneCount];
          for (int i = 0; i < _boneNames.Length; i++)
            _boneNames[i] = ((AvatarBone)i).ToString();
        }

        return _boneNames;
      }
    }
    private static string[] _boneNames;


    /// <summary>
    /// Gets or sets the name of the avatar pose. (Same as the name of the 
    /// <see cref="SkeletonPose"/>.)
    /// </summary>
    /// <value>
    /// The name of the avatar pose. (Same as the name of the <see cref="SkeletonPose"/>.)
    /// </value>
    public string Name
    {
      get { return _skeletonPose.Name; }
      set { _skeletonPose.Name = value; }
    }


    /// <summary>
    /// Gets the facial expression at the current time position.
    /// </summary>
    /// <value>The expression of at the current time position.</value>
    public AvatarExpression Expression
    {
      get { return _expression; }
      set { _expression = value; }
    }
    private AvatarExpression _expression;
    private readonly DelegateAnimatableProperty<AvatarExpression> _expressionWrapper;


    /// <summary>
    /// Gets the skeleton pose at the current time position.
    /// </summary>
    /// <value>The skeleton pose at the current time position.</value>
    public SkeletonPose SkeletonPose
    {
      get { return _skeletonPose; }
    }
    private readonly SkeletonPose _skeletonPose;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="AvatarPose"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="AvatarPose"/> class for the given
    /// avatar renderer.
    /// </summary>
    /// <param name="avatarRenderer">
    /// The avatar renderer. The avatar renderer must already be in the "ready" state 
    /// (<c>AvatarRender.State == AvatarRendererState.Ready</c>), otherwise an exception is 
    /// thrown.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="avatarRenderer"/> is <see langword="null"/>.
    /// </exception>
    public AvatarPose(AvatarRenderer avatarRenderer)
      : this(CreateSkeleton(avatarRenderer))
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AvatarPose"/> class for the given skeleton.
    /// </summary>
    /// <param name="skeleton">The skeleton.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeleton"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="skeleton"/> is not a valid Xbox LIVE Avatar skeleton.
    /// </exception>
    public AvatarPose(Skeleton skeleton)
    {
      if (skeleton.NumberOfBones != AvatarRenderer.BoneCount)
        throw new ArgumentException("The specified skeleton is not a valid Avatar skeleton.", "skeleton");

      _expressionWrapper = new DelegateAnimatableProperty<AvatarExpression>(
        () => _expression,      // Getter
        e => _expression = e);  // Setter

      _skeletonPose = SkeletonPose.Create(skeleton);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a skeleton for the given avatar renderer.
    /// </summary>
    /// <param name="avatarRenderer">The avatar renderer.</param>
    /// <returns>The skeleton.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="avatarRenderer"/> is <see langword="null"/>.
    /// </exception>
    private static Skeleton CreateSkeleton(AvatarRenderer avatarRenderer)
    {
      if (avatarRenderer == null)
        throw new ArgumentNullException("avatarRenderer");

      var skeleton = new Skeleton(avatarRenderer.ParentBones, BoneNames, ConvertToSrt(avatarRenderer.BindPose));
      return skeleton;
    }


    /// <summary>
    /// Converts the bind pose matrices to SRT transforms.
    /// </summary>
    /// <param name="bindPoses">The bind pose matrices.</param>
    /// <returns>The equivalent SRT transforms.</returns>
    private static IList<SrtTransform> ConvertToSrt(IList<Matrix> bindPoses)
    {
      var numberOfBones = bindPoses.Count;
      var result = new SrtTransform[numberOfBones];

      for (int i = 0; i < numberOfBones; i++)
        result[i] = SrtTransform.FromMatrix(bindPoses[i]);

      return result;
    }
    #endregion


    //--------------------------------------------------------------
    #region IAnimatableObject
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IEnumerable<IAnimatableProperty> IAnimatableObject.GetAnimatedProperties()
    {
      if (((IAnimatableProperty)_expressionWrapper).IsAnimated)
        yield return _expressionWrapper;

      if (((IAnimatableProperty)_skeletonPose).IsAnimated)
        yield return _skeletonPose;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      switch (name)
      {
        case "Expression":
          return _expressionWrapper as IAnimatableProperty<T>;
        case "SkeletonPose":
          return _skeletonPose as IAnimatableProperty<T>;
        default:
          return null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region IAvatarAnimation
    //--------------------------------------------------------------

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <value>
    /// The getter always returns <see cref="TimeSpan.Zero"/>. The setter throws a 
    /// <see cref="NotImplementedException"/>.
    /// </value>
    /// <exception cref="NotImplementedException">
    /// <see cref="AvatarPose"/> does not implement a setter for
    /// <see cref="IAvatarAnimation.CurrentPosition"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    TimeSpan IAvatarAnimation.CurrentPosition
    {
      get { return TimeSpan.Zero; }
      set { throw new NotImplementedException("AvatarSkeletonPose does not implement a setter for IAvatarAnimation.CurrentPosition."); }
    }


    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <value>Always returns <see cref="TimeSpan.Zero"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    TimeSpan IAvatarAnimation.Length { get { return TimeSpan.Zero; } }


    /// <summary>
    /// Gets the current position of the bones.
    /// </summary>
    /// <value>The current position of the bones</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    ReadOnlyCollection<Matrix> IAvatarAnimation.BoneTransforms
    {
      get
      {
        if (_readOnlyCollection == null)
          _readOnlyCollection = new ReadOnlyCollection<Matrix>(_boneTransforms);

        Update();

        return _readOnlyCollection;
      }
    }


    private ReadOnlyCollection<Matrix> _readOnlyCollection;
    private readonly Matrix[] _boneTransforms = new Matrix[AvatarRenderer.BoneCount];


    /// <summary>
    /// Updates the current time position of the avatar animation.
    /// </summary>
    /// <param name="elapsedAnimationTime">Ignored.</param>
    /// <param name="loop">Ignored.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void IAvatarAnimation.Update(TimeSpan elapsedAnimationTime, bool loop)
    {
      Update();
    }


    /// <summary>
    /// Updates avatar pose.
    /// </summary>
    private void Update()
    {
      for (int i = 0; i < _boneTransforms.Length; i++)
        _boneTransforms[i] = _skeletonPose.GetBoneTransform(i);
    }
    #endregion
  }
}
#endif
