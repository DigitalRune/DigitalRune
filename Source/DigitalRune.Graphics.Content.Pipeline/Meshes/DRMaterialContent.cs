// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores the processed data for a <strong>Material</strong> asset.
  /// </summary>  
  public class DRMaterialContent : MaterialContent
  {
    /// <summary>
    /// Gets or sets the imported material definition (XML file).
    /// </summary>
    /// <value>The imported material definition (XML file).</value>
    public XDocument Definition { get; set; }


    /// <summary>
    /// Gets or sets the effect bindings per render pass.
    /// </summary>
    /// <value>The effect bindings per render pass.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public Dictionary<string, DREffectBindingContent> Passes { get; set; }
  }
}
