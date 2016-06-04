using System.Collections.Generic;
using DigitalRune.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Animation
{
  // A simple sprite class.
  public class Sprite
  {
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _texture;

    public Vector2 Position { get; set; }
    public Color Color { get; set; }

    public Sprite(SpriteBatch spriteBatch, Texture2D texture)
    {
      _spriteBatch = spriteBatch;
      _texture = texture;
    }

    public void Draw()
    {
      // Draw sprite centered at Position.
      Vector2 position = Position - new Vector2(_texture.Width, _texture.Height) / 2.0f;

      _spriteBatch.Begin();
      _spriteBatch.Draw(_texture, position, Color);
      _spriteBatch.End();
    }
  }


  // This class inherits from Sprite. This class implements IAnimatableObject and makes the 
  // Position and the Color property animatable.
  public class AnimatableSprite : Sprite, IAnimatableObject
  {
    // A DelegateAnimatableProperty is an IAnimatableProperty that uses delegate methods
    // to get/set the animated value.
    private readonly DelegateAnimatableProperty<Vector2> _animatablePosition;
    private readonly DelegateAnimatableProperty<Color> _animatableColor;

    // Animatable objects need to have a name. This name is referenced in ITimeline.TargetObject.
    public string Name { get; set; }

    public AnimatableSprite(string name, SpriteBatch spriteBatch, Texture2D texture)
      : base(spriteBatch, texture)
    {
      Name = name;

      _animatablePosition = new DelegateAnimatableProperty<Vector2>(
        () => Position,
        v => Position = v);

      _animatableColor = new DelegateAnimatableProperty<Color>(
        () => Color,
        c => Color = c);
    }

    // Enumerate all animatable properties.
    public IEnumerable<IAnimatableProperty> GetAnimatedProperties()
    {
      yield return _animatablePosition;
      yield return _animatableColor;
    }

    // Get an animatable property by name. This method is used by the animation service to
    // get a property set in IAnimation.TargetProperty.
    public IAnimatableProperty<T> GetAnimatableProperty<T>(string name)
    {
      switch (name)
      {
        case "Position":
          return _animatablePosition as IAnimatableProperty<T>;
        case "Color":
          return _animatableColor as IAnimatableProperty<T>;
        default:
          return null;
      }
    }
  }
}
