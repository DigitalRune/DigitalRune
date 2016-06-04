// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Splits an animation into separate animations based on an XML file defining the splits.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Some FBX exporters support only a single animation (take) per model. One solution to support 
  /// multiple animations per model is to concatenate all animations into a single long one. A 
  /// separate, manually created XML file defines the animation sections.
  /// </para>
  /// <example>
  /// The XML file allows to specify the start and end times in "seconds" or in "frames". The file 
  /// must use this format:
  /// <code lang="xml">
  /// <![CDATA[
  /// <?xml version="1.0" encoding="utf-8"?>
  /// <Animations Framerate="24">   <!-- Framerate only needed if you want to specify start and end in frames. -->
  ///   <!-- Using time in seconds: -->
  ///   <Animation Name="Walk" StartTime="0" EndTime="1.5"/>
  /// 
  ///   <!-- Or using frames: -->
  ///   <Animation Name="Walk2" StartFrame="45" EndFrame="60"/>
  /// </Animations>
  /// ]]>
  /// </code>
  /// </example>
  /// <para>
  /// The method <see cref="Split(AnimationContentDictionary,string,ContentIdentity,ContentProcessorContext)"/> 
  /// parses the XML split definition file, removes the original animation and cuts it into separate
  /// animations.
  /// </para>
  /// </remarks>
  internal static class AnimationSplitter
  {
    /// <overloads>
    /// <summary>
    /// Splits the animation in the specified animation dictionary into several separate animations.
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Splits the animation in the specified animation dictionary into several separate animations.
    /// </summary>
    /// <param name="animationDictionary">The animation dictionary.</param>
    /// <param name="splitFile">
    /// The path of the XML file defining the splits. This path is relative to the folder of the 
    /// model file. Usually it is simply the filename, e.g. "Dude_AnimationSplits.xml".
    /// </param>
    /// <param name="contentIdentity">The content identity.</param>
    /// <param name="context">The content processor context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contentIdentity"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    public static void Split(AnimationContentDictionary animationDictionary, string splitFile, ContentIdentity contentIdentity, ContentProcessorContext context)
    {
      if (animationDictionary == null)
        return;

      if (string.IsNullOrEmpty(splitFile))
        return;

      if (contentIdentity == null)
        throw new ArgumentNullException("contentIdentity");

      if (context == null)
        throw new ArgumentNullException("context");

      if (animationDictionary.Count == 0)
      {
        context.Logger.LogWarning(null, contentIdentity, "The model does not have an animation. Animation splitting is skipped.");
        return;
      }

      if (animationDictionary.Count > 1)
        context.Logger.LogWarning(null, contentIdentity, "The model contains more than 1 animation. The animation splitting is performed on the first animation. Other animations are deleted!");

      // Load XML file.
      splitFile = ContentHelper.FindFile(splitFile, contentIdentity);
      XDocument document = XDocument.Load(splitFile, LoadOptions.SetLineInfo);

      // Let the content pipeline know that we depend on this file and we need to 
      // rebuild the content if the file is modified.
      context.AddDependency(splitFile);

      // Parse XML.
      var animationsElement = document.Element("Animations");
      if (animationsElement == null)
      {
        context.Logger.LogWarning(null, contentIdentity, "The animation split file \"{0}\" does not contain an <Animations> root node.", splitFile);
        return;
      }

      var wrappedContext = new ContentPipelineContext(context);
      var splits = ParseAnimationSplitDefinitions(animationsElement, contentIdentity, wrappedContext);
      if (splits == null || splits.Count == 0)
      {
        context.Logger.LogWarning(null, contentIdentity, "The XML file with the animation split definitions is invalid or empty. Animation is not split.");
        return;
      }

      // Split animations.
      Split(animationDictionary, splits, contentIdentity, context);
    }


    /// <summary>
    /// Splits the animation in the specified animation dictionary into several separate animations.
    /// </summary>
    /// <param name="animationDictionary">The animation dictionary.</param>
    /// <param name="splits">The animation split definitions.</param>
    /// <param name="contentIdentity">The content identity.</param>
    /// <param name="context">The content processor context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contentIdentity"/> or <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    public static void Split(AnimationContentDictionary animationDictionary, IList<AnimationSplitDefinition> splits, ContentIdentity contentIdentity, ContentProcessorContext context)
    {
      if (splits == null || splits.Count == 0)
        return;

      if (animationDictionary == null)
        return;

      if (contentIdentity == null)
        throw new ArgumentNullException("contentIdentity");

      if (context == null)
        throw new ArgumentNullException("context");

      if (animationDictionary.Count == 0)
      {
        context.Logger.LogWarning(null, contentIdentity, "The model does not have an animation. Animation splitting is skipped.");
        return;
      }

      if (animationDictionary.Count > 1)
        context.Logger.LogWarning(null, contentIdentity, "The model contains more than 1 animation. The animation splitting is performed on the first animation. Other animations are deleted!");

      // Get first animation.
      var originalAnimation = animationDictionary.First().Value;

      // Clear animation dictionary. - We do not keep the original animations!
      animationDictionary.Clear();

      // Add an animation to animationDictionary for each split.
      foreach (var split in splits)
      {
        TimeSpan startTime = split.StartTime;
        TimeSpan endTime = split.EndTime;

        var newAnimation = new AnimationContent
        {
          Name = split.Name,
          Duration = endTime - startTime
        };

        // Process all channels.
        foreach (var item in originalAnimation.Channels)
        {
          string channelName = item.Key;
          AnimationChannel originalChannel = item.Value;
          if (originalChannel.Count == 0)
            return;

          AnimationChannel newChannel = new AnimationChannel();

          // Add all key frames to the channel that are in the split interval.
          foreach (AnimationKeyframe keyFrame in originalChannel)
          {
            TimeSpan time = keyFrame.Time;
            if (startTime <= time && time <= endTime)
            {
              newChannel.Add(new AnimationKeyframe(keyFrame.Time - startTime, keyFrame.Transform));
            }
          }

          // Add channel if it contains key frames.
          if (newChannel.Count > 0)
            newAnimation.Channels.Add(channelName, newChannel);
        }

        if (newAnimation.Channels.Count == 0)
        {
          var message = string.Format(CultureInfo.InvariantCulture, "The split animation '{0}' is empty.", split.Name);
          throw new InvalidContentException(message, contentIdentity);
        }

        if (animationDictionary.ContainsKey(split.Name))
        {
          var message = string.Format(CultureInfo.InvariantCulture, "Cannot add split animation '{0}' because an animation with the same name already exits.", split.Name);
          throw new InvalidContentException(message, contentIdentity);
        }

        animationDictionary.Add(split.Name, newAnimation);
      }
    }


    /// <summary>
    /// Parses the animation split definitions defined by the specified XML element.
    /// </summary>
    /// <param name="animationsElement">The XML element that defines the animation splits.</param>
    /// <param name="contentIdentity">The content identity.</param>
    /// <param name="context">The context.</param>
    /// <returns>The list of animation split definitions.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="animationsElement"/>, <paramref name="contentIdentity"/>, or 
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    internal static List<AnimationSplitDefinition> ParseAnimationSplitDefinitions(XElement animationsElement, ContentIdentity contentIdentity, ContentPipelineContext context)
    {
      if (animationsElement == null)
        throw new ArgumentNullException("animationsElement");
      if (contentIdentity == null)
        throw new ArgumentNullException("contentIdentity");
      if (context == null)
        throw new ArgumentNullException("context");

      // The frame rate needs to be set, when the splits are defined in frames.
      double? framerate = (double?)animationsElement.Attribute("Framerate");

      // Read all animation splits.
      var splits = new List<AnimationSplitDefinition>();
      foreach (var animationElement in animationsElement.Elements("Animation"))
      {
        var name = animationElement.GetMandatoryAttribute("Name", contentIdentity);

        double? startTime = (double?)animationElement.Attribute("StartTime");
        double? endTime = (double?)animationElement.Attribute("EndTime");

        int? startFrame = (int?)animationElement.Attribute("StartFrame");
        int? endFrame = (int?)animationElement.Attribute("EndFrame");

        bool? addLoopFrame = (bool?)animationsElement.Attribute("AddLoopFrame");

        if (startTime == null && startFrame == null)
        {
          string message = XmlHelper.GetExceptionMessage(animationElement, "The animation element does not contain a valid \"StartTime\" or \"StartFrame\" attribute.");
          throw new InvalidContentException(message, contentIdentity);
        }

        if (endTime == null && endFrame == null)
        {
          string message = XmlHelper.GetExceptionMessage(animationElement, "The animation element does not contain a valid \"EndTime\" or \"EndFrame\" attribute.");
          throw new InvalidContentException(message, contentIdentity);
        }

        if (framerate == null && (startTime == null || endTime == null))
        {
          string message = XmlHelper.GetExceptionMessage(animationsElement, "The animations element must have a <Framerate> element if start and end are specified in frames.");
          throw new InvalidContentException(message, contentIdentity);
        }

        startTime = startTime ?? startFrame.Value / framerate.Value;
        endTime = endTime ?? endFrame.Value / framerate.Value;
        TimeSpan start = new TimeSpan((long)(startTime.Value * TimeSpan.TicksPerSecond));
        TimeSpan end = new TimeSpan((long)(endTime.Value * TimeSpan.TicksPerSecond));

        if (start > end)
        {
          string message = XmlHelper.GetExceptionMessage(animationElement, "Invalid animation element: The start time is larger than the end time.");
          throw new InvalidContentException(message, contentIdentity);
        }

        splits.Add(new AnimationSplitDefinition { Name = name, StartTime = start, EndTime = end, AddLoopFrame = addLoopFrame });
      }

      return splits;
    }
  }
}
