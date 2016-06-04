// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Assimp;
using DigitalRune.Animation;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Errors;
using DigitalRune.Editor.Game;
using DigitalRune.Editor.Properties;
using DigitalRune.Editor.Outlines;
using DigitalRune.Editor.Output;
using DigitalRune.Editor.Status;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Windows;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using Path = System.IO.Path;
using DRPath = DigitalRune.Storages.Path;
using static System.FormattableString;


namespace DigitalRune.Editor.Models
{
    /// <summary>
    /// Represents a 3D model asset.
    /// </summary>
    /// <remarks>
    /// The model is disposed when the document is disposed.
    /// </remarks>
    internal partial class ModelDocument : Document
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ModelsExtension _modelsExtension;

        // Mandatory services
        private readonly IDocumentService _documentService;
        private readonly IStatusService _statusService;
        private readonly IOutputService _outputService;
        private readonly IAnimationService _animationService;
        private readonly IMonoGameService _monoGameService;

        // Optional services
        private readonly IOutlineService _outlineService;
        private readonly IPropertiesService _propertiesService;
        private readonly IErrorService _errorService;

        // Disposable resources
        private TempDirectoryHelper _tempDirectoryHelper;
        private MonoGameContent _monoGameContent;

        private Assimp.Scene _assimpScene;

        internal string CurrentAnimation;
        private AnimationController _animationController;

        private readonly List<Error> _errors = new List<Error>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether DigitalRune Graphics (property
        /// <see cref="ModelNode"/>) or only MonoGame (property <see cref="Model"/>) is used.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if DigitalRune Graphics (property <see cref="ModelNode"/>) is
        /// used; otherwise, <see langword="false"/> if only MonoGame (property <see cref="Model"/>)
        /// is used.
        /// </value>
        public bool UseDigitalRuneGraphics
        {
            get { return _useDigitalRuneGraphics; }
            set
            {
                if (SetProperty(ref _useDigitalRuneGraphics, value))
                {
                    _modelsExtension.UseDigitalRuneGraphics = value;

                    // Reload.
                    if (State != ModelDocumentState.Loading)
                        LoadAsync(false);
                }
            }
        }
        private bool _useDigitalRuneGraphics;


        /// <summary>
        /// Gets or sets the DigitalRune model scene node. (Only used if 
        /// <see cref="UseDigitalRuneGraphics"/> is <see langword="true"/>.
        /// </summary>
        /// <value>The DigitalRune model scene node.</value>
        public ModelNode ModelNode
        {
            get { return _modelNode; }
            set { SetProperty(ref _modelNode, value); }
        }
        private ModelNode _modelNode;


        /// <summary>
        /// Gets or sets the MonoGame model. (Only used if <see cref="UseDigitalRuneGraphics"/> is
        /// <see langword="false"/>.
        /// </summary>
        /// <value>The MonoGame model.</value>
        public Model Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }
        private Model _model;


        public ModelDocumentState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
        private ModelDocumentState _state;


        public bool HasAnimations
        {
            get { return _hasAnimations; }
            set { SetProperty(ref _hasAnimations, value); }
        }
        private bool _hasAnimations;


