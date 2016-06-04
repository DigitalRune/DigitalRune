// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Samples;


namespace DigitalRune.Editor.Models
{
    /// <summary>
    /// Shows a 3D model for viewing and editing.
    /// </summary>
    internal class ModelDocumentViewModel : DocumentViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private GameExtension _gameExtension;
        private ModelNode _groundModelNode;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public new ModelDocument Document
        {
            get { return (ModelDocument)base.Document; }
        }


        // True if document state and view model graphics are not loading. --> Use this flag to
        // disable controls during loading and to avoid recursive calls.
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }
        private bool _isLoaded;


        // Used to disable the graphics engine combo box.
        public bool CanChangeGraphicsEngine
        {
            get { return _canChangeGraphicsEngine; }
            set { SetProperty(ref _canChangeGraphicsEngine, value); }
        }
        private bool _canChangeGraphicsEngine;


        public ModelNode ModelNode { get; private set; }


        public CameraNode CameraNode
        {
            get { return _cameraNode; }
            set { SetProperty(ref _cameraNode, value); }
        }
        private CameraNode _cameraNode;


        /// <summary>
        /// Gets the graphics screens to be rendered.
        /// </summary>
        /// <value>The graphics screens to be rendered.</value>
        public IList<GraphicsScreen> GraphicsScreens { get; } = new List<GraphicsScreen>();


        public bool UseDeferredLighting
        {
            get { return _useDeferredLighting; }
            set
            {
                if (SetProperty(ref _useDeferredLighting, value))
                    if (IsLoaded)
                        SwitchGraphicsScreen();
            }
        }
        private bool _useDeferredLighting;


        public GraphicsEngine GraphicsEngine
        {
            get { return _graphicsEngine; }
            set
            {
                if (SetProperty(ref _graphicsEngine, value))
                    Document.UseDigitalRuneGraphics = (value == GraphicsEngine.DigitalRune);
            }
        }
        private GraphicsEngine _graphicsEngine = GraphicsEngine.DigitalRune;


        public bool ShowGroundPlane
        {
            get { return _showGroundPlane; }
            set
            {
                if (SetProperty(ref _showGroundPlane, value))
                    _groundModelNode.IsEnabled = value;
            }
        }
        private bool _showGroundPlane = true;


        public bool ShowBoundingShapes
        {
            get { return _showBoundingShapes; }
            set
            {
                if (SetProperty(ref _showBoundingShapes, value))
                    if (IsLoaded)
                        UpdateDebugDrawing();
            }
        }
        private bool _showBoundingShapes;


        public bool ShowSkeleton
        {
            get { return _showSkeleton; }
            set
            {
                if (SetProperty(ref _showSkeleton, value))
                    if (IsLoaded)
                        UpdateDebugDrawing();
            }
        }

        private bool _showSkeleton;


        public bool HasSkeleton
        {
            get { return _hasSkeleton; }
            set
            {
                if (SetProperty(ref _hasSkeleton, value))
                    if (IsLoaded)
                        UpdateDebugDrawing();
            }
        }
        private bool _hasSkeleton;


        public bool ShowIntermediateRenderTargets
        {
            get { return _showIntermediateRenderTargets; }
            set
            {
                if (SetProperty(ref _showIntermediateRenderTargets, value))
                    if (IsLoaded)
                        UpdateDebugDrawing();
            }
        }
        private bool _showIntermediateRenderTargets;


        public CameraMode CameraMode
        {
            get { return _cameraMode; }
            set
            {
                if (SetProperty(ref _cameraMode, value))
                {
                    // Zoom is limited when orbiting. Otherwise camera could move behind model.
                    if (value == CameraMode.Fly)
                        ZoomMinDistance = float.NaN;
                    else
                        ZoomMinDistance = 0;
                }
            }
        }
        private CameraMode _cameraMode = CameraMode.Turntable;


        public float ZoomSpeed
        {
            get { return _zoomSpeed; }
            set { SetProperty(ref _zoomSpeed, value); }
        }
        private float _zoomSpeed = 0.5f;


        public float ZoomMinDistance
        {
            get { return _zoomMinDistance; }
            set { SetProperty(ref _zoomMinDistance, value); }
        }
        private float _zoomMinDistance;


        public float MoveSpeed
        {
            get { return _moveSpeed; }
            set { SetProperty(ref _moveSpeed, value); }
        }
        private float _moveSpeed = 5;


        public Vector3F ModelCenter      // For camera orbiting.
        {
            get { return _modelCenter; }
            set { SetProperty(ref _modelCenter, value); }
        }
        private Vector3F _modelCenter;


        public DelegateCommand ResetCameraCommand { get; }


        public DelegateCommand RecreatedModelAndMaterialDescriptionCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelDocumentViewModel" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        public ModelDocumentViewModel(ModelDocument document)
            : base(document)
        {
            ResetCameraCommand = new DelegateCommand(ResetCameraPose);
            RecreatedModelAndMaterialDescriptionCommand = new DelegateCommand(Recreate, CanRecreate);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                _gameExtension = Document.Editor.Extensions.OfType<GameExtension>().FirstOrDefault().ThrowIfMissing();
                _gameExtension.GameLogicUpdating += OnGameLogicUpdating;

                Document.PropertyChanged += OnDocumentPropertyChanged;
                Initialize();
            }

            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                _gameExtension.GameLogicUpdating -= OnGameLogicUpdating;
                _gameExtension = null;

                Document.PropertyChanged -= OnDocumentPropertyChanged;
                Reset(true, true);
            }

            base.OnDeactivated(eventArgs);
        }


        private void OnDocumentPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (!IsOpen)
                return;

            if (string.IsNullOrEmpty(eventArgs.PropertyName) || eventArgs.PropertyName == nameof(ModelDocument.State))
                Initialize();
        }


        private void Initialize()
        {
            if (Document.State == ModelDocumentState.Loading)
            {
                IsLoaded = false;
                CanChangeGraphicsEngine = false;
                Reset(false, false);
                if (GraphicsScreens.Count == 0)
                    InitializeGraphicsScreen();

                ResetCameraPose();
            }
            else
            {
                IsLoaded = false;

                GraphicsEngine = Document.UseDigitalRuneGraphics ? GraphicsEngine.DigitalRune : GraphicsEngine.MonoGame;

                bool resetScreen = false;
                if (UseDeferredLighting && !Document.UseDigitalRuneGraphics)
                {
                    // Cannot use deferred lighting with MonoGame-only model.
                    UseDeferredLighting = false;
                    resetScreen = true;
                }
                else if (GraphicsScreens.Count > 0)
                {
                    // Do we need to switch the graphics screen?
                    if (UseDeferredLighting && GraphicsScreens[0] is BasicGraphicsScreen
                        || !UseDeferredLighting && GraphicsScreens[0] is DeferredGraphicsScreen)
                    {
                        resetScreen = true;
                    }
                }

                Reset(resetScreen, false);

                if (GraphicsScreens.Count == 0)
                    InitializeGraphicsScreen();

                InitializeModel();
                ResetCameraPose();
                IsLoaded = true;
                CanChangeGraphicsEngine = !Document.IsXnb;
            }

            UpdateDebugDrawing();

            RecreatedModelAndMaterialDescriptionCommand.RaiseCanExecuteChanged();
        }


        // Called when UseDeferredLighting is changed.
        private void SwitchGraphicsScreen()
        {
            Reset(true, false);
            InitializeGraphicsScreen();
            InitializeModel();
            UpdateDebugDrawing();
        }


        private void Reset(bool resetScreens, bool resetPersistentNodes)
        {
            // Remove model node from previous scene.
            ModelNode?.Parent?.Children.Remove(Document.ModelNode);

            // Dispose model clones.
            if (ModelNode != Document.ModelNode)
                Dispose(ModelNode);
            ModelNode = null;

            if (GraphicsScreens.Count > 0)
            {
                var basicGraphicsScreen = GraphicsScreens[0] as BasicGraphicsScreen;
                if (basicGraphicsScreen != null)
                    basicGraphicsScreen.Model = null;
            }

            if (resetScreens)
            {
                // Keep these nodes to keep camera orientation and to avoid unnecessary reloading.
                CameraNode?.Parent?.Children.Remove(CameraNode);
                _groundModelNode?.Parent?.Children.Remove(_groundModelNode);

                // Dispose graphics screens.
                foreach (var graphicsScreen in GraphicsScreens)
                    graphicsScreen.SafeDispose();

                GraphicsScreens.Clear();
            }

            if (resetPersistentNodes)
            {
                Dispose(CameraNode);
                CameraNode = null;

                Dispose(_groundModelNode);
                _groundModelNode = null;
            }
        }


        private static void Dispose(SceneNode sceneNode)
        {
            if (sceneNode != null)
            {
                sceneNode.Parent?.Children.Remove(sceneNode);
                sceneNode.Dispose(false);
            }
        }


        private void InitializeGraphicsScreen()
        {
            Debug.Assert(GraphicsScreens.Count == 0, "Reset graphics screens before calling InitializeGraphicsScreen().");

            var services = Document.Editor.Services;
            var graphicsService = services.GetInstance<IGraphicsService>().ThrowIfMissing();

            // Initialize graphics screens.
            var graphicsScreen = UseDeferredLighting
                                 ? (GraphicsScreen)new DeferredGraphicsScreen(services)
                                 : new BasicGraphicsScreen(services);
            GraphicsScreens.Add(graphicsScreen);
            GraphicsScreens.Add(new DebugGraphicsScreen(services));

            // Add default lighting.
            var scene = UseDeferredLighting
                        ? ((DeferredGraphicsScreen)graphicsScreen).Scene
                        : ((BasicGraphicsScreen)graphicsScreen).Scene;
            GameHelper.AddLights(scene);

            // Add a ground plane (useful for orientation and to check model shadows).
            if (_groundModelNode == null)
            {
                var content = services.GetInstance<ContentManager>().ThrowIfMissing();
                _groundModelNode = content.Load<ModelNode>("DigitalRune.Editor.Game/Models/Misc/GroundPlane/GroundPlane").Clone();
            }
            _groundModelNode.IsEnabled = ShowGroundPlane;
            scene.Children.Add(_groundModelNode);

            // Add camera.
            if (CameraNode == null)
            {
                var projection = new PerspectiveProjection();
                projection.SetFieldOfView(
                  ConstantsF.PiOver4,
                  graphicsService.GraphicsDevice.Viewport.AspectRatio,
                  0.1f,
                  10000.0f);
                CameraNode = new CameraNode(new Camera(projection)) { Name = "CameraPerspective", };
            }

            if (UseDeferredLighting)
                ((DeferredGraphicsScreen)graphicsScreen).ActiveCameraNode = CameraNode;
            else
                ((BasicGraphicsScreen)graphicsScreen).CameraNode = CameraNode;
        }


        private void InitializeModel()
        {
            Debug.Assert(GraphicsScreens.Count != 0, "Initialize graphics screens before calling InitializeModel().");

            if (Document.ModelNode != null)
            {
                ModelNode = Document.ModelNode;

                // The first view model can use the original model node. All other view models
                // use clones because we cannot put one model node into several scenes.
                if (Document.ViewModels[0] != this)
                    ModelNode = ModelNode.Clone();

                if (Document.HasAnimations)
                {
                    // We want to animate all clones together. --> Make all clones use the skeleton
                    // pose instance of the original mesh node.
                    var originalMeshNode = Document.ModelNode.GetDescendants().OfType<MeshNode>().FirstOrDefault();
                    var clonedMeshNode = ModelNode.GetDescendants().OfType<MeshNode>().FirstOrDefault();
                    if (originalMeshNode != null && clonedMeshNode != null)
                        clonedMeshNode.SkeletonPose = originalMeshNode.SkeletonPose;

                    HasSkeleton = true;
                }
                else
                {
                    HasSkeleton = false;
                }

                var scene = UseDeferredLighting
                            ? ((DeferredGraphicsScreen)GraphicsScreens[0]).Scene
                            : ((BasicGraphicsScreen)GraphicsScreens[0]).Scene;
                scene.Children.Add(ModelNode);
            }
            else if (Document.Model != null)
            {
                HasSkeleton = false;
                ((BasicGraphicsScreen)GraphicsScreens[0]).Model = Document.Model;
            }
        }


        /// <summary>
        /// Positions the camera so that it sees the whole model and is not to near or to far away.
        /// </summary>
        private void ResetCameraPose()
        {
            if (ModelNode == null && Document.Model == null)
            {
                var lookAtMatrix = Matrix44F.CreateLookAt(new Vector3F(10, 10, 10), new Vector3F(0, 1, 0), new Vector3F(0, 1, 0));
                _cameraNode.PoseWorld = Pose.FromMatrix(lookAtMatrix).Inverse;
                ModelCenter = new Vector3F(0, 1, 0);
                MoveSpeed = 5;
                ZoomSpeed = MoveSpeed / 10;
                return;
            }

            // Get combined AABB of scene nodes.
            Aabb aabb = new Aabb();
            if (ModelNode != null)
            {
                foreach (var meshNode in ModelNode.GetSubtree().OfType<MeshNode>())
                    aabb.Grow(meshNode.Aabb);
            }
            else
            {
                Matrix[] boneTransforms = new Matrix[Document.Model.Bones.Count];
                Document.Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                foreach (var mesh in Document.Model.Meshes)
                {
                    var sphere = mesh.BoundingSphere;
                    sphere = sphere.Transform(boneTransforms[mesh.ParentBone.Index]);
                    var partCenter = (Vector3F)sphere.Center;
                    var partRadius = new Vector3F(sphere.Radius);
                    aabb.Grow(new Aabb(partCenter - partRadius, partCenter + partRadius));
                }
            }

            // Set camera position.
            var center = aabb.Center;
            var radius = aabb.Extent.Length / 2;
            var gamma = _cameraNode.Camera.Projection.FieldOfViewY / 2;
            var distance = Math.Max((float)(radius / Math.Tan(gamma)) * 0.7f, 1);  // * 0.x to move a bit closer otherwise distance is usually to large.
            _cameraNode.PoseWorld = Pose.FromMatrix(
                Matrix44F.CreateLookAt(center + new Vector3F(distance), center, Vector3F.Up)).Inverse;

            // Center for camera orbiting.
            ModelCenter = center;

            // Make navigation speed relative to model size.
            MoveSpeed = Math.Max(radius * 4, 1);
            ZoomSpeed = MoveSpeed / 10;
        }


        private void UpdateDebugDrawing()
        {
            if (GraphicsScreens.Count == 0)
                return;

            var debugRenderer = UseDeferredLighting
                                ? ((DeferredGraphicsScreen)GraphicsScreens[0]).DebugRenderer
                                : ((BasicGraphicsScreen)GraphicsScreens[0]).DebugRenderer;

            debugRenderer.Clear();
            debugRenderer.DrawAxes(Pose.Identity, 1, true);

            // Draw status message.
            if (ModelNode == null && Document.Model == null)
            {
                string text = null;
                if (Document.State == ModelDocumentState.Loading)
                    text = "Loading...";
                else if (Document.State == ModelDocumentState.Error)
                    text = "Error. See Output window and Errors window for more information.";

                if (text != null)
                    debugRenderer.DrawText(text, ModelCenter, new Vector2F(0.5f), Color.White, true);

                return;
            }

            // Draw AABB.
            if (ShowBoundingShapes)
            {
                if (ModelNode != null)
                {
                    foreach (var meshNode in ModelNode.GetDescendants().OfType<MeshNode>())
                        debugRenderer.DrawObject(meshNode, Color.Orange, true, false);
                }
                else if (Document.Model != null)
                {
                    Matrix[] boneTransforms = new Matrix[Document.Model.Bones.Count];
                    Document.Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    foreach (var mesh in Document.Model.Meshes)
                    {
                        var sphere = mesh.BoundingSphere;
                        sphere = sphere.Transform(boneTransforms[mesh.ParentBone.Index]);
                        debugRenderer.DrawSphere(sphere.Radius, new Pose((Vector3F)sphere.Center), Color.Orange, true, false);
                    }
                }
            }

            // Draw skeleton.
            if (HasSkeleton && ShowSkeleton)
                foreach (var meshNode in ModelNode.GetDescendants().OfType<MeshNode>())
                    debugRenderer.DrawSkeleton(meshNode, meshNode.Aabb.Extent.Length / 20, Color.Orange, true);

            // Visualize intermediate render targets.
            if (Document.UseDigitalRuneGraphics && UseDeferredLighting)
                ((DeferredGraphicsScreen)GraphicsScreens[0]).VisualizeIntermediateRenderTargets = ShowIntermediateRenderTargets;

            // Draw selected outline items.
            Color highlightColor = Color.LightBlue;
            var selectedItems = Document.Outline?.SelectedItems;
            if (selectedItems?.Count > 0 
                && Document.CurrentAnimation == null) // DebugRenderer does not support animations.
            {
                foreach (var item in selectedItems)
                {
                    // Skip if any parent is selected.
                    var parent = item.Parent;
                    bool skip = false;
                    while (parent != null)
                    {
                        if (parent.IsSelected)
                        {
                            skip = true;
                            break;
                        }

                        parent = parent.Parent;
                    }

                    if (skip)
                        continue;

                    if (item.UserData is SceneNode)
                    {
                        var sceneNode = (SceneNode)item.UserData;
                        debugRenderer.DrawModel(sceneNode, highlightColor, true, true);
                    }
                    else if (item.UserData is Mesh)
                    {
                        var mesh = (Mesh)item.UserData;
                        var sceneNode = (SceneNode)item.Parent.UserData;
                        debugRenderer.DrawMesh(mesh, sceneNode.PoseWorld, sceneNode.ScaleWorld, highlightColor, true, true);
                    }
                    else if (item.UserData is Submesh)
                    {
                        var submesh = (Submesh)item.UserData;
                        var sceneNode = (SceneNode)item.Parent.Parent.UserData;
                        debugRenderer.DrawMesh(submesh, sceneNode.PoseWorld, sceneNode.ScaleWorld, highlightColor, true, true);
                    }
                    else if (item.UserData is Material)
                    {
                        var material = (Material)item.UserData;
                        var mesh = (Mesh)item.Parent.UserData;
                        var materialIndex = mesh.Materials.IndexOf(material);
                        var sceneNode = (SceneNode)item.Parent.Parent.UserData;
                        foreach(var sm in mesh.Submeshes)
                            if (sm.MaterialIndex == materialIndex)
                                debugRenderer.DrawMesh(sm, sceneNode.PoseWorld, sceneNode.ScaleWorld, highlightColor, true, true);
                    }
                }
            }
        }


        private void OnGameLogicUpdating(object sender, EventArgs eventArgs)
        {
            // The skeleton is the only animated debug info which needs to be updated every frame.
            // If something is selected, we check every frame.
            if (IsLoaded && (HasSkeleton && ShowSkeleton || Document.Outline?.SelectedItems.Count > 0))
                UpdateDebugDrawing();
        }


        private bool CanRecreate()
        {
            return IsLoaded && !Document.IsXnb;
        }


        private void Recreate()
        {
            Document.LoadAsync(true);
        }
        #endregion
    }
}
