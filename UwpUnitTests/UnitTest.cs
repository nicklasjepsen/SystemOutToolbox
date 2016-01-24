using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SystemOut.Toolbox.Core;
using SystemOut.Toolbox.Core.Ntp;
using SystemOut.Toolbox.Uwp;
using Windows.Networking;
using Windows.Networking.Sockets;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace UwpUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        public const string NtpServer = "pool.ntp.org";

        [TestMethod]
        public async Task TestGetIPEndpointForHost()
        {
            var host = HostTestClass.SystemOut;
            var ipEndpoint = await UwpSpecificNtpCode.GetIPEndpointForHost(host.Hostname, host.Port);
            Assert.AreEqual(host.IP, ipEndpoint.Address.ToString());
        }

        [TestMethod]
        public void AssertRecievedBytesNotNull()
        {
            var uwpSpecific = new UwpSpecificNtpCode();
            var bytes = uwpSpecific.GetData(NtpServer);
            Assert.IsNotNull(bytes);
        }
        private bool statsUpdated = false;
        private ManualResetEvent statsUpdatedEvent = new ManualResetEvent(false);

        [TestMethod]
        public async Task AssertDateTime()
        {
            var sntp = new SntpClient();


            statsUpdated = false;
            statsUpdatedEvent.Reset();

            sntp.Completed += Sntp_Completed;
            sntp.GetTimeAsync(await GetIPEndpointForHost("ntp.time.org", 123));
            statsUpdatedEvent.WaitOne();

            Assert.IsTrue(statsUpdated);
            Assert.AreEqual(DateTime.Today.DayOfYear, (await DateTimeExtensions.GetInternetTime()).DayOfYear);
        }

        private void Sntp_Completed(object sender, SntpResultEventArgs e)
        {
            statsUpdated = true;
        }

        public static async Task<IPEndPoint> GetIPEndpointForHost(string hostname, int port)
        {
            var host = await GetFirstHost(hostname, port + string.Empty);
            var ip = IPAddress.Parse(host.ToString());
            return new IPEndPoint(ip, port);
        }

        async static Task<HostName> GetFirstHost(string hostname, string port)
        {
            var host = new HostName(hostname);
            var eps = await DatagramSocket.GetEndpointPairsAsync(host, port);
            return eps.Any() ? eps.First().RemoteHostName : null;
        }

        [TestMethod]
        public async Task TestGetNtpTime()
        {
            var datetime = await DateTimeExtensions.GetNetworkTime(new UwpSpecificNtpCode());
            Assert.AreEqual(DateTime.Today.DayOfYear, datetime.DayOfYear);
        }


        class HostTestClass
        {
            public string Hostname { get; private set; }
            public string IP { get; private set; }
            public int Port { get; private set; }

            public static HostTestClass SystemOut => new HostTestClass { Hostname = "systemout.net", Port = 80, IP = "87.60.166.22" };
        }
    }
}
