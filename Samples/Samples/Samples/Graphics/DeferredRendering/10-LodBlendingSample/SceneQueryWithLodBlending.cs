#if !WP7 && !WP8
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;

namespace Samples
{
  // This class adds LOD blending support to the CustomSceneQuery.
  public class SceneQueryWithLodBlending : CustomSceneQuery
  {
    protected override void AddNodeEx(SceneNode node, RenderContext context)
    {
      bool hasMaxDistance = Numeric.IsPositiveFinite(node.MaxDistance);
      var lodGroupNode = node as LodGroupNode;
      bool isLodGroupNode = (lodGroupNode != null);

      float distance = 0;
      if (hasMaxDistance || isLodGroupNode)
      {
        // ----- Calculate view-normalized distance.
        // The view-normalized distance is the distance between scene node and 
        // camera node corrected by the camera field of view. This metric is used
        // for distance culling and LOD selection.
        var cameraNode = context.LodCameraNode;
        distance = GraphicsHelper.GetViewNormalizedDistance(node, cameraNode);

        // Apply LOD bias. (The LOD bias is a factor that can be used to increase
        // or decrease the viewing distance.)
        distance *= cameraNode.LodBias * context.LodBias;
      }

      // ----- Distance Culling: Check whether scene node is within MaxDistance.
      if (hasMaxDistance && distance >= node.MaxDistance)
        return;   // Ignore scene node.

      // ----- Optional: Fade out scene node near MaxDistance to avoid popping.
      if (context.LodBlendingEnabled && node.SupportsInstanceAlpha())
      {
        float d = node.MaxDistance - distance;
        float alpha = (d < context.LodHysteresis) ? d / context.LodHysteresis : 1;
        node.SetInstanceAlpha(alpha);
      }

      if (isLodGroupNode)
      {
        // ----- Evaluate LOD group.
        var lodSelection = lodGroupNode.SelectLod(context, distance);

        // ----- Optional: LOD Blending.
        if (lodSelection.Next != null
            && context.LodBlendingEnabled
            && lodSelection.Current.SupportsInstanceAlpha()
            && lodSelection.Next.SupportsInstanceAlpha())
        {
          // The LOD group is currently transitioning between two LODs. Both LODs
          // have an "InstanceAlpha" material parameter, i.e. we can create a 
          // smooth transition by blending between both LODs.
          // --> Render both LODs using screen door transparency (stipple patterns).
          // The current LOD (alpha = 1 - t) is faded out and the next LOD is faded 
          // in (alpha = t). 
          // The fade-in uses the regular stipple pattern and the fade-out needs to 
          // use the inverted stipple pattern. If the alpha value is negative the 
          // shader will use the inverted stipple pattern - see effect Material.fx.)
          AddSubtree(lodSelection.Current, context);
          lodSelection.Current.SetInstanceAlpha(-(1 - lodSelection.Transition));

          AddSubtree(lodSelection.Next, context);
          lodSelection.Next.SetInstanceAlpha(lodSelection.Transition);
        }
        else
        {
          // No blending between two LODs. Just show current LOD.
          if (lodSelection.Current.SupportsInstanceAlpha())
            lodSelection.Current.SetInstanceAlpha(1);

          AddSubtree(lodSelection.Current, context);
        }
      }
      else
      {
        // ----- Handle normal nodes.
        AddNode(node);
      }
    }
  }
}
#endif