using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystenOut.Toolbox.Core.Classic;

namespace ClassicUnitTests
{
    [TestClass]
    public class SNTPProviderUnitTest
    {
        [TestMethod]
        public void TestGetNetworkTime_AssertDayOfYearMatch()
        {
            Assert.AreEqual(DateTime.Today.DayOfYear, SNTPProvider.GetNetworkTime().DayOfYear);
        }
    }
}
