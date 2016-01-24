using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// Represents the stratum.
    /// </summary>
    public enum Stratum
    {
        /// <summary>
        /// Unspecified or invalid.
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// A primary server (e.g. equipped with a GPS receiver)
        /// </summary>
        PrimaryServer,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer1,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer2,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer3,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer4,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer5,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer6,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer7,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer8,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer9,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer10,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer11,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer12,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer13,
        /// <summary>
        /// A secondary server (via NTP).
        /// </summary>
        SecondaryServer14,
        /// <summary>
        /// Unsynchronized.
        /// </summary>
        Unsynchronized
    }
}
