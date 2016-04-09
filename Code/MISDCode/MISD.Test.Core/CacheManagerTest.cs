using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MISD.Core;

namespace MISD.Test.Core
{
    [TestClass]
    public class CacheManagerTest
    {
        [TestMethod]
        public void TestAdd()
        {
            CacheManager<string, Tuple<string>> cacheManager = new CacheManager<string, Tuple<string>>();

            string key1 = "Key 1";
            Tuple<string> value1 = new Tuple<string>("Value 1");

            string key2 = "Key 2";
            Tuple<string> value2 = new Tuple<string>("Value 2");

            string key3 = "Key 3";
            Tuple<string> value3 = new Tuple<string>("Value 3");

            string duplicateKey1 = "Key 1";
            Tuple<string> duplicateValue1 = new Tuple<string>("Value 1");

            cacheManager.Add(key1, value1);
            cacheManager.Add(key2, value2);
            cacheManager.Add(key3, value3);

            cacheManager.Add(duplicateKey1, duplicateValue1);

            Assert.AreEqual(cacheManager.Get(key1), value1);
            Assert.AreEqual(cacheManager.Get(key2), value2);
            Assert.AreEqual(cacheManager.Get(key3), value3);

            Assert.IsNull(cacheManager.Get("Unknown key"));
        }
    }
}