        public bool IsXnb
        {
            get { return _isXnb; }
            set { SetProperty(ref _isXnb, value); }
        }
        private bool _isXnb;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDocument"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="documentType">The type of the document.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> or <paramref name="documentType"/> is <see langword="null"/>.
        /// </exception>
        internal ModelDocument(IEditorService editor, DocumentType documentType)
          : base(editor, documentType)
        {
            _modelsExtension = editor.Extensions.OfType<ModelsExtension>().FirstOrDefault().ThrowIfMissing();
            _useDigitalRuneGraphics = _modelsExtension.UseDigitalRuneGraphics;

            var services = Editor.Services;
            _documentService = services.GetInstance<IDocumentService>().ThrowIfMissing();
            _statusService = services.GetInstance<IStatusService>().ThrowIfMissing();
            _outputService = services.GetInstance<IOutputService>().ThrowIfMissing();
            _animationService = services.GetInstance<IAnimationService>().ThrowIfMissing();
            _monoGameService = services.GetInstance<IMonoGameService>().ThrowIfMissing();
            _outlineService = services.GetInstance<IOutlineService>().WarnIfMissing();
            _propertiesService = services.GetInstance<IPropertiesService>().WarnIfMissing();
            _errorService = services.GetInstance<IErrorService>().WarnIfMissing();

            Editor.ActiveDockTabItemChanged += OnEditorDockTabItemChanged;

            UpdateProperties();
            UpdateOutline();
        }


        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    Editor.ActiveDockTabItemChanged -= OnEditorDockTabItemChanged;
                    Reset();
                }

                // Release unmanaged resources.
            }

            base.Dispose(disposing);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override DocumentViewModel OnCreateViewModel()
        {
            return new ModelDocumentViewModel(this);
        }


        private void Reset()
        {
            StopAnimation();

            _monoGameContent?.Dispose();
            _monoGameContent = null;

            Debug.Assert(ModelNode == null || ModelNode.IsDisposed, "ModelNode should be disposed together with the ContentManager.");
            ModelNode = null;
            Model = null;

            _tempDirectoryHelper?.Dispose();
            _tempDirectoryHelper = null;

            _assimpScene = null;

            if (_outlineService != null && _outlineService.Outline == Outline)
                _outlineService.Outline = null;

            if (_propertiesService != null && _propertiesService.PropertySource == _currentPropertySource)
                _propertiesService.PropertySource = null;

            CleanErrors();
        }


        /// <inheritdoc/>
        protected override void OnLoad()
        {
            // Open document immediately. Show "Loading..." in scene while model is processed.
            LoadAsync(false);
        }


        internal Task LoadAsync(bool recreateModelAndMaterialFiles)
        {
            if (State == ModelDocumentState.Loading)
                throw new EditorException("The previous model is still being loaded.");

            State = ModelDocumentState.Loading;
            Reset();

            // Show status message.
            var status = new StatusViewModel
            {
                Message = "Loading 3D model...",
                ShowProgress = true,
                Progress = double.NaN,
            };
            _statusService.Show(status);

            var task = LoadAsync(Uri.LocalPath, recreateModelAndMaterialFiles);

            status.Track(task, "3D model loaded.", "Could not load 3D model.");

            return task;
        }


        private async Task LoadAsync(string fileName, bool recreateModelAndMaterialFiles)
        {
            Debug.Assert(_tempDirectoryHelper == null);
            Debug.Assert(_monoGameContent == null);
            Debug.Assert(ModelNode == null);
            Debug.Assert(Model == null);
            Debug.Assert(_assimpScene == null);
            Debug.Assert(State == ModelDocumentState.Loading);

            string extension = Path.GetExtension(fileName);
            IsXnb = string.Compare(extension, ".XNB", StringComparison.OrdinalIgnoreCase) == 0;

            // ----- Build XNB
            string directoryName;
            try
            {
                if (IsXnb)
                {
                    // Get the folder that contains the XNB.
                    directoryName = Path.GetDirectoryName(fileName);
                }
                else if (GameContentBuilder.IsSupportedModelFileExtension(extension))
                {
                    // Build the XNB and get the output folder.
                    var buildResult = await Task.Run(() => BuildXnb(Editor.Services, Editor.ApplicationName, fileName, UseDigitalRuneGraphics, recreateModelAndMaterialFiles));
                    directoryName = buildResult.Item1;
                    _tempDirectoryHelper = buildResult.Item2;
                }
                else
                {
                    throw new EditorException(Invariant($"Unsupported 3D model file format (file extension \"{extension}\")."));
                }
            }
            catch (Exception)
            {
                if (IsDisposed)
                {
                    // Document was closed during loading.
                    Reset();
                    return;
                }

                State = ModelDocumentState.Error;
                UpdateProperties();
                UpdateOutline();

                // The GameContentBuilder logs to the output service.
                _outputService.Show();

                throw;
            }

            if (IsDisposed)
            {
                // Document was closed during loading.
                Reset();
                return;
            }

            Debug.Assert(directoryName != null);

            // ----- Load XNB
            try
            {
                // Get asset name for use with ContentManager. XNBs and unprocessed models
                // use different folder hierarchies.
                string assetFileName;
                if (IsXnb)
                {
                    // Use absolute path.
                    assetFileName = fileName;
                }
                else
                {
                    // The asset is built relative to the root folder (e.g. "C:\"). The folder
                    // hierarchy (from root to asset) is rebuilt in the temporary output folder.

                    // Make file name relative to root.
                    assetFileName = DRPath.GetRelativePath(Path.GetPathRoot(fileName), fileName);

                    // Get absolute file name relative to temporary output folder.
                    assetFileName = Path.Combine(directoryName, assetFileName);

                    // Change extension. .fbx --> .xnb
                    assetFileName = Path.ChangeExtension(assetFileName, "xnb");
                }

                _monoGameContent = await Task.Run(() => _monoGameService.LoadXnb(directoryName, assetFileName, cacheResult: false));

                if (_monoGameContent.Asset is ModelNode)
                {
                    ModelNode = (ModelNode)_monoGameContent.Asset;
                    UseDigitalRuneGraphics = true;
                    HasAnimations = ModelNode.GetDescendants()
                                             .OfType<MeshNode>()
                                             .FirstOrDefault()?
                                             .Mesh?
                                             .Animations?
                                             .Count > 0;
                }
                else if (_monoGameContent.Asset is Model)
                {
                    Model = (Model)_monoGameContent.Asset;
                    UseDigitalRuneGraphics = false;
                    HasAnimations = false;

                    // Enable default lighting.
                    var effects = Model.Meshes
                                       .SelectMany(m => m.Effects)
                                       .OfType<IEffectLights>();
                    foreach (var effect in effects)
                        effect.EnableDefaultLighting();
                }
                else
                {
                    throw new EditorException("XNB does not contain ModelNode or Model.");
                }
            }
            catch (Exception exception)
            {
                Reset();

                if (IsDisposed)
                    return;

                State = ModelDocumentState.Error;
                Logger.Error(exception, "XNB could not be loaded.");

                // Let LoadAsync return and fail, then show message box.
                WindowsHelper.BeginInvokeOnUI(() =>
                {
                    var message = Invariant($"XNB could not be loaded:\n\n\"{exception.Message}\"");
                    MessageBox.Show(message, Editor.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
                });

                throw;
            }

            if (IsDisposed)
            {
                // Document was closed during loading.
                Reset();
                return;
            }

            // ----- Load Assimp scene
            try
            {
                if (!IsXnb)
                    _assimpScene = await Task.Run(() => LoadAssimp(fileName));
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Assimp could not read model file.");
            }

            if (IsDisposed)
            {
                // Document was closed during loading.
                Reset();
                return;
            }

            State = ModelDocumentState.Loaded;

            // ----- Validate model
            ValidateModelNode();
            // TODO: Validate MonoGame Model.

            // If there are errors or warnings, show Output and Errors window.
            // (Drawback: This steals focus from the document.)
            //if (_errors.Count > 0)
            //{
            //    _outputService?.Show();
            //    _errorService?.Show();
            //}

            // ----- Update outline and properties
            UpdateOutline();
            UpdateProperties();
        }


        /// <summary>
        /// Converts the specified model (FBX, X, DAE) file to an XNB file. (Throws on failure.)
        /// </summary>
        /// <param name="services">The service locator.</param>
        /// <param name="applicationName">
        /// The name of the application (used for temporary folder).
        /// </param>
        /// <param name="fileName">The absolute path and name of the model file.</param>
        /// <param name="createModelNode">
        /// <see langword="true"/> to create a DigitalRune <see cref="ModelNode"/>; or
        /// <see langword="false"/> to create a MonoGame <see cref="Model"/>.
        /// </param>
        /// <param name="recreateModelAndMaterialFiles">
        /// <see langword="true"/> to recreate model description and material definition files.
        /// </param>
        /// <returns>
        /// A tuple containing
        ///   (absolute path and name of XNB file, <see cref="TempDirectoryHelper"/>).
        /// </returns>
        /// <exception cref="EditorException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Tuple<string, TempDirectoryHelper> BuildXnb(IServiceLocator services, string applicationName, string fileName, bool createModelNode, bool recreateModelAndMaterialFiles)
        {
            Debug.Assert(services != null);
            Debug.Assert(applicationName != null);
            Debug.Assert(applicationName.Length > 0);
            Debug.Assert(fileName != null);
            Debug.Assert(fileName.Length > 0);

            var tempDirectoryHelper = new TempDirectoryHelper(applicationName, "ModelDocument");
            try
            {
                var contentBuilder = new GameContentBuilder(services)
                {
                    IntermediateFolder = tempDirectoryHelper.TempDirectoryName + "\\obj",
                    OutputFolder = tempDirectoryHelper.TempDirectoryName + "\\bin",
                };

                string processorName;
                var processorParams = new OpaqueDataDictionary();
                if (createModelNode)
                {
                    processorName = "GameModelProcessor";
                    processorParams.Add("RecreateModelDescriptionFile", recreateModelAndMaterialFiles);
                    processorParams.Add("RecreateMaterialDefinitionFiles", recreateModelAndMaterialFiles);
                }
                else
                {
                    processorName = "ModelProcessor";
                }

                string errorMessage;
                bool success = contentBuilder.Build(Path.GetFullPath(fileName), null, processorName, processorParams, out errorMessage);
                if (!success)
                    throw new EditorException(Invariant($"Could not process 3d model: {fileName}.\n See output window for details."));

                // Return output folder into which we built the XNB.
                var outputFolder = contentBuilder.OutputFolder;
                if (!Path.IsPathRooted(outputFolder))
                    outputFolder = Path.GetFullPath(Path.Combine(contentBuilder.ExecutableFolder, contentBuilder.OutputFolder));

                return Tuple.Create(outputFolder, tempDirectoryHelper);
            }
            catch
            {
                tempDirectoryHelper.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Loads the model using Assimp. (Throws on failure.)
        /// </summary>
        /// <param name="fileName">The absolute path and name of the model.</param>
        /// <returns>The Assimp scene.</returns>
        private static Assimp.Scene LoadAssimp(string fileName)
        {
            using (var importer = new AssimpContext())
            {
                importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
                return importer.ImportFile(fileName,
                                           PostProcessSteps.FindDegenerates |
                                           PostProcessSteps.FindInvalidData |
                                           PostProcessSteps.FlipUVs |               // Required for Direct3D
                                           PostProcessSteps.FlipWindingOrder |      // Required for Direct3D
                                           PostProcessSteps.JoinIdenticalVertices |
                                           PostProcessSteps.ImproveCacheLocality |
                                           PostProcessSteps.OptimizeMeshes |
                                           PostProcessSteps.Triangulate);
            }
        }


        /// <inheritdoc/>
        protected override void OnSave()
        {
            throw new NotImplementedException();
        }


        private void OnEditorDockTabItemChanged(object sender, EventArgs eventArgs)
        {
            if (_documentService.ActiveDocument == this)
            {
                // This document is active and can control the tool windows.
                if (_propertiesService != null)
                    _propertiesService.PropertySource = _currentPropertySource;
                if (_outlineService != null)
                    _outlineService.Outline = Outline;
            }
            else if (_outlineService != null
                     && Editor.ActiveDockTabItem == _outlineService.OutlineViewModel
                     && _outlineService.Outline == Outline)
            {
                // The user has switched to the Outline window, which shows this model outline. 
                // Other documents may have changed the Properties windows in the meantime.
                // --> Show model properties in Properties window.
                if (_propertiesService != null)
                    _propertiesService.PropertySource = _currentPropertySource;
            }
        }


        public void PlayAnimation(string name)
        {
            if (CurrentAnimation == name)
                return;

            StopAnimation();

            CurrentAnimation = name;

            // Start selected animation.
            var meshNode = ModelNode.GetDescendants().OfType<MeshNode>().First();
            var mesh = meshNode.Mesh;
            var animation = mesh.Animations[name];
            var loopingAnimation = new TimelineClip(animation)
            {
                Duration = TimeSpan.MaxValue,
                LoopBehavior = LoopBehavior.Cycle,
            };
            _animationController = _animationService.StartAnimation(loopingAnimation, (IAnimatableProperty)meshNode.SkeletonPose);

            // Update view model IsPlaying flags.
            foreach (var animationPropertyViewModel in _animationPropertyViewModels)
                if (animationPropertyViewModel.Name == CurrentAnimation)
                    animationPropertyViewModel.IsPlaying = true;
        }


        public void StopAnimation()
        {
            if (CurrentAnimation == null)
                return;

            CurrentAnimation = null;

            // Stop currently running animation.
            _animationController.Stop();
            _animationController.Recycle();

            // Set bind pose.
            var meshNode = ModelNode.GetDescendants().OfType<MeshNode>().First();
            meshNode.SkeletonPose.ResetBoneTransforms();

            // Update view model IsPlaying flags.
            foreach (var animationPropertyViewModel in _animationPropertyViewModels)
                animationPropertyViewModel.IsPlaying = false;
        }


        private void CleanErrors()
        {
            if (_errorService != null)
                foreach (var item in _errors)
                    _errorService.Errors.Remove(item);

            _errors.Clear();
        }


        private void AddWarning(string message)
        {
            _outputService.WriteLine("WARNING: " + message);

            var item = new Error(ErrorType.Warning, message, Path.GetFileName(Uri.LocalPath));
            _errors.Add(item);
            _errorService?.Errors.Add(item);
        }
        #endregion
    }
}
