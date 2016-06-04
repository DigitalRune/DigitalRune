// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Builds a material, which can be used for rendering a model.
  /// </summary>
  [ContentProcessor(DisplayName = "Material - DigitalRune Graphics")]
  public class DRMaterialProcessor : ContentProcessor<MaterialContent, DRMaterialContent>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The name of the default render pass.
    /// </summary>
    public const string DefaultPass = "Default";
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the color used when color keying for a texture is enabled. When color keying, 
    /// all pixels of a specified color are replaced with transparent black.
    /// </summary>
    /// <value>Color value of the material to replace with transparent black.</value>
    [DefaultValue(typeof(Color), "255, 0, 255, 255")]
    [DisplayName("Color Key Color")]
    [Description("If the model's textures are color keyed, pixels of this color are replaced with transparent black.")]
    public virtual Color ColorKeyColor
    {
      get { return _colorKeyColor; }
      set { _colorKeyColor = value; }
    }
    private Color _colorKeyColor = Color.Magenta;


    /// <summary>
    /// Gets or sets a value indicating whether color keying of a model is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if color keying is enabled; <see langword="false"/> otherwise.
    /// </value>
    [DefaultValue(false)]
    [DisplayName("Color Key Enabled")]
    [Description("If enabled, the model's textures are color keyed. Pixels matching the value of \"Color Key Color\" are replaced with transparent black.")]
    public virtual bool ColorKeyEnabled
    {
      get { return _colorKeyEnabled; }
      set { _colorKeyEnabled = value; }
    }
    private bool _colorKeyEnabled;


    /// <summary>
    /// Gets or sets a value indicating whether a full chain of mipmaps is generated from the source 
    /// material. Existing mipmaps of the material are not replaced.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if mipmap generation is enabled; <see langword="false"/> otherwise.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [DefaultValue(true)]
    [DisplayName("Generate Mipmaps")]
    [Description("If enabled, a full mipmap chain is generated for the model's textures. Existing mipmaps are not replaced.")]
    public virtual bool GenerateMipmaps
    {
      get { return _generateMipmaps; }
      set { _generateMipmaps = value; }
    }
    private bool _generateMipmaps = true;


    /// <summary>
    /// Gets or sets the gamma of the input textures.
    /// </summary>
    /// <value>The gamma of the input textures. The default value is 2.2.</value>
    [DefaultValue(2.2f)]
    [DisplayName("Input Gamma")]
    [Description("Specifies the gamma of the input textures.")]
    public virtual float InputTextureGamma
    {
      get { return _inputTextureGamma; }
      set { _inputTextureGamma = value; }
    }
    private float _inputTextureGamma = 2.2f;


    /// <summary>
    /// Gets or sets the gamma of the output textures.
    /// </summary>
    /// <value>The gamma of the output textures. The default value is 2.2.</value>
    [DefaultValue(2.2f)]
    [DisplayName("Output Gamma")]
    [Description("Specifies the gamma of the output textures.")]
    public virtual float OutputTextureGamma
    {
      get { return _outputTextureGamma; }
      set { _outputTextureGamma = value; }
    }
    private float _outputTextureGamma = 2.2f;


    /// <summary>
    /// Gets or sets a value indicating whether alpha premultiply of textures is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if alpha premultiply is enabled; otherwise, <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [DefaultValue(true)]
    [DisplayName("Premultiply Texture Alpha")]
    [Description("If enabled, the model's textures are converted to premultiplied alpha format.")]
    public virtual bool PremultiplyTextureAlpha
    {
      get { return _premultiplyTextureAlpha; }
      set { _premultiplyTextureAlpha = value; }
    }
    private bool _premultiplyTextureAlpha = true;


    /// <summary>
    /// Gets or sets a value indicating whether resizing of textures are enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if resizing is enabled; <see langword="false"/> otherwise.
    /// </value>
    /// <remarks>
    /// Typically used to maximize compatibility with a graphics card because many graphics cards 
    /// do not support a material size that is not a power of two. If 
    /// <see cref="ResizeTexturesToPowerOfTwo"/> is enabled, textures are resized to the next 
    /// largest power of two.
    /// </remarks>
    [DefaultValue(false)]
    [DisplayName("Resize Textures to Power of Two")]
    [Description("If enabled, the model's existing textures are resized to the next largest power of two, maximizing compatibility. Many graphics cards do not support textures sizes that are not a power of two.")]
    public virtual bool ResizeTexturesToPowerOfTwo
    {
      get { return _resizeTexturesToPowerOfTwo; }
      set { _resizeTexturesToPowerOfTwo = value; }
    }
    private bool _resizeTexturesToPowerOfTwo;


    /// <summary>
    /// Gets or sets the texture format of output materials.
    /// </summary>
    /// <value>The texture format of the output.</value>
    /// <remarks>
    /// Materials can either be left unchanged from the source asset, converted to a corresponding 
    /// <see cref="Color"/>, or compressed using the appropriate 
    /// <see cref="DRTextureFormat.Dxt"/> format.
    /// </remarks>
    [DefaultValue(DRTextureFormat.Dxt)]
    [DisplayName("Texture Format")]
    [Description("Specifies the SurfaceFormat type of processed textures. Textures can either remain unchanged the source asset, converted to the Color format, DXT compressed, or DXT5nm compressed.")]
    public virtual DRTextureFormat TextureFormat
    {
      get { return _textureFormat; }
      set { _textureFormat = value; }
    }
    private DRTextureFormat _textureFormat = DRTextureFormat.Dxt;


    /// <summary>
    /// Gets or sets the reference alpha value, which is used in the alpha test.
    /// </summary>
    /// <value>The reference alpha value, which is used in the alpha test.</value>
    [DefaultValue(0.9f)]
    [DisplayName("Reference Alpha")]
    [Description("Specifies the reference alpha value, which is used in the alpha test.")]
    public virtual float ReferenceAlpha
    {
      get { return _referenceAlpha; }
      set { _referenceAlpha = value; }
    }
    private float _referenceAlpha = 0.9f;


    /// <summary>
    /// Gets or sets a value indicating whether the alpha of the textures should be scaled to 
    /// achieve the same alpha test coverage as in the source images.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to scale the alpha values of the textures; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    [DefaultValue(false)]
    [DisplayName("Scale Alpha To Coverage")]
    [Description("Specifies whether the alpha of the textures should be scaled to achieve the same alpha test coverage as in the source images.")]
    public virtual bool ScaleTextureAlphaToCoverage
    {
      get { return _scaleTextureAlphaToCoverage; }
      set { _scaleTextureAlphaToCoverage = value; }
    }
    private bool _scaleTextureAlphaToCoverage;


    /// <summary>
    /// Gets or sets the type of the default effect.
    /// </summary>
    /// <value>The type of the default effect.</value>
    /// <remarks>
    /// If the model does not contain effect code or reference a effect file, then the default 
    /// effect file will be used. If the type is <see cref="DREffectType.CustomEffect"/>, then
    /// <see cref="DefaultEffectFile"/> must be set.
    /// </remarks>
    [DefaultValue(DREffectType.BasicEffect)]
    [DisplayName("Default Effect Type")]
    [Description("Specifies the default effect. If the model does not contain effect code or reference a effect file, then the default effect file will be used. (If the CustomEffect is selected, DefaultEffectFile must be set.)")]
    public virtual DREffectType DefaultEffectType
    {
      get { return _defaultEffectType; }
      set { _defaultEffectType = value; }
    }
    private DREffectType _defaultEffectType = DREffectType.BasicEffect;


    /// <summary>
    /// Gets or sets the path of the default DirectX effect file.
    /// </summary>
    /// <value>The path of the default DirectX effect file.</value>
    /// <remarks>
    /// If the model does not contain effect code or reference a effect file, then the default 
    /// effect file will be used. If <see cref="DefaultEffectType"/> is set to 
    /// <see cref="DREffectType.CustomEffect"/>, then this property must specify a DirectX effect
    /// file. (The path of the filename is relative to the content root directory.)
    /// </remarks>
    [DefaultValue("")]
    [DisplayName("Default Effect Path")]
    [Description("Specifies the default DirectX effect file containing a custom effect. If the model does not contain effect code or reference a effect file and Default Effect Type is set to CustomEffect, then this default effect file will be used. (The path of the filename is relative to the content root directory.)")]
    public virtual string DefaultEffectFile
    {
      get { return _defaultEffectFile; }
      set { _defaultEffectFile = value; }
    }
    private string _defaultEffectFile = "";
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Processes the specified input data and returns the result.
    /// </summary>
    /// <param name="input">Existing content object being processed.</param>
    /// <param name="context">Contains any required custom process parameters.</param>
    /// <returns>A typed object representing the processed input.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="input"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public override DRMaterialContent Process(MaterialContent input, ContentProcessorContext context)
    {
      if (input == null)
        throw new ArgumentNullException("input");
      if (context == null)
        throw new ArgumentNullException("context");

      var material = input as DRMaterialContent;
      if (material != null && material.Definition != null)
      {
        // DigitalRune Material
        return ProcessInternal(material, context);
      }
      else
      {
        // XNA Material
        return ProcessInternal(input, context);
      }
    }


    // Process DigitalRune material.
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private DRMaterialContent ProcessInternal(DRMaterialContent material, ContentProcessorContext context)
    {
      material.Passes = new Dictionary<string, DREffectBindingContent>();

      // Parse XML file.
      var document = material.Definition;
      var identity = material.Identity;

      var materialElement = document.Root;
      if (materialElement == null || materialElement.Name != "Material")
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Root element \"<Material>\" is missing in XML.");
        throw new InvalidContentException(message, identity);
      }

      // Override material name, if attribute is set.
      material.Name = (string)materialElement.Attribute("Name") ?? material.Name;

      // Create effect bindings for all render passes.
      foreach (var passElement in materialElement.Elements("Pass"))
      {
        string pass = passElement.GetMandatoryAttribute("Name", identity);
        if (material.Passes.ContainsKey(pass))
        {
          string message = XmlHelper.GetExceptionMessage(passElement, "Duplicate entry. The pass \"{0}\" was already defined.", pass);
          throw new InvalidContentException(message, identity);
        }

        var binding = new DREffectBindingContent { Name = pass, Identity = identity };

        // Skip this pass if the graphics profile does not match the actual target profile.
        string profile = (string)passElement.Attribute("Profile") ?? "ANY";
        string profileLower = profile.ToUpperInvariant();
        if (profileLower == "REACH")
        {
          if (context.TargetProfile != GraphicsProfile.Reach)
            continue;
        }
        else if (profileLower == "HIDEF")
        {
          if (context.TargetProfile != GraphicsProfile.HiDef)
            continue;
        }
        else if (profileLower != "ANY")
        {
          string message = XmlHelper.GetExceptionMessage(passElement, "Unknown profile: \"{0}\". Allowed values are \"Reach\", \"HiDef\" or \"Any\"", profile);
          throw new InvalidContentException(message, identity);
        }

        // ----- Effect
        string effectName = passElement.GetMandatoryAttribute("Effect", identity);
        switch (effectName)
        {
          case "AlphaTestEffect":
            binding.EffectType = DREffectType.AlphaTestEffect;
            break;
          case "BasicEffect":
            binding.EffectType = DREffectType.BasicEffect;
            break;
          case "DualTextureEffect":
            binding.EffectType = DREffectType.DualTextureEffect;
            break;
          case "EnvironmentMapEffect":
            binding.EffectType = DREffectType.EnvironmentMapEffect;
            break;
          case "SkinnedEffect":
            binding.EffectType = DREffectType.SkinnedEffect;
            break;
          default:
            binding.EffectType = DREffectType.CustomEffect;
            if (string.IsNullOrEmpty(Path.GetExtension(effectName)))
            {
              // The effect is a prebuilt asset. effectName is the name of the asset,
              // for example: effectName = "DigitalRune\GBuffer".
              binding.EffectAsset = effectName;
            }
            else
            {
              // The effect is a local .fx file.
              effectName = ContentHelper.FindFile(effectName, identity);
              binding.Effect = new ExternalReference<EffectContent>(effectName);
            }
            break;
        }

        ProcessEffect(binding, context);

        // ----- Parameters
        foreach (var parameterElement in passElement.Elements("Parameter"))
        {
          string name = parameterElement.GetMandatoryAttribute("Name", identity);
          if (binding.OpaqueData.ContainsKey(name))
          {
            string message = XmlHelper.GetExceptionMessage(parameterElement, "Duplicate entry. The parameter \"{0}\" was already defined.", name);
            throw new InvalidContentException(message, identity);
          }

          object value = parameterElement.Attribute("Value").ToParameterValue(null, identity);
          if (value != null)
            binding.OpaqueData.Add(name, value);
        }

        // ----- Textures
        foreach (var textureElement in passElement.Elements("Texture"))
        {
          string name = textureElement.GetMandatoryAttribute("Name", identity);
          if (binding.Textures.ContainsKey(name))
          {
            string message = XmlHelper.GetExceptionMessage(textureElement, "Duplicate entry. The texture \"{0}\" was already defined.", name);
            throw new InvalidContentException(message, identity);
          }

          string fileName = textureElement.GetMandatoryAttribute("File", identity);
          fileName = ContentHelper.FindFile(fileName, identity);

          // Texture processor parameters.
          var colorKeyAttribute = textureElement.Attribute("ColorKey");
          bool colorKeyEnabled = colorKeyAttribute != null;
          Color colorKeyColor = colorKeyAttribute.ToColor(Color.Magenta, identity);
          bool generateMipmaps = (bool?)textureElement.Attribute("GenerateMipmaps") ?? true;
          float inputGamma = (float?)textureElement.Attribute("InputGamma") ?? 2.2f;
          float outputGamma = (float?)textureElement.Attribute("OutputGamma") ?? 2.2f;
          bool premultiplyAlpha = (bool?)textureElement.Attribute("PremultiplyAlpha") ?? true;
          bool resizeToPowerOfTwo = (bool?)textureElement.Attribute("ResizeToPowerOfTwo") ?? false;
          DRTextureFormat textureFormat = textureElement.Attribute("Format").ToTextureFormat(DRTextureFormat.Dxt, identity);
          float referenceAlpha = (float?)textureElement.Attribute("ReferenceAlpha") ?? 0.9f;
          bool scaleAlphaToCoverage = (bool?)textureElement.Attribute("ScaleAlphaToCoverage") ?? false;

          // Store texture parameters in opaque data.
          var texture = new ExternalReference<TextureContent>(fileName);
          var defaultTextureProcessor = new DRTextureProcessor();
          if (colorKeyColor != defaultTextureProcessor.ColorKeyColor)
            texture.OpaqueData.Add("ColorKeyColor", colorKeyColor);
          if (colorKeyEnabled != defaultTextureProcessor.ColorKeyEnabled)
            texture.OpaqueData.Add("ColorKeyEnabled", colorKeyEnabled);
          if (generateMipmaps != defaultTextureProcessor.GenerateMipmaps)
            texture.OpaqueData.Add("GenerateMipmaps", generateMipmaps);
          if (inputGamma != defaultTextureProcessor.InputGamma)
            texture.OpaqueData.Add("InputGamma", inputGamma);
          if (outputGamma != defaultTextureProcessor.OutputGamma)
            texture.OpaqueData.Add("OutputGamma", outputGamma);
          if (premultiplyAlpha != defaultTextureProcessor.PremultiplyAlpha)
            texture.OpaqueData.Add("PremultiplyAlpha", premultiplyAlpha);
          if (resizeToPowerOfTwo != defaultTextureProcessor.ResizeToPowerOfTwo)
            texture.OpaqueData.Add("ResizeToPowerOfTwo", resizeToPowerOfTwo);
          if (textureFormat != defaultTextureProcessor.Format)
            texture.OpaqueData.Add("Format", textureFormat);
          if (referenceAlpha != defaultTextureProcessor.ReferenceAlpha)
            texture.OpaqueData.Add("ReferenceAlpha", referenceAlpha);
          if (scaleAlphaToCoverage != defaultTextureProcessor.ScaleAlphaToCoverage)
            texture.OpaqueData.Add("ScaleAlphaToCoverage", scaleAlphaToCoverage);

          binding.Textures.Add(name, texture);
        }

        ProcessTextures(binding, context, identity);

        material.Passes.Add(pass, binding);
      }

      return material;
    }


    // Process XNA material.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private DRMaterialContent ProcessInternal(MaterialContent input, ContentProcessorContext context)
    {
      // Create effect binding for default render pass.
      var binding = new DREffectBindingContent
      {
        Name = DefaultPass,
        Identity = input.Identity
      };

      // Copy opaque data from material content.
      ValidateOpaqueData(input);

      foreach (var entry in input.OpaqueData)
        binding.OpaqueData.Add(entry.Key, entry.Value);

      foreach (var entry in input.Textures)
        binding.Textures.Add(entry.Key, entry.Value);

      // ----- Effect
      if (input is AlphaTestMaterialContent)
      {
        binding.EffectType = DREffectType.AlphaTestEffect;
      }
      else if (input is DualTextureMaterialContent)
      {
        binding.EffectType = DREffectType.DualTextureEffect;
      }
      else if (input is EnvironmentMapMaterialContent)
      {
        binding.EffectType = DREffectType.EnvironmentMapEffect;
      }
      else if (input is SkinnedMaterialContent)
      {
        binding.EffectType = DREffectType.SkinnedEffect;
      }
      else
      {
        var effectMaterial = input as EffectMaterialContent;
        if (effectMaterial == null || effectMaterial.Effect == null || string.IsNullOrEmpty(effectMaterial.Effect.Filename))
        {
          // The material is either
          //  - a BasicMaterialContent (default)
          //  - or an EffectMaterialContent without an effect.
          // --> Use the DefaultEffect.
          binding.EffectType = DefaultEffectType;

          if (DefaultEffectType == DREffectType.CustomEffect)
          {
            if (String.IsNullOrEmpty(DefaultEffectFile))
              throw new InvalidContentException("DefaultEffectType is set to CustomEffect, but DefaultEffectFile is null or empty.", input.Identity);

            string fileName = ContentHelper.FindFile(DefaultEffectFile, input.Identity);
            binding.Effect = new ExternalReference<EffectContent>(fileName);
          }
        }
      }

      ProcessEffect(binding, context);

      // ----- Textures
      foreach (var texture in binding.Textures.Values)
      {
        // Store texture parameters in opaque data.
        texture.OpaqueData.Clear();
        texture.OpaqueData.Add("ColorKeyColor", ColorKeyColor);
        texture.OpaqueData.Add("ColorKeyEnabled", ColorKeyEnabled);
        texture.OpaqueData.Add("GenerateMipmaps", GenerateMipmaps);
        texture.OpaqueData.Add("InputGamma", InputTextureGamma);
        texture.OpaqueData.Add("OutputGamma", OutputTextureGamma);
        texture.OpaqueData.Add("PremultiplyAlpha", PremultiplyTextureAlpha);
        texture.OpaqueData.Add("ResizeToPowerOfTwo", ResizeTexturesToPowerOfTwo);
        texture.OpaqueData.Add("Format", TextureFormat);
        texture.OpaqueData.Add("ReferenceAlpha", ReferenceAlpha);
        texture.OpaqueData.Add("ScaleAlphaToCoverage", ScaleTextureAlphaToCoverage);
      }

      ProcessTextures(binding, context, input.Identity);

      // Create DigitalRune material with default render pass.
      return new DRMaterialContent
      {
        Name = input.Name,
        Identity = input.Identity,
        Passes = new Dictionary<string, DREffectBindingContent>
        {
          { DefaultPass, binding }
        }
      };
    }


    private void ProcessEffect(DREffectBindingContent binding, ContentProcessorContext context)
    {
      if (binding.EffectType == DREffectType.CustomEffect)
      {
        if (!string.IsNullOrEmpty(binding.EffectAsset))
        {
          // The effect is a prebuilt asset.
          return;
        }

        // Build effect.
        if (binding.Effect == null)
        {
          string message = string.Format(CultureInfo.InvariantCulture, "Material \"{0}\" does not have an effect.", binding.Name);
          throw new InvalidContentException(message, binding.Identity);
        }

        if (String.IsNullOrEmpty(binding.Effect.Filename))
        {
          string message = string.Format(CultureInfo.InvariantCulture, "Material \"{0}\" does not have a valid effect file. File name is null or empty.", binding.Name);
          throw new InvalidContentException(message, binding.Identity);
        }

        binding.CompiledEffect = OnBuildEffect(binding.Effect, context);
      }
    }


    /// <summary>
    /// Builds effect content.
    /// </summary>
    /// <param name="effect">An external reference to the effect content.</param>
    /// <param name="context">Context for the specified processor.</param>
    /// <returns>A platform-specific compiled binary effect.</returns>
    /// <remarks>
    /// If the input to process is of type <see cref="EffectMaterialContent"/>, this function will 
    /// be called to request that the <see cref="EffectContent"/> be built. The 
    /// <see cref="EffectProcessor"/> is used to process the <see cref="EffectContent"/>. Subclasses 
    /// of <see cref="MaterialProcessor"/> can override this function to modify the parameters used 
    /// to build <see cref="EffectContent"/>. For example, a different version of this function 
    /// could request a different processor for the <see cref="EffectContent"/>. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual ExternalReference<CompiledEffectContent> OnBuildEffect(ExternalReference<EffectContent> effect, ContentProcessorContext context)
    {
#if MONOGAME
      return context.BuildAsset<EffectContent, CompiledEffectContent>(effect, typeof(EffectProcessor).Name);
#else
      if (string.IsNullOrEmpty(ContentHelper.GetMonoGamePlatform()))
        return context.BuildAsset<EffectContent, CompiledEffectContent>(effect, typeof(EffectProcessor).Name);
      else
        return context.BuildAsset<EffectContent, CompiledEffectContent>(effect, "MGEffectProcessor");
#endif
    }


    private void ProcessTextures(DREffectBindingContent material, ContentProcessorContext context, ContentIdentity identity)
    {
      // We have to call OnBuildTexture (which calls BuildAsset) to replace the 
      // current texture references (.jpg, .bmp, etc.) with references to the built 
      // textures (.xnb). 

      // For stock effects we remove all unnecessary textures.
      if (material.EffectType == DREffectType.AlphaTestEffect
          || material.EffectType == DREffectType.BasicEffect
          || material.EffectType == DREffectType.SkinnedEffect)
      {
        var texture = material.Texture;
        material.Textures.Clear();

        if (texture != null)
          material.Texture = BuildTexture("Texture", texture, context, identity);

        return;
      }

      if (material.EffectType == DREffectType.DualTextureEffect)
      {
        var texture = material.Texture;
        var texture2 = material.Texture2;
        material.Textures.Clear();

        if (texture != null)
          material.Texture = BuildTexture(DualTextureMaterialContent.TextureKey, texture, context, identity);
        if (texture2 != null)
          material.Texture2 = BuildTexture(DualTextureMaterialContent.Texture2Key, texture2, context, identity);

        return;
      }

      if (material.EffectType == DREffectType.EnvironmentMapEffect)
      {
        var texture = material.Texture;
        var environmentMap = material.EnvironmentMap;
        material.Textures.Clear();

        if (texture != null)
          material.Texture = BuildTexture(EnvironmentMapMaterialContent.TextureKey, texture, context, identity);
        if (environmentMap != null)
          material.EnvironmentMap = BuildTexture(EnvironmentMapMaterialContent.EnvironmentMapKey, environmentMap, context, identity);

        return;
      }

      // Custom effect: Build all textures stored in the MaterialContent.
      foreach (var entry in material.Textures.ToArray())
        material.Textures[entry.Key] = BuildTexture(entry.Key, entry.Value, context, identity);
    }


    private ExternalReference<TextureContent> BuildTexture(string textureName, ExternalReference<TextureContent> texture, ContentProcessorContext context, ContentIdentity identity)
    {
      // Finding resources is not as robust in MonoGame: Fix path.
      texture.Filename = ContentHelper.FindFile(texture.Filename, identity);

      return OnBuildTexture(textureName, texture, context);
    }


    /// <summary>
    /// Builds texture content.
    /// </summary>
    /// <param name="textureName">
    /// The name of the texture. This should correspond to the key used to store the texture in
    /// <see cref="MaterialContent.Textures"/>.
    /// </param>
    /// <param name="texture">
    /// The asset to build. This should be a member of <see cref="MaterialContent.Textures"/>. The
    /// opaque data dictionary stored in <paramref name="texture"/> may contain parameters for the 
    /// <see cref="DRTextureProcessor"/>.
    /// </param>
    /// <param name="context">Context for the specified processor.</param>
    /// <returns>The reference to the built texture.</returns>
    /// <remarks>
    /// <paramref name="textureName"/> can be used to determine which processor to use. For example,
    /// if a texture is being used as a normal map, the user may not want to use DXT compression.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual ExternalReference<TextureContent> OnBuildTexture(string textureName, ExternalReference<TextureContent> texture, ContentProcessorContext context)
    {
      // Processor parameters are stored in opaque data!
      var processorParameters = texture.OpaqueData;

      return context.BuildAsset<TextureContent, TextureContent>(texture, typeof(DRTextureProcessor).Name, processorParameters, typeof(DRTextureImporter).Name, null);
    }


    private static void ValidateOpaqueData(ContentItem input)
    {
      foreach (var data in input.OpaqueData)
      {
        if (data.Key == "Effect" || data.Key == "CompiledEffect")
        {
          // "Effect" and "CompiledEffect" have already been handled.
          continue;
        }

        // All other opaque data only make sense if the can be used as effect parameter 
        // value. Only certain types can be used for effect parameters.
        if (!ContentHelper.IsValidTypeForEffectParameter(data.Value))
        {
          string message = string.Format(
            CultureInfo.InvariantCulture,
            "Material pass \"{0}\" contains invalid type for effect parameter. Name = \"{1}\", Type = \"{2}\".",
            input.Name, data.Key, data.Value.GetType());
          throw new InvalidContentException(message, input.Identity);
        }
      }
    }
    #endregion
  }
}
