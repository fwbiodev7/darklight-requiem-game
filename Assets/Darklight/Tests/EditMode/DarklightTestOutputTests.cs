using System;
using System.IO;
using NUnit.Framework;
namespace Darklight.Tests
{
    public sealed class DarklightTestOutputTests
    {
        [Test] public void NestedArtifactsRemainUnderThePortableRoot()
        {
            string directory=DarklightTestOutput.GetDirectory("portability/nested");
            Assert.That(directory,Does.StartWith(DarklightTestOutput.Root+"/"));
            Assert.That(Directory.Exists(directory),Is.True);
            Assert.That(DarklightTestOutput.GetFilePath("portability/nested","../capture.json"),Is.EqualTo(directory+"capture.json"));
        }
        [TestCase("../outside")][TestCase("nested/../../outside")][TestCase("..\\outside")]
        public void TraversalCannotCreateExternalArtifactDirectories(string category)
        {Assert.Throws<ArgumentException>(()=>DarklightTestOutput.GetDirectory(category));}
        [Test] public void AbsoluteAndEmptyFileTargetsAreRejected()
        {
            Assert.Throws<ArgumentException>(()=>DarklightTestOutput.GetDirectory(Path.GetTempPath()));
            Assert.Throws<ArgumentException>(()=>DarklightTestOutput.GetFilePath("portability",""));
        }
    }
}
