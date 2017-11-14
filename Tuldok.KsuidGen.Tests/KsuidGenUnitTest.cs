using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tuldok.KsuidGen.Tests
{
    [TestClass]
    public class KsuidGenUnitTest
    {
        const int MaxKsuidLength = 27;
        Ksuid ksuid = new Ksuid();

        [TestMethod]
        public async Task ProperKsuidGeneration()
        {
            var uid = await ksuid.Generate();
            Debug.WriteLine(uid);
            Assert.AreEqual(MaxKsuidLength, uid.Length);
            Debug.WriteLine(uid);
        }
    }
}
