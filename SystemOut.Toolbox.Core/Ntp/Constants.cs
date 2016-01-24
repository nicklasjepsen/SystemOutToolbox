using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// Defines vital constants for the NTP protocol.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The port number used by internet time servers.
        /// </summary>
        public const int NtpPort = 123;

        /// <summary>
        /// The version number of the protocol this library implements.
        /// Details: http://tools.ietf.org/html/rfc5905
        /// </summary>
        public const int VersionNumber = 4;

        /// <summary>
        /// The reference time used to calculate with and convert the NTP timestamps.
        /// </summary>
        public static readonly DateTime NtpReferenceDateTime = new DateTime(1900, 1, 1, 0, 0, 0);
    }
}
