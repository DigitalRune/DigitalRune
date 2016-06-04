using DigitalRune.Collections;
using DigitalRune.Particles.Effectors;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests
{
  [TestFixture]
  public class ParticleHelperTest
  {
    //[Test]
    //public void GetMissingInputParameters()
    //{
    //  var ps = new ParticleSystem();
    //  ps.Parameters.AddUniform<float>("Position");
    //  ps.Parameters.AddUniform<float>("StartPosition");

    //  var m = ps.GetMissingInputParameters();
    //  Assert.AreEqual(0, m.Count);

    //  var lerpEffector1F = new SingleLerpEffector
    //  {
    //    ValueParameter = "Position",
    //    StartParameter = "StartPosition",
    //    EndParameter = "EndPosition",
    //    FactorParameter = "Factor"
    //  };
    //  ps.Effectors.Add(lerpEffector1F);

    //  m = ps.GetMissingInputParameters();
    //  Assert.AreEqual(2, m.Count);
    //  Assert.IsTrue(m.Contains(new Pair<ParticleEffector, string>(ps.Effectors[0], "EndPosition")));
    //  Assert.IsTrue(m.Contains(new Pair<ParticleEffector, string>(ps.Effectors[0], "Factor")));

    //  lerpEffector1F.FactorParameter = "NormalizedAge";

    //  m = ps.GetMissingInputParameters();
    //  Assert.AreEqual(1, m.Count);
    //  Assert.IsTrue(m.Contains(new Pair<ParticleEffector, string>(ps.Effectors[0], "EndPosition")));

    //  ps.Parameters.AddUniform<float>("EndPosition");

    //  m = ps.GetMissingInputParameters();
    //  Assert.AreEqual(0, m.Count);
    //}


    //[Test]
    //public void GetMissingOutputParameters()
    //{
    //  var ps = new ParticleSystem();
    //  ps.Parameters.AddUniform<float>("StartPosition");

    //  var m = ps.GetMissingOutputParameters();
    //  Assert.AreEqual(0, m.Count);

    //  var lerpEffector1F = new SingleLerpEffector
    //  {
    //    ValueParameter = "Position",
    //    StartParameter = "StartPosition",
    //    EndParameter = "EndPosition",
    //    FactorParameter = "Factor"
    //  };
    //  ps.Effectors.Add(lerpEffector1F);

    //  m = ps.GetMissingOutputParameters();
    //  Assert.AreEqual(1, m.Count);
    //  Assert.IsTrue(m.Contains(new Pair<ParticleEffector, string>(ps.Effectors[0], "Position")));

    //  ps.Effectors.Add(new StartValueEffector<float> { Parameter = "Size" });

    //  m = ps.GetMissingOutputParameters();
    //  Assert.AreEqual(2, m.Count);
    //  Assert.IsTrue(m.Contains(new Pair<ParticleEffector, string>(ps.Effectors[0], "Position")));
    //  Assert.IsTrue(m.Contains(new Pair<ParticleEffector, string>(ps.Effectors[1], "Size")));

    //  ps.Parameters.AddVarying<float>("Position");
    //  ps.Parameters.AddVarying<float>("Size");

    //  m = ps.GetMissingOutputParameters();
    //  Assert.AreEqual(0, m.Count);
    //}


    //[Test]
    //public void GetUninitializedParameters()
    //{
    //  var ps = new ParticleSystem();

    //  var m = ps.GetUninitializedParameters();
    //  Assert.AreEqual(0, m.Count);

    //  ps.Parameters.AddUniform<float>("StartPosition");

    //  m = ps.GetUninitializedParameters();
    //  Assert.AreEqual(1, m.Count);
    //  Assert.IsTrue(m.Contains("StartPosition"));

    //  ps.Parameters.AddVarying<float>("Position");

    //  ps.Effectors.Add(new LinearVelocityEffector { PositionParameter = "Position" });

    //  m = ps.GetUninitializedParameters();
    //  Assert.AreEqual(2, m.Count);
    //  Assert.IsTrue(m.Contains("StartPosition"));
    //  Assert.IsTrue(m.Contains("Position"));

    //  ps.Effectors.Add(new StartValueEffector<float> { Parameter = "StartPosition" });

    //  m = ps.GetUninitializedParameters();
    //  Assert.AreEqual(1, m.Count);
    //  Assert.IsTrue(m.Contains("Position"));

    //  var lerpEffector1F = new SingleLerpEffector
    //  {
    //    ValueParameter = "Position",
    //    StartParameter = "StartPosition",
    //    EndParameter = "EndPosition",
    //    FactorParameter = "NormalizedAge"
    //  };
    //  ps.Effectors.Add(lerpEffector1F);

    //  m = ps.GetUninitializedParameters();
    //  Assert.AreEqual(0, m.Count);
    //}
  }
}
