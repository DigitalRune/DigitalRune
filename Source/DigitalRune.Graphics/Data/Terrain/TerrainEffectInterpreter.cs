// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the descriptions for effects used by the <see cref="Terrain"/>.
  /// </summary>
  /// <remarks>
  /// See <see cref="TerrainEffectParameterSemantics"/> for a list of supported semantics.
  /// </remarks>
  public class TerrainEffectInterpreter : DictionaryEffectInterpreter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainEffectInterpreter"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public TerrainEffectInterpreter()
    {
      // Terrain
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainClearValues, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainClearValues, i, EffectParameterHint.PerInstance));

      // TerrainTile
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileOrigin, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileOrigin, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileSize, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileSize, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileHeightTexture, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileHeightTexture, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileHeightTextureSize, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileHeightTextureSize, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileNormalTexture, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileNormalTexture, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileNormalTextureSize, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileNormalTextureSize, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileHoleTexture, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileHoleTexture, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainTileHoleTextureSize, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainTileHoleTextureSize, i, EffectParameterHint.PerInstance));

      // TerrainNode
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainHoleThreshold, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainHoleThreshold, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainBaseClipmap, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainBaseClipmap, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapCellSize, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainBaseClipmapCellSize, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapCellsPerLevel, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainBaseClipmapCellsPerLevel, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapNumberOfLevels, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainBaseClipmapNumberOfLevels, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapNumberOfColumns, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainBaseClipmapNumberOfColumns, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapLevelBias, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainBaseClipmapLevelBias, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapOrigins, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainBaseClipmapOrigins, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmap, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmap, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapCellSizes, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmapCellSizes, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapCellsPerLevel, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmapCellsPerLevel, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapNumberOfLevels, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmapNumberOfLevels, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapNumberOfColumns, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmapNumberOfColumns, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapLevelBias, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmapLevelBias, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapOrigins, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmapOrigins, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapOffsets, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailClipmapOffsets, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainDetailFadeRange, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainDetailFadeRange, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(TerrainEffectParameterSemantics.TerrainEnableAnisotropicFiltering, (p, i) => new EffectParameterDescription(p, TerrainEffectParameterSemantics.TerrainEnableAnisotropicFiltering, i, EffectParameterHint.PerInstance));
    }
  }
}
