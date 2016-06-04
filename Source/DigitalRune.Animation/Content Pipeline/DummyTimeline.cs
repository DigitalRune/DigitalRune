// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Content
{
  internal class DummyTimeline : Singleton<DummyTimeline>, ITimeline
  {
    public FillBehavior FillBehavior { get { throw new InvalidOperationException(); } }
    public string TargetObject { get { throw new InvalidOperationException(); } }
    public AnimationInstance CreateInstance() { throw new InvalidOperationException(); }
    public TimeSpan? GetAnimationTime(TimeSpan time) { throw new InvalidOperationException(); }
    public AnimationState GetState(TimeSpan time) { throw new InvalidOperationException(); }
    public TimeSpan GetTotalDuration() { throw new InvalidOperationException(); }
  }
}
