using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoRaVAC.Core.Services;

namespace SoRaVAC.Core.UnitTest
{
    [TestClass]
    public class NewReleaseCheckerUnitTest
    {
        [TestMethod]
        [DataRow("1.2", "1.2")]
        [DataRow("1.2.3", "1.2.3")]
        [DataRow("1.2.3.4", "1.2.3.4")]
        [DataRow("1.2.3.4", "1.2.3.4.5")]
        public void ExtractVersionFromTagName_TestMethodSimple(string expected, string candidate)
        {
            Assert.AreEqual(expected, NewReleaseChecker.ExtractVersionFromTagName(candidate));
        }
        [TestMethod]
        [DataRow("1.2", "prerelease_1.2")]
        [DataRow("1.2.3", "beta-1.2.3")]
        [DataRow("1.2.3.4", "v1.2.3.4")]
        public void ExtractVersionFromTagName_TestMethodPrefix(string expected, string candidate)
        {
            Assert.AreEqual(expected, NewReleaseChecker.ExtractVersionFromTagName(candidate));
        }
        [TestMethod]
        [DataRow("1.2", "1.2 beta")]
        [DataRow("1.2.3", "1.2.3-0025")]
        [DataRow("1.2.3.4", "1.2.3.4_build-5.2")]
        public void ExtractVersionFromTagName_TestMethodSuffix(string expected, string candidate)
        {
            Assert.AreEqual(expected, NewReleaseChecker.ExtractVersionFromTagName(candidate));
        }
        [TestMethod]
        [DataRow("1.2", "prerelease_1.2 beta")]
        [DataRow("1.2.3", "beta-1.2.3-0025")]
        [DataRow("1.2.3.4", "v1.2.3.4_build-5.2")]
        public void ExtractVersionFromTagName_TestMethodPrefixSuffix(string expected, string candidate)
        {
            Assert.AreEqual(expected, NewReleaseChecker.ExtractVersionFromTagName(candidate));
        }
    }
}
