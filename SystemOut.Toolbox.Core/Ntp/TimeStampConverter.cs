using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// A helper class that converts between .NET time formats and NTP's time formats.
    /// </summary>
    public static class TimestampConverter
    {
        // As defined in http://www.ietf.org/rfc/rfc5905.txt
        private const long Fric = 0x10000L; // 65536
        private const long Frac = 0x100000000L; // 4294967296

        /// <summary>
        /// Converts a .NET <see cref="DateTime"/> instance to a raw NTP timestamp representation.
        /// </summary>
        /// <param name="dateTime">The date time to convert.</param>
        /// <returns>The NTP representation of the date time.</returns>
        public static byte[] ToNtpTimestamp(DateTime dateTime)
        {
            // convert to whole and fraction seconds in "NTP epoch".
            var nowAsNtpSeconds = (dateTime - Constants.NtpReferenceDateTime).TotalSeconds;

            // convert to bytes
            // important: there is a bug in the emulator for WP7 that lets the following conversion fail:
            // var result = (UInt64)Math.Floor(nowAsNtpSeconds * Frac);
            // More information on the problem and details about the workaround applied below can be found here:
            // http://www.pitorque.de/MisterGoodcat/post/Windows-Phone-7-The-kind-of-bug-you-dont-want-to-discover.aspx
            var temp = Math.Floor(nowAsNtpSeconds * Frac);
            UInt64 workaround;
            if (temp > Int64.MaxValue)
            {
                workaround = (UInt64)(temp - Int64.MaxValue) + Int64.MaxValue;
            }
            else
            {
                workaround = (UInt64)temp;
            }

            // proceed
            var result = BitConverter.GetBytes(workaround);
            return result;
        }

        /// <summary>
        /// Converts a raw NTP timestamp to a .NET <see cref="DateTime"/> type.
        /// </summary>
        /// <param name="timestamp">The timestamp to convert.</param>
        /// <returns>The converted .NET date time.</returns>
        public static DateTime FromNtpTimestamp(byte[] timestamp)
        {
            return FromNtpTimestamp(timestamp, 0);
        }

        /// <summary>
        /// Converts a raw NTP timestamp to a .NET <see cref="DateTime"/> type, starting at the given offset in the raw data.
        /// </summary>
        /// <param name="rawData">The rawData that contains an NTP timestamp at the given offset.</param>
        /// <param name="offset">The offset the timestamp should be read from.</param>
        /// <returns>The converted .NET date time.</returns>
        public static DateTime FromNtpTimestamp(byte[] rawData, int offset)
        {
            // get the seconds
            var temp = BitConverter.ToDouble(rawData, offset);
            var totalSeconds = (double)temp / Frac;

            // convert to datetime
            var result = Constants.NtpReferenceDateTime + TimeSpan.FromSeconds(totalSeconds);
            return result;
        }

        /// <summary>
        /// Converts a .NET <see cref="TimeSpan"/> instance to a raw NTP short timespan representation.
        /// </summary>
        /// <param name="timeSpan">The timespan to convert.</param>
        /// <returns>The NTP representation of the timespan.</returns>
        public static byte[] ToNtpShort(TimeSpan timeSpan)
        {
            // get the seconds
            var totalSeconds = timeSpan.TotalSeconds;

            // convert to bytes
            var temp = (UInt32)Math.Floor(totalSeconds * Fric);
            var result = BitConverter.GetBytes(temp);

            return result;
        }

        /// <summary>
        /// Converts a raw NTP short timespan to a .NET <see cref="TimeSpan"/> type.
        /// </summary>
        /// <param name="timestamp">The NTP short timespan to convert.</param>
        /// <returns>The converted .NET timespan.</returns>
        public static TimeSpan FromNtpShort(byte[] ntpShort)
        {
            return FromNtpShort(ntpShort, 0);
        }

        /// <summary>
        /// Converts a raw NTP short timespan to a .NET <see cref="TimeSpan"/> type, starting at the given offset in the raw data.
        /// </summary>
        /// <param name="rawData">The rawData that contains an NTP short timespan at the given offset.</param>
        /// <param name="offset">The offset the short timespan should be read from.</param>
        /// <returns>The converted .NET timespan.</returns>
        public static TimeSpan FromNtpShort(byte[] rawData, int offset)
        {
            // get the seconds
            var temp = BitConverter.ToDouble(rawData, offset);
            var totalSeconds = (double)temp / Fric;

            // convert to timespan
            var result = TimeSpan.FromSeconds(totalSeconds);
            return result;
        }
    }
}
