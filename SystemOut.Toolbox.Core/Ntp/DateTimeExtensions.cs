using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace SystemOut.Toolbox.Core
{
    public interface IPlatFormSpecificCode
    {
        Task<byte[]> GetData(string ntpServer);
    }

    public class DateTimeExtensions
    {
        public static async Task<DateTime> GetInternetTime()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "text/html");
            var timeString = await client.GetStringAsync("http://www.timeapi.org/utc/now");

            DateTime time;
            if (DateTime.TryParse(timeString, out time))
                return time;
            throw new Exception($"Unable to parse the string {timeString} into a DateTime object.");
        }

        public static async Task<DateTime> GetNetworkTime(IPlatFormSpecificCode platform)
        {
            const string ntpServer = "pool.ntp.org";
            
            var ntpData = await platform.GetData(ntpServer);
            ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            //var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            //var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //socket.Connect(ipEndPoint);
            //socket.Send(ntpData);
            //socket.Receive(ntpData);
            //socket.Close();


            ulong intPart = (ulong) ntpData[40] << 24 | (ulong) ntpData[41] << 16 | (ulong) ntpData[42] << 8 |
                            (ulong) ntpData[43];
            ulong fractPart = (ulong) ntpData[44] << 24 | (ulong) ntpData[45] << 16 | (ulong) ntpData[46] << 8 |
                              (ulong) ntpData[47];

            var milliseconds = (intPart*1000) + ((fractPart*1000)/0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long) milliseconds);

            return networkDateTime;
        }
    }

}
