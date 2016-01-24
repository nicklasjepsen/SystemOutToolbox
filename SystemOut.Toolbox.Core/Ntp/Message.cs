using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SystemOut.Toolbox.Core.Ntp
{
    /// <summary>
    /// Represents an NTP message that can be send to and received from an internet time server.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The fixed header size is 12 words (48 bytes); this amount of data needs to be present in any case.
        /// </summary>
        private const int MinimumMessageSize = 48;

        // some offsets in the message
        private const byte StratumOffset = 1;
        private const byte PollOffset = 2;
        private const byte PrecisionOffset = 3;
        private const byte RootDelayOffset = 4;
        private const byte RootDispersionOffset = 8;
        private const byte ReferenceIdOffset = 12;
        private const byte ReferenceTimestampOffset = 16;
        private const byte OriginTimestampOffset = 24;
        private const byte ReceiveTimestampOffset = 32;
        private const byte TransmitTimestampOffset = 40;

        private byte[] _rawData;

        /// <summary>
        /// Gets or sets the leap indicator, which indicates an impending leap second to be inserted or deleted in the last minute of the current month.
        /// </summary>
        public LeapIndicator LeapIndicator
        {
            get
            {
                GuardHeader();
                return (LeapIndicator)((_rawData[0] >> 6) & 0x3);
            }
            set
            {
                GuardHeader();
                _rawData[0] |= (byte)(((int)value & 0x3) << 6);
            }
        }

        /// <summary>
        /// Gets or sets the version. Should always be <see cref="Constants.VersionNumber"/>.
        /// </summary>
        public int Version
        {
            get
            {
                GuardHeader();
                return ((_rawData[0] >> 3) & 0x7);
            }
            set
            {
                GuardHeader();
                _rawData[0] |= (byte)((value & 0x7) << 3);
            }
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        public Mode Mode
        {
            get
            {
                GuardHeader();
                return (Mode)(_rawData[0] & 0x7);
            }
            set
            {
                GuardHeader();
                _rawData[0] |= (byte)((int)value & 0x7);
            }
        }

        /// <summary>
        /// Gets or sets the stratum.
        /// </summary>
        public Stratum Stratum
        {
            get
            {
                GuardHeader();
                return (Stratum)_rawData[StratumOffset];
            }
            set
            {
                GuardHeader();
                _rawData[StratumOffset] = (byte)value;
            }
        }

        /// <summary>
        /// Gets or sets the poll, which represents the maximum interval between
        /// successive messages, in log2 seconds.  Suggested default limits for
        /// minimum and maximum poll intervals are 6 and 10, respectively.
        /// Note: Internally a data range of <c>sbyte.MinValue</c> through <c>sbyte.MaxValue</c> is used.
        /// </summary>
        public int Poll
        {
            get
            {
                GuardHeader();
                return (sbyte)_rawData[PollOffset];
            }
            set
            {
                GuardHeader();
                if (value < sbyte.MinValue || value > sbyte.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("Poll");
                }

                _rawData[PollOffset] = (byte)(sbyte)value;
            }
        }

        /// <summary>
        /// Gets or sets the precision, which represents the precision of the 
        /// system clock, in log2 seconds.  For instance, a value of -18
        /// corresponds to a precision of about one microsecond. 
        /// Note: Internally a data range of <c>sbyte.MinValue</c> through <c>sbyte.MaxValue</c> is used.
        /// </summary>
        public int Precision
        {
            get
            {
                GuardHeader();
                return (sbyte)_rawData[PrecisionOffset];
            }
            set
            {
                GuardHeader();
                if (value < sbyte.MinValue || value > sbyte.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("Precision");
                }

                // TODO: is this correct?
                _rawData[PrecisionOffset] = (byte)(sbyte)value;
            }
        }

        /// <summary>
        /// Gets or sets the root delay which indicates the total round-trip delay to the reference clock.
        /// </summary>
        public TimeSpan RootDelay
        {
            get
            {
                GuardHeader();
                return TimestampConverter.FromNtpShort(_rawData, RootDelayOffset);
            }
            set
            {
                GuardHeader();
                var ntpValue = TimestampConverter.ToNtpShort(value);
                Array.Copy(ntpValue, 0, _rawData, RootDelayOffset, ntpValue.Length);
            }
        }

        /// <summary>
        /// Gets or sets the root dispersion which indicates the total dispersion to the reference clock.
        /// </summary>
        public TimeSpan RootDispersion
        {
            get
            {
                GuardHeader();
                return TimestampConverter.FromNtpShort(_rawData, RootDispersionOffset);
            }
            set
            {
                GuardHeader();
                var ntpValue = TimestampConverter.ToNtpShort(value);
                Array.Copy(ntpValue, 0, _rawData, RootDispersionOffset, ntpValue.Length);
            }
        }

        /// <summary>
        /// Gets or sets the raw reference id as byte array.
        /// The interpretation of the id depends on the value of the Stratum property:
        /// - For a stratum value of <c>Invalid</c>, this is a four-character ASCII [RFC1345] string,
        ///   called the "kiss code", used for debugging and monitoring purposes. 
        ///   Use the <see cref="ReferenceIdAsString"/> property to access the string representation of the id then.
        /// - For a stratum value of <c>PrimaryServer</c>, this is a four-octet, left-justified, zero-padded ASCII
        ///   string assigned to the reference clock. The authoritative list of Reference Identifiers is maintained by IANA; 
        ///   however, any string beginning with the ASCII character "X" is reserved for unregistered experimentation and development.
        ///   Use the <see cref="ReferenceIdAsString"/> property to access the string representation of the id then.
        /// - For stratum values of <c>SecondaryServerN</c> this is the reference identifier of the server and can be used to detect timing
        ///   loops. If using the IPv4 address family, the identifier is the four-octet IPv4 address.
        ///   Use the <see cref="ReferenceIdAsIPAddress"/> property to access the IP address representation of the id then.
        /// </summary>
        public byte[] RawReferenceId
        {
            get
            {
                GuardHeader();
                var result = new byte[4];
                Array.Copy(_rawData, ReferenceIdOffset, result, 0, 4);
                return result;
            }
            set
            {
                GuardHeader();
                Array.Copy(value, 0, _rawData, ReferenceIdOffset, 4);
            }
        }

        /// <summary>
        /// Gets the reference id as string. Accessing the id as string is only valid if the stratum value
        /// is <c>Invalid</c> (then this property contains the "kiss code") or <c>PrimaryServer</c> (then
        /// the property contains either the IANA assigned identifier, or an unregistered identifier starting with "X").
        /// </summary>
        public string ReferenceIdAsString
        {
            get
            {
                // make sure to remove \0 terminator
                return Encoding.UTF8.GetString(RawReferenceId, 0, RawReferenceId.Length).Trim('\0');
            }
        }

        /// <summary>
        /// Gets the reference id as IP address. Accessing the id as IP address is only valid if the stratum value
        /// indicates a secondary server.
        /// </summary>
        public IPAddress ReferenceIdAsIPAddress
        {
            get
            {
                return new IPAddress(RawReferenceId);
            }
        }

        /// <summary>
        /// Gets or sets the reference timestamp which indicates the time when the system clock was last set or corrected.
        /// </summary>
        public DateTime ReferenceTimestamp
        {
            get
            {
                GuardHeader();
                return TimestampConverter.FromNtpTimestamp(_rawData, ReferenceTimestampOffset);
            }
            set
            {
                GuardHeader();
                var ntpValue = TimestampConverter.ToNtpTimestamp(value);
                Array.Copy(ntpValue, 0, _rawData, ReferenceTimestampOffset, ntpValue.Length);
            }
        }

        /// <summary>
        /// Gets or sets the origin timestamp which indicates the time at the client when the request departed for the server.
        /// </summary>
        public DateTime OriginTimestamp
        {
            get
            {
                GuardHeader();
                return TimestampConverter.FromNtpTimestamp(_rawData, OriginTimestampOffset);
            }
            set
            {
                GuardHeader();
                var ntpValue = TimestampConverter.ToNtpTimestamp(value);
                Array.Copy(ntpValue, 0, _rawData, OriginTimestampOffset, ntpValue.Length);
            }
        }

        /// <summary>
        /// Gets or sets the receive timestamp which indicates the time at the server when the request arrived from the client.
        /// </summary>
        public DateTime ReceiveTimestamp
        {
            get
            {
                GuardHeader();
                return TimestampConverter.FromNtpTimestamp(_rawData, ReceiveTimestampOffset);
            }
            set
            {
                GuardHeader();
                var ntpValue = TimestampConverter.ToNtpTimestamp(value);
                Array.Copy(ntpValue, 0, _rawData, ReceiveTimestampOffset, ntpValue.Length);
            }
        }

        /// <summary>
        /// Gets or sets the transmit timestamp which indicates the time at the server when the response left for the client.
        /// </summary>
        public DateTime TransmitTimestamp
        {
            get
            {
                GuardHeader();
                return TimestampConverter.FromNtpTimestamp(_rawData, TransmitTimestampOffset);
            }
            set
            {
                GuardHeader();
                var ntpValue = TimestampConverter.ToNtpTimestamp(value);
                Array.Copy(ntpValue, 0, _rawData, TransmitTimestampOffset, ntpValue.Length);
            }
        }

        /// <summary>
        /// Gets or sets the destination timestamp which indicates the time at which the message arrived at the client.
        /// Note: this value is not included as a header field in the message; it is determined upon arrival of the data at the client.
        /// </summary>
        public DateTime DestinationTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the local clock offset computed from the given local and server timestamps.
        /// Note: this is only valid for messages that have been received from a time server.
        /// </summary>
        public TimeSpan LocalClockOffset
        {
            get
            {
                GuardHeader();

                // compute and return the offset
                var offset = ((ReceiveTimestamp - OriginTimestamp) - (DestinationTimestamp - TransmitTimestamp)).TotalSeconds / 2.0;
                return TimeSpan.FromSeconds(offset);
            }
        }

        private void GuardHeader()
        {
            if (_rawData == null || _rawData.Length < MinimumMessageSize)
            {
                throw new InvalidOperationException("Not initialized or header has the wrong format/length.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class and preconfigures the <see cref="Version"/>
        /// value to <see cref="Constants.VersionNumber"/>.
        /// </summary>
        public Message()
        {
            _rawData = new byte[MinimumMessageSize];
            Version = Constants.VersionNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class, preconfigured with the given raw data.
        /// </summary>
        public Message(byte[] rawData)
        {
            if (rawData == null || rawData.Length < MinimumMessageSize)
            {
                throw new ArgumentException("The given data is invalid (smaller than the required minimum message size).", "rawData");
            }

            _rawData = rawData;

            if (Version > Constants.VersionNumber)
            {
                throw new ArgumentException("The provided data uses a newer NTP version than supported.", "rawData");
            }
        }

        /// <summary>
        /// Returns a copy of the message as raw byte array.
        /// Note: this does not include non-networking related properties (e.g. <see cref="DestinationTimestamp"/>.
        /// </summary>
        /// <returns>The raw representation of the networking relevant information of the message.</returns>
        public byte[] ToRawData()
        {
            return (byte[])_rawData.Clone();
        }

        /// <summary>
        /// Creates a text representation of the message that contains all its contained information.
        /// </summary>
        /// <returns>A text representation of the message that contains all its contained information.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Leap indicator: ");
            sb.AppendLine(LeapIndicator.ToString());
            sb.Append("Version: ");
            sb.AppendLine(Version.ToString());
            sb.Append("Mode: ");
            sb.AppendLine(Mode.ToString());
            sb.Append("Stratum: ");
            sb.AppendLine(Stratum.ToString());
            sb.Append("Poll (log2): ");
            sb.AppendLine(Poll.ToString());
            sb.Append("Precision (log2): ");
            sb.AppendLine(Precision.ToString());
            sb.Append("Root delay: ");
            sb.AppendLine(RootDelay.ToString("G"));
            sb.Append("Root dispersion: ");
            sb.AppendLine(RootDispersion.ToString("G"));
            if (Stratum == Stratum.Invalid)
            {
                sb.Append("Reference Id (kiss code): ");
                sb.AppendLine(ReferenceIdAsString);
            }
            else if (Stratum == Stratum.PrimaryServer)
            {
                sb.Append("Reference Id (IANA or X-custom): ");
                sb.AppendLine(ReferenceIdAsString);
            }
            else
            {
                sb.Append("Reference Id (IPv4): ");
                sb.AppendLine(ReferenceIdAsIPAddress.ToString());
            }

            // timestamps
            sb.Append("Reference timestamp: ");
            sb.AppendLine(ReferenceTimestamp.ToString("o"));
            sb.Append("Origin timestamp: ");
            sb.AppendLine(OriginTimestamp.ToString("o"));
            sb.Append("Receive timestamp: ");
            sb.AppendLine(ReceiveTimestamp.ToString("o"));
            sb.Append("Transmit timestamp: ");
            sb.AppendLine(TransmitTimestamp.ToString("o"));
            sb.Append("Destination timestamp: ");
            sb.AppendLine(DestinationTimestamp.ToString("o"));

            sb.Append("Local clock offset: ");
            sb.AppendLine(LocalClockOffset.ToString("G"));

            return sb.ToString();
        }
    }
}
