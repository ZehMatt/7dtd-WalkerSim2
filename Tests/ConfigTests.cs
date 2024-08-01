using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace WalkerSim.Tests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void TestLoad()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFile = Path.Combine(assemblyPath, "WalkerSim.xml");
            var config = Config.LoadFromFile(configFile);
            Assert.IsNotNull(config);
        }
    }
}
