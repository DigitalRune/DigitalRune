#if !WP7 && !WP8
using System;
using System.Linq;
using DigitalRune;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Practices.ServiceLocation;


namespace Samples
{
  // Adds GUI controls for the DeferredGraphicsScreen to the Options window.
  public class DeferredGraphicsOptionsObject : GameObject
  {
    private readonly IServiceLocator _services;


    public DeferredGraphicsOptionsObject(IServiceLocator services)
    {
      _services = services;
    }


    protected override void OnLoad()
    {
      // ----- Get common services, game objects, etc.
      var graphicsService = _services.GetInstance<IGraphicsService>();
      var deferredGraphicsScreen = graphicsService.Screens.OfType<DeferredGraphicsScreen>().FirstOrDefault();
      if (deferredGraphicsScreen == null)
        return;

      var hdrFilter = deferredGraphicsScreen.PostProcessors.OfType<HdrFilter>().FirstOrDefault();

      var sampleFramework = _services.GetInstance<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("DeferredGraphicsScreen");
      var intermediateTargetPanel = SampleHelper.AddGroupBox(optionsPanel, "Render targets");

      SampleHelper.AddCheckBox(
        intermediateTargetPanel,
        "Draw intermediate render targets",
        deferredGraphicsScreen.VisualizeIntermediateRenderTargets,
        isChecked => deferredGraphicsScreen.VisualizeIntermediateRenderTargets = isChecked,
        "1st row: G-Buffer 0 (depth), G-Buffer 1 (normals and glossiness),\n" +
        "Light Buffer 0 (diffuse), Light Buffer 1 (specular),\n" +
        "2nd row: Shadow masks");

      SampleHelper.AddDropDown(
        intermediateTargetPanel,
        "Debug Mode",
        EnumHelper.GetValues(typeof(DeferredGraphicsDebugMode)),
        (int)deferredGraphicsScreen.DebugMode,
        item => deferredGraphicsScreen.DebugMode = (DeferredGraphicsDebugMode)item,
        "Render an intermediate render target to the back buffer for debugging.");

      var lodPanel = SampleHelper.AddGroupBox(optionsPanel, "Level of detail (LOD)");

      SampleHelper.AddCheckBox(
        lodPanel,
        "Enable LOD",
        deferredGraphicsScreen.EnableLod,
        isChecked => deferredGraphicsScreen.EnableLod = isChecked);

      var particlePanel = SampleHelper.AddGroupBox(optionsPanel, "Particle rendering");

      SampleHelper.AddCheckBox(
        particlePanel,
        "Enable soft particles",
        deferredGraphicsScreen.EnableSoftParticles,
        isChecked => deferredGraphicsScreen.EnableSoftParticles = isChecked);

      SampleHelper.AddCheckBox(
        particlePanel,
        "Render particles into low-resolution offscreen buffer",
        deferredGraphicsScreen.EnableOffscreenParticles,
        isChecked => deferredGraphicsScreen.EnableOffscreenParticles = isChecked);

      // ----- Shadow mask controls
      var shadowMaskPanel = SampleHelper.AddGroupBox(optionsPanel, "Shadow Mask");

      var blurCheckBox = SampleHelper.AddCheckBox(
        shadowMaskPanel,
        "Blur shadow mask",
        deferredGraphicsScreen.ShadowMaskRenderer.Filter.Enabled,
        isChecked => deferredGraphicsScreen.ShadowMaskRenderer.Filter.Enabled = isChecked);

      var bilateralCheckBox = SampleHelper.AddCheckBox(
        shadowMaskPanel,
        "Blur is bilateral",
        ((Blur)deferredGraphicsScreen.ShadowMaskRenderer.Filter).IsBilateral,
        isChecked => ((Blur)deferredGraphicsScreen.ShadowMaskRenderer.Filter).IsBilateral = isChecked);

      // Disable bilateral check box if blur is not checked.
      bilateralCheckBox.IsEnabled = blurCheckBox.IsChecked;
      var blurIsCheckedProperty = blurCheckBox.Properties.Get<bool>(ToggleButton.IsCheckedPropertyId);
      var bilateralIsEnabledProperty = bilateralCheckBox.Properties.Get<bool>(UIControl.IsEnabledPropertyId);
      blurIsCheckedProperty.Changed += bilateralIsEnabledProperty.Change;

      var anisotropicCheckBox = SampleHelper.AddCheckBox(
        shadowMaskPanel,
        "Blur is anisotropic",
        ((Blur)deferredGraphicsScreen.ShadowMaskRenderer.Filter).IsAnisotropic,
        isChecked => ((Blur)deferredGraphicsScreen.ShadowMaskRenderer.Filter).IsAnisotropic = isChecked);

      // Disable anisotropic check box if blur is not checked.
      anisotropicCheckBox.IsEnabled = blurCheckBox.IsChecked;
      var anisotropicIsEnabledProperty = anisotropicCheckBox.Properties.Get<bool>(UIControl.IsEnabledPropertyId);
      blurIsCheckedProperty.Changed += anisotropicIsEnabledProperty.Change;

      var depthScaleSlider = SampleHelper.AddSlider(
        shadowMaskPanel,
        "Blur depth scaling",
        "F2",
        0.0f,
        1.0f,
        ((Blur)deferredGraphicsScreen.ShadowMaskRenderer.Filter).DepthScaling,
        value => ((Blur)deferredGraphicsScreen.ShadowMaskRenderer.Filter).DepthScaling = value);

      // Disable depth scale slider if blur is not checked or bilateral/anisotropic is not checked.
      depthScaleSlider.IsEnabled = blurCheckBox.IsChecked && (bilateralCheckBox.IsChecked || anisotropicCheckBox.IsChecked);
      EventHandler<EventArgs> setDepthScaleSliderIsEnabledHandler = (s, e) =>
      {
        depthScaleSlider.IsEnabled = blurCheckBox.IsChecked
                                     && (bilateralCheckBox.IsChecked || anisotropicCheckBox.IsChecked);
      };
      blurCheckBox.Click += setDepthScaleSliderIsEnabledHandler;
      bilateralCheckBox.Click += setDepthScaleSliderIsEnabledHandler;
      anisotropicCheckBox.Click += setDepthScaleSliderIsEnabledHandler;

      var halfResCheckBox = SampleHelper.AddCheckBox(
        shadowMaskPanel,
        "Use half resolution",
        deferredGraphicsScreen.ShadowMaskRenderer.UseHalfResolution,
        isChecked => deferredGraphicsScreen.ShadowMaskRenderer.UseHalfResolution = isChecked);

      var upsampleDepthSensitivitySlider = SampleHelper.AddSlider(
        shadowMaskPanel,
        "Upsample depth sensitivity",
        "F0",
        0.0f,
        10000.0f,
        deferredGraphicsScreen.ShadowMaskRenderer.UpsampleDepthSensitivity,
        value => deferredGraphicsScreen.ShadowMaskRenderer.UpsampleDepthSensitivity = value);

      // Disable depth sensitivity slider if half-res is not checked.
      upsampleDepthSensitivitySlider.IsEnabled = halfResCheckBox.IsChecked;
      var upsampleDepthSensitivityIsEnabledProperty = upsampleDepthSensitivitySlider.Properties.Get<bool>(UIControl.IsEnabledPropertyId);
      var halfResIsCheckedProperty = halfResCheckBox.Properties.Get<bool>(ToggleButton.IsCheckedPropertyId);
      halfResIsCheckedProperty.Changed += upsampleDepthSensitivityIsEnabledProperty.Change;

      var postProcessingPanel = SampleHelper.AddGroupBox(optionsPanel, "Post processing");

      if (hdrFilter != null)
      {
        SampleHelper.AddSlider(
          postProcessingPanel,
          "Tone mapping middle gray",
          "F2",
          0,
          1,
          hdrFilter.MiddleGray,
          value => hdrFilter.MiddleGray = value);

        SampleHelper.AddSlider(
          postProcessingPanel,
          "Min exposure",
          "F2",
          0,
          10,
          hdrFilter.MinExposure,
          value => hdrFilter.MinExposure = value);

        SampleHelper.AddSlider(
          postProcessingPanel,
          "Max exposure",
          "F2",
          0,
          10,
          hdrFilter.MaxExposure,
          value => hdrFilter.MaxExposure = value);

        SampleHelper.AddSlider(
          postProcessingPanel,
          "Adaption speed",
          "F2",
          0,
          0.2f,
          hdrFilter.AdaptionSpeed,
          value => hdrFilter.AdaptionSpeed = value);

        SampleHelper.AddCheckBox(
          postProcessingPanel,
          "Enable blue shift",
          hdrFilter.EnableBlueShift,
          isChecked => hdrFilter.EnableBlueShift = isChecked);

        SampleHelper.AddSlider(
          postProcessingPanel,
          "Blue shift center",
          "F4",
          0,
          0.005f,
          hdrFilter.BlueShiftCenter,
          value => hdrFilter.BlueShiftCenter = value);

        var miscPanel = SampleHelper.AddGroupBox(optionsPanel, "Misc");

        SampleHelper.AddDropDown(
          miscPanel,
          "Ambient occlusion type",
          new[] { AmbientOcclusionType.None, AmbientOcclusionType.SSAO, AmbientOcclusionType.SAO, },
          (int)deferredGraphicsScreen.LightBufferRenderer.AmbientOcclusionType,
          item => deferredGraphicsScreen.LightBufferRenderer.AmbientOcclusionType = item);

        SampleHelper.AddCheckBox(
          miscPanel,
          "Draw reticle",
          deferredGraphicsScreen.DrawReticle,
          isChecked => deferredGraphicsScreen.DrawReticle = isChecked);
      }
    }
  }
}
#endif
