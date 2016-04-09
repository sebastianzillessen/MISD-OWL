using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MISD.Core;

namespace MISD.Test.Core
{
    [TestClass]
    public class LayoutTest
    {
        [TestMethod]
        public void TestParseXml()
        {
            Layout l = new Layout();
            l.ParseToXml();
            Console.WriteLine(l.Data);
            Assert.IsTrue(false);
        }
    }
}
