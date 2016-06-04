// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Provides helper methods for game scenes.
    /// </summary>
    public static class GameHelper
    {
        /// <summary>
        /// Add light sources for standard three-point lighting to the given scene.
        /// </summary>
        /// <remarks>
        /// The key light has a shadow map.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static void AddLights(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            float hdrScale = 1;

            // ----- Ambient light
            var ambientLight = new AmbientLight
            {
                Color = new Vector3F(0.05333332f, 0.09882354f, 0.1819608f),
                Intensity = 1,
                HemisphericAttenuation = 0,
                HdrScale = hdrScale,
            };
            scene.Children.Add(new LightNode(ambientLight));

            // ----- Key Light with shadow map
            var keyLight = new DirectionalLight
            {
                Color = new Vector3F(1, 0.9607844f, 0.8078432f),
                DiffuseIntensity = 1,
                SpecularIntensity = 1,
                HdrScale = hdrScale,
            };
            var keyLightNode = new LightNode(keyLight)
            {
                Name = "KeyLight",
                Priority = 10,   // This is the most important light.
                PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(-0.5265408f, -0.5735765f, -0.6275069f))),
                // This light uses Cascaded Shadow Mapping.
                Shadow = new CascadedShadow
                {
                    PreferredSize = 1024,
                    NumberOfCascades = 4,
                    Distances = new Vector4F(2, 4, 10, 20f),
                    MinLightDistance = 8,
                    FilterRadius = 2,
                    NumberOfSamples = 16,
                    CascadeSelection = ShadowCascadeSelection.BestDithered,
                }
            };
            scene.Children.Add(keyLightNode);

            // ----- Fill light
            var fillLight = new DirectionalLight
            {
                Color = new Vector3F(0.9647059f, 0.7607844f, 0.4078432f),
                DiffuseIntensity = 1,
                SpecularIntensity = 0,
                HdrScale = hdrScale,
            };
            var fillLightNode = new LightNode(fillLight)
            {
                Name = "FillLight",
                PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.7198464f, 0.3420201f, 0.6040227f))),
            };
            scene.Children.Add(fillLightNode);

            // ----- Back light
            var backLight = new DirectionalLight
            {
                Color = new Vector3F(0.3231373f, 0.3607844f, 0.3937255f),
                DiffuseIntensity = 1,
                SpecularIntensity = 1,
                HdrScale = hdrScale,
            };
            var backLightNode = new LightNode(backLight)
            {
                Name = "BackLight",
                PoseWorld = new Pose(QuaternionF.CreateRotation(Vector3F.Forward, new Vector3F(0.4545195f, -0.7660444f, 0.4545195f))),
            };
            scene.Children.Add(backLightNode);
        }
    }
}
