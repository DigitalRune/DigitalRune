#if !ANDROID
using System.IO;
using DigitalRune.Ionic.Zip;
using NUnit.Framework;


namespace DigitalRune.Ionic.Tests
{
  [TestFixture]
  public class ZipFileTest
  {
    // The folder that contains the files used in this unit tests.
    private const string RootPath = @"..\..\Testcases\";

    private const string Lorem = @"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";


    [Test]
    public void CheckDirectorySeparator()
    {
      using (var stream = File.OpenRead(RootPath + "Package.zip"))
      using (var zipFile = ZipFile.Read(stream))
      {
        Assert.True(zipFile.ContainsEntry("Folder\\C.txt"));
        Assert.True(zipFile.ContainsEntry("Folder/C.txt"));
      }
    }


    [Test]
    public void CheckZipFile()
    {
      using (var stream = File.OpenRead(RootPath + "Package.zip"))
      using (var zipFile = ZipFile.Read(stream))
      {
        CheckEntries(zipFile);
        CheckContents(zipFile);
      }
    }


    [Test]
    public void CheckZipFileWithZipCrypto()
    {
      using (var stream = File.OpenRead(RootPath + "Package_ZipCrypto.zip"))
      using (var zipFile = ZipFile.Read(stream))
      {
        zipFile.Password = "password";
        CheckEntries(zipFile);
        CheckContents(zipFile);
      }
    }


    [Test]
    public void CheckZipFileWithAES256()
    {
      using (var stream = File.OpenRead(RootPath + "Package_AES256.zip"))
      using (var zipFile = ZipFile.Read(stream))
      {
        zipFile.Password = "password";
        CheckEntries(zipFile);
        CheckContents(zipFile);
      }
    }


    private static void CheckEntries(ZipFile zipFile)
    {
      // Depending on the ZIP tool: The folder "Folder\" is not always in Entries.
      Assert.True(zipFile.Entries.Count == 4 || zipFile.Entries.Count == 5);

      Assert.True(zipFile.ContainsEntry("A.txt"));
      Assert.True(zipFile.ContainsEntry("B.txt"));
      Assert.True(zipFile.ContainsEntry("Folder/C.txt"));
      Assert.True(zipFile.ContainsEntry("Lorem.txt"));
    }


    private static void CheckContents(ZipFile zipFile)
    {
      Assert.AreEqual("Contents of \"A.txt\".", ReadFile(zipFile, "A.txt"));
      Assert.AreEqual("Contents of \"B.txt\".", ReadFile(zipFile, "B.txt"));
      Assert.AreEqual("Contents of \"C.txt\".", ReadFile(zipFile, "Folder/C.txt"));
      Assert.AreEqual(Lorem, ReadFile(zipFile, "Lorem.txt"));
    }


    private static string ReadFile(ZipFile zipFile, string filename)
    {
      using (var reader = new StreamReader(zipFile[filename].OpenReader()))
      {
        return reader.ReadToEnd();
      }
    }
  }
}
#endif