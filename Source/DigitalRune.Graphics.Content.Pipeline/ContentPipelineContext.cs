// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Wraps a <see cref="ContentImporterContext"/> or a <see cref="ContentProcessorContext"/> and
  /// provides a common API.
  /// </summary>
  /// <remarks>
  /// This is a workaround because <see cref="ContentImporterContext"/> and
  /// <see cref="ContentProcessorContext"/> do not have a shared base class.
  /// </remarks>
  internal class ContentPipelineContext
  {
    private readonly ContentImporterContext _importerContext;
    private readonly ContentProcessorContext _processorContext;


    /// <summary>
    /// Gets the absolute path to the root of the build intermediate (object) directory.
    /// </summary>
    /// <value>The absolute path to the root of the build intermediate (object) directory.</value>
    public string IntermediateDirectory
    {
      get
      {
        if (_importerContext != null)
          return _importerContext.IntermediateDirectory;
        if (_processorContext != null)
          return _processorContext.IntermediateDirectory;

        return null;
      }
    }


    /// <summary>
    /// Gets the logger for the content importer or processor.
    /// </summary>
    /// <value>The logger for the content importer or processor.</value>
    public ContentBuildLogger Logger
    {
      get
      {
        if (_importerContext != null)
          return _importerContext.Logger;
        if (_processorContext != null)
          return _processorContext.Logger;

        return null;
      }
    }


    /// <summary>
    /// Gets the absolute path to the root of the build output (binaries) directory.
    /// </summary>
    /// <value>The absolute path to the root of the build output (binaries) directory.</value>
    public string OutputDirectory
    {
      get
      {
        if (_importerContext != null)
          return _importerContext.OutputDirectory;
        if (_processorContext != null)
          return _processorContext.OutputDirectory;

        return null;
      }
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPipelineContext"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPipelineContext"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public ContentPipelineContext(ContentImporterContext context)
    {
      _importerContext = context;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPipelineContext"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public ContentPipelineContext(ContentProcessorContext context)
    {
      _processorContext = context;
    }


    /// <summary>
    /// Adds a dependency to the specified file. This causes a rebuild of the file, when modified,
    /// on subsequent incremental builds.
    /// </summary>
    /// <param name="filename">The name of an asset file.</param>
    public void AddDependency(string filename)
    {
      if (_importerContext != null)
        _importerContext.AddDependency(filename);
      if (_processorContext != null)
        _processorContext.AddDependency(filename);
    }
  }
}
