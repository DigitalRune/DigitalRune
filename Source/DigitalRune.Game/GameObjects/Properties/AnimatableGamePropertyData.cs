// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Animation;


namespace DigitalRune.Game
{
  /// <summary>
  /// Wraps a <see cref="GameProperty{T}"/> and makes it animatable.
  /// </summary>
  /// <typeparam name="T">The type of the property value.</typeparam>
  internal sealed class AnimatableGamePropertyData<T> : GamePropertyData<T>, IAnimatableProperty<T>
  {
    // The base.Value is the BaseValue. This class adds the AnimationValue.
    // This class needs to handle events itself because users will get direct access via
    // IAnimatableProperty.
    //
    // Important:
    // Do not mix up 
    // - HasLocalValue and base.HasLocalValue
    // - Value and base.Value


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly GameObject _owner;
    private readonly GamePropertyMetadata<T> _metadata;

    private T _animationValue;
    private bool _isAnimated;

    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public override T Value
    {
      get
      {
        // Animated value.
        if (IsAnimated)
          return _animationValue;

        // Local value
        if (base.HasLocalValue)
          return base.Value;

        // Template value
        if (_owner.Template != null)
          return new GameProperty<T>(_owner.Template, _metadata).Value;

        // Metadata default value
        return _metadata.DefaultValue;
      }
      set
      {
        // Remember old value (= animated value, local value, template value or default value).
        T oldValue = Value;
        T newValue = value;

        if (!IsAnimated)
        {
          // This property is not animated. Therefore it behaves like a normal property.
          // Do the same as GameProperty<T>.setValue.

          // Now we have a local value, even if the new value is the same as the default value!
          base.HasLocalValue = true;
          base.Value = oldValue;

          // Skip events if oldValue == newValue.
          if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

          // Raise Changing event.
          var property = new GameProperty<T>(_owner, _metadata);
          OnChanging(property, oldValue, ref newValue);

          // Raise Changed event if value has changed.
          if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
          {
            // Set coerced value.
            base.Value = newValue;
            OnChanged(property, oldValue, newValue);
          }
        }
        else
        {
          // The value is animated. We must change BaseValue without events!

          // Now we have a local value, even if the new value is the same as the default value!
          base.HasLocalValue = true;
          base.Value = value;
        }
      }
    }


    public override bool HasLocalValue
    {
      get { return base.HasLocalValue || IsAnimated; }
      set { base.HasLocalValue = value; }
    }


    public T BaseValue
    {
      get
      {
        // Local value
        if (base.HasLocalValue)
          return base.Value;

        // Template value
        if (_owner.Template != null)
          return new GameProperty<T>(_owner.Template, _metadata).Value;

        // Metadata default value
        return _metadata.DefaultValue;
      }
    }


    object IAnimatableProperty.BaseValue
    {
      get { return BaseValue; }
    }


    public bool HasBaseValue
    {
      get { return true; }
    }


    public T AnimationValue
    {
      get { return _animationValue; }
      set
      {
        if (!IsAnimated)
        {
          _animationValue = value;
          return;
        }

        T oldValue = _animationValue;
        T newValue = value;

        // Skip events if oldValue == newValue.
        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
          return;

        // Raise Changing event.
        var property = new GameProperty<T>(_owner, _metadata);
        OnChanging(property, oldValue, ref newValue);

        // Raise Changed event if value has changed.
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
          // Set coerced value.
          _animationValue = newValue;
          OnChanged(property, oldValue, newValue);
        }
      }
    }



    object IAnimatableProperty.AnimationValue
    {
      get { return _animationValue; }
    }


    public bool IsAnimated
    {
      get { return _isAnimated; }
      set
      {
        if (_isAnimated == value)
          return;

        _isAnimated = value;

        if (_isAnimated)
        {
          // Setting IsAnimated from false to true.

          // Remember old value (= local value, template value or default value).
          T oldValue = BaseValue;
          T newValue = _animationValue;

          // Skip events if oldValue == newValue.
          if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

          // Raise Changing event.
          var property = new GameProperty<T>(_owner, _metadata);
          OnChanging(property, oldValue, ref newValue);

          // Raise Changed event if value has changed.
          if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
          {
            // Set coerced value.
            _animationValue = newValue;
            OnChanged(property, oldValue, newValue);
          }
        }
        else
        {
          // Setting IsAnimated from true to false.

          // Remember old value (= animated value, local value, template value or default value).
          T oldValue = _animationValue;
          T newValue = BaseValue;

          // Skip events if oldValue == newValue.
          if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

          // Raise Changing event.
          var property = new GameProperty<T>(_owner, _metadata);
          OnChanging(property, oldValue, ref newValue);

          // Raise Changed event if value has changed.
          if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
          {
            // Set coerced value.
            base.Value = newValue;
            OnChanged(property, oldValue, newValue);
          }
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public AnimatableGamePropertyData(GameObject owner, GamePropertyMetadata<T> metadata, GamePropertyData<T> data)
      : base(data)
    {
      Debug.Assert(owner != null, "owner must not be null.");
      Debug.Assert(metadata != null, "metadata must not be null.");

      _owner = owner;
      _metadata = metadata;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
