// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of an animation value and defines operations that can be applied to
  /// animation values.
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <remarks>
  /// <para>
  /// The <see cref="IAnimationValueTraits{T}"/> describe the operations that can be performed on a 
  /// certain type of animation value. This abstraction is necessary in order to treat different 
  /// types of animations with the same code.
  /// </para>
  /// <para>
  /// The interface <see cref="IAnimationValueTraits{T}"/> and its implementing classes are used 
  /// only internally in DigitalRune Animation. It is safe to ignore these types. (The following 
  /// information is only relevant for users who plan to implement new types of animation values and 
  /// want to reuse the existing animation classes.)
  /// </para>
  /// <para>
  /// <strong>Reference Types vs. Value Types:</strong> Animation values can be reference types as
  /// well as value types, which makes the task a little complicated. Most method parameters need to
  /// be passed by reference to efficiently handle both cases.
  /// </para>
  /// <para>
  /// <strong>Create, Recycle and Copy:</strong> The methods <see cref="Create"/> and 
  /// <see cref="Recycle"/> can be used to create and recycle animation values. 
  /// (<see cref="Create"/> returns a previously recycled instance, if any is available. It is 
  /// recommended to recycle and reuse animation values if they allocate memory on the managed 
  /// heap.) 
  /// </para>
  /// <para>
  /// <strong>Set and Reset:</strong> The method <see cref="Set"/> is called when the animation 
  /// system sets an animation value to an <see cref="IAnimatableProperty{T}"/>; <see cref="Reset"/>
  /// is called when all animations are removed from an <see cref="IAnimatableProperty{T}"/>.
  /// </para>
  /// <para>
  /// The method <see cref="Copy"/> copies that data of one animation value to another.
  /// </para>
  /// <para>
  /// <strong>Add, Identity, Invert and Multiply:</strong> In order to handle all types of animation 
  /// with the same code we need to apply some math: The animation values form an 
  /// <see href="http://en.wikipedia.org/wiki/Group_(mathematics)">algebraic group</see>. The group 
  /// has a group operation (see <see cref="Add"/>) that represents the application of an animation 
  /// value to a given value, an identity element (see <see cref="SetIdentity"/>) and a function 
  /// that computes the inverse of an animation value (see <see cref="Invert"/>). For efficiency, 
  /// there is also a function <see cref="Multiply"/> that computes the repeated application of the 
  /// same value. For example, if <b>+</b> represents the group operation then 
  /// <c>Multiply(x,3) = x <b>+</b> x <b>+</b> x</c>.
  /// </para>
  /// <para>
  /// <strong>Group Operation:</strong> The group operation depends on the type of animation value.
  /// Here are some examples: If the animation value is a real number the group operation is a
  /// simple addition. The group operation of a n-dimensional vector is a vector addition. If the
  /// animation value is a unit quaternion that represents a rotation, the group operation is a 
  /// quaternion product (because two quaternions are combined using the quaternion product). Etc.
  /// </para>
  /// <para>
  /// Note that, in general the group operations are not a commutative, meaning that the order of 
  /// the operands is important! For example, the quaternion product is not commutative.
  /// </para>
  /// <para>
  /// <strong>Identity Element:</strong> Every group has an identity element regarding the group 
  /// operation. For example, the identity element of a scalar addition is 0. The identity of the 
  /// vector addition is the zero vector. The identity of the quaternion product is the identity
  /// quaternion. Etc.
  /// </para>
  /// <para>
  /// <strong>Inverse:</strong> Every value of the group has an inverse. For example, the inverse
  /// element of a real number <i>r</i> is -<i>r</i>. The inverse of an n-dimensional vector 
  /// <i>v</i> is -<i>v</i>. The inverse of a unit quaternion <i>q</i> is <i>q</i><sup>-1</sup>. 
  /// Etc.
  /// </para>
  /// <para>
  /// <strong>Interpolation:</strong> Another important part of animation is interpolation 
  /// ("animation blending"). The interpolation of animation values is in most cases a weighted 
  /// combination of the animation values. Animation values need to support two types of 
  /// interpolation:
  /// <list type="bullet">
  /// <item>
  /// <term>Interpolation of two values</term>
  /// <description>
  /// Animations can be concatenated by adding them to animation composition chains. The output of
  /// one stage in the composition chain is blended with the output of the previous stage. The 
  /// interpolation of two animation values is implemented by <see cref="Interpolate"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <term>Interpolation of <i>n</i> values</term>
  /// <description>
  /// Animations can be combined using a <see cref="BlendGroup"/>. The values of all animations in 
  /// the blend group are blended together. A group can contain more than two animations. The 
  /// blending of <i>n</i> animation values is implemented by <see cref="BeginBlend"/>, 
  /// <see cref="BlendNext"/>, and <see cref="EndBlend"/>. (The blend operation is split into 3
  /// operations which need to be called successively. <see cref="BlendNext"/> needs to be called
  /// for each animation value.)
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Optimizations:</strong> In order to improve performance - in particular on the Xbox
  /// 360 - most parameters are passed by reference. All operation happen in-place, i.e. no new 
  /// objects are allocated.
  /// </para>
  /// </remarks>
  public interface IAnimationValueTraits<T>
  {
    /// <summary>
    /// Creates an animation value. (If the animation value is a heap object, then method reuses any
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="reference">
    /// In: A reference value. This value serves as a reference for allocating a new value of the 
    /// same type. For example, if <paramref name="value"/> needs to be initialized with a certain
    /// settings, the settings can be copied from <paramref name="reference"/>.
    /// </param>
    /// <param name="value">Out: A new animation value.</param>
    /// <seealso cref="Recycle"/>
    void Create(ref T reference, out T value);


    /// <summary>
    /// Recycles an animation value.
    /// </summary>
    /// <param name="value">In/Out: The animation value to be recycled.</param>
    /// <seealso cref="Create"/>
    void Recycle(ref T value);

    
    /// <summary>
    /// Copies the specified animation value.
    /// </summary>
    /// <param name="source">In: The source value.</param>
    /// <param name="target">Out: The target value.</param>
    void Copy(ref T source, ref T target);


    /// <summary>
    /// Sets the animation value of the given <see cref="IAnimatableProperty{T}"/>.
    /// </summary>
    /// <param name="value">In: The value to write to <paramref name="property"/>.</param>
    /// <param name="property">
    /// The <see cref="IAnimatableProperty{T}"/> that stores the animation value.
    /// </param>
    void Set(ref T value, IAnimatableProperty<T> property);


    /// <summary>
    /// Resets the animation value of the given <see cref="IAnimatableProperty{T}"/>.
    /// </summary>
    /// <param name="property">The <see cref="IAnimatableProperty{T}"/>.</param>
    void Reset(IAnimatableProperty<T> property);


    /// <summary>
    /// Gets the identity.
    /// </summary>
    /// <param name="identity">Out: The identity.</param>
    void SetIdentity(ref T identity);


    /// <summary>
    /// Gets the inverse of an animation value.
    /// </summary>
    /// <param name="value">In: The animation value.</param>
    /// <param name="inverse">Out: The inverse of <paramref name="value"/>.</param>
    void Invert(ref T value, ref T inverse);


    /// <summary>
    /// Adds the given animation values.
    /// </summary>
    /// <param name="value0">In: The first value.</param>
    /// <param name="value1">In: The second value.</param>
    /// <param name="result">
    /// Out: The sum <paramref name="value0"/> + <paramref name="value1"/>.
    /// </param>
    /// <remarks>
    /// For some types the add operation is not commutative. This is the case if
    /// <paramref name="value0"/> and <paramref name="value1"/> represent transformations. In this
    /// case this method returns a combined transformation where <paramref name="value0"/> 
    /// is applied before <paramref name="value1"/>.
    /// </remarks>
    void Add(ref T value0, ref T value1, ref T result);


    /// <summary>
    /// Multiplies an animation value by a given factor.
    /// </summary>
    /// <param name="value">In: The value.</param>
    /// <param name="factor">The factor.</param>
    /// <param name="result">
    /// Out: The product of <paramref name="value"/> and <paramref name="factor"/>.
    /// </param>
    void Multiply(ref T value, int factor, ref T result);


    /// <summary>
    /// Performs a linear interpolation between two animation values.
    /// </summary>
    /// <param name="source">In: The source value.</param>
    /// <param name="target">In: The target value.</param>
    /// <param name="parameter">
    /// The interpolation parameter; also known as <i>interpolation factor</i> or <i>weight of the 
    /// target value</i>.
    /// </param>
    /// <param name="result">Out: The result of the interpolation.</param>
    void Interpolate(ref T source, ref T target, float parameter, ref T result);


    /// <summary>
    /// Begins the interpolation of <i>n</i> animation values.
    /// </summary>
    /// <param name="value">Out: The start value of the blend operation.</param>
    void BeginBlend(ref T value);


    /// <summary>
    /// Blends the given animation value to the current value.
    /// </summary>
    /// <param name="value">
    /// In/Out: The current animation value. (The intermediate result of the blend operation).
    /// </param>
    /// <param name="nextValue">
    /// In: The next animation value which should be blended to <paramref name="value"/>.
    /// </param>
    /// <param name="normalizedWeight">
    /// The normalized weight of <paramref name="nextValue"/>. ('Normalized' means that the sum of 
    /// the animation weights need to be 1.)
    /// </param>
    void BlendNext(ref T value, ref T nextValue, float normalizedWeight);


    /// <summary>
    /// Finalizes the interpolation of <i>n</i> animation values.
    /// </summary>
    /// <param name="value">
    /// In: The current animation value. (The intermediate result of the blend operation.)<br/>
    /// Out: The result of the blend operation.
    /// </param>
    void EndBlend(ref T value);
  }
}
