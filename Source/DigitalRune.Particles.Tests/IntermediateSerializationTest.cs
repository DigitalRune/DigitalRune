using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Particles.Effectors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using NUnit.Framework;


namespace DigitalRune.Particles.Tests
{
  [TestFixture]
  public class IntermediateSerializationTest
  {
    public static T Serialize<T>(T data)
    {
      var stringBuilder = new StringBuilder();

      XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

      using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings))
      {
        IntermediateSerializer.Serialize(writer, data, null);
      }

      var xml = stringBuilder.ToString();
      Trace.WriteLine("-----------------------------");
      Trace.WriteLine(xml);
      Trace.WriteLine("-----------------------------");


      using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
      {
        return IntermediateSerializer.Deserialize<T>(reader, null);
      }
    }

    [Test]
    public void DistributionTest()
    {
      var udd0 = new UniformDistributionD { MinValue = 1.001, MaxValue = 2.0002 };
      var udd1 = Serialize(udd0);
      Assert.IsTrue(udd0.MinValue == udd1.MinValue);
      Assert.IsTrue(udd0.MaxValue == udd1.MaxValue);

      var cdd0 = new DirectionDistribution { Deviation = 1.001f, Direction = new Vector3F(1, 2, 3) };
      var cdd1 = Serialize(cdd0);
      Assert.IsTrue(cdd0.Deviation == cdd1.Deviation);
      Assert.IsTrue(cdd0.Direction == cdd1.Direction);
    }


    [Test]
    public void EffectorTest()
    {
      var sve0 = new StartValueEffector<float> { Parameter = "Abc", Distribution = new UniformDistributionF(1, 2) };
      var sve1 = Serialize(sve0);
      Assert.IsTrue(sve0.Parameter == sve1.Parameter);

      sve0 = new StartValueEffector<float> { Parameter = "Abc", DefaultValue = -7 };
      sve1 = Serialize(sve0);
      Assert.IsTrue(sve0.DefaultValue == sve1.DefaultValue);
    }

    [Test]
    public void ParticleSystemTest()
    {
      // ----- Simple properties.
      var ps0 = new ParticleSystem
      {
        Enabled = false,
        EnableMultithreading = true,
        MaxNumberOfParticles = 777,
        Name = "Abc",
        CurrentDelay = new TimeSpan(1, 2, 3, 4),
        InitialDelay = new TimeSpan(4, 5, 6, 7),
        PreloadDeltaTime = new TimeSpan(12343324),
        PreloadDuration = new TimeSpan(893274893457389),
        ReferenceFrame = ParticleReferenceFrame.Local,
        TimeScaling = 1.2f,
        Pose = new Pose(new Vector3F(1, 2, 3), new QuaternionF(1, 2, 3, 5).Normalized),
        Random = new Random(123),
        RenderData = 123,
        UserData = "aslfjk",
        Shape = new BoxShape(1, 2, 3),
      };
      var ps1 = Serialize(ps0);
      Assert.IsTrue(ps0.Enabled == ps1.Enabled);
      Assert.IsTrue(ps0.EnableMultithreading == ps1.EnableMultithreading);
      Assert.IsTrue(ps0.MaxNumberOfParticles == ps1.MaxNumberOfParticles);
      Assert.IsTrue(ps0.Name == ps1.Name);
      Assert.IsTrue(ps0.CurrentDelay == ps1.CurrentDelay);
      Assert.IsTrue(ps0.InitialDelay == ps1.InitialDelay);
      Assert.IsTrue(ps0.PreloadDeltaTime == ps1.PreloadDeltaTime);
      Assert.IsTrue(ps0.PreloadDuration == ps1.PreloadDuration);
      Assert.IsTrue(ps0.ReferenceFrame == ps1.ReferenceFrame);
      Assert.IsTrue(ps0.TimeScaling == ps1.TimeScaling);
      Assert.IsTrue(ps0.Pose == ps1.Pose);
      //Assert.IsTrue(ps0.Random == ps1.Random);   // Not comparable --> 
      Assert.IsTrue(ps1.Random != null);   
      Assert.IsTrue(((IGeometricObject)ps0).Scale == ((IGeometricObject)ps1).Scale);
      Assert.IsTrue((int)ps0.RenderData == (int)ps1.RenderData);
      Assert.IsTrue((string)ps0.UserData == (string)ps1.UserData);

      // ----- Shape property.
      ps0 = new ParticleSystem
      {       
        Shape = new BoxShape(1, 2, 3),
      };

      ps1 = Serialize(ps0);
      Assert.IsTrue(((BoxShape)ps0.Shape).WidthX == ((BoxShape)ps1.Shape).WidthX);
      Assert.IsTrue(((BoxShape)ps0.Shape).WidthY == ((BoxShape)ps1.Shape).WidthY);
      Assert.IsTrue(((BoxShape)ps0.Shape).WidthZ == ((BoxShape)ps1.Shape).WidthZ);


      // ----- Effectors property.
      ps0 = new ParticleSystem();
      ps0.Effectors.Add(new LinearVelocityEffector { DirectionParameter = "Dir", Enabled = false });
      ps0.Effectors.Add(new SingleDampingEffector { DampingParameter = "Damp", });
      ps1 = Serialize(ps0);
      Assert.IsTrue(ps1.Effectors.Count == 2);
      Assert.IsTrue(((LinearVelocityEffector)ps1.Effectors[0]).DirectionParameter == "Dir");
      Assert.IsTrue(((LinearVelocityEffector)ps1.Effectors[0]).Enabled == false);
      Assert.IsTrue(((LinearVelocityEffector)ps1.Effectors[0]).ParticleSystem == ps1);
      Assert.IsTrue(((SingleDampingEffector)ps1.Effectors[1]).DampingParameter == "Damp");
      Assert.IsTrue(((SingleDampingEffector)ps1.Effectors[1]).ParticleSystem == ps1);

      // ----- Children property.
      ps0 = new ParticleSystem();
      ps0.Children = new ParticleSystemCollection();
      ps0.Children.Add(new ParticleSystem { Name = "ChildA" });
      ps0.Children.Add(new ParticleSystem { Shape = new SphereShape(77) });
      ps1 = Serialize(ps0);
      Assert.IsTrue(ps1.Children.Count == 2);
      Assert.IsTrue(ps1.Children[0].Name == "ChildA");
      Assert.IsTrue(((SphereShape)ps1.Children[1].Shape).Radius == 77);

      // ----- Children property.
      ps0 = new ParticleSystem();
      ps0.Parameters.AddUniform<Vector3F>("Vector").DefaultValue = new Vector3F(1, 2, 3);
      ps0.Parameters.AddVarying<float>("Strength");
      ps1 = Serialize(ps0);
      Assert.IsTrue(ps1.Parameters.Count() == 2);
      Assert.IsTrue(ps1.Parameters.ToArray()[0].Name == "Vector");
      Assert.IsTrue(ps1.Parameters.Get<Vector3F>("Vector").DefaultValue == new Vector3F(1, 2, 3));
      Assert.IsTrue(ps1.Parameters.ToArray()[1].Name == "Strength");
    }
  }
}
